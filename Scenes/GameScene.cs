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

    private enum EnemyType
    {
        Zombie,
        BigZombie,
        FastZombie
    }

    public static class CellType
    {
        public const int WALKABLE = 1 << 0; // 1
        public const int WALL = 1 << 1; // 2
        public const int PLAYER = 1 << 2; // 4
        public const int ZOMBIE = 1 << 3; // 8
        public const int BULLET = 1 << 4; // 16
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
    /// Defines the current level being played.
    /// </summary>
    private Level _currentLevel;

    /// <summary>
    /// The current state of the game (playing, paused, game over).
    /// </summary>
    private GameState _state;

    /// <summary>
    /// The spawn configuration loaded from JSON.
    /// </summary>
    private SpawnConfig _spawnConfig = SpawnConfigLoader.Load("Content/settings/spawn_config.json");

    /// <summary>
    /// The current turn number in the game.
    /// </summary>
    private int _currentTurn = 0;

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
        Random rng = new Random();

        _player.Dispose();
        // Dispose of existing zombies and bullets
        foreach (var z in _zombies)
            z.Dispose();
        _zombies.Clear();
        _bullets.Clear();
    
        // Define the level layout based on the tilemap
        _currentLevel.InitializeGrid(_currentLevel.Layers[0].Columns, _currentLevel.Layers[0].Rows);

        OnTimeToAddNewNPC();

        Point playerPos = new Point(_currentLevel.Layers[0].Columns / 2, _currentLevel.Layers[0].Rows / 2);

        _player.Initialize(playerPos, _currentLevel._levelGrid);

        _player.PlayerMoved += OnPlayerHasMoved;

        _countdownTillNextSpawn = Game1.SpawnDelay;
        _score = 0;
        _ui.UpdateScoreText(_score);

        foreach (var cat in _spawnConfig.Categories)
            cat.ResetSpawnTimer(rng);

        // Schedule the first spawn for each rule
        foreach (var cat in _spawnConfig.Categories)
        {
            foreach (var rule in cat.Rules)
            {
                rule.ScheduleNextSpawn(0, rng);
            }
        }

        _currentTurn = 0;

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

        _currentLevel = new Level(Core.Content, "levels/level-01.json", "Background", "MainLayer");

        // Create the animated sprite for the player from the atlas.
        AnimatedSprite playerAnimation = _atlas.CreateAnimatedSprite("player-animation-idle");
        playerAnimation.Scale = new Vector2(5.0f, 5.0f);

        // Create the player.
        _player = new ObjectPlayer(playerAnimation);
    }

    private void OnPlayerHasMoved(object sender, EventArgs args)
    {
        // Debug print map
        // for (int y = 0; y < _currentLevel._levelGrid.GetLength(1); y++)
        // {
        //     for (int x = 0; x < _currentLevel._levelGrid.GetLength(0); x++)
        //     {
        //         Console.Write((_currentLevel._levelGrid[x, y] + " ").PadLeft(3));
        //     }
        //     Console.WriteLine();
        // }

        foreach (var zombie in _zombies)
            if (zombie._gridPosition == _player._gridPosition)
                OnPlayerZombieCollision(null, EventArgs.Empty);
        if ((_currentLevel._levelGrid[_player._gridPosition.X, _player._gridPosition.Y] & CellType.BULLET) != 0)
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
                        _currentLevel._levelGrid[bullet._gridPosition.X, bullet._gridPosition.Y] &= ~CellType.BULLET;
                        _bullets.RemoveAt(i);
                        // Remove the first zombie in the list
                        _zombies[0].Dispose();
                        _zombies.RemoveAt(0);
                        _score += 100; // Increase score for hitting a zombie
                        _ui.UpdateScoreText(_score);
                    }
                }
            }
        }

        _currentTurn++;
        foreach (var cat in _spawnConfig.Categories)
        {
            if (!cat.CanSpawn(_currentTurn))
                continue;

            var rule = cat.PickWeighted(new Random());
            if (rule == null)
                continue;

            Console.WriteLine($"Spawning {rule.EntityType} from category {cat.Name} at turn {_currentTurn}");

            cat.ScheduleNextSpawn(_currentTurn, rule, new Random());
        }

        --_countdownTillNextSpawn;
        if (_countdownTillNextSpawn <= 0)
        {
            OnTimeToAddNewNPC();
            _countdownTillNextSpawn = Game1.SpawnDelay;
        }
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

        _player.HandleInput(_currentLevel._levelGrid);

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
        // Randomize through enemy types to choose zombie type to spawn
        Random random = new Random();
        var newZombie = null as Zombie;

        EnemyType enemyType = (EnemyType)random.Next(0, Enum.GetValues(typeof(EnemyType)).Length);

        // Spawn a new BigZombie (stronger zombie) with its own AnimatedSprite instance
        var newZombieSprite = _atlas.CreateAnimatedSprite("zombie-animation-idle");
        switch (enemyType)
        {
            case EnemyType.BigZombie:
                newZombie = new BigZombie(newZombieSprite, _player, _currentLevel._levelGrid);
                break;
            case EnemyType.FastZombie:
                newZombie = new FastZombie(newZombieSprite, _player, _currentLevel._levelGrid);
                break;
            
            default:
                newZombie = new Zombie(newZombieSprite, _player, _currentLevel._levelGrid);
                break;
        }
        Point newZombieStartPos = FindStartingPositionForZombies(_currentLevel._levelGrid);
        if (newZombieStartPos != new Point(-1, -1))
            newZombie.Initialize(newZombieStartPos, _currentLevel._levelGrid);

        // Spawn a new NPC2 (bullet) with its own AnimatedSprite instance
        var bulletSprite = _atlas.CreateAnimatedSprite("bullet-animation-idle");
        bulletSprite.Scale = new Vector2(5.0f, 5.0f);
        var newBullet = new Bullet(bulletSprite);
        Point bulletStartPos = FindStartingPositionForBullets(_currentLevel._levelGrid);
        if (bulletStartPos != new Point(-1, -1))
            newBullet.Initialize(bulletStartPos, _currentLevel._levelGrid);

        _zombies.Add(newZombie);
        _bullets.Add(newBullet);

        newZombie.ZombieHasCollidedWithPlayer += OnPlayerZombieCollision;
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

        _currentLevel.Draw(Core.SpriteBatch);

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
