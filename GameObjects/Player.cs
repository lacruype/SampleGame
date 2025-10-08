using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using SampleGame.Scenes;

namespace SampleGame.GameObjects;

public class ObjectPlayer
{
    /* ================================== ATTRIBUTES ================================== */
    /// <summary>
    /// The animated sprite used to draw the player.
    /// </summary>
    public AnimatedSprite _sprite;

    /// <summary>
    /// The tile position of the player.
    /// </summary>
    public Point _gridPosition { get; set; }   // e.g., (5, 2)

    /// <summary>
    /// The position of the player on the screen
    /// </summary>
    public Vector2 _screenPosition => new Vector2(
        _gridPosition.X * Game1.TileSize,
        _gridPosition.Y * Game1.TileSize
    );

    /* ================================== EVENTS ================================== */

    /* ================================== CONSTRUCTORS ================================== */
    /// <summary>
    /// Creates a new ObjectPlayer using the specified animated sprite.
    /// </summary>
    /// <param name="sprite">The AnimatedSprite to use when drawing the frog.</param>
    public ObjectPlayer(AnimatedSprite sprite)
    {
        _sprite = sprite;
    }

    /* ================================== METHODS ================================== */
    /// <summary>
    /// Initializes the player, can be used to reset it back to an initial state.
    /// </summary>
    /// <param name="startingPosition">The position the player should start at.</param>
    public void Initialize(Point startingPosition, int[,] levelGrid)
    {
        _gridPosition = startingPosition;
        MoveTo(_gridPosition, levelGrid);
    }

    /// <summary>
    /// Updates the player.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
    public void Update(GameTime gameTime)
    {
        // Update the animated sprite.
        _sprite.Update(gameTime);

        // Handle any player input
        // HandleInput();
    }

    /// <summary>
    /// Draws the slime.
    /// </summary>
    public void Draw()
    {
        _sprite.Draw(Core.SpriteBatch, _screenPosition);
    }

    public void MoveTo(Point newPosition, int[,] _levelGrid)
    {
        _levelGrid[_gridPosition.X, _gridPosition.Y] &= ~GameScene.CellType.PLAYER;
        _gridPosition = newPosition;
        _levelGrid[_gridPosition.X, _gridPosition.Y] |= GameScene.CellType.PLAYER;
    }
}
