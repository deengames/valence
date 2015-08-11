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
using FlatRedBall.Graphics;
using FlatRedBall;
using System.Collections.Generic;
using DeenGames.Valence.Model;
using FlatRedBall.Input;
using FlatRedBall.Graphics.Animation;
using SilverArcade.SilverSprite.Graphics;
using System.Linq;
using System.Text;
using DeenGames.Valence.Controls;
using DeenGames.Utils.Tower.Core;
using DeenGames.Utils.Tower.Controls;
using DeenGames.Valence.Utils;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using CSharpCity.Utils.Tools;

namespace DeenGames.Valence.Screens
{
    public class CoreGameScreen : TowerScreen
    {
        private Model.CoreModel model = Model.CoreModel.Instance;
        
        private Sprite _nextAtom;
        private TowerText _nextAtomText = null;
        private IList<Tile> _reactingTiles = new List<Tile>();
        private DateTime _startHoverTime = DateTime.MinValue;
        private IList<Text> _reactionText = new List<Text>();
        private InfoPanel infoPanel = InfoPanel.Instance;
        private TowerMessagePanel _tutorialPanel;
        private bool _tutorialPanelShowing = false;
        private TowerText _levelText;
        private OrderTilesByAtomNameComparer _orderTilesByAtomNameComparer = new OrderTilesByAtomNameComparer();
        private IList<Tile> _fadingInTiles = new List<Tile>();
        private Dictionary<int, string[]> _tutorialMessages = new Dictionary<int,string[]>();

        private string[] _greatWorkMessages = new string[] {
            "Awesome!", "Great Work!", "Excellent!", "Outstanding!", "Nice!"
        };

        private TowerText _atomOrPointCountText;
        private TowerText _timeText;
		private Sprite _gameComplete;

		#region constants
        // This allows us to create v-shaped molecules, but not long lines.
        // You can make /-shaped Be-Cl-Cl, too...
        const int CENTER_OF_BOARD_X = 225;
        const int CENTER_OF_BOARD_Y = 250;
        #endregion

