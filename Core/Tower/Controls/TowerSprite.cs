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
using FlatRedBall.Input;
using DeenGames.Utils.Tower.Core;
using FlatSilverBallTemplate.Screens;
using SilverArcade.SilverSprite.Graphics;

namespace DeenGames.Valence.Tower.Controls
{
    public class TowerSprite// : PositionedObject
    {
        public delegate void ClickedDelegate(Sprite actualSpriteClicked);
        public event ClickedDelegate Click;

		public delegate void MouseOverDelegate();
		public event MouseOverDelegate MouseOver;

		public delegate void OnMouseEnterDelegate();
		public event OnMouseEnterDelegate OnMouseEnter;

        private Sprite _sprite;

		private bool _mouseWasOverLastTick = false;

        public TowerSprite(Screen screen, string texture)
        {
            if (screen is TowerScreen)
            {
                // Automatically managed and removed
                TowerScreen t = screen as TowerScreen;
                this._sprite = t.AddSprite(texture);
                this._sprite.PixelSize = 0.5f;
                //this._sprite.AttachTo(this, true);
                this._sprite.CustomBehavior += new SpriteCustomBehavior(_sprite_CustomBehavior);
            }
            else
            {
                // User must manage them since we can't touch mTexts and mSprites.
                throw new ArgumentException(TowerUtils.USE_TOWER_FRAMEWORK_MESSAGE);
            }
        }

        internal Sprite InternalSprite { get { return this._sprite; } }

        void _sprite_CustomBehavior(Sprite sprite)
        {
            if (this.Click != null &&
                InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton) &&
                InputManager.Mouse.IsOn3D(this._sprite, false))
            {
                // Clicked on us
                this.Click.Invoke(sprite);
            }

			if (this.MouseOver != null &&
				InputManager.Mouse.IsOn3D(this._sprite, false))
			{
				this.MouseOver.Invoke();
			}

			if (this.OnMouseEnter != null)
			{
				if (InputManager.Mouse.IsOn3D(this._sprite, false) && !this._mouseWasOverLastTick)
				{
					this.OnMouseEnter.Invoke();
				}

				this._mouseWasOverLastTick = InputManager.Mouse.IsOn3D(this._sprite, false);
			}
        }

        #region "inherited" methods

        public float X
        {
            set { this._sprite.X = value; }
            get { return this._sprite.X; }
        }

        public float Y
        {
            set { this._sprite.Y = value; }
            get { return this._sprite.Y; }
        }

        public float Z
        {
            set { this._sprite.Z = value; }
            get { return this._sprite.Z; }
        }

        public float ScaleX
        {
            set { this._sprite.ScaleX = value; }
            get { return this._sprite.ScaleX; }
        }

        public float ScaleY
        {
            set { this._sprite.ScaleY = value; }
            get { return this._sprite.ScaleY; }
        }

        public int Width { get { return this._sprite.Texture.Width; } }
        public int Height { get { return this._sprite.Texture.Height; } }

		public float RightTextureCoordinate
		{
			get { return this._sprite.RightTextureCoordinate; }
			set { this._sprite.RightTextureCoordinate = value; }
		}

		public float BottomTextureCoordinate
		{
			get { return this._sprite.BottomTextureCoordinate; }
			set { this._sprite.BottomTextureCoordinate = value; }
		}

		public Texture2D Texture { get { return this._sprite.Texture; } }

		public void AttachTo(PositionedObject newParent, bool changeRelative)
		{
			this._sprite.AttachTo(newParent, changeRelative);
		}
        #endregion
    }
}
