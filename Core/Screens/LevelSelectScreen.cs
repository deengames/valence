using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeenGames.Utils.Tower;
using FlatRedBall.Graphics;
using FlatRedBall;
using DeenGames.Utils.Tower.Core;
using FlatRedBall.Math.Geometry;
using SilverArcade.SilverSprite.Graphics;
using DeenGames.Valence.Utils;
using DeenGames.Valence.Model;
using DeenGames.Utils.Tower.Controls;
using DeenGames.Valence.Tower.Controls;

namespace DeenGames.Valence.Screens
{
    public class LevelSelectScreen : TowerScreen
    {
        public override void Initialize()
        {
			this.FadeOutImmediately();

			this.AddSprite("Content/Menus/menu-screen.jpg");
			Sprite title = this.AddSprite("Content/Menus/select-level.png");

            const int VERTICAL_OFFSET = 160;
            const int HORIZONTAL_OFFSET_FROM_SCREEN = 30;
            const int VERTICAL_PADDING_BETWEEN_ROWS = 10;

			int SQUARE_WIDTH = 0;
			int SQUARE_HEIGHT = 0;

			// Start Y of levels, plus our height, plus 8-16 padding
			title.Y = VERTICAL_OFFSET + title.Texture.Height + 16;

            int currentLevel = CoreModel.Instance.MaxLevelReached;

            // 12 normal levels
			for (int i = 1; i <= CoreModel.NUMBER_OF_LEVELS; i++)
            {
                TowerSprite square;
                if (i <= currentLevel)
                {
                    square = new TowerSprite(this, "Content/Menus/normal-level.png");
                }
                else
                {
                    square = new TowerSprite(this, "Content/Menus/inactive-level.png");
                }

				SQUARE_WIDTH = square.Width;
				SQUARE_HEIGHT = square.Height;

                square.X = -300 + HORIZONTAL_OFFSET_FROM_SCREEN +
                    (((i - 1) % 3) * square.Width);
                
                square.Y = VERTICAL_OFFSET -
                    (((i - 1) / 3) * (VERTICAL_PADDING_BETWEEN_ROWS + square.Height) );

                square.Click += new TowerSprite.ClickedDelegate(levelSquare_Click);

                TowerText t = new TowerText(this.AddText(i.ToString()));
                t.X = square.X;

				if (i <= currentLevel)
				{
					t.Colour(0, 0, 0);
					t.Y = square.Y + 3;
				}
				else
				{
					t.Colour(255, 255, 255);
					t.Y = square.Y - 10;
				}

                t.Z = square.Z + 1;
                t.AttachTo(square, true);
            }

            currentLevel -= CoreModel.NUMBER_OF_LEVELS;

            // 12 puzzle levels
            for (int i = 1; i <= CoreModel.NUMBER_OF_LEVELS; i++)
            {
                TowerSprite square;
                if (i <= currentLevel)
                {
                    square = new TowerSprite(this, "Content/Menus/puzzle-level.png");
                }
                else
                {
                    square = new TowerSprite(this, "Content/Menus/inactive-level.png");
                }

                square.X = (square.Width / 2) +
                    (((i - 1) % 3) * square.Width);

                square.Y = VERTICAL_OFFSET -
                    (((i - 1) / 3) * (VERTICAL_PADDING_BETWEEN_ROWS + square.Height));
                
                square.Click += new TowerSprite.ClickedDelegate(levelSquare_Click);

                TowerText t = new TowerText(this.AddText(string.Format("P{0}", i)));
                t.X = square.X;

				if (i <= currentLevel)
				{
					t.Colour(0, 0, 0);
					t.Y = square.Y + 3;
				}
				else
				{
					t.Colour(255, 255, 255);
					t.Y = square.Y - 10;
				}

                t.Z = square.Z + 1;
				
                t.AttachTo(square, true);
            }

			Tower3SliceButton backButton = new Tower3SliceButton(this, "Back", "bubble", 18, -5);
			backButton.X = 340;//;HORIZONTAL_OFFSET_FROM_SCREEN + (1.25f * SQUARE_WIDTH);
			backButton.Y = -275; //;VERTICAL_OFFSET - (4 * SQUARE_HEIGHT) - (6 * VERTICAL_PADDING_BETWEEN_ROWS);
            backButton.Click += new TowerBaseButton.ClickedDelegate(backButton_Click);

            if (CoreModel.Instance.MaxLevelReached >= CoreModel.AVALANCHE_LEVEL)
            {
				TowerSprite avalanche = new TowerSprite(this, "Content/Menus/avalanche-level.png");
				avalanche.X = -325 + HORIZONTAL_OFFSET_FROM_SCREEN + (0.5f * SQUARE_WIDTH);
				avalanche.Y = VERTICAL_OFFSET - (4 * SQUARE_HEIGHT) - (6 * VERTICAL_PADDING_BETWEEN_ROWS);
                avalanche.Click += new TowerSprite.ClickedDelegate(avalanche_Click);
				
				TowerText avalancheText = new TowerText(this.AddText("Avalanche"));
				avalancheText.X = avalanche.X;
				avalancheText.Y = avalanche.Y + 5;
				avalancheText.Z = avalanche.Z + 1;
				avalancheText.Colour(0, 0, 0);
				avalancheText.AttachTo(avalanche, true);
            
                TowerSprite trickle = new TowerSprite(this, "Content/Menus/trickle-level.png");
				trickle.X = avalanche.X + SQUARE_WIDTH + 50;
                trickle.Y = avalanche.Y;
                trickle.Click += new TowerSprite.ClickedDelegate(trickle_Click);

				TowerText trickleText = new TowerText(this.AddText("Trickle"));
				trickleText.X = trickle.X;
				trickleText.Y = trickle.Y + 9;
				trickleText.Z = trickle.Z + 1;
				trickleText.AttachTo(trickle, true);
            }

			base.Initialize();

			this.FadeIn();
        }

