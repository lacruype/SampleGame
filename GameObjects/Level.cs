using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;

namespace SampleGame.GameObjects
{
    public class Level
    {
        protected List<Tilemap> layers = new List<Tilemap>();

        // Array of int defining the level layout.
        // 0 = walkable, 1 = obstacle, 2 = Player, 3 = Ennemies (obstacle too)
        public int[,] _levelGrid;

        public void AddLayer(Tilemap layer)
        {
            if (layer != null)
            {
                layers.Add(layer);
            }
        }

        public void RemoveLayer(int index)
        {
            if (index >= 0 && index < layers.Count)
            {
                layers.RemoveAt(index);
            }
        }

        public IReadOnlyList<Tilemap> Layers => layers.AsReadOnly();

        // Draw all layers
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach (var layer in layers)
            {
                layer.Draw(spriteBatch);
            }
        }

        public void InitializeGrid(int width, int height)
        {
            _levelGrid = new int[width, height];

            // TEMPORARY INITIALIZING
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        _levelGrid[x, y] = 1;
                    else
                        _levelGrid[x, y] = 0;
                }
            }
        }
    }
}