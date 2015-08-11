using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeenGames.Utils.Tower;
using DeenGames.Utils.AStarPathFinder;
using DeenGames.Utils;
using DeenGames.Valence.Controls;
using DeenGames.Utils.Tower.Core;
using DeenGames.Valence.Utils;
using System.Windows.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Numerics;
using CSharpCity.Utils.Tools;
using System.Threading;
using com.deengames.utils.silverlight;

namespace DeenGames.Valence.Model
{
    public class CoreModel
    {
		public enum GameStates { Normal, GameOver, GameComplete };

		public FeatManager FeatManager { get; set; }

        private GameStates _state = GameStates.Normal;
        private static CoreModel _model = new CoreModel();

        private int _currentLevel = 1;
        private int _countdownTimerElapsedSeconds = 0;
        private int _countdownTimerSecondsBeforeTimerIsComplete = 0;

        public const int BOARD_WIDTH = 10;
        public const int BOARD_HEIGHT = 10;
        private const int START_TIME_IN_SECONDS_FOR_CRYSTAL_ENERGIZING = 40;

        #region level constants
        public const int NUMBER_OF_LEVELS = 12;                     // 1-12
        public const int FIRST_PUZZLE_LEVEL = NUMBER_OF_LEVELS + 1; // 13
        public const int LAST_PUZZLE_LEVEL = NUMBER_OF_LEVELS * 2;  // 24
        public const int AVALANCHE_LEVEL = LAST_PUZZLE_LEVEL + 1;   // 25
        public const int TRICKLE_LEVEL = AVALANCHE_LEVEL + 1;       // 26
        #endregion

        private const string MAX_LEVEL_KEY = "Max Level"; // used to persist data
        private int _maxLevelReached = 1;

        private DispatcherTimer _countdownTickTimer = new DispatcherTimer();

        private Tile[,] _board = new Tile[BOARD_WIDTH, BOARD_HEIGHT];

        private Toolbox _toolbox = new Toolbox();
        private IList<Atom> _boardAtoms = new List<Atom>();
        
        private Atom _nextAtom;
        private PathFinderFast _pathFinder = new PathFinderFast(new byte[2, 2]);

        public delegate void MachineCreatesAtomsDelegate(IList<Tile> tilesWithNewAtoms);
        public event MachineCreatesAtomsDelegate MachineCreatesAtoms;

        public delegate void MachineTicksDelegate(int secondsLeft);
        public event MachineTicksDelegate MachineTicks;

        public delegate void GameOverOccuredDelegate();
        public event GameOverOccuredDelegate GameOver;

		public const string SOUND_FILE_PATH = "Content/Sound/";

        public BigInteger Points { get; set; }

        private Random _rGen = new Random();

		public String LevelName { get; set; }

        private static object padlock = new object();
         
        private CoreModel() {
            this._countdownTickTimer.Interval = TimeSpan.FromSeconds(1);
            this._countdownTickTimer.Tick += new EventHandler(_countdownTickTimer_Tick);
            loadLastReachedLevel();

            // Resume
            this._currentLevel = this._maxLevelReached;
			this.FeatManager = new FeatManager();
        }

        private void loadLastReachedLevel()
        {
            string maxLevelData = PersistentStorage.GetItem(MAX_LEVEL_KEY);

            if (maxLevelData == null)
            {
                _maxLevelReached = 1;
            }
            else
            {
                _maxLevelReached = PersistentStorage.GetWholeNumberItem(MAX_LEVEL_KEY);
            }

            if (_maxLevelReached < 0 || _maxLevelReached > TRICKLE_LEVEL)
            {
                // Corrupt data. Oh well.
                _maxLevelReached = 1;
            }
        }

        public int MaxLevelReached { get { return this._maxLevelReached; } }

        public GameStates GameState { get { return this._state; } }

        public void StopCountdownTimer()
        {
            this._countdownTickTimer.Stop();
        }

        public void StartCountdownTimer()
        {
            this._countdownTickTimer.Start();
        }

