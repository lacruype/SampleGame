using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using SampleGame.Scenes;

namespace SampleGame.GameObjects
{
    public class Level
    {
        /* ================================== ATTRIBUTES ================================== */
        public List<Tilemap> layers { get; private set; }

        // Array of int defining the level layout.
        // 0 = walkable, 1 = obstacle, 2 = Player, 3 = Ennemies (obstacle too)
        public int[,] _levelGrid;
        /* ================================== EVENTS ================================== */
        /* ================================== CONSTRUCTORS ================================== */
        /// <summary>
        /// Default constructor for the Level class.
        /// </summary>
        public Level()
        {
            layers = new List<Tilemap>();
            InitializeGrid(10, 10);
        }

        /// <summary>
        /// Creates a new Level by loading multiple layers from a JSON file.
        /// </summary>
        /// <param name="content">The ContentManager to use for loading assets.</param>
        /// <param name="filename">The path to the JSON file containing the level data.</param>
        /// <param name="layerNames">The names of the layers to load, in the order they should be drawn.</param>
        public Level(ContentManager content, string filename, params string[] layerNames)
        {
            layers = new List<Tilemap>();

            foreach (string layerName in layerNames)
            {
                Tilemap tilemap = Tilemap.FromFileJSON(content, filename, layerName);
                tilemap.Scale = new Vector2(5.0f, 5.0f);
                layers.Add(tilemap);
            }
        }

        /* ================================== METHODS ================================== */

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
                        _levelGrid[x, y] |= GameScene.CellType.WALL;
                    else
                        _levelGrid[x, y] |= GameScene.CellType.WALKABLE;
                }
            }
        }
    }
}