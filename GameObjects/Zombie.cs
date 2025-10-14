using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using SampleGame.Scenes;

namespace SampleGame.GameObjects;

public class Zombie : IDisposable
{
    /* ================================== ATTRIBUTES ================================== */
    /// <summary>
    /// The animated sprite used to draw the ObjectNPC.
    /// </summary>
    private AnimatedSprite _sprite;

    /// <summary>
    /// The tile position of the Zombie.
    /// </summary>
    public Point _gridPosition { get; set; }   // e.g., (5, 2)

    /// <summary>
    /// The position of the Zombie on the screen
    /// </summary>
    public Vector2 _screenPosition => new Vector2(
        _gridPosition.X * Game1.TileSize,
        _gridPosition.Y * Game1.TileSize
    );

    /// <summary>
    /// The player instance, used to check for collisions. Only needed because the zombie moves
    /// </summary>
    private ObjectPlayer _player;

    /// <summary>
    /// The level grid the Zombie is navigating.
    /// </summary>
    private int[,] _levelGrid;

    /// <summary>
    /// The movement span of the zombie, defining how many tiles it can move at once.
    /// </summary>
    public int _moveSpan { get; set; } = 1;

    /// <summary>
    /// The countdown timer to control how often the zombie moves.
    /// </summary>
    public int moveCountdown { get; set; } = 2;

    /// <summary>
    /// The countdown timer to control how often the zombie resets its move countdown.
    /// </summary>
    public int _resetMoveCountdown { get; set; } = 2;

    /* ================================== EVENTS ================================== */
    /// <summary>
    /// Event invoked when the Zombie collides with the player.
    /// </summary>
    public event EventHandler ZombieHasCollidedWithPlayer;

    /* ================================== CONSTRUCTOR ================================= */
    /// <summary>
    /// Creates a new ObjectNPC using the specified animated sprite and sound effect.
    /// </summary>
    /// <param name="sprite">The AnimatedSprite ot use when drawing the bat.</param>
    public Zombie(AnimatedSprite sprite, ObjectPlayer player, int[,] levelGrid)
    {
        _sprite = sprite;
        _player = player;
        _levelGrid = levelGrid;

        _sprite.Scale = new Vector2(5.0f, 5.0f);

        _player.PlayerMoved += OnPlayerMoved;
    }

    public virtual void Dispose()
    {
        _levelGrid[_gridPosition.X, _gridPosition.Y] &= ~GameScene.CellType.ZOMBIE;
        _player.PlayerMoved -= OnPlayerMoved;
    }

    /* ================================== METHODS ================================== */
    /// <summary>
    /// Initializes the ObjectNPC with the starting position.
    /// </summary>
    /// <param name="startingPosition"></param>
    public void Initialize(Point startingPosition, int[,] levelGrid)
    {
        MoveTo(startingPosition, levelGrid);
    }

    private void OnPlayerMoved(object sender, EventArgs e)
    {
        --moveCountdown;
        if (moveCountdown > 0)
            return;
        moveCountdown = _resetMoveCountdown; // Reset countdown

        for (int i = 0; i < _moveSpan; i++)
        {
            Pathfinder pathfinder = new(_levelGrid);
            Point nextPosition = pathfinder.GetNextPosition(_gridPosition, _player._gridPosition);

            MoveTo(nextPosition, _levelGrid);
            if (_gridPosition == _player._gridPosition)
            {
                // Handle collision with player
                ZombieHasCollidedWithPlayer?.Invoke(this, EventArgs.Empty);
                break;
            }
        }
    }

    public void MoveTo(Point newPosition, int[,] _levelGrid)
    {
        _levelGrid[_gridPosition.X, _gridPosition.Y] &= ~GameScene.CellType.ZOMBIE;
        if (_gridPosition.X < newPosition.X)
            _sprite.Effects = SpriteEffects.FlipHorizontally; // Facing right
        else if (_gridPosition.X > newPosition.X)
            _sprite.Effects = SpriteEffects.None; // Facing left
        _gridPosition = newPosition;
        _levelGrid[_gridPosition.X, _gridPosition.Y] |= GameScene.CellType.ZOMBIE;
    }


    /// <summary>
    /// Updates the bat.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
    public void Update(GameTime gameTime)
    {
        // Update the animated sprite
        _sprite.Update(gameTime);
    }

    /// <summary>
    /// Draws the bat.
    /// </summary>
    public void Draw()
    {
        _sprite.Draw(Core.SpriteBatch, _screenPosition);
    }
}