        void _countdownTickTimer_Tick(object sender, EventArgs e)
        {
            this._countdownTimerElapsedSeconds++;

            if (this._countdownTimerElapsedSeconds >= this._countdownTimerSecondsBeforeTimerIsComplete) {
                if (
                    // Normal, Avalanche, and Trickle
                    (this.CurrentLevel <= NUMBER_OF_LEVELS || this.CurrentLevel == AVALANCHE_LEVEL || this.CurrentLevel == TRICKLE_LEVEL)
                    && this.MachineCreatesAtoms != null)
                {
                    this.SpewOutAtomsFromMachine();
                    this._countdownTimerElapsedSeconds = 0;
                }
            }
            if (this.MachineTicks != null) {
                this.MachineTicks.Invoke(this._countdownTimerSecondsBeforeTimerIsComplete - this._countdownTimerElapsedSeconds);
            }
        }

        public IList<Tile> SpewOutAtomsFromMachine()
        {

            int numAtoms = 0;
            if (this.CurrentLevel < FIRST_PUZZLE_LEVEL)
            {
                // Normal mode
                numAtoms = _rGen.Next(1, this._currentLevel);
            }
            else if (this.CurrentLevel == AVALANCHE_LEVEL)
            {
                // Avalanche
                numAtoms = 10;
            }
            else if (this.CurrentLevel == TRICKLE_LEVEL)
            {
                // Trickle mode
                numAtoms = 1;
            }

            int numEmptyTiles = this.countEmptyTilesOnTheBoard();
            bool isGameOver = false;

            if (numAtoms > numEmptyTiles)
            {
                // Game over!! Generate enough to fill the board. So the player
                // sees the board fill up with tiles. THEN game over strikes.
                numAtoms = numEmptyTiles;
                isGameOver = true;
            }

            IList<Tile> targetTiles = new List<Tile>();
            Tile randomTile;

            // We KNOW this will work, because numAtoms <= numEmptyTiles)
            while (targetTiles.Count < numAtoms)
            {
                int x = this._rGen.Next(0, BOARD_WIDTH);
                int y = this._rGen.Next(0, BOARD_WIDTH);
                randomTile = this.Board[x, y];

                if (randomTile.IsEmpty())
                {
                    randomTile.Atom = this._toolbox.GetRandomAtomForSpewing();
                    targetTiles.Add(randomTile);
                }
            }

            if (MachineCreatesAtoms != null)
            {
                MachineCreatesAtoms.Invoke(targetTiles);
            }

            if (isGameOver)
            {
                this.SignalGameOver();
            }

			AudioManager.Instance.PlaySound(SOUND_FILE_PATH + "atoms-generated.mp3");

            return targetTiles;
        }

        public void SignalGameOver()
        {
            this._countdownTickTimer.Stop();
            {

                if (this._state != GameStates.GameOver)
                {
            this._state = GameStates.GameOver;

            if (this.GameOver != null)
            {
                this.GameOver.Invoke();
            }
        }
            }
        }

        private int countEmptyTilesOnTheBoard()
        {
            int emptyTiles = 0;

            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                for (int i = 0; i < BOARD_WIDTH; i++)
                {
                    if (this._board[i, j].IsEmpty() == true)
                    {
                        emptyTiles++;
                    }
                }
            }

            return emptyTiles;
        }

        public int CurrentLevel
        {
            get { return this._currentLevel; }
            set { this._currentLevel = value; }
        }

        public void StartAtFirstLevel()
        {
            this._currentLevel = 1;
        }

        public static CoreModel Instance { get { return _model; } }

        public Random RandomGenerator { get { return this._rGen; } }

        public Tile[,] Board { get { return this._board; } }

        public string BoardToString()
        {
            StringBuilder toReturn = new StringBuilder();
            Atom currentAtom;

            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                for (int i = 0; i < BOARD_WIDTH; i++)
                {
                    currentAtom = this._board[i, j].Atom;
                    toReturn.Append(currentAtom.Element);

                    if (j < BOARD_HEIGHT - 1)
                    {
                        toReturn.Append(", ");
                    }
                }

                toReturn.Append('\n');
            }

