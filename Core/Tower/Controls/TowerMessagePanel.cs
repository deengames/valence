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
using DeenGames.Utils.Tower;
using DeenGames.Valence.Model;
using FlatRedBall.Graphics;
using FlatRedBall;
using FlatSilverBallTemplate.Screens;
using FlatRedBall.Input;
using DeenGames.Utils.Tower.Core;

namespace DeenGames.Utils.Tower.Controls
{
    public class TowerMessagePanel
    {
        private Text _text;
        private Sprite _background;
        public delegate void EventDelegate();
        public event EventDelegate Click;
        public event EventDelegate FadeOutComplete;
        private int _messageIndex = 0;
        private string[] _messages = null;
        private int _padding = 16;

        public TowerMessagePanel(Screen screen, string backgroundSprite, string[] texts)
            : this(screen, backgroundSprite, texts[0])
        {
            this._messages = texts;
            this._messageIndex = 0;
        }

        public TowerMessagePanel(Screen screen, string backgroundSprite, string text)
        {
            if (screen is TowerScreen) {
                // Automatically managed and removed
                TowerScreen t = screen as TowerScreen;
                this._background = t.AddSprite(backgroundSprite);
                this._text = t.AddText(string.Empty);
            } else {
                // User must manage them since we can't touch mTexts and mSprites.
                throw new ArgumentException(TowerUtils.USE_TOWER_FRAMEWORK_MESSAGE);
            }

            this._background.PixelSize = 0.5f;
            // +4 to offset so we don't TOUCH the border
            
            this._text.Scale = 18;
            this.DisplayText = text;

            this._background.CustomBehavior += new SpriteCustomBehavior(_background_CustomBehavior);

            // We need to be really high up.
            this._background.Z = 1;
            this._text.Z = this._background.Z + 1;
            
        }

        void _background_CustomBehavior(Sprite sprite)
        {
            if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton) &&
                InputManager.Mouse.IsOn3D(this._background, false))
            {
                // Clicked on us; if we have multiple texts, advance.
                // If not, hide.
                if (this.Click != null)
                {
                    this.Click.Invoke();
                }

                if (this._messages != null && this._messageIndex < this._messages.Length - 1)
                {
                    this._messageIndex++;
                    this.Show(this._messages[this._messageIndex]);
                }
                else
                {
                    this._messageIndex = 0;
                    this._messages = null;
                    this.Hide();
                }
            }

            if (this._background.Alpha <= 0 && this._text.Alpha <= 0 &&
                this._background.AlphaRate < 0 && this._text.AlphaRate < 0 &&
                this.FadeOutComplete != null)
            {
                this.FadeOutComplete.Invoke();
                this._background.AlphaRate = 0;
                this._text.AlphaRate = 0;
            }
        }

        public string DisplayText
        {
            set
            {
                this._text.DisplayText = value;
                this._text.InsertNewLines(this._background.Texture.Width - (2 * this._padding));
            }
        }
        
        public void Show(string text)
        {
            this.DisplayText = text;

            // fade in if we're hidden
            if (this._background.Alpha <= 0 && this._text.Alpha <= 0)
            {
                this._background.AlphaRate = 2;
                this._text.AlphaRate = this._background.AlphaRate;
            }
        }

        public void Hide()
        {
            // fade out
            this._background.AlphaRate = -2;
            this._text.AlphaRate = this._background.AlphaRate;
        }
    }
}
