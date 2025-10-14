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

    public static class CellType
    {
        public const int WALKABLE = 1 << 0; // 1
        public const int WALL     = 1 << 1; // 2
        public const int PLAYER   = 1 << 2; // 4
        public const int ZOMBIE   = 1 << 3; // 8
        public const int BULLET   = 1 << 4; // 16
    }

    // Reference to the player.
    private ObjectPlayer _player;

    // Reference to the zombie.
    private List<Zombie> _zombies = new List<Zombie>();
    // We used to store shared AnimatedSprite instances here, but that caused
    // flipping/effects on one NPC to affect all others. Instead keep a
    // reference to the atlas and create a fresh AnimatedSprite per instance.

    /// <summary>
    /// Reference to the bullets.
    /// </summary>
    private List<Bullet> _bullets = new List<Bullet>();

    /// <summary>
    /// The texture atlas to create per-entity AnimatedSprite instances from.
    /// </summary>
    private TextureAtlas _atlas;

    /// <summary>
    /// Defines the tilemap to draw.
    /// </summary>
    private Level _level_01;

    /// <summary>
    /// The current state of the game (playing, paused, game over).
    /// </summary>
    private GameState _state;

    /// <summary>
    /// Indicates whether the player has moved during the current update.
    /// </summary>
    public bool _hasMoved = false;

    /// <summary>
    /// Countdown timer for the next ennemy move.
    /// </summary>
    private int _countdownTillNextEnemyMove;

    /// <summary>
    /// Countdown timer for the next enemy spawn.
    /// </summary>
    private int _countdownTillNextSpawn;

    /// <summary>
    /// Player's score.
    /// </summary>
    private int _score = 0;

    /// <summary>
    /// The user interface for the game scene.
    /// </summary>
    private GameSceneUI _ui;

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
        _zombies.Clear();
        _bullets.Clear();
    
        // Define the level layout based on the tilemap
        _level_01.InitializeGrid(_level_01.Layers[0].Columns, _level_01.Layers[0].Rows);

        OnTimeToAddNewNPC();

        Point playerPos = new Point(_level_01.Layers[0].Columns / 2, _level_01.Layers[0].Rows / 2);

        _player.Initialize(playerPos, _level_01._levelGrid);

        _countdownTillNextEnemyMove = Game1.MoveDelay;
        _countdownTillNextSpawn = Game1.SpawnDelay;
        _score = 0;
        _ui.UpdateScoreText(_score);

        _state = GameState.Playing;
    }

    private Point FindStartingPositionForZombies(int[,] levelGrid)
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
                // If the tile is walkable and at least 3 tiles away from the player, add it to the list
                if (levelGrid[x, y] == CellType.WALKABLE && IsFarEnoughFromPlayer(new Point(x, y), 3))
                    walkableTiles.Add(new Point(x, y));
            }
        }

        if (walkableTiles.Count == 0)
        {
            Console.WriteLine("No walkable tiles found to add Zombie");
            // Returns a null Point
            return new Point(-1, -1);
        }

        // Pick a random one
        int index = random.Next(walkableTiles.Count);
        return walkableTiles[index];
    }

    private bool IsFarEnoughFromPlayer(Point tile, int minDistance)
    {
        int distanceX = Math.Abs(tile.X - _player._gridPosition.X);
        int distanceY = Math.Abs(tile.Y - _player._gridPosition.Y);
        return (distanceX >= minDistance || distanceY >= minDistance);
    }

    private Point FindStartingPositionForBullets(int[,] levelGrid)
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
                if (levelGrid[x, y] == CellType.WALKABLE && IsFarEnoughFromPlayer(new Point(x, y), 1))
                    walkableTiles.Add(new Point(x, y));
            }
        }

        if (walkableTiles.Count == 0)
        {
            Console.WriteLine("No walkable tiles found to add Bullet");
            return new Point(-1, -1);
        }

        // Pick a random one
        int index = random.Next(walkableTiles.Count);
        return walkableTiles[index];
    }

    public override void LoadContent()
    {
        _atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _level_01 = new Level(Core.Content, "levels/level-01.json", "Background", "MainLayer");

        // Create the animated sprite for the player from the atlas.
        AnimatedSprite playerAnimation = _atlas.CreateAnimatedSprite("player-animation-idle");
        playerAnimation.Scale = new Vector2(5.0f, 5.0f);

        // Create the player.
        _player = new ObjectPlayer(playerAnimation);
    }

    private void UpdateMovementPlayer()
    {
        Point _potentialPosition = _player._gridPosition;
        // Need to check if the player really moved and not just pressed the button and hit a wall
        _hasMoved = false;

        /* Player Movement */
        if (GameController.MoveLeft())
        {
            _player._sprite.Effects = SpriteEffects.FlipHorizontally;
            _potentialPosition -= new Point(1, 0);
        }
        else if (GameController.MoveRight())
        {
            _player._sprite.Effects = SpriteEffects.None;
            _potentialPosition += new Point(1, 0);
        }
        else if (GameController.MoveUp())
            _potentialPosition -= new Point(0, 1);
        else if (GameController.MoveDown())
            _potentialPosition += new Point(0, 1);

        if (_potentialPosition == _player._gridPosition || 
        (_level_01._levelGrid[_potentialPosition.X, _potentialPosition.Y] & CellType.WALL) != 0)
            return;

        _player.MoveTo(_potentialPosition, _level_01._levelGrid);
        foreach (var zombie in _zombies)
            if (zombie._gridPosition == _player._gridPosition)
                OnPlayerZombieCollision();
        _hasMoved = true;
        _countdownTillNextSpawn--;
        if ((_level_01._levelGrid[_player._gridPosition.X, _player._gridPosition.Y] & CellType.BULLET) != 0)
        {
            // Iterate backwards to safely remove items from the list while iterating
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                if (bullet._gridPosition == _player._gridPosition)
                {
                    if (_zombies.Count > 0)
                    {
                        // The bullet has been used so we delete the flag BULLET from its tile
                        _level_01._levelGrid[bullet._gridPosition.X, bullet._gridPosition.Y] &= ~CellType.BULLET;
                        _bullets.RemoveAt(i);
                        // The zombie has been hit so we delete the flag ZOMBIE from its tile
                        _level_01._levelGrid[_zombies[0]._gridPosition.X,
                                            _zombies[0]._gridPosition.Y] &= ~CellType.ZOMBIE;
                        _zombies.RemoveAt(0);
                        _score += 100; // Increase score for hitting a zombie
                        _ui.UpdateScoreText(_score);
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
        foreach (var zombie in _zombies)
        {
            Point nextPosition = pathfinder.GetNextPosition(zombie._gridPosition, _player._gridPosition);
            zombie.MoveTo(nextPosition, _level_01._levelGrid);
            if (zombie._gridPosition == _player._gridPosition)
            {
                // Handle collision with player
                OnPlayerZombieCollision();
            }
        }
    }

    private void OnPlayerZombieCollision()
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
            _countdownTillNextEnemyMove--;
        if (_countdownTillNextEnemyMove <= 0)
        {
            _countdownTillNextEnemyMove = Game1.MoveDelay;
            UpdateMovementEnemies(gameTime);
        }

        _player.Update(gameTime);
        foreach (var zombie in _zombies)
            zombie.Update(gameTime);
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

    private void OnPlayerZombieCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void OnTimeToAddNewNPC()
    {
        // Spawn a new NPC (zombie) with its own AnimatedSprite instance
        var zombieSprite = _atlas.CreateAnimatedSprite("zombie-animation-idle");
        zombieSprite.Scale = new Vector2(5.0f, 5.0f);
        var newZombie = new Zombie(zombieSprite);
        Point zombieStartPos = FindStartingPositionForZombies(_level_01._levelGrid);
        if (zombieStartPos != new Point(-1, -1))
            newZombie.Initialize(zombieStartPos, _level_01._levelGrid);

        // Spawn a new NPC2 (bullet) with its own AnimatedSprite instance
        var bulletSprite = _atlas.CreateAnimatedSprite("bullet-animation-idle");
        bulletSprite.Scale = new Vector2(5.0f, 5.0f);
        var newBullet = new Bullet(bulletSprite);
        Point bulletStartPos = FindStartingPositionForBullets(_level_01._levelGrid);
        if (bulletStartPos != new Point(-1, -1))
            newBullet.Initialize(bulletStartPos, _level_01._levelGrid);

        _zombies.Add(newZombie);
        _bullets.Add(newBullet);
    }

    private void GameOver()
    {
        // Show the game over panel.
        _ui.ShowGameOverPanel();

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

        foreach (var zombie in _zombies)
            zombie.Draw();

        foreach (var bullet in _bullets)
            bullet.Draw();

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        // Draw the UI.
        _ui.Draw();
    }
}
