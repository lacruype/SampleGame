using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace SampleGame.GameObjects;

public class ObjectNPC1
{
    /* ================================== ATTRIBUTES ================================== */
    /// <summary>
    /// The animated sprite used to draw the ObjectNPC.
    /// </summary>
    private AnimatedSprite _sprite;

    /// <summary>
    /// The tile position of the ObjectNPC1.
    /// </summary>
    public Point _gridPosition { get; set; }   // e.g., (5, 2)

    /// <summary>
    /// The position of the ObjectNPC1 on the screen
    /// </summary>
    public Vector2 _screenPosition => new Vector2(
        _gridPosition.X * Game1.TileSize,
        _gridPosition.Y * Game1.TileSize
    );

    /* ================================== EVENTS ================================== */

    /* ================================== CONSTRUCTOR ================================= */
    /// <summary>
    /// Creates a new ObjectNPC using the specified animated sprite and sound effect.
    /// </summary>
    /// <param name="sprite">The AnimatedSprite ot use when drawing the bat.</param>
    public ObjectNPC1(AnimatedSprite sprite)
    {
        _sprite = sprite;
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

    public void MoveTo(Point newPosition, int[,] _levelGrid)
    {
        _levelGrid[_gridPosition.X, _gridPosition.Y] = 0;
        _gridPosition = newPosition;
        _levelGrid[_gridPosition.X, _gridPosition.Y] = 1;
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
