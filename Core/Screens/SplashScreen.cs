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
using DeenGames.Utils.Tower.Core;
using System.Windows.Threading;
using FlatSilverBallTemplate.Screens;
using FlatRedBall;
using CSharpCity.Utils.Tools;
using DeenGames.Valence.Model;
using DeenGames.Valence.Tower.Controls;

namespace DeenGames.Valence.Screens
{
    public class SplashScreen : TowerScreen
    {
        DispatcherTimer _splashTimer = new DispatcherTimer();

        public override void Initialize()
        {
            this.FadeOutImmediately();

            Sprite background = this.AddSprite("Content/1x1.png");
            background.ScaleX = 400;
            background.ScaleY = 300;

            TowerSprite logo = new TowerSprite(this, "Content/deengames-logo.jpg");
			logo.Click += (sprite) => {
				System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://www.deengames.com"), "_newWindow");
			};

			EmbeddedResourceHelper.ProjectNamespace = "DeenGames.Valence";
			// Sneaky way of playing this with asynchronous loading. MAN this is tough!
			//AudioManager.Instance.LoadSound(CoreModel.SOUND_FILE_PATH + "takbeer-of-athan.mp3", true);
			AudioManager.Instance.LoadSounds(CoreModel.SOUND_FILE_PATH);

			this._splashTimer.Interval = TimeSpan.FromSeconds(4);

            this._splashTimer.Tick += new EventHandler(splashTimer_Tick);
            this.FadeOutComplete += new FadeEventDelegate(SplashScreen_FadeOutComplete);
			this.FadeInComplete += new FadeEventDelegate(SplashScreen_FadeInComplete);
            
            base.Initialize();

            this.FadeIn();
        }

		void SplashScreen_FadeInComplete(TowerScreen.FadeOutMode mode)
		{
			this._splashTimer.Start();
		}

        void splashTimer_Tick(object sender, EventArgs e)
        {
            this._splashTimer.Stop();
            this.FadeOut();
        }

        void SplashScreen_FadeOutComplete(TowerScreen.FadeOutMode mode)
        {
            MoveToScreen(typeof(MainMenuScreen));
        }
    }
}
