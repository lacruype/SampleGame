using System;
using MonoGameLibrary.Graphics;

namespace SampleGame.GameObjects;

public class FastZombie : Zombie, IDisposable
{
    public FastZombie(AnimatedSprite sprite, ObjectPlayer player, int[,] levelGrid) : base(sprite, player, levelGrid)
    {
        _moveSpan = 1; // FastZombie can move 1 tile at once
        _resetMoveCountdown = 1; // FastZombie moves every 1 updates
        moveCountdown = _resetMoveCountdown; // Initialize countdown

        sprite.Color = Microsoft.Xna.Framework.Color.Black; // Differentiate FastZombie visually
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
