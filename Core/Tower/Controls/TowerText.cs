using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FlatRedBall.Graphics;
using FlatSilverBallTemplate.Screens;
using DeenGames.Utils.Tower.Core;
using DeenGames.Valence.Tower.Controls;

namespace DeenGames.Utils.Tower.Controls
{
    // Supposedly extends Text, but we favour composition over inheritence.
    public class TowerText
    {
        private Text _baseText;
        private Text _effectText = null;

        public TowerText(Text baseText)
        {
            this._baseText = baseText;
        }

        public void AddShadow()
        {
            AddShadow(2, 2);
        }

        public void AddShadow(int xOffset, int yOffset)
        {
            Screen screen = ScreenManager.CurrentScreen;
            this._effectText = this._baseText.Clone();

            if (screen is TowerScreen)
            {
                TowerScreen ts = screen as TowerScreen;
                ts.AddText(this._effectText);
            }
            else
            {
                // User must self-manage since we can't touch mTexts
                throw new ArgumentException(TowerUtils.USE_TOWER_FRAMEWORK_MESSAGE);
            }
            
            // Requirement: baseText Z of 1, so that shadowText Z is >= 0
            if (this._baseText.Z == 0) { this._baseText.Z++; }

            this._effectText.AttachTo(this._baseText, true);
            // Make sure this is negative so that positive Y offsets are downward.
            yOffset = -Math.Abs(yOffset);
            this._effectText.RelativeX = xOffset;
            this._effectText.RelativeY = yOffset;
            this._effectText.RelativeZ = -1;
            
            this._effectText.Red = 0;
            this._effectText.Blue = 0;
            this._effectText.Green = 0;
        }

        public void Colour(int red, int green, int blue)
        {
            if (red < 0 || red > 255)
            {
                throw new ArgumentOutOfRangeException("Red component (" + red + ") must be in the range [0 .. 255]");
            }
            else if (green < 0 || green > 255)
            {
                throw new ArgumentOutOfRangeException("Green component (" + green + ") must be in the range [0 .. 255]");
            }
            else if (blue < 0)
            {
                throw new ArgumentOutOfRangeException("Blue component (" + blue + ") must be in the range [0 .. 255]");
            }
            else
            {
                // Validation passed!
                this._baseText.Red = (red / 255f);
                this._baseText.Green = (green / 255f);
                this._baseText.Blue = (blue / 255f);
            }
        }

        public Text BaseText { get { return this._baseText; } }
        public Text EffectText { get { return this._effectText; } }

        #region inherited methods
        public string DisplayText
        {
            get { return this._baseText.DisplayText; }
            set
            {
                this._baseText.DisplayText = value;
                if (this._effectText != null)
                {
                    this._effectText.DisplayText = value;
                }
            }
        }
        public float AlphaRate
        {
            get { return this._baseText.AlphaRate; }
            set {
                this._baseText.AlphaRate = value;
                if (this._effectText != null)
                {
                    this._effectText.AlphaRate = value;
                }
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get { return this._baseText.HorizontalAlignment; }
            set
            {
                this._baseText.HorizontalAlignment = value;
                if (this._effectText != null)
                {
                    this._effectText.HorizontalAlignment = value;
                }
            }
        }

        public float Scale
        {
            get { return this._baseText.Scale; }
            set
            {
                this._baseText.Scale = value;
                if (this._effectText != null)
                {
                    this._effectText.Scale = value;
                }
            }
        }

        public float X
        {
            get { return this._baseText.X; }
            set { this._baseText.X = value; }
        }

        public float Y
        {
            get { return this._baseText.Y; }
            set { this._baseText.Y = value;}
        }

        public float Z
        {
            get { return this._baseText.Z; }
            set { this._baseText.Z = value; }
        }
        
        public float XVelocity
        {
            get { return this._baseText.XVelocity; }
            set { this._baseText.XVelocity = value; }
        }

        public float YVelocity
        {
            get { return this._baseText.YVelocity; }
            set { this._baseText.YVelocity = value; }
        }
        
        public void InsertNewLines(int maxWidth)
        {
            this._baseText.InsertNewLines(maxWidth);
            if (this._effectText != null)
            {
                this._effectText.InsertNewLines(maxWidth);
            }
        }
        #endregion

        public void AttachTo(TowerSprite sprite, bool changeRelative)
        {
            this.BaseText.AttachTo(sprite.InternalSprite, changeRelative);
        }

        public bool Visible
        {
            get
            {
                return this._baseText.Visible;
            }
            set
            {
                this._baseText.Visible = value;
                if (this._effectText != null)
                {
                    this._effectText.Visible = value;
                }
            }
        }
    }
}
