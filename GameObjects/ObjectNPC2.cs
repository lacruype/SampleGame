using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace SampleGame.GameObjects;

public class ObjectNPC2
{
    /* ================================== ATTRIBUTES ================================== */
    /// <summary>
    /// The animated sprite used to draw the ObjectNPC.
    /// </summary>
    private AnimatedSprite _sprite;
 
    /// <summary>
    /// The tile position of the ObjectNPC2.
    /// </summary>
    public Point _gridPosition { get; set; }   // e.g., (5, 2)

    /// <summary>
    /// The position of the ObjectNPC2 on the screen
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
    public ObjectNPC2(AnimatedSprite sprite)
    {
        _sprite = sprite;
    }

    /* ================================== METHODS ================================== */
    /// <summary>
    /// Initializes the ObjectNPC with the starting position.
    /// </summary>
    /// <param name="startingPosition"></param>
    public void Initialize(Point startingPosition)
    {
        _gridPosition = startingPosition;
    }

    /// <summary>
    /// Updates the bullet.
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
