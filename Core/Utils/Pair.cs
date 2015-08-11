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

namespace DeenGames.Valence.Utils
{
    public class Pair<T>
    {
        private T _firstItem;
        private T _secondItem;

        public Pair(T first, T second)
        {
            this._firstItem = first;
            this._secondItem = second;
        }

        public T First { get { return _firstItem; } }
        public T Second { get { return _secondItem; } }
    }
}
