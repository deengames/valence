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
    public class Tower3SliceButton : TowerBaseButton
    {
        private Sprite _leftSprite;
        private Sprite _centerSprite;
        private Sprite _rightSprite;

		private Sprite _leftMouseOverSprite;
		private Sprite _centerMouseOverSprite;
		private Sprite _rightMouseOverSprite;

		private const int SCALE_PADDING = 2; // Make text padded a bit.

        public Tower3SliceButton(Screen screen, string text, string imagePrefix, int fontSize = 12, int textOffset = 0, string mouseOverPrefix = null, string imageExtension="png") : base(screen, text)
        {
			if (string.IsNullOrEmpty(imagePrefix))
			{
				throw new ArgumentOutOfRangeException("Image prefix must be specified. eg. if it's bubble, we look for bubble-left.png, bubble-center.png, and bubble-right.png");
			}
            
            if (screen is TowerScreen)
            {
                TowerScreen ts = screen as TowerScreen;
				this.Text.BaseText.Scale = fontSize;

                this._centerSprite = ts.AddSprite("Content/Button/" + imagePrefix + "-center." + imageExtension);
                this._leftSprite = ts.AddSprite("Content/Button/" + imagePrefix + "-left." + imageExtension);
				this._rightSprite = ts.AddSprite("Content/Button/" + imagePrefix + "-right." + imageExtension);

                #region button images
                // Length = number of characters, scale = font-size (approximation of pixel size?)
                // Everything is relative to this guy, so set him first.
                this._centerSprite.ScaleX = this.Text.BaseText.DisplayText.Length  * (this.Text.BaseText.Scale / 2.5f);
                this._centerSprite.ScaleY = this.Text.BaseText.Scale + SCALE_PADDING;
				this._centerSprite.Y = textOffset;
                this._centerSprite.AttachTo(this.Text.BaseText, true);

				this._leftSprite.ScaleX = this.Text.BaseText.Scale;
				this._leftSprite.ScaleY = this.Text.BaseText.Scale + SCALE_PADDING;
				this._leftSprite.X -= this._centerSprite.ScaleX + (this._leftSprite.ScaleX / 2);
				this._leftSprite.Y = textOffset;
                this._leftSprite.AttachTo(this.Text.BaseText, true);

				this._rightSprite.ScaleX = this.Text.BaseText.Scale;
				this._rightSprite.ScaleY = this.Text.BaseText.Scale + SCALE_PADDING;
                this._rightSprite.X += this._centerSprite.ScaleX + (this._rightSprite.ScaleX / 2);
				this._rightSprite.Y = textOffset;
                this._rightSprite.AttachTo(this.Text.BaseText, true);
                #endregion

         
                #region on-click event handlers
                this._centerSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._leftSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
                this._rightSprite.CustomBehavior += new SpriteCustomBehavior(imageClicked_CustomBehavior);
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
                this._centerSprite.Visible = value;
                this._leftSprite.Visible = value;
                this._rightSprite.Visible = value;
            }
        }

		public float Height
		{
			get
			{
				return (2 * this._centerSprite.ScaleY);
			}
		}
	}
}