        public override void Initialize()
        {
            model.SetGameStateToNormal();

            this.FadeOutImmediately();
            this.initializeLevelTutorials();

            this.removeOldSprites();
            model.CreateBoard();
            this._levelText = new TowerText(this.AddText(string.Format("Puzzle: {0}", model.CurrentLevel)));

            performPerLevelPreSetup();

            Sprite background = this.AddSprite("Content/metal-background.jpg");

            Sprite statusBar = this.AddSprite("Content/status-bar.png");
            statusBar.Y = -275; // bottom of the screen = 300, half us = 25

            Sprite board = this.AddSprite("Content/metal-board.jpg");
            board.Y = 25;

            Sprite frieze = this.AddSprite("Content/frieze.png");
            frieze.Y = 25;

            #region status-bar
            this._levelText.X = -375;
            this._levelText.Y = statusBar.Y;
            this._levelText.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Left;
            this._levelText.Colour(0, 0, 0);
            this._levelText.Scale = 24;

            if (model.CurrentLevel >= CoreModel.FIRST_PUZZLE_LEVEL && model.CurrentLevel <= CoreModel.LAST_PUZZLE_LEVEL)
            {
                // Puzzle mode; add atom counter
                this._atomOrPointCountText = new TowerText(this.AddText(string.Format("Atoms Left: {0}", model.Toolbox.NumAtomsLeft)));
                this._atomOrPointCountText.Scale = this._levelText.Scale;
                this._atomOrPointCountText.Y = this._levelText.Y;
                this._atomOrPointCountText.X = this._levelText.X + 350;
                this._atomOrPointCountText.Colour(0, 0, 0);
            }
            else if (model.CurrentLevel == CoreModel.AVALANCHE_LEVEL || model.CurrentLevel == CoreModel.TRICKLE_LEVEL)
            {
                this._atomOrPointCountText = new TowerText(this.AddText(string.Format("Points: {0}", model.Points)));
                this._atomOrPointCountText.Scale = this._levelText.Scale;
                this._atomOrPointCountText.Y = this._levelText.Y;
                this._atomOrPointCountText.X = this._levelText.X + 300;
                this._atomOrPointCountText.Colour(0, 0, 0);
            }

            TowerText nextText = new TowerText(this.AddText("Next: "));
            nextText.X = 300;
            nextText.Y = statusBar.Y;
            nextText.Colour(0, 0, 0);
            nextText.Scale = this._levelText.Scale;
            
            this._nextAtom = this.AddSprite("Content/Atoms/" + model.NextAtom.Element + ".png");
            _nextAtom.X = nextText.X + 50;
            _nextAtom.Y = nextText.Y;

            this._nextAtomText = new TowerText(this.AddText(model.NextAtom.Element));
            _nextAtomText.X = _nextAtom.X;
            _nextAtomText.Y = _nextAtom.Y;

            this._timeText = new TowerText(this.AddText("Time: ∞"));
            this._timeText.X = this._nextAtomText.X - 200;
            this._timeText.Y = statusBar.Y;
            this._timeText.Colour(0, 0, 0);
            this._timeText.Scale = this._levelText.Scale;
            #endregion

            for (int x = 0; x < Model.CoreModel.BOARD_WIDTH; x++)
            {
                for (int y = 0; y < Model.CoreModel.BOARD_HEIGHT; y++)
                {
                    Sprite tile = this.AddSprite("Content/tile.png");
                    tile.X = (x * 50) - CENTER_OF_BOARD_X;
                    tile.Y = (y * -50) + CENTER_OF_BOARD_Y;
                    tile.Alpha = 0.75f;
                    model.Board[x, y].Sprite = tile;
                }
            }

            infoPanel.InitializeToScreen(this);

            // Clear out old event handlers because they carry over
            model.ClearEventHandlers();
            model.MachineCreatesAtoms += new DeenGames.Valence.Model.CoreModel.MachineCreatesAtomsDelegate(model_MachineCreatesAtoms);
            model.MachineTicks += new DeenGames.Valence.Model.CoreModel.MachineTicksDelegate(model_MachineTicks);
            model.GameOver += new DeenGames.Valence.Model.CoreModel.GameOverOccuredDelegate(model_GameOver);
            
            
            // Avalanche and Trickle mode NEVER end. Evar.
            if (model.CurrentLevel != CoreModel.AVALANCHE_LEVEL && model.CurrentLevel != CoreModel.TRICKLE_LEVEL)
            {
                this.FadeOutComplete += new FadeEventDelegate(CoreGameScreen_LevelCompleteBlackOutComplete);
            }

            performPerLevelPostSetup();
			this._levelText.DisplayText = model.LevelName;
            this.updateAtomsLeftOrPointsCounter();

            this.FadeIn();
            base.Initialize();
        }

        private void updateAtomsLeftOrPointsCounter() {
            if (this._atomOrPointCountText != null)
            {
                if (model.CurrentLevel == CoreModel.AVALANCHE_LEVEL || model.CurrentLevel == CoreModel.TRICKLE_LEVEL)
                {
                    this._atomOrPointCountText.DisplayText = string.Format("Points: {0}", model.Points);
                }
                else
                {
                    // Puzzle mode
                    this._atomOrPointCountText.DisplayText = string.Format("Atoms Left: {0}", model.Toolbox.NumAtomsLeft);
                }
            }
        }

