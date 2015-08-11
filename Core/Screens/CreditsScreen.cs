using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeenGames.Utils.Tower.Core;
using FlatRedBall;
using DeenGames.Utils;
using DeenGames.Utils.Tower.Controls;
using DeenGames.Valence.Tower.Controls;
using CSharpCity.Utils.Tools;
using System.Collections.Generic;
using DeenGames.Valence.Model;

namespace DeenGames.Valence.Screens
{
	public class CreditsScreen : TowerScreen
	{
		TowerText _infoText;

		Dictionary<TowerSprite, int> _imageToImageIndex = new Dictionary<TowerSprite, int>();

		string[] _captions = new string[] {
			"Title screen by Ramon Puasa Jr.",
			"Starfield by Bernhard Stockmann (http://www.gimpusers.com/tutorials/starfield-tutorial.html)",
			"Blueprint background 1 by Gregor909 (http://www.sxc.hu/browse.phtml?f=view&id=968699)",
			"Metal Floor 5 by sigma (http://www.cydonian.com/photos/img1632.htm)",
			"Metal Texture #8 by designm.ag (http://www.flickr.com/photos/designmag/3501608001/)",
			"Wall Distressed Section 01 by angelboiii (http://www.filterforge.com/filters/1384.html)",
			"Diamond Plate by Mike Blackney (http://www.filterforge.com/filters/879.html)",
			"Floor Metal Plate 02 by angelboiii (http://www.filterforge.com/filters/1357.html)",
			"Warning Stripes by MichaelTannock (http://www.filterforge.com/filters/2056.html)",
			"click 1 d by TicTacShutUp (http://www.freesound.org/samplesViewSingle.php?id=406)",
			"Zap by jnr hacksaw (http://www.freesound.org/samplesViewSingle.php?id=11221)",
			"cartoon yuppie shout by Robinhood76 (http://www.freesound.org/samplesViewSingle.php?id=95535)",
			"power_down by themfish (http://www.freesound.org/samplesViewSingle.php?id=34203)",
			"fx1 by Kekskiller (http://www.freesound.org/samplesViewSingle.php?id=87113)",
			"Till With Bell by Benboncan (http://www.freesound.org/samplesViewSingle.php?id=91924)"
		};

		string[] _soundFiles = new string[] {
			"atom-placed.mp3",
			"atoms-generated.mp3",
			"level-complete.mp3",
			"machine-powers-down.mp3",
			"reaction.mp3",
			"ulimited-mode-atoms-cleared.mp3"
		};

		public override void Initialize()
		{
			base.Initialize();
			this.FadeOutImmediately();

			this.AddSprite("Content/menus/menu-screen.jpg");
			Sprite title = this.AddSprite("Content/credits/credits.png");
			// Align with top of the center area, plus our height, plus 8-16 padding
			title.Y = 160 + title.Texture.Height + 16;

			const int SPACE_BETWEEN_IMAGES = 4;
			const int FULL_IMAGE_SIZE = 128;
			const int ACTUAL_IMAGE_SIZE = FULL_IMAGE_SIZE - 2; // 2 for border
			Point TOP_LEFT_OF_IMAGE_AREA = new Point(-325 + FULL_IMAGE_SIZE / 2, 200 - FULL_IMAGE_SIZE / 2);

			// Make a 5x3 grid of sprites
			// See procurement plan for first 13; last two are button/titlescreen
			string[] images = new string[] {
				"titlescreen.jpg",
				"story-1.jpg",
				"story-2.jpg",
				"metal-background.jpg",
				"metal-under-board.jpg",
				"eroded-metal.jpg",
				"extruded-metal.jpg",
				"metal-gate.jpg",
				"stripes.jpg",
				"speaker-icon.png",
				"speaker-icon.png",
				"speaker-icon.png",
				"speaker-icon.png",
				"speaker-icon.png",
				"speaker-icon.png"
			};

			int imageIndex = 0;
			foreach (string imageFileName in images)
			{
				TowerSprite image = new TowerSprite(this, "Content/Credits/" + imageFileName);
				// Shrink to 126x126
				image.RightTextureCoordinate = (ACTUAL_IMAGE_SIZE / image.Texture.Width);
				image.BottomTextureCoordinate = (ACTUAL_IMAGE_SIZE / image.Texture.Height);
				FlatRedBall.Math.Geometry.AxisAlignedRectangle border = this.AddAxisAlignedRectangle(ACTUAL_IMAGE_SIZE / 2, ACTUAL_IMAGE_SIZE / 2);
				image.AttachTo(border, true);

				// Can't move image, it's bound to border
				border.X = TOP_LEFT_OF_IMAGE_AREA.X + (imageIndex % 5 * ((SPACE_BETWEEN_IMAGES + ACTUAL_IMAGE_SIZE)));
				border.Y = TOP_LEFT_OF_IMAGE_AREA.Y - ((imageIndex / 5) * (SPACE_BETWEEN_IMAGES + ACTUAL_IMAGE_SIZE));

				this._imageToImageIndex[image] = imageIndex;

				image.OnMouseEnter += () => {
					int index = this._imageToImageIndex[image];
					this._infoText.DisplayText = this._captions[index];
					if (index >= 9)
					{
						// Sound file
						AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + this._soundFiles[index - 9]);
					}
				};
				
				imageIndex++;
			}

			Sprite infoWindow = this.AddSprite("Content/Credits/credits-info-window.png");
			infoWindow.Y = -219;

			_infoText = new TowerText(this.AddText("Some items used with the implicit permission of their authors. Hover over items for details."));
			_infoText.Y = infoWindow.Y;

			Tower3SliceButton backButton = new Tower3SliceButton(this, "Back", "bubble", 18, -5);
			backButton.X = 340;
			backButton.Y = -275;
			backButton.Click += () =>
			{
				this.FadeOutComplete += (fadeType) =>
				{
					MoveToScreen(typeof(MainMenuScreen));
				};
				this.FadeOut();
			};

			this.FadeIn();
		}

		public override void Activity(bool firstTimeCalled)
		{
			base.Activity(firstTimeCalled);
		}
	}
}
