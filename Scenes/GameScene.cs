using System;
using SampleGame.GameObjects;
using SampleGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace SampleGame.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    // Reference to the player.
    private ObjectPlayer _player;

    // Reference to the fly.
    private List<ObjectNPC1> _flies = new List<ObjectNPC1>();
    private AnimatedSprite flyAnimation;

    // Reference to the bullets.
    private List<ObjectNPC2> _bullets = new List<ObjectNPC2>();
    private AnimatedSprite bulletAnimation;

    // Defines the tilemap to draw.
    private Level _level_01;

    private GameSceneUI _ui;

    private GameState _state;

    /// <summary>
    /// Indicates whether the player has moved during the current update.
    /// </summary>
    public bool _hasMoved = false;

    /// <summary>
    /// Countdown timer for the next ennemy move.
    /// </summary>
    private int _countdownTillNextMove;

    /// <summary>
    /// Countdown timer for the next enemy spawn.
    /// </summary>
    private int _countdownTillNextSpawn;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // During the game scene, we want to disable exit on escape. Instead,
        // the escape key will be used to return back to the title screen.
        Core.ExitOnEscape = false;

        // Create any UI elements from the root element created in previous
        // scenes.
        GumService.Default.Root.Children.Clear();

        // Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize a new game to be played.
        InitializeNewGame();
    }

    private void InitializeUI()
    {
        // Clear out any previous UI element incase we came here
        // from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _ui = new GameSceneUI();
    }

    private void InitializeNewGame()
    {
        _flies.Clear();
        _bullets.Clear();
    
        // Define the level layout based on the tilemap
        _level_01.InitializeGrid(_level_01.Layers[0].Columns, _level_01.Layers[0].Rows);

        _flies.Add(new ObjectNPC1(flyAnimation));
        _bullets.Add(new ObjectNPC2(bulletAnimation));

        Point playerPos = new Point(_level_01.Layers[0].Columns / 2, _level_01.Layers[0].Rows / 2);

        // Position of player is defined by a 1 in the level layout
        _level_01._levelGrid[playerPos.X, playerPos.Y] = 1;

        _player.Initialize(playerPos);

        // Position of flies is defined by a 1 in the level layout
        foreach (var fly in _flies)
            fly.Initialize(FindStartingPositionForNPCs(_level_01._levelGrid), _level_01._levelGrid);

        foreach (var bullet in _bullets)
            bullet.Initialize(FindStartingPositionForNPCs(_level_01._levelGrid));

        // Subscribe every fly to the collision event
        // foreach (var fly in _flies)
        // fly.CollisionWithPlayer += OnPlayerFlyCollision;

        _countdownTillNextMove = Game1.MoveDelay;
        _countdownTillNextSpawn = Game1.SpawnDelay;

        _state = GameState.Playing;
    }

    private Point FindStartingPositionForNPCs(int[,] levelGrid)
    {
        Random random = new Random();
        int width = levelGrid.GetLength(0);
        int height = levelGrid.GetLength(1);

        // Collect all walkable tiles
        List<Point> walkableTiles = new List<Point>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (levelGrid[x, y] == 0) // 0 = walkable
                    walkableTiles.Add(new Point(x, y));
            }
        }

        if (walkableTiles.Count == 0)
            throw new InvalidOperationException("No walkable tiles found in the level grid.");

        // Pick a random one
        int index = random.Next(walkableTiles.Count);
        return walkableTiles[index];
    }

    public override void LoadContent()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        // // Load the tilemap definition.
        // _level_01 = new Level();

        // Tilemap _tilemap = Tilemap.FromFile(Core.Content, "images/tilemap-definition.xml");
        // _tilemap.Scale = new Vector2(5.0f, 5.0f);
        // _level_01.AddLayer(_tilemap);

        // _tilemap = Tilemap.FromFile(Core.Content, "images/level-01-MainLayer.xml");
        // _tilemap.Scale = new Vector2(5.0f, 5.0f);
        // _level_01.AddLayer(_tilemap);

        _level_01 = new Level(Core.Content, "levels/level-01.json", "Background", "MainLayer");

        // Create the animated sprite for the player from the atlas.
        AnimatedSprite playerAnimation = atlas.CreateAnimatedSprite("player-animation-idle");
        playerAnimation.Scale = new Vector2(5.0f, 5.0f);

        // Create the animated sprite for the fly from the atlas.
        flyAnimation = atlas.CreateAnimatedSprite("fly-animation-idle");
        flyAnimation.Scale = new Vector2(5.0f, 5.0f);

        // Create the animated sprite for the bullet from the atlas.
        bulletAnimation = atlas.CreateAnimatedSprite("bullet-animation-idle");
        bulletAnimation.Scale = new Vector2(5.0f, 5.0f);

        // Create the player.
        _player = new ObjectPlayer(playerAnimation);
    }

    private void UpdateMovementPlayer()
    {
        Point _potentialPosition = _player._gridPosition;
        _hasMoved = false;

        /* Player Movement */
        if (GameController.MoveLeft())
            _potentialPosition -= new Point(1, 0);
        else if (GameController.MoveRight())
            _potentialPosition += new Point(1, 0);
        else if (GameController.MoveUp())
            _potentialPosition -= new Point(0, 1);
        else if (GameController.MoveDown())
            _potentialPosition += new Point(0, 1);

        if (_potentialPosition != _player._gridPosition &&
            _level_01._levelGrid[_potentialPosition.X, _potentialPosition.Y] != 1)
        {
            _player.MoveTo(_potentialPosition, _level_01._levelGrid);
            _hasMoved = true;
            _countdownTillNextSpawn--;
            // Iterate backwards to safely remove items from the list while iterating
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                if (bullet._gridPosition == _player._gridPosition)
                {
                    if (_flies.Count > 0)
                    {
                        _bullets.RemoveAt(i);
                        _level_01._levelGrid[_flies[0]._gridPosition.X, _flies[0]._gridPosition.Y] = 0;
                        _flies.RemoveAt(0);
                    }
                }
            }
        }

        if (_countdownTillNextSpawn <= 0)
        {
            OnTimeToAddNewNPC();
            _countdownTillNextSpawn = Game1.SpawnDelay;
        }
    }

    public void UpdateMovementEnemies(GameTime gameTime)
    {
        /* Pathfinding for flies */
        Pathfinder pathfinder = new(_level_01._levelGrid);
        foreach (var fly in _flies)
        {
            Point nextPosition = pathfinder.GetNextPosition(fly._gridPosition, _player._gridPosition);
            fly.MoveTo(nextPosition, _level_01._levelGrid);
            if (fly._gridPosition == _player._gridPosition)
            {
                // Handle collision with player
                OnPlayerFlyCollision();
            }
        }
    }

    private void OnPlayerFlyCollision()
    {
        GameOver();
    }

    public override void Update(GameTime gameTime)
    {
        // Ensure the UI is always updated.
        _ui.Update(gameTime);

        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            InitializeNewGame();

        // If the game is in a game over state, immediately return back
        // here.
        if (_state == GameState.GameOver)
        {
            return;
        }

        // If the pause button is pressed, toggle the pause state.
        if (GameController.Pause())
        {
            TogglePause();
        }

        // At this point, if the game is paused, just return back early.
        if (_state == GameState.Paused)
        {
            return;
        }

        UpdateMovementPlayer();
        if (_hasMoved)
            _countdownTillNextMove--;
        if (_countdownTillNextMove <= 0)
        {
            _countdownTillNextMove = Game1.MoveDelay;
            UpdateMovementEnemies(gameTime);
        }

        _player.Update(gameTime);
        foreach (var fly in _flies)
            fly.Update(gameTime);
        foreach (var bullet in _bullets)
            bullet.Update(gameTime);

        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            Core.Instance.Exit();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            // We're now unpausing the game, so hide the pause panel.
            // _ui.HidePausePanel();

            // And set the state back to playing.
            _state = GameState.Playing;
        }
        else
        {
            // We're now pausing the game, so show the pause panel.
            // _ui.ShowPausePanel();

            // And set the state to paused.
            _state = GameState.Paused;
        }
    }

    private void OnPlayerFlyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void OnTimeToAddNewNPC()
    {
        // Spawn a new NPC
        var newNPC = new ObjectNPC1(flyAnimation);
        newNPC.Initialize(FindStartingPositionForNPCs(_level_01._levelGrid), _level_01._levelGrid);

        // Spawn a new NPC2
        var newNPC2 = new ObjectNPC2(bulletAnimation);
        newNPC2.Initialize(FindStartingPositionForNPCs(_level_01._levelGrid));

        _flies.Add(newNPC);
        _bullets.Add(newNPC2);
    }

    private void GameOver()
    {
        // Show the game over panel.
        // _ui.ShowGameOverPanel();

        // Set the game state to game over.
        _state = GameState.GameOver;
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _level_01.Draw(Core.SpriteBatch);

        _player.Draw();

        foreach (var fly in _flies)
            fly.Draw();

        foreach (var bullet in _bullets)
            bullet.Draw();

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        // Draw the UI.
        _ui.Draw();
    }
}
