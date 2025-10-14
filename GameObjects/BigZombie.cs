using System;
using MonoGameLibrary.Graphics;

namespace SampleGame.GameObjects;

public class BigZombie : Zombie, IDisposable
{
    public BigZombie(AnimatedSprite sprite, ObjectPlayer player, int[,] levelGrid) : base(sprite, player, levelGrid)
    {
        _moveSpan = 2; // BigZombie can move 2 tiles at once
        _resetMoveCountdown = 3; // BigZombie moves every 3 updates
        moveCountdown = _resetMoveCountdown; // Initialize countdown

        sprite.Color = Microsoft.Xna.Framework.Color.Red; // Differentiate BigZombie visually
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
