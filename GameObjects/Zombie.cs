using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using SampleGame.Scenes;

namespace SampleGame.GameObjects;

public class Zombie
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

    /* ================================== EVENTS ================================== */

    /* ================================== CONSTRUCTOR ================================= */
    /// <summary>
    /// Creates a new ObjectNPC using the specified animated sprite and sound effect.
    /// </summary>
    /// <param name="sprite">The AnimatedSprite ot use when drawing the bat.</param>
    public Zombie(AnimatedSprite sprite)
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
        _levelGrid[_gridPosition.X, _gridPosition.Y] &= ~GameScene.CellType.ZOMBIE;
        if (_gridPosition.X < newPosition.X)
            _sprite.Effects = SpriteEffects.FlipHorizontally;
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
