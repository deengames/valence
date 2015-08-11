using FlatRedBall.Graphics;
using FlatRedBall;
using SilverArcade.SilverSprite;
using DeenGames.Utils.Tower;
using FlatRedBall.Input;
using FlatSilverBallTemplate.Screens;
using FlatRedBall.Gui;
using DeenGames.Valence.Controls;
using DeenGames.Utils.Tower.Core;
using DeenGames.Utils.Tower.Controls;
using DeenGames.Valence.Model;
using DeenGames.Valence.Utils;
using CSharpCity.Utils.Tools;
using System;

namespace DeenGames.Valence.Screens
{
    public class MainMenuScreen : TowerScreen
    {
        public override void Initialize()
        {
            this.FadeOutImmediately();
			
			this.AddSprite("Content/titlescreen.jpg");

            // Force initialization of model so we can get level data
            Model.CoreModel m = Model.CoreModel.Instance;

            Tower3SliceButton newGame = new Tower3SliceButton(this, "New Game", "bubble", 18, -5);
			newGame.Y = 70;
            newGame.Click += new Tower3SliceButton.ClickedDelegate(newGame_Click);
			
            Tower3SliceButton relativeTo = newGame;
            if (Model.CoreModel.Instance.CurrentLevel > 1)
            {
                Tower3SliceButton selectLevel = new Tower3SliceButton(this, "Select Level", "bubble", 18, -5);
                selectLevel.X = newGame.X;
                selectLevel.Y = newGame.Y - (1.3f * newGame.Height);
                selectLevel.Click += new Tower3SliceButton.ClickedDelegate(continueGame_Click);
                relativeTo = selectLevel;
            }

			Tower3SliceButton credits = new Tower3SliceButton(this, "Credits", "bubble", 18, -5);
            credits.X = relativeTo.X;
            credits.Y = relativeTo.Y - (1.3f * relativeTo.Height);
			credits.Click += new TowerBaseButton.ClickedDelegate(credits_Click);

			Tower3SliceButton moreGames = new Tower3SliceButton(this, "More Games", "bubble", 18, -5);
			moreGames.X = credits.X;
			moreGames.Y = credits.Y - (1.3f * credits.Height);
			moreGames.Click += () =>
			{
				System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://www.deengames.com"), "_newWindow");
			};
			
            // Reset points here for Avalanche/Trickle modes.
            CoreModel.Instance.Points = 0;

            this.FadeIn();            
            
            base.Initialize();
        }

		void credits_Click()
		{
			this.FadeOutComplete += new FadeEventDelegate(MainMenuScreen_FadeOutComplete_CreditsSelect);
			this.FadeOut();
		}

        void continueGame_Click()
        {
            this.FadeOutComplete += new FadeEventDelegate(MainMenuScreen_FadeOutComplete_LevelSelect);
            this.FadeOut();
        }

		void MainMenuScreen_FadeOutComplete_CreditsSelect(TowerScreen.FadeOutMode mode)
		{
			MoveToScreen(typeof(CreditsScreen));
		}

        void MainMenuScreen_FadeOutComplete_LevelSelect(TowerScreen.FadeOutMode mode)
        {
            MoveToScreen(typeof(LevelSelectScreen));
        }

        void MainMenuScreen_FadeOutComplete_NewGame(TowerScreen.FadeOutMode mode)
        {
            if (Model.CoreModel.Instance.CurrentLevel == 1)
            {
                MoveToScreen(typeof(StoryScreen));
            }
            else
            {
                MoveToScreen(typeof(CoreGameScreen));
            }
        }

        void newGame_Click()
        {
            Model.CoreModel.Instance.StartAtFirstLevel();
            this.FadeOutComplete += new FadeEventDelegate(MainMenuScreen_FadeOutComplete_NewGame);
            this.FadeOut();
        }
    }
}
