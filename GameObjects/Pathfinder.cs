using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SampleGame.Scenes; // for Vector2

namespace SampleGame.GameObjects
{
    public class Pathfinder
    {
        private int[,] grid;
        private int width, height;

        private static readonly (int x, int y)[] Directions =
        {
            ( 1,  0),
            (-1,  0),
            ( 0,  1),
            ( 0, -1)
        };

        public Pathfinder(int[,] grid)
        {
            this.grid = grid;
            width = grid.GetLength(0);
            height = grid.GetLength(1);
        }

        private class Node
        {
            public int X, Y;
            public int G; // cost from start
            public int H; // heuristic to goal
            public int F => G + H;
            public Node Parent;
        }

        // Basic Manhattan distance
        private int Heuristic((int x, int y) a, (int x, int y) b)
            => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        // Find path using A*
        private List<(int x, int y)> FindPath((int x, int y) start, (int x, int y) goal)
        {
            var open = new List<Node>();
            var closed = new HashSet<(int, int)>();

            var startNode = new Node
            {
                X = start.x,
                Y = start.y,
                G = 0,
                H = Heuristic(start, goal)
            };
            open.Add(startNode);

            while (open.Count > 0)
            {
                // Pick node with lowest F
                open.Sort((a, b) => a.F.CompareTo(b.F));
                var current = open[0];
                open.RemoveAt(0);

                if (current.X == goal.x && current.Y == goal.y)
                    return ReconstructPath(current);

                closed.Add((current.X, current.Y));

                foreach (var (dx, dy) in Directions)
                {
                    int nx = current.X + dx;
                    int ny = current.Y + dy;

                    if (!IsInsideGrid(nx, ny))
                        continue;

                    int cell = grid[nx, ny];
                    if ((cell & GameScene.CellType.WALL) != 0 ||
                        (cell & GameScene.CellType.ZOMBIE) != 0) // wall or other zombie
                        continue;

                    if (closed.Contains((nx, ny)))
                        continue;

                    int tentativeG = current.G + 1;
                    var neighbor = open.Find(n => n.X == nx && n.Y == ny);

                    if (neighbor == null)
                    {
                        neighbor = new Node
                        {
                            X = nx,
                            Y = ny,
                            G = tentativeG,
                            H = Heuristic((nx, ny), goal),
                            Parent = current
                        };
                        open.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.Parent = current;
                    }
                }
            }

            return null; // no path
        }

        private bool IsInsideGrid(int x, int y)
            => x >= 0 && y >= 0 && x < width && y < height;

        private List<(int x, int y)> ReconstructPath(Node node)
        {
            var path = new List<(int, int)>();
            while (node != null)
            {
                path.Add((node.X, node.Y));
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Returns the next tile position the fly should move to, or its current position if blocked.
        /// </summary>
        public Point GetNextPosition(Point flyPos, Point targetPos)
        {
            (int x, int y) start = (flyPos.X, flyPos.Y);
            (int x, int y) goal = (targetPos.X, targetPos.Y);

            if (goal == (-1, -1))
                return flyPos; // player not found

            var path = FindPath(start, goal);
            if (path == null || path.Count < 2)
                return flyPos; // no path or already on goal

            var next = path[1];
            return new Point(next.x, next.y);
        }
    }
}
