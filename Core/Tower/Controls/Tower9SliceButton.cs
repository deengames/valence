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
    public class Tower9SliceButton : TowerBaseButton
    {
        private Sprite _topLeftSprite;
        private Sprite _topCenterSprite;
        private Sprite _topRightSprite;

        private Sprite _middleLeftSprite;
        private Sprite _middleCenterSprite;
        private Sprite _middleRightSprite;

        private Sprite _bottomLeftSprite;
        private Sprite _bottomCenterSprite;
        private Sprite _bottomRightSprite;

        public Tower9SliceButton(Screen screen, string text) : base(screen, text)
        {
            //Screen screen = ScreenManager.CurrentScreen;
            if (screen is TowerScreen)
            {
                TowerScreen ts = screen as TowerScreen;
                this._bottomCenterSprite = ts.AddSprite("Content/Button/bottom-center.png");
                this._bottomLeftSprite = ts.AddSprite("Content/Button/bottom-left.png");
                this._bottomRightSprite = ts.AddSprite("Content/Button/bottom-right.png");
                this._middleCenterSprite = ts.AddSprite("Content/Button/middle-center.png");
                this._middleLeftSprite = ts.AddSprite("Content/Button/middle-left.png");
                this._middleRightSprite = ts.AddSprite("Content/Button/middle-right.png");
                this._topCenterSprite = ts.AddSprite("Content/Button/top-center.png");
                this._topLeftSprite = ts.AddSprite("Content/Button/top-left.png");
                this._topRightSprite = ts.AddSprite("Content/Button/top-right.png");

                // Validate LHS sprites are the same width
                if (this._topLeftSprite.Texture.Width != this._middleLeftSprite.Texture.Width ||
                    this._middleLeftSprite.Texture.Width != this._bottomLeftSprite.Texture.Width)
                {
                    throw new ArgumentException(string.Format("Top-left, middle-left, and bottom-left images need to be the same width; currently, they're {0}px, {1}px, and {2}px respectively.", this._topLeftSprite.Texture.Width, this._middleLeftSprite.Texture.Width, this._bottomLeftSprite.Texture.Width));
                }

                // Validate RHS sprites are the same width
                if (this._topRightSprite.Texture.Width != this._middleRightSprite.Texture.Width ||
                    this._middleRightSprite.Texture.Width != this._bottomRightSprite.Texture.Width)
                {
                    throw new ArgumentException(string.Format("Top-right, middle-right, and bottom-right images need to be the same width; currently, they're {0}px, {1}px, and {2}px respectively.", this._topRightSprite.Texture.Width, this._middleRightSprite.Texture.Width, this._bottomRightSprite.Texture.Width));
                }

                #region button images
                // Length = number of characters, scale = font-size (approximation of pixel size?)
                // Everything is relative to this guy, so set him first.
                this._middleCenterSprite.ScaleX = this.Text.BaseText.DisplayText.Length * (this.Text.BaseText.Scale / 2);
                this._middleCenterSprite.ScaleY = this.Text.BaseText.Scale;
                this._middleCenterSprite.AttachTo(this.Text.BaseText, true);

                #region top images
                this._topLeftSprite.X -= this._middleCenterSprite.ScaleX + (this._topLeftSprite.Texture.Width / 2);
                this._topLeftSprite.Y += this._middleCenterSprite.Texture.Height + (this._topLeftSprite.Texture.Height * 1.5f);
                this._topLeftSprite.AttachTo(this.Text.BaseText, true);

                this._topCenterSprite.ScaleX = this._middleCenterSprite.ScaleX;
                this._topCenterSprite.X = this._topLeftSprite.X + (this._topLeftSprite.Texture.Width / 2) + this._topCenterSprite.ScaleX;
                this._topCenterSprite.Y = this._topLeftSprite.Y;
                this._topCenterSprite.AttachTo(this._topLeftSprite, true);

                this._topRightSprite.X += this._middleCenterSprite.ScaleX + (this._topRightSprite.Texture.Width / 2);
                this._topRightSprite.Y += this._middleCenterSprite.Texture.Height + (this._topRightSprite.Texture.Height * 1.5f);
                this._topRightSprite.AttachTo(this.Text.BaseText, true);
                #endregion

                #region bottom images
                this._bottomLeftSprite.X -= this._middleCenterSprite.ScaleX + (this._bottomLeftSprite.Texture.Width / 2);
                this._bottomLeftSprite.Y -= this._middleCenterSprite.Texture.Height + (this._bottomLeftSprite.Texture.Height * 1.5f);
                this._bottomLeftSprite.AttachTo(this.Text.BaseText, true);

                this._bottomCenterSprite.ScaleX = this._middleCenterSprite.ScaleX;
                this._bottomCenterSprite.X = this._bottomLeftSprite.X + (this._bottomLeftSprite.Texture.Width / 2) + this._bottomCenterSprite.ScaleX;
                this._bottomCenterSprite.Y = this._bottomLeftSprite.Y;
                this._bottomCenterSprite.AttachTo(this._bottomLeftSprite, true);

                this._bottomRightSprite.X += this._middleCenterSprite.ScaleX + (this._bottomRightSprite.Texture.Width / 2);
                this._bottomRightSprite.Y -= this._middleCenterSprite.Texture.Height + (this._bottomRightSprite.Texture.Height * 1.5f);
                this._bottomRightSprite.AttachTo(this.Text.BaseText, true);
                #endregion

                #region middle images
                this._middleLeftSprite.X = this._topLeftSprite.X;
                this._middleLeftSprite.ScaleY = this._middleCenterSprite.ScaleY;
                this._middleLeftSprite.AttachTo(this._topLeftSprite, true);

                this._middleRightSprite.X = this._topRightSprite.X;
                this._middleRightSprite.ScaleY = this._middleCenterSprite.ScaleY;
                this._middleRightSprite.AttachTo(this._topRightSprite, true);
                #endregion

                #region on-click event handlers
                this._bottomCenterSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._bottomLeftSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._bottomRightSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._middleCenterSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._middleLeftSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._middleRightSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._topCenterSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._topLeftSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._topRightSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                #endregion

                #endregion
            }
            else
            {
                // fail.
                throw new InvalidOperationException(TowerUtils.USE_TOWER_FRAMEWORK_MESSAGE);
            }
        }

		public override float X { get { return this.Text.X; } set { this.Text.X = value; } }
		public override float Y { get { return this.Text.Y; } set { this.Text.Y = value; } }
        public override bool Visible
        {
            get
            {
                return this.Text.Visible;
            }
            set
            {
				base.Visible = value;
                this._bottomCenterSprite.Visible = value;
                this._bottomLeftSprite.Visible = value;
                this._bottomRightSprite.Visible = value;
                this._middleCenterSprite.Visible = value;
                this._middleLeftSprite.Visible = value;
                this._middleRightSprite.Visible = value;
                this._topCenterSprite.Visible = value;
                this._topLeftSprite.Visible = value;
                this._topRightSprite.Visible = value;
            }
        }

		public float Height
		{
			get
			{
				return (2 * this._topCenterSprite.ScaleY) +
					(2 * this._middleCenterSprite.ScaleY) +
					(2 * this._bottomCenterSprite.ScaleY);
			}
		}
	}
}