		void trickle_Click(Sprite actualSpriteClicked)
		{
			CoreModel.Instance.CurrentLevel = CoreModel.TRICKLE_LEVEL;
			this.FadeOutComplete += new FadeEventDelegate(LevelSelectScreen_FadeOutComplete);
			this.FadeOut();
		}

		void avalanche_Click(Sprite actualSpriteClicked)
		{
			CoreModel.Instance.CurrentLevel = CoreModel.AVALANCHE_LEVEL;
			this.FadeOutComplete += new FadeEventDelegate(LevelSelectScreen_FadeOutComplete);
			this.FadeOut();
		}

        private void levelSquare_Click(Sprite actualSpriteClicked)
        {
            Text t = actualSpriteClicked.Children[0] as Text;
            string levelString = t.DisplayText;
            int level = 0;

            if (levelString.StartsWith("P"))
            {
                levelString = levelString.Substring(1);
                level = int.Parse(levelString);
                level += CoreModel.NUMBER_OF_LEVELS;
            }
            else
            {
                level = int.Parse(levelString);
            }

            if (level <= CoreModel.Instance.MaxLevelReached)
            {
                CoreModel.Instance.CurrentLevel = level;
                this.FadeOutComplete += new FadeEventDelegate(LevelSelectScreen_FadeOutComplete);
                this.FadeOut();
            }
        }

        void LevelSelectScreen_FadeOutComplete(TowerScreen.FadeOutMode mode)
        {
            MoveToScreen(typeof(CoreGameScreen));
        }

		void LevelSelectScreen_FadeOutComplete_MainMenu(TowerScreen.FadeOutMode mode)
        {
			MoveToScreen(typeof(MainMenuScreen));
        }

        private void backButton_Click()
        {
            this.FadeOutComplete += new FadeEventDelegate(LevelSelectScreen_FadeOutComplete_MainMenu);
			this.FadeOut();
        }
    }
}
