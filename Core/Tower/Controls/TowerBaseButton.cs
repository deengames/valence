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
using FlatSilverBallTemplate.Screens;
using DeenGames.Utils.Tower;
using FlatRedBall;
using DeenGames.Utils.Tower.Core;
using FlatRedBall.Input;

namespace DeenGames.Utils.Tower.Controls
{
	/// <summary>
	/// A class that shares stuff common to 9-slice and
	/// 3-slice buttons both.
	/// </summary>
    public abstract class TowerBaseButton
    {
        public delegate void ClickedDelegate();
        public event ClickedDelegate Click;

        protected TowerText Text { get; set; }

        public TowerBaseButton(Screen screen, string text)
        {
            if (screen is TowerScreen)
            {
                TowerScreen ts = screen as TowerScreen;
                this.Text = new TowerText(ts.AddText(text));
            }
            else
            {
                // fail.
                throw new InvalidOperationException(TowerUtils.USE_TOWER_FRAMEWORK_MESSAGE);
            }
        }

        protected void imageClicked_CustomBehavior(Sprite sprite)
        {
            // if (clicked on this sprite, and there's a click-handler assigned)
            if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton) && InputManager.Mouse.IsOn3D(sprite, true) && this.Click != null)
            {
                this.Click.Invoke();
            }
        }

        public virtual float X { get { return this.Text.X; } set { this.Text.X = value; } }
		public virtual float Y { get { return this.Text.Y; } set { this.Text.Y = value; } }
		public virtual bool Visible
        {
            get
            {
                return this.Text.Visible;
            }
            set
            {
                this.Text.Visible = value;
            }
        }
	}
}