        void reactionComputingThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
				// Don't let multiple threads eat atoms. Nor let two check for two reactions at
				// the same time with an overlapping atom.
				lock (CoreModel.Instance)
				{
					IList<Tile> tilesInReaction = e.Result as IList<Tile>;
					// Final check if someone ate our atoms in-between and unbalanced us.
					if (tilesInReaction != null && tilesInReaction.Count > 0 && tilesInReaction.Sum(t => t.Atom.IonCharge) == 0)
					{
						// Add points. Not displayed outside of Avalanche and Trickle, oh well.
						model.Points += tilesInReaction.Sum(t => t.Atom.Points);
						this.updateAtomsLeftOrPointsCounter();

						StringBuilder equation = new StringBuilder();

						tilesInReaction = tilesInReaction.OrderBy(tile => tile, this._orderTilesByAtomNameComparer).ToList();

						String previousElement = tilesInReaction[0].Atom.Element;
						String currentElement = "";

						int quantity = 0;
						Tile finalTile = tilesInReaction[tilesInReaction.Count - 1];

						// Equations only on non-Avalanche and non-Trickle mode.

						foreach (Tile reactionTile in tilesInReaction)
						{
							// For some reason, these appear at high levels in large chains.
							// HACK: don't display it.
							if (reactionTile.IsEmpty())
							{
								continue;
							}

							reactionTile.AtomSprite.ColorOperation = ColorOperation.Add;
							reactionTile.AtomSprite.RedRate = 3f;
							reactionTile.AtomSprite.GreenRate = 3f;
							reactionTile.AtomSprite.BlueRate = 3f;
							this._reactingTiles.Add(reactionTile);

							// Print out something nice, like 2Fl + H + Li.
							// Or, in Avalanche/Trickle, 1 + 16 + 9.
							if (model.CurrentLevel != CoreModel.AVALANCHE_LEVEL && model.CurrentLevel != CoreModel.TRICKLE_LEVEL)
							{
								currentElement = reactionTile.Atom.Element;
								if (previousElement == currentElement)
								{
									quantity++;
								}
								else if (previousElement != currentElement)
								{
									// Print nothing if quantity  = 1, i.e. we like to
									// see "H + Cl" not "1H + 1Cl"
									equation.Append(string.Format("{0}{1} + ", (quantity > 1 ? quantity.ToString() : ""), previousElement));
									// 1, not 0, because we won't do this elsewhere; fixes bug where
									// Fl-Fl-H-H becomes 2Fl + H.
									quantity = 1;
								}

								previousElement = reactionTile.Atom.Element;
							}
							else
							{
								equation.Append(string.Format("{0} + ", reactionTile.Atom.Points));
							}

							reactionTile.Empty(); // Prevent quick clickers from chaining
						}

						// Dump of last element
						if (model.CurrentLevel != CoreModel.AVALANCHE_LEVEL && model.CurrentLevel != CoreModel.TRICKLE_LEVEL)
						{
							equation.Append(string.Format("{0}{1} + ", (quantity > 1 ? quantity.ToString() : ""), previousElement));
						}

						if (equation.Length >= 3)
						{
							// Trickle mode bug?
							equation.Remove(equation.Length - 3, 3); // trailing " + "
						}
						TowerText equationText = new TowerText(this.AddText(equation.ToString()));
						equationText.YVelocity = 15;
						equationText.AlphaRate = -0.1f;
						equationText.Scale = 24;
						equationText.AddShadow();
						equationText.InsertNewLines(SpriteManager.Camera.Width);

						AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + "reaction.mp3");

						this._reactionText.Add(equationText.BaseText);
						this._reactionText.Add(equationText.EffectText);
					}

