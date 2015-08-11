using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using DeenGames.Utils.AStarPathFinder;
using DeenGames.Valence.Controls;

namespace DeenGames.Valence.Model
{
    public class Molecule
    {
        private IList<Tile> _atomTiles = new List<Tile>();
        public IList<Tile> AtomTiles { get { return this._atomTiles; } set { this._atomTiles = value; } }
        
        public int NetCharge {
            get
            {
                int net = 0;
                foreach (Tile t in this._atomTiles)
                {
                    Atom a = t.Atom;
                    net += a.IonCharge;
                }

                return net;
            }
        }

        public int Electronegativity
        {
            get
            {
                int net = 0;
                foreach (Tile t in this._atomTiles)
                {
                    Atom a = t.Atom;
                    net += a.Electronegativity;
                }

                return net;
            }
        }

        public byte[,] ByteGrid
        {
            get
            {
                int maxX = 0;
                int maxY = 0;
                int minX = int.MaxValue;
                int minY = int.MaxValue;

                foreach (Tile t in this.AtomTiles)
                {
                    if (t.X > maxX) { maxX = t.X; }
                    if (t.Y > maxY) { maxY = t.Y; }
                    if (t.X < minX) { minX = t.X; }
                    if (t.Y < minY) { minY = t.Y; }
                }

                int width = maxX - minX + 1;
                int height = maxY - minY + 1;

                // surround with a border of walls
                //width += 2;
                //height += 2;

                // Make grid a power of two; burn you, pointers!!
                int max = Math.Max(CoreModel.BOARD_WIDTH, CoreModel.BOARD_HEIGHT);
                max = PathFinderHelper.RoundToNearestPowerOfTwo(max);

                height = max;
                width = max;

                byte[,] grid = new byte[width, height];
                
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        grid[i, j] = PathFinderHelper.BLOCKED_TILE;
                    }
                }

                foreach (Tile t in this.AtomTiles)
                {
                    grid[t.X, t.Y] = PathFinderHelper.EMPTY_TILE;
                }

                return grid;
            }
        }

        // makes debugging easier
        public override string ToString()
        {
            string toReturn = "";
            foreach (Tile t in this._atomTiles)
            {
                toReturn += t.Atom.Element;
            }
            return toReturn;
        }
    }
}
