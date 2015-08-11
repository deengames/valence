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
using DeenGames.Valence.Controls;
using System.Collections.Generic;

namespace DeenGames.Valence.Utils
{
    public class OrderTilesByAtomNameComparer : IComparer<Tile>
    {
        public int Compare(Tile x, Tile y) {
            return x.Atom.Element.CompareTo(y.Atom.Element);
        }
    }
}
