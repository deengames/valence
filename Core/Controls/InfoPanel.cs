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
using DeenGames.Utils.Tower.Core;

namespace DeenGames.Valence.Controls
{
    public class InfoPanel
    {
        private static InfoPanel _instance = new InfoPanel();
        public static InfoPanel Instance { get { return _instance; } }

        private Text _atomNameText;
        private Text _atomValenceText;
        private Text _atomElectronegativityText;
        private Text _atomDescription;
        private Sprite _background;

        private InfoPanel() {}
        
        public void InitializeToScreen(TowerScreen screen) {
            _atomNameText = screen.AddText(string.Empty);
            _atomValenceText = screen.AddText(string.Empty);
            _atomElectronegativityText = screen.AddText(string.Empty);
            _atomDescription = screen.AddText(string.Empty);
            _background = screen.AddSprite("Content/message-panel.png");

            _atomNameText.Y = 70;
            _atomValenceText.Y = _atomNameText.Y;
            _atomElectronegativityText.Y = _atomNameText.Y;
            _atomDescription.Y = _atomNameText.Y - 20;

            _atomNameText.Scale = 18;
            _atomValenceText.Scale = 18;
            _atomElectronegativityText.Scale = 18;
            _atomDescription.Scale = 18;

            _atomNameText.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Left;
            _atomNameText.X = -175;

            _atomDescription.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Left;
            _atomDescription.X = _atomNameText.X;
            _atomDescription.VerticalAlignment = FlatRedBall.Graphics.VerticalAlignment.Top;

            _atomElectronegativityText.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Right;
            _atomElectronegativityText.X = (this._background.Texture.Width / 2) - 25;

            // Climb on top of everything
            this._background.Z = 1;
            this._atomDescription.Z = 2;
            this._atomElectronegativityText.Z = 2;
            this._atomNameText.Z = 2;
            this._atomValenceText.Z = 2;

            this.Hide();
        }

        public void Show(Atom atom)
        {
            this._atomDescription.AlphaRate = 0;
            this._atomElectronegativityText.AlphaRate = 0;
            this._atomNameText.AlphaRate = 0;
            this._atomValenceText.AlphaRate = 0;
            this._background.AlphaRate = 0;

            this._atomDescription.Alpha = 1;
            this._atomElectronegativityText.Alpha = 1;
            this._atomNameText.Alpha = 1;
            this._atomValenceText.Alpha = 1;
            this._background.Alpha = 1;

            this._atomDescription.DisplayText = atom.Description;
            this._atomDescription.InsertNewLines((this._background.ScaleX * 2) - 45); // -45 to avoid touching the border

            this._atomElectronegativityText.DisplayText = string.Format("E: {0}", atom.Electronegativity);
            this._atomNameText.DisplayText = atom.Name;
            this._atomValenceText.DisplayText = string.Format("Valence: {0}", atom.IonCharge);
        }

        public void Hide()
        {
            // fade out
            this._atomDescription.AlphaRate = -2;
            this._atomElectronegativityText.AlphaRate = this._atomDescription.AlphaRate;
            this._atomNameText.AlphaRate = this._atomDescription.AlphaRate;
            this._atomValenceText.AlphaRate = this._atomDescription.AlphaRate;
            this._background.AlphaRate = this._atomDescription.AlphaRate;
        }
    }
}
