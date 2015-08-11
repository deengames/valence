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
using FlatRedBall.Graphics;
using System.Text;

namespace DeenGames.Utils.Tower.Core
{
    /// <summary>
    /// A generic utility class, with odds and ends in it.
    /// </summary>
    public static class TowerUtils
    {
        internal const string USE_TOWER_FRAMEWORK_MESSAGE = "This method doesn't work with regular Screens; please use TowerScreen.";

        /// <summary>
        /// Colour some text. This uses intuitive [0..255] values,
        /// rather than whacked-out [0..1] values.
        /// Throws an exception if RGB is out of [0..255].
        /// </summary>
        /// <param name="text"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>

        public static double DistanceBetweenPoints(int x1, int y1, int x2, int y2)
        {
            // sqrt[(x2-x1)^2 + (y2-y1)^2]
            double x2MinusX1Squared = Math.Pow(x2 - x1, 2);
            double y2MinusY1Squared = Math.Pow(y2 - y1, 2);
            return Math.Sqrt(x2MinusX1Squared + y2MinusY1Squared);
        }
    }
}
