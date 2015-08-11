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
using FlatRedBall;
using FlatRedBall.Input;
using FlatSilverBallTemplate.Screens;

namespace DeenGames.Valence.Screens
{
    public class StoryScreen : TowerScreen
    {
        int frameShowing = 1;
        private Sprite i1;

        public override void Initialize()
        {
            this.FadeOutImmediately();

            Sprite i2 = this.AddSprite("Content/Story/intro-2.jpg");
            i1 = this.AddSprite("Content/Story/intro-1.jpg");

            base.Initialize();

            this.FadeIn();
        }

        public override void Activity(bool firstTimeCalled)
        {
            if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton))
            {
                this.FadeOut();

                if (frameShowing == 1)
                {
                    this.FadeOutComplete += new FadeEventDelegate(StoryScreen_FadeOutStory);
                }
            }

            base.Activity(firstTimeCalled);
        }


        void StoryScreen_FadeOutStory(TowerScreen.FadeOutMode mode)
        {
            i1.Visible = false;
            this.FadeIn();
            this.FadeOutComplete -= StoryScreen_FadeOutStory;
            this.FadeOutComplete += StoryScreen_FadeOutToGame;
        }

        void StoryScreen_FadeOutToGame(TowerScreen.FadeOutMode mode)
        {
            MoveToScreen(typeof(CoreGameScreen));
        }
    }
}
