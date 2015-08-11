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
using DeenGames.Valence.Model;

namespace DeenGames.Valence.Utils
{
    public class OrderTilesByElectronegativityWithAtomComparer : IComparer<Tile>
    {
        private Atom _centerAtom;

        public OrderTilesByElectronegativityWithAtomComparer(Atom centerAtom)
        {
            this._centerAtom = centerAtom;
        }

        public int Compare(Tile x, Tile y) {
            // Sort by electronegativity, descending
            return y.Atom.Electronegativity.CompareTo(x.Atom.Electronegativity);
        }
    }
}