					if (model.CurrentLevel >= CoreModel.FIRST_PUZZLE_LEVEL &&
						model.CurrentLevel <= CoreModel.LAST_PUZZLE_LEVEL &&
						model.NextAtom == Atom.NONE &&
						!model.IsLevelOver())
					{
						// Puzzle mode, out of atoms; game over.
						model.SignalGameOver();
					}
				}
            }
        }

        void reactionComputingThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
                return;
            }

            Tile clickedTile = e.Argument as Tile;
			if (clickedTile.IsEmpty())
			{
				// Tile was emptied. Atom is gone. Sigh.
				e.Result = new List<Tile>();
			}
			else
			{
				IList<Tile> tilesInReaction = model.GetTilesInReaction(clickedTile);
				e.Result = tilesInReaction;
			}
            
        }

        private void removeOldSprites()
        {
            // If there was game over, then we New Game again, then
            // some sprites are left in the model. Yech.
            for (int i = 0; i < Model.CoreModel.BOARD_WIDTH; i++)
            {
                for (int j = 0; j < Model.CoreModel.BOARD_HEIGHT; j++)
                {
                    Tile currentTile = model.Board[i, j];
                    if (currentTile != null)
                    {
                        currentTile.Atom = Atom.NONE;

                        if (currentTile.AtomSprite != null)
                        {
                            this.RemoveResource(currentTile.AtomSprite);
                            currentTile.AtomSprite = null;
                        }

                        if (currentTile.AtomText != null)
                        {
                            this.RemoveResource(currentTile.AtomText);
                            currentTile.AtomText = null;
                        }
                    }
                }
            }
        }

        void model_GameOver()
        {
            if (model.CurrentLevel != CoreModel.AVALANCHE_LEVEL && model.CurrentLevel != CoreModel.TRICKLE_LEVEL)
            {
                this.FadeOutComplete -= CoreGameScreen_LevelCompleteBlackOutComplete;
            }
            this.FadeInComplete += new FadeEventDelegate(CoreGameScreen_ReturnToMainMenuFadeInComplete);

            this.FadeOutHalf();
			TowerText t = new TowerText(this.AddText("Game Over!"));
            t.AddShadow();
            t.Scale = 72;

            // Go on TOP of the shadow.
            t.Z = this.GetTopZValueAndMoveFadeToTop() + 1;

			string returnToMainMenuText = "";
			if (model.CurrentLevel == CoreModel.AVALANCHE_LEVEL || model.CurrentLevel == CoreModel.TRICKLE_LEVEL)
			{
				returnToMainMenuText += "(You scored " + model.Points + " points.) ";
				if (model.Points >= 25000)
				{
                    model.FeatManager.GrantFeat(@"z00KSNDLsTvhZ1oK-8WjmdcEWRE2fF6zKCiARZp_1laXXOXd_Q1IQq_STqr4VwAe_TzKaivRDOhQdyoxiwo_HPtOOuIg9gdW79F6wy6CWjDg9tE-o7ntFhVXw_K-MgU6");
				}
			}
			returnToMainMenuText += "Click to return to the main menu.";

            TowerText t2 = new TowerText(this.AddText(returnToMainMenuText));
            t2.AddShadow();
            t2.Y = t2.Y - (t.Scale / 2) - 8; // Below the above text. Pad 8px.
            t2.Scale = 24;
            t2.Z = t.Z;

            this.ManageForGarbageCollection(t);
            this.ManageForGarbageCollection(t2);
        }

        void CoreGameScreen_ReturnToMainMenuFadeInComplete(TowerScreen.FadeOutMode mode)
        {
            MoveToScreen(typeof(MainMenuScreen));
        }

        private bool isTutorialForCurrentLevel()
        {
            return this._tutorialMessages.Keys.Contains(model.CurrentLevel);
        }

        private void initializeLevelTutorials()
        {
            // Add levels; key = level number, value = tutorial messages
            this._tutorialMessages[1] = new string[] {
                "Welcome to Valence! The goal is to stablize the machine's power grid. Complete each level by clearing all the atoms off the board.",
                "Click on the board to place the next atom (you can preview it at the bottom of the screen).",
                "To clear atoms, you need to combine atoms of opposing valences to form a stable, net-zero valence molecule. For example, Hydrogen has a valence of +1, and Chlorine has -1; if you pair them together, you get HCl, with a net valence of zero.",
                "Other atoms have higher valencies, and require more complex combinations. You can hover your mouse over any atom to see its valence.",
                "Whenever the machine cycles its power to maximum, more atoms will precipitate onto the board. You can see the count-down timer on the bottom of the screen; it'll start as soon as this tutorial closes.)",
				"But beware -- if the power grid ever fills up with atoms, the machine will explode, and it's game over!",
                "Hurry -- the world is counting on you ..."
            };

			this._tutorialMessages[7] = new string[] {
				"This level introduces noble gases (the pink and purple stars). They are inert -- they don't react (electronegativity of zero). To remove them, form reactions around and through them ..."
			};

            this._tutorialMessages[CoreModel.FIRST_PUZZLE_LEVEL] = new string[] {
                "Welcome to puzzle mode! In this mode! In this mode, you have a limited number of atoms to use to solve each board. (You have to play through to see what atoms you get.)",
                "In this level, you only have one Hg(II) atom at your disposal. Place it near the top of the diamond to finish the diamond and complete the level. Note that placing it anywhere else will result in game over -- no atoms left.",
                "Electronegativity is key to completing many of the puzzles. Pay special attention, and think out the reaction ahead of time. (You can hover over atoms to see their electronegativity (E)). More electronegative atoms react before less electronegative atoms ..."
            };

			this._tutorialMessages[CoreModel.AVALANCHE_LEVEL] = new string[] {
				"Welcome to Avalanche mode! Here, atoms keep coming at you; every ten seconds, ten appear on the screen. You get points for clearing atoms (based on their valence).",
				"See how many points you can get! (We a feat on DeenGames.com for high points scored.) You also get 1000 points for clearing the board in entirety ..."
			};

			this._tutorialMessages[CoreModel.TRICKLE_LEVEL] = new string[] {
				"Welcome to Trickle mode! Like avalanche mode, atoms keep coming, and you get points; but in this mode, it's one atom every second! Whew! Show us those nerves of steel ..."
			};
        }

        void model_MachineTicks(int secondsLeft)
        {
            if (model.GameState != DeenGames.Valence.Model.CoreModel.GameStates.GameOver)
            {
                this._timeText.DisplayText = string.Format("Time: {0}", secondsLeft);
            }
        }

        void model_MachineCreatesAtoms(IList<Tile> tilesWithNewAtoms)
        {
            foreach (Tile t in tilesWithNewAtoms)
            {
                drawTile(t);
                fadeTileIn(t);
            }
        }

        private void fadeTileIn(Tile t)
        {
            // Be yellow. Fade in.
            t.AtomSprite.ColorOperation = ColorOperation.Add;
            t.AtomSprite.Red = 1;
            t.AtomSprite.Green = 1;
            t.AtomSprite.RedRate = -1;
            t.AtomSprite.GreenRate = -1;
            this._fadingInTiles.Add(t);
        }

        private void performPerLevelPostSetup()
        {
            for (int i = 0; i < Model.CoreModel.BOARD_WIDTH; i++)
            {
                for (int j = 0; j < Model.CoreModel.BOARD_HEIGHT; j++)
                {
                    drawTile(model.Board[i, j]);
                }
            }

            this.drawNextAtom();

			if (isTutorialForCurrentLevel())
			{
				this._tutorialPanel = new TowerMessagePanel(this, "Content/tutorial-panel.png", this._tutorialMessages[model.CurrentLevel]);
				this._tutorialPanel.FadeOutComplete += new TowerMessagePanel.EventDelegate(tutorialPanel_FadeOutComplete);
				this._tutorialPanelShowing = true;

				// Timer shouldn't start until this guy closes
				model.StopCountdownTimer();
			}
        }

        private void performPerLevelPreSetup()
        {
            model.GenerateBoard();
        }

        void CoreGameScreen_LevelCompleteBlackOutComplete(TowerScreen.FadeOutMode mode)
        {
            TowerText levelUp = new TowerText(this.AddText(string.Format("Level complete! on to level {0} ...", model.CurrentLevel + 1)));
            levelUp.AddShadow();
            levelUp.AlphaRate = -0.25f;
            levelUp.Scale = 44;

            this._reactionText.Add(levelUp.BaseText);
            this._reactionText.Add(levelUp.EffectText);

			// Detect game completion
			if (model.CurrentLevel == CoreModel.FIRST_PUZZLE_LEVEL - 1 && model.GameState != CoreModel.GameStates.GameComplete)
			{
				// Completed normal mode. Woohoo!
				AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + "machine-powers-down.mp3");
				this.FadeInComplete -= CoreGameScreen_LevelCompleteBlackOutComplete;
				this.FadeOutComplete += new FadeEventDelegate(CoreGameScreen_MoveToMainMenuFadeOutComplete);
				_gameComplete = this.AddSprite("Content/Story/story-mode-complete.jpg");
				_gameComplete.Z = this.GetTopZValueAndMoveFadeToTop() + 1;
				model.SetGameStateToGameComplete();
                model.FeatManager.GrantFeat(@"mrv9c_D9NCZXnKzNUMCIXP-cqZI4ZzWld6CXZjHLeEZAKLKnvoBxsxxuUJ98xtzenJJTB8nLzXF3nwTMTMXnetEQxDDu_H53A_AB1SzZaEp8v6cc4fPfSqYUnCJ4u1T6");
			}
			if (model.CurrentLevel == CoreModel.LAST_PUZZLE_LEVEL && model.GameState != CoreModel.GameStates.GameComplete)
			{
				// Completed puzzle mode. Woohoo!
				AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + "machine-powers-down.mp3");
				this.FadeInComplete -= CoreGameScreen_LevelCompleteBlackOutComplete;
				this.FadeOutComplete += new FadeEventDelegate(CoreGameScreen_MoveToMainMenuFadeOutComplete);
				_gameComplete = this.AddSprite("Content/Story/game-complete.jpg");
				_gameComplete.Z = this.GetTopZValueAndMoveFadeToTop() + 1;
                model.FeatManager.GrantFeat(@"nS9N7CWfMXA3GNRjT8Wf868wqK6MT_s7k5iS7h8txeeLl6TRTOvKcRAuobehkekHGLWXafTNAgcstbvcPamDnRHiKpBuG0H2YXANVGCE6GsajGep4tJ2SONyeVWGNLCa");
				model.SetGameStateToGameComplete();
			}
			else
			{
				// Regular mode. Not Avalanche or Trickle.
				model.MoveToNextLevel();
				performPerLevelPreSetup();
				performPerLevelPostSetup();
				this.FadeIn();
			}
			
			model.SaveLevelReached();
        }

        void tutorialPanel_FadeOutComplete()
        {
            this._tutorialPanelShowing = false;
            if (CoreModel.Instance.CurrentLevel < CoreModel.FIRST_PUZZLE_LEVEL || CoreModel.Instance.CurrentLevel > CoreModel.LAST_PUZZLE_LEVEL)
            {
                model.StartCountdownTimer();
            }
        }

        public override void Activity(bool firstTimeCalled)
        {
            base.Activity(firstTimeCalled);

			removeDeadSprites();
			removeDeadText();
			undoColourOperationsOnFadedInTiles();

            if (model.GameState == CoreModel.GameStates.GameOver)
            {
                if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton))
                {
                    this.FadeOutComplete += new FadeEventDelegate(CoreGameScreen_MoveToMainMenuFadeOutComplete);
                    this.FadeOut();
                }
            }
			else if (model.GameState == CoreModel.GameStates.GameComplete)
			{
				if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton))
				{
					this.FadeOut();
				}
			}
			else
			{
				if (this._tutorialPanelShowing == false)
				{
					Tile t = findTileMouseIsOver();

					if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton))
					{
						// Left mouse button pushed - down this frame, not down last frame.
						if (t != null && t.IsEmpty())
						{
							t.Atom = model.NextAtom;
							model.PickNextAtom();

							this.drawTile(t);
							AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + "atom-placed.mp3");
							this.drawNextAtom();

							this.updateAtomsLeftOrPointsCounter();

							BackgroundWorker reactionComputingThread = new BackgroundWorker();
							reactionComputingThread.DoWork += new DoWorkEventHandler(reactionComputingThread_DoWork);
							reactionComputingThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(reactionComputingThread_RunWorkerCompleted);
							reactionComputingThread.RunWorkerAsync(t);
						}
					}

					else
					{
						if (t != null && !t.IsEmpty())
						{
							if (this._startHoverTime == DateTime.MinValue)
							{
								this._startHoverTime = DateTime.Now;
							}
							else
							{
								TimeSpan time = (DateTime.Now - this._startHoverTime);
								if (time.TotalMilliseconds >= 500)
								{
									infoPanel.Show(t.Atom);
								}
							}
						}
						else if (InputManager.Mouse.IsOn3D(this._nextAtom, false))
						{
							infoPanel.Show(model.NextAtom);
						}
						else
						{
							infoPanel.Hide();
							this._startHoverTime = DateTime.MinValue;
						}
					}

					if (model.IsLevelOver())
					{
						if (model.CurrentLevel != CoreModel.AVALANCHE_LEVEL && model.CurrentLevel != CoreModel.TRICKLE_LEVEL)
						{
							this.FadeOutHalf();
						}
						else
						{
							// Show random "good work" message, and add 1000 points
							string message = this._greatWorkMessages[model.RandomGenerator.Next() % this._greatWorkMessages.Length] + " (+1000 points)";
							model.Points += 1000;
							this.updateAtomsLeftOrPointsCounter();
							AudioManager.Instance.PlaySound(CoreModel.SOUND_FILE_PATH + "ulimited-mode-atoms-cleared.mp3");

							TowerText messageText = new TowerText(this.AddText(message));
							messageText.YVelocity = 15;
							messageText.AlphaRate = -0.05f;
							messageText.Scale = 48;
							messageText.AddShadow();
							messageText.Colour(255, 225, 64);

							this._reactionText.Add(messageText.BaseText);
							this._reactionText.Add(messageText.EffectText);

							// Generate 40 tiles on the board.
							int numGenerated = 0;
							while (numGenerated < 40)
							{
								IList<Tile> tiles = model.SpewOutAtomsFromMachine();
								numGenerated += tiles.Count;
								foreach (Tile tile in tiles)
								{
									drawTile(tile);
									fadeTileIn(tile);
								}
							}
						}
					}
				}
			}
        }

        void CoreGameScreen_MoveToMainMenuFadeOutComplete(TowerScreen.FadeOutMode mode)
        {
            // Move blackout back to the top
            this.GetTopZValueAndMoveFadeToTop();
            MoveToScreen(typeof(MainMenuScreen));
        }

        private void undoColourOperationsOnFadedInTiles()
        {
            // Null check: if we place an atom RIGHT next to one that just
            // appeared, that can cause a null-atomSprite tile.
            IList<Tile> reactedTiles = this._fadingInTiles.Where(tile => 
                tile.AtomSprite == null || (tile.AtomSprite.Red <= 0 && tile.AtomSprite.Green <= 0
                && tile.AtomSprite.Blue <= 0)).ToList();

            foreach (Tile t in reactedTiles) {
                if (t.AtomSprite != null)
                {
                    // Reset the fade-from-yellow effect.
                    t.AtomSprite.ColorOperation = ColorOperation.Texture;
                    t.AtomSprite.Red = 0;
                    t.AtomSprite.Green = 0;
                    t.AtomSprite.RedRate = 0;
                    t.AtomSprite.GreenRate = 0;
                }
                this._fadingInTiles.Remove(t);
            }
        }

        private void removeDeadText()
        {
            IList<Text> deadText = this._reactionText.Where(t => t.Alpha <= 0).ToList();
            foreach (Text t in deadText)
            {
                this._reactionText.Remove(t);
                this.RemoveText(t);
            }
            deadText.Clear();
        }

        private void removeDeadSprites()
        {
            // Remove dead sprites
            IList<Tile> deadTiles = this._reactingTiles.Where(tile =>
                tile.AtomSprite.Red >= 1 && tile.AtomSprite.Green >= 1 &&
                tile.AtomSprite.Blue >= 1).ToList();

            foreach (Tile reactionTile in deadTiles)
            {
                this._reactingTiles.Remove(reactionTile);

                // deletes view, as well
                reactionTile.Empty();

                // Sometimes, null happens
                if (reactionTile.AtomSprite != null)
                {
                    this.RemoveSprite(reactionTile.AtomSprite);
                    // necessary for GC, else they persist
                    reactionTile.AtomSprite = null;
                }
                if (reactionTile.AtomText != null)
                {
                    this.RemoveText(reactionTile.AtomText);
                    // necessary for GC, else they persist
                    reactionTile.AtomText = null;
                }
            }
        }

        private void drawNextAtom()
        {
            // set in initialize; DRY code, ya'ne
            float oldX = this._nextAtom.X;
            float oldY = this._nextAtom.Y;
            SpriteManager.RemoveSprite(this._nextAtom);

            if (model.NextAtom != Atom.NONE)
            {
                this._nextAtom = this.AddSprite("Content/Atoms/" + model.NextAtom.Element + ".png");
                this._nextAtom.X = oldX;
                this._nextAtom.Y = oldY;
				this._nextAtom.ScaleX *= 0.9f;
				this._nextAtom.ScaleY *= 0.9f;
                this._nextAtomText.DisplayText = model.NextAtom.Element;
            }
        }

        private void drawTile(Tile t)
        {
            if (!t.IsEmpty())
            {
                Atom a = t.Atom;
                Sprite s = this.AddSprite("Content/Atoms/" + a.Element + ".png");
                s.X = (t.X * 50) - CENTER_OF_BOARD_X;
                s.Y = (t.Y * -50) + CENTER_OF_BOARD_Y;

                Text e = this.AddText(a.Element);
                e.Scale = 16;
                e.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Center;
                e.X = s.X;
                e.Y = s.Y;

                // Do some clean-up. Just incase.
                if (t.AtomSprite != null)
                {
                    this.RemoveResource(t.AtomSprite);
                }

                if (t.AtomText != null)
                {
                    this.RemoveResource(t.AtomText);
                }
                
                t.AtomSprite = s;
                t.AtomText = e;
            }
        }

        private Tile findTileMouseIsOver()
        {
            for (int y= 0; y < Model.CoreModel.BOARD_HEIGHT; y++) {
                for (int x = 0; x < Model.CoreModel.BOARD_WIDTH; x++) {
                    if (InputManager.Mouse.IsOn3D(model.Board[x, y].Sprite, false))
                    {
                        return model.Board[x, y];
                    }
                }
            }

            return null;
        }
    }
}
