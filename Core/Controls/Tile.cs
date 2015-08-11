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
using FlatRedBall;
using System.Collections.Generic;
using FlatRedBall.Graphics;
using DeenGames.Valence.Model;

namespace DeenGames.Valence.Controls
{
    public class Tile
    {
        public Tile(int x, int y)
        {
            this._x = x;
            this._y = y;
        }

        private Atom _atom = Atom.NONE;
        private Text _atomText;
        private Sprite _atomSprite;

        // Our display object
        private Sprite _sprite;
        private int _x;
        private int _y;

        public Tile(Atom atom, Sprite atomSprite, Text atomText)
        {
            this._atom = atom;
            this._atomSprite = atomSprite;
            this._atomText = atomText;
        }

        public bool IsEmpty()
        {
            return this._atom == Atom.NONE;
        }

        public Atom Atom { get { return this._atom; } set { this._atom = value; } }
        public Sprite Sprite { get { return this._sprite; } set { this._sprite = value; } }
        public int X { get { return this._x; } set { this._x = value; } }
        public int Y { get { return this._y; } set { this._y = value; } }

        public Sprite AtomSprite
        {
            get { return this._atomSprite; }
            set { this._atomSprite = value; }
        }

        public Text AtomText
        {
            get { return this._atomText; }
            set { this._atomText = value; }
        }

        internal void Empty()
        {
            this._atom = Atom.NONE;
        }

        // for debugging
        public override string ToString()
        {
            return this.Atom.Element + " at " + this.X + ", " + this.Y;
        }
    }
}