            return toReturn.ToString();
        }

		private string GetPuzzleBoardAndToolboxDefinition(int puzzleLevel)
		{
			return EmbeddedResourceFileReader.ReadFile("Content/PuzzleLevels/Puzzle" + puzzleLevel + ".txt");
		}

		public void LoadBoard(int puzzleLevel)
		{
			string boardString = this.GetPuzzleBoardAndToolboxDefinition(puzzleLevel);
			string[] rows = boardString.Split('\n');
			string currentRow = "";
			IList<Atom> AllAtoms = Atom.AllAtoms;

			_toolbox.Empty();
			this.LevelName = rows[0].Trim();

			// Read in a tab-delimited grid of atom elements
			for (int i = 1; i < rows.Length - 1; i++)
			{
				int lastAtomIndex = 0;
				currentRow = rows[i].Trim();
				if (currentRow == "") continue;
				while (currentRow.Length > 0)
				{
					int endPos = currentRow.IndexOf('\t');
					if (endPos == -1)
					{
						// last element
						endPos = currentRow.Length;
					}

					string element = currentRow.Substring(0, endPos);
					Atom atom = AllAtoms.Where(a => a.Element == element).ToList()[0];
					// i-1 because first row is the level name; so data is staggered
					this._board[lastAtomIndex, i - 1].Atom = atom;
					currentRow = currentRow.Substring(endPos);

					// Remove if non-last atom
					if (currentRow.StartsWith("\t"))
					{
						currentRow = currentRow.Substring(1);
					}
					lastAtomIndex++;
				}
			}

			// Read in a toolbox definition
			string toolboxDef = rows[rows.Length - 1];
			// We expect something like: H, Fl, 2Na, P
			// Don't allow dupes, since toolbox doesn't allow dupes.
			IList<string> elementsSeen = new List<string>();
			string[] atomDefs = toolboxDef.Split(',');
			string curAtom;
			Regex atomDefRegex = new Regex(@"([0-9]*)([a-zA-Z\(\)]+)");
			string atomQuantity;

			foreach (string atom in atomDefs)
			{
				curAtom = atom.Trim();
				Match m = atomDefRegex.Match(curAtom);
				int numAtoms = 1;
				atomQuantity = m.Groups[1].Value;
				if (!string.IsNullOrEmpty(atomQuantity))
				{
					numAtoms = int.Parse(atomQuantity);
				}
				Atom toAdd = Atom.AllAtoms.First(a => a.Element == m.Groups[2].Value);
				this._toolbox.AddAtom(toAdd, numAtoms);
			}
		}

        public Toolbox Toolbox { get { return this._toolbox; } }

        public Atom NextAtom { get { return this._nextAtom; } }

        public void CreateBoard()
        {
            for (int i = 0; i < BOARD_WIDTH; i++)
            {
                for (int j = 0; j < BOARD_HEIGHT; j++)
                {
                    this._board[i, j] = new Tile(i, j);
                }
            }
        }

        public void GenerateBoard()
        {
            if (this.CurrentLevel <= NUMBER_OF_LEVELS)
            {
                // Normal mode
                generateNormalModeBoard();
                this._countdownTickTimer.Start();
				this.LevelName = "Level " + this.CurrentLevel;
            }
            else if (this.CurrentLevel <= LAST_PUZZLE_LEVEL)
            {
                // Puzzle mode
                generatePuzzleModeLevel();
                this._countdownTickTimer.Stop();
				// Level Name is loaded
            }
            else
            {
                // Avalanche and Trickle. They act like Normal mode.
                generateNormalModeBoard();
				if (this.CurrentLevel == AVALANCHE_LEVEL)
				{
					this.LevelName = "Avalanche";
				}
				else
				{
					this.LevelName = "Trickle";
				}
                this._countdownTickTimer.Start();
            }

            this.PickNextAtom();
        }

        private void generatePuzzleModeLevel()
        {
            // -1 cos from base 1 to base 0
            int currentPuzzleLevel = this.CurrentLevel - NUMBER_OF_LEVELS;
            this.LoadBoard(currentPuzzleLevel);
        }

        private void generateNormalModeBoard()
        {
            this._countdownTickTimer.Stop();

            this._countdownTimerElapsedSeconds = 0;

            if (this.CurrentLevel == AVALANCHE_LEVEL) {
                this._countdownTimerSecondsBeforeTimerIsComplete = 10;
            } else if (this.CurrentLevel == TRICKLE_LEVEL) {
                this._countdownTimerSecondsBeforeTimerIsComplete = 1;
            } else {
                // t_n = 40, 38, 36, 34, ... 20
                // Min of 14 is HARD; so stick to that.
                this._countdownTimerSecondsBeforeTimerIsComplete =
                        CoreModel.START_TIME_IN_SECONDS_FOR_CRYSTAL_ENERGIZING -
                        ((this._currentLevel - 1) * 2);
            }

            this._toolbox.Empty();

            /*
             LEVELS:
                1-3: Normal
                4-6: Metals
                7-9: inert gases (see BoardAtoms for those, they don't go in the toolbox)
                10-12: Atoms with |valence| = 1 disappear
            */
            const int ADD_INERT_GASES_AT_LEVEL_N = 7;
            const int REMOVE_SMALL_VALENCES_AT_LEVEL_N = 10;

            if (this.CurrentLevel <= REMOVE_SMALL_VALENCES_AT_LEVEL_N)
            {
                this._toolbox.AddAtom(Atom.Flourine);
                this._toolbox.AddAtom(Atom.Hydrogen);
            }

            if (this.CurrentLevel >= 2)
            {
                this._toolbox.AddAtom(Atom.Beryllium);
                this._toolbox.AddAtom(Atom.Boron);

                if (this.CurrentLevel <= REMOVE_SMALL_VALENCES_AT_LEVEL_N)
                {
                    this._toolbox.AddAtom(Atom.Chlorine);
                }
            }

            if (this.CurrentLevel >= 3)
            {
                this._toolbox.AddAtom(Atom.Carbon);

                if (this.CurrentLevel <= REMOVE_SMALL_VALENCES_AT_LEVEL_N)
                {
                    this._toolbox.AddAtom(Atom.Lithium);
                }

                this._toolbox.AddAtom(Atom.Nitrogen);
            }

            if (this.CurrentLevel >= 4)
            {
                this._toolbox.AddAtom(Atom.Oxygen);
                this._toolbox.AddAtom(Atom.Calcium);
                this._toolbox.AddAtom(Atom.Aluminium);

                if (this.CurrentLevel <= REMOVE_SMALL_VALENCES_AT_LEVEL_N)
                {
                    this._toolbox.AddAtom(Atom.Sodium);
                    this._toolbox.AddAtom(Atom.Iodine);
                }
            }

            if (this.CurrentLevel >= 5)
            {

                this._toolbox.AddAtom(Atom.CopperII);
                this._toolbox.AddAtom(Atom.IronII);
                this._toolbox.AddAtom(Atom.IronIII);
                this._toolbox.AddAtom(Atom.LeadII);
                this._toolbox.AddAtom(Atom.LeadIV);

                if (this.CurrentLevel <= REMOVE_SMALL_VALENCES_AT_LEVEL_N)
                {
                    this._toolbox.AddAtom(Atom.CopperI);
                    this._toolbox.AddAtom(Atom.MercuryI);
                }

                this._toolbox.AddAtom(Atom.MercuryII);
            }

            if (this.CurrentLevel >= 6)
            {
                this._toolbox.AddAtom(Atom.Potassium);
                this._toolbox.AddAtom(Atom.Sulphur);
                this._toolbox.AddAtom(Atom.Calcium);
            }

            this._boardAtoms = new List<Atom>(this.Toolbox.Atoms);
            if (this.CurrentLevel >= ADD_INERT_GASES_AT_LEVEL_N)
            {
                this._boardAtoms.Add(Atom.Helium);
                this._boardAtoms.Add(Atom.Neon);
                this._boardAtoms.Add(Atom.Xenon);
            }

            int numAtoms;
            if (this.CurrentLevel > NUMBER_OF_LEVELS) {
                // Avalanche and Trickle.
                numAtoms = 50;
            } else {
                // t_n = 8n (8 * 12 = 96)
                numAtoms = this._currentLevel * 8;
            }

            // This can still occasionally lock up on high levels. Our strategy is, if you
            // can't do this in ~2N^2 tries, clear the board and start all over again.
            // This is due to our balancing algorithm. Maybe that's tweakable.
            // On level 10, this happens around one time in three.

            int numTries = 0;
            const int MAX_TRIES = 200;

            for (int i = 0; i < numAtoms; i++)
            {
                numTries = 0;
                Atom proposedAtom = this._boardAtoms[_rGen.Next(0, this._boardAtoms.Count)];

                int x = _rGen.Next(0, BOARD_WIDTH);
                int y = _rGen.Next(0, BOARD_HEIGHT);
                Tile current = this._board[x, y];

                while (!current.IsEmpty() || !tileFollowsDifficultyBalancingAlgorithm(current, proposedAtom))
                {
                    x = _rGen.Next(0, BOARD_WIDTH);
                    y = _rGen.Next(0, BOARD_HEIGHT);
                    current = this._board[x, y];

                    numTries++;

                    if (numTries >= MAX_TRIES)
                    {
                        numTries = 0;
                        i = 0;
                        this.clearBoard();
                    }
                }

                current.Atom = proposedAtom;
            }
            
        }

        public void PickNextAtom()
        {
            this._nextAtom = this._toolbox.GetNextAtom();
        }

        private void clearBoard()
        {
            for (int j = 0; j < CoreModel.BOARD_HEIGHT; j++)
            {
                for (int i = 0; i < CoreModel.BOARD_WIDTH; i++)
                {
                    this.Board[i, j].Empty();
                }
            }
        }

        private bool tileFollowsDifficultyBalancingAlgorithm(Tile current, Atom proposedAtom)
        {
            IList<Tile> adjacents = this.getNonEmptyAdjacentTiles(current);

            if (adjacents.Count == 0)
            {
                return true;
            }
            else
            {
                IList<Tile> oppositeValence;

                if (proposedAtom.IonCharge > 0)
                {
                    oppositeValence = adjacents.Where(t => t.Atom.IonCharge < 0).ToList();
                }
                else
                {
                    oppositeValence = adjacents.Where(t => t.Atom.IonCharge > 0).ToList();
                }

                // Fail if 50% or more of atoms are opposite valence
                if (oppositeValence.Count >= 0.5 * adjacents.Count)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsLevelOver()
        {
            bool isOver = true;
            for (int y = 0; y < CoreModel.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < CoreModel.BOARD_WIDTH; x++)
                {
                    if (this.Board[x, y].IsEmpty() == false)
                    {
                        isOver = false;
                    }
                }
            }

            return isOver;
        }

        public int NetValence
        {
            get
            {
                int netValence = 0;
                for (int i = 0; i < CoreModel.BOARD_WIDTH; i++)
                {
                    for (int j = 0; j < CoreModel.BOARD_HEIGHT; j++)
                    {
                        netValence += this.Board[i, j].Atom.IonCharge;
                    }
                }

                return netValence;
            }
        }

        #region calculating reactions
        // Our real, core algorithm. Let's do it!
        internal IList<Tile> GetTilesInReaction(Tile newAtomTile) {
            // get all adjacent atoms. True = base case
            IList<Tile> potentialReactionTiles = new List<Tile>();
            this.getPotentialReactionTiles(newAtomTile, potentialReactionTiles);

            // It's fast, and it's great, but fails in wierd cases.
            // Like B-Fl-Fl-Fl, where you place the Fl closest to the B last.
            // If we carry over connected atoms, this case works, but other cases bork.
            // Like this case:
            // B Fl
            // B Fl
            //   Fl

            IList<Molecule> potentialMolecules = this.calculateReactionsUsingBestFirstSearch(potentialReactionTiles, newAtomTile, new List<Tile>(), new List<Tile>());

            // If best-first didn't find anything, revert to brute-froce.
            if (potentialMolecules.Count == 0)
            {
                potentialMolecules = this.calculateReactionsUsingBruteForceCombinations(potentialReactionTiles, newAtomTile);
            }

			// All of the following calls are not necessary in the brute-force case.
			// But the performance is trivial, so, do it anyway to simplify the code.
            // Weed out net != 0
            potentialMolecules = potentialMolecules.Where(m => m.NetCharge == 0).ToList();

            // Fix for bug where you can form |-shaped HCl around a Beryllium atom
            potentialMolecules = weedOutNonConnectedMolecules(potentialMolecules);
            
            // Sort by electronegativity, get the top result
            Molecule formedMolecule = getMostElectronegative(potentialMolecules);

            IList<Tile> toReturn = new List<Tile>();
            if (formedMolecule != null)
            {
                foreach (Tile t in formedMolecule.AtomTiles)
                {
                    toReturn.Add(t);
                }
            }

            return toReturn;
        }

        // It's complicated. We use two collections: one that looks at ALL the atoms we see, ever,
        // and one that looks at only the atoms we've seen so far -- the latter back-pedals when
        // we run out of adjacencies, and the former perseveres. This covers all kinds of cases,
        // like (* indicates last atom placed):
        // B Fl
        // B Fl
        //   Fl*
        // ... which is covered by back-pedalling only, and like:
        // B Cl* Cl Cl
        // ... which is covered by looking at the total only.
        private IList<Molecule> calculateReactionsUsingBestFirstSearch(IList<Tile> potentialReactionTiles, Tile centerTile, IList<Tile> foundSoFarInPath, IList<Tile>foundSoFarInTotal)
        {
            // These IFs cover a tricky case where we search exhaustively and don't find
            // a molecule, then when backtracking, backtrack into something we've seen
            // already. We shouldn't add a tile twice, and this fixes the bug that does that
            // (visible in puzzle one -- it adds the bottom-most Fl twice (6, 3).)
            if (!foundSoFarInPath.Contains(centerTile))
            {
                foundSoFarInPath.Add(centerTile);
            }

            if (!foundSoFarInTotal.Contains(centerTile))
            {
                foundSoFarInTotal.Add(centerTile);
            }
            
            Molecule m1 = new Molecule();
            foreach (Tile t in foundSoFarInPath)
            {
                m1.AtomTiles.Add(t);
            }

            if (m1.NetCharge == 0)
            {
                // Base case: we're net zero!
                List<Molecule> toReturn = new List<Molecule>();
                toReturn.Add(m1);
                return toReturn;
            }

            Molecule m2 = new Molecule();
            foreach (Tile t in foundSoFarInTotal)
            {
                m2.AtomTiles.Add(t);
            }

            if (m2.NetCharge == 0)
            {
                // Base case: we're net zero!
                List<Molecule> toReturn = new List<Molecule>();
                toReturn.Add(m2);
                return toReturn;
            }

            // Recursive case: keep looking.
            // Find adjacent molecules
            IList<Tile> adjacents = potentialReactionTiles.Where(t => TowerUtils.DistanceBetweenPoints(t.X, t.Y, centerTile.X, centerTile.Y) == 1).ToList();

            // Exclude tiles we already saw. Use current path, for back-pedalling.
            adjacents = adjacents.Where(t => !foundSoFarInPath.Contains(t) && !foundSoFarInTotal.Contains(t)).ToList();

            // Sort by |my valence + their valence|; 
            adjacents = adjacents.OrderBy(t => t, new OrderTilesByElectronegativityWithAtomComparer(centerTile.Atom)).ToList();

            // Base case: no adjacents.
            if (adjacents.Count == 0)
            {
                return new List<Molecule>();
            }
            else
            {
                // Take the best, and move on
                foreach (Tile adj in adjacents)
                {
                    // Copy foundSoFar so we can return up the stack and keep looking on
                    // a different path. Without screwing up our "found so far" data.
                    IList<Tile> foundDupe = new List<Tile>();
                    foreach (Tile t in foundSoFarInPath)
                    {
                        foundDupe.Add(t);
                    }

                    IList<Molecule> toReturn = this.calculateReactionsUsingBestFirstSearch(potentialReactionTiles, adj, foundDupe, foundSoFarInTotal);
                    if (toReturn.Count > 0)
                    {
                        return toReturn;
                    }
                }

                // Found nothing.
                return new List<Molecule>();
            }
        
        }

        private IList<Molecule> calculateReactionsUsingBruteForceCombinations(IList<Tile> potentialReactionTiles, Tile userClickedTile)
        {
            IList<Molecule> toReturn = new List<Molecule>();
            // Generate all possible combinations of (2 .. N) molecules.
            // Since the internal function handles all the combinationing, we just
            // call it with the biggest atom count, and it gets all the combinations of [2 .. N]
            IList<Molecule> combos = getCombinationsOf(potentialReactionTiles.Count, potentialReactionTiles, userClickedTile);
            return combos;
        }

        private Molecule getMostElectronegative(IList<Molecule> molecules)
        {
            if (molecules.Count == 0)
            {
                return null;
            }
            else
            {
                Molecule toReturn = molecules[0];
                foreach (Molecule m in molecules)
                {
                    if (m.Electronegativity > toReturn.Electronegativity)
                    {
                        toReturn = m;
                    }
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Given a list of potential molecules, weed out all the ones where
        /// the atoms are not all adjacent to each other. This can occur due
        /// to a bug-fix for I-shaped molecules not reacting, where we
        /// extended molecule detection to all non-empty adjacent tiles.
        /// As a result, you can have H-Be-Cl forming H-Cl, which is wrong.
        /// This function will fix that, by checking that H and Cl are adjacent.
        /// </summary>
        /// <param name="molecules"></param>
        /// <returns></returns>
        private IList<Molecule> weedOutNonConnectedMolecules(IList<Molecule> molecules)
        {
            IList<Molecule> toReturn = new List<Molecule>();
            // The solution to this is really stupid. Checking for adjacencies
            // won't work; that will allow H-H-Be-Cl-Cl to form H-H-Cl-Cl, because
            // the two Hs are adjacent, as are the two Cls. Lame, dude.
            // The solution? Use path-finding to make sure there's a solid path of
            // atoms connecting every atom to every other atom. Yech.

			// Multi-Threading is a bloody business. There are lots of casualties.
			// Remove said casualties -- molecules that have "NONE" atoms in them.
			molecules = molecules.Where(m => !m.AtomTiles.Any(t => t.Atom == Atom.NONE)).ToList();

            foreach (Molecule candidate in molecules)
            {
                this._pathFinder = new PathFinderFast(candidate.ByteGrid);
                _pathFinder.Diagonals = false;

                bool allPairsHaveAPath = true;
                List<PathFinderNode> path;
                Tile startTile;
                Tile endTile;
                Point start;
                Point end;

                for (int i = 0; i < candidate.AtomTiles.Count; i++)
                {
                    startTile = candidate.AtomTiles[i];
                    start = new Point(startTile.X, startTile.Y);

                    for (int j = i + 1; j < candidate.AtomTiles.Count; j++)
                    {
                        endTile = candidate.AtomTiles[j];
                        end = new Point(endTile.X, endTile.Y);

                        path = this._pathFinder.FindPath(start, end);
                        if (path == null)
                        {
                            allPairsHaveAPath = false;
                        }
                    }
                }

                if (allPairsHaveAPath == true)
                {
                    toReturn.Add(candidate);
                }
            }

            return toReturn;
        }

        #region combination-collecting
		private IList<Molecule> getCombinationsOf(int i, IList<Tile> potentialReactionTiles, Tile userClickedTile)
		{
			// Is there a better way? Ionno.
			// Let's return the FIRST molecule we find that matches. Instead of returning
			// like 2000 potential molecules.
			IList<Molecule> twos = new List<Molecule>();
			IList<Molecule> threes = new List<Molecule>();
			IList<Molecule> fours = new List<Molecule>();
			IList<Molecule> fives = new List<Molecule>();
			IList<Molecule> sixes = new List<Molecule>();
			IList<Molecule> sevens = new List<Molecule>();
			IList<Molecule> eights = new List<Molecule>();
			IList<Molecule> nines = new List<Molecule>();

			// Our "working set" of "do we have anything?"
			IList<Molecule> temp = new List<Molecule>();

			if (i >= 2)
			{
				twos = getCombinationsOfTwo(potentialReactionTiles, userClickedTile);
				temp = twos.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}

			if (i >= 3)
			{
				threes = getIncrementalCombinations(potentialReactionTiles, twos);
				temp = threes.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}

			if (i >= 4)
			{
				// Optimize: dump un-needed stuff
				twos.Clear();
				fours = getIncrementalCombinations(potentialReactionTiles, threes);
				temp = fours.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}

			if (i >= 5)
			{
				threes.Clear();
				fives = getIncrementalCombinations(potentialReactionTiles, fours);
				temp = fives.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}

			if (i >= 6)
			{
				fours.Clear();
				sixes = getIncrementalCombinations(potentialReactionTiles, fives);
				temp = sixes.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}

			if (i >= 7)
			{
				fives.Clear();
				sevens = getIncrementalCombinations(potentialReactionTiles, sixes);
				temp = sevens.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}


			if (i >= 8)
			{
				sixes.Clear();
				eights = getIncrementalCombinations(potentialReactionTiles, sevens);
				temp = eights.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}


			if (i >= 9)
			{
				sevens.Clear();
				nines = getIncrementalCombinations(potentialReactionTiles, eights);
				temp = nines.Where(m => m.NetCharge == 0).ToList();
				if (temp.Count > 0)
				{
					temp = weedOutNonConnectedMolecules(temp);
					if (temp.Count > 0)
					{
						return temp;
					}
				}
			}




			// Didn't find it yet and return? Sucks to be you!
			return new List<Molecule>();
		}

        private IList<Molecule> getIncrementalCombinations(IList<Tile> potentialReactionTiles, IList<Molecule> previousSet)
        {
            IList<Molecule> toReturn = new List<Molecule>();
			
			// This thread needs to be more sleepy. To prevent lag.
			int throttleCount = 0;
			const int THROTTLE_AFTER_COUNT = 10;

            // Trick: if we are generating combos of five, and we just did twos, we have:
            // AB, AC, AD, BC, BD, BE, CD, CE
            // We now take each of these, and add whatever's after the last one
            // eg. for AB, we look at: C, D, E to make ABC, ABD, ABE!
            // BRILLIANT, I tell you!!
            foreach (Molecule m in previousSet)
            {
                Tile lastTile = m.AtomTiles[m.AtomTiles.Count - 1];
                IList<Tile> validTiles = new List<Tile>();

                for (int i = potentialReactionTiles.IndexOf(lastTile) + 1; i < potentialReactionTiles.Count; i++)
                {
                    validTiles.Add(potentialReactionTiles[i]);
                }

                foreach (Tile t in validTiles)
                {
                    Molecule newbie = new Molecule();

                    foreach (Tile at in m.AtomTiles)
                    {
                        newbie.AtomTiles.Add(at);
                    }

                    newbie.AtomTiles.Add(t);
                    toReturn.Add(newbie);
					throttleCount++;
					throttleCount %= THROTTLE_AFTER_COUNT;
					if (throttleCount == THROTTLE_AFTER_COUNT - 1)
					{
						Thread.Sleep(10);
					}
                }
            }

            return toReturn;
        }


        private IList<Molecule> getCombinationsOfTwo(IList<Tile> potentialReactionTiles, Tile userClickedTile)
        {
            IList<Molecule> toReturn = new List<Molecule>();
            Molecule combo;

            for (int i = 0; i < potentialReactionTiles.Count; i++)
            {
                for (int j = i + 1; j < potentialReactionTiles.Count; j++)
                {
                    combo = new Molecule();
                    combo.AtomTiles.Add(potentialReactionTiles[i]);
                    combo.AtomTiles.Add(potentialReactionTiles[j]);

                    // Bug-fix and optimization: only molecules with the user-clicked tile are legit.
                    if (combo.AtomTiles.Contains(userClickedTile))
                    {
                        toReturn.Add(combo);
                    }
                }
            }

            return toReturn;
        }
        #endregion

        /// <summary>
        /// Get potential reaction tiles. These are non-empty tiles
        /// that are non-diagonally adjacent to centerTile.
        /// This also includes tiles that are adjacent to adjacent tiles.
        /// </summary>
        /// <param name="centerTile"></param>
        /// <returns></returns>
        private void getPotentialReactionTiles(Tile centerTile, IList<Tile> toReturn)
        {
            if (!toReturn.Contains(centerTile))
            {
                toReturn.Add(centerTile);
            }

            IList<Tile> temp = getNonEmptyAdjacentTiles(centerTile);
            

            foreach (Tile t in temp.Where(t => !toReturn.Contains(t)))
            {
                // Take adjacencies to all our atoms.
                getPotentialReactionTiles(t, toReturn);
            }
        }

        private IList<Tile> getNonEmptyAdjacentTiles(Tile centerTile)
        {
            IList<Tile> toReturn = new List<Tile>();

            if (centerTile.X > 0)
            {
                // left
                toReturn.Add(this._board[centerTile.X - 1, centerTile.Y]);
            }
            if (centerTile.X < BOARD_WIDTH - 1)
            {
                // right
                toReturn.Add(this._board[centerTile.X + 1, centerTile.Y]);
            }

            if (centerTile.Y > 0)
            {
                toReturn.Add(this._board[centerTile.X, centerTile.Y - 1]);
            }
            if (centerTile.Y < BOARD_HEIGHT - 1)
            {
                toReturn.Add(this._board[centerTile.X, centerTile.Y + 1]);
            }

            return toReturn.Where(t => !t.IsEmpty()).ToList();
        }
        #endregion


        public void SetGameStateToNormal()
        {
            this._state = GameStates.Normal;
        }

        public void MoveToNextLevel()
        {
            this._currentLevel++;
			AudioManager.Instance.PlaySound(SOUND_FILE_PATH + "level-complete.mp3");
        }

        public void SaveLevelReached()
        {
            // Don't overwrite higher levels with lower levels
            if (this._maxLevelReached < this.CurrentLevel)
            {
                PersistentStorage.StoreItem(MAX_LEVEL_KEY, this.CurrentLevel);
				this._maxLevelReached = this.CurrentLevel;
            }
        }

        public void ClearEventHandlers()
        {
            this.MachineCreatesAtoms = null;
            this.MachineTicks = null;
            this.GameOver = null;
        }

		internal void SetGameStateToGameComplete()
		{
			this._state = GameStates.GameComplete;
		}
	}
}
