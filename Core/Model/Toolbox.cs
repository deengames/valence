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
using System.Collections.Generic;
using System.Linq;

namespace DeenGames.Valence.Model
{
    public class Toolbox
    {
		// All modes: a bunch of atoms (a set). Puzzle mode goes in order.
		private IList<Atom> _atoms = new List<Atom>();
		// Puzzle mode: quantities per atom.
		private IList<int> _atomQuantities = new List<int>();

        public Atom GetRandomAtomForSpewing()
        {
            return this._atoms[CoreModel.Instance.RandomGenerator.Next(0, this._atoms.Count)];
        }

        public void Empty()
        {
            this._atoms.Clear();
			this._atomQuantities.Clear();
        }

        public int NumAtomsLeft
        {
            get
            {
                return this._atomQuantities.Sum();
            }
        }

        public bool IsOutOfAtoms()
        {
            return this.NumAtomsLeft == 0;
        }

        public void AddAtom(Atom atom, int quantity = 1)
        {
            if (!this._atoms.Contains(atom))
            {
				this._atoms.Add(atom);
				this._atomQuantities.Add(quantity);
            }
            else
            {
				int index = this._atoms.IndexOf(atom);
				this._atomQuantities[index] += quantity;
            }
        }

        internal IList<Atom> Atoms { get { return this._atoms; } }

        public Atom GetNextAtom()
        {
            Atom next = Atom.NONE;

            if (CoreModel.Instance.CurrentLevel < CoreModel.FIRST_PUZZLE_LEVEL || CoreModel.Instance.CurrentLevel > CoreModel.LAST_PUZZLE_LEVEL)
            {
                // Normal mode, avalanche mode, trickle mode.
                // Pick randomly, ignore quantity

                IList<Atom> toolboxNegativeIons = this._atoms.Where(a => a.IonCharge < 0).ToList();
                IList<Atom> toolboxPositiveIons = this._atoms.Where(a => a.IonCharge > 0).ToList();

                int netValence = CoreModel.Instance.NetValence;

                // Pick positive or negative
                bool pickPositive = true;
                if (netValence < -2)
                {
                    // 1/n chance of negative
                    // eg. 5 valence gives us a 20% chance of negative
                    pickPositive = !(CoreModel.Instance.RandomGenerator.Next(0, 100) <= 100 / -netValence);
                }
				else if (netValence > 2)
				{
					// 1/n chance of positive
					// eg. 4 valence gives us 25% chance of positive
					pickPositive = CoreModel.Instance.RandomGenerator.Next(0, 100) <= 100 / netValence;
				}
				else
				{
					// 50-50 chance
					pickPositive = CoreModel.Instance.RandomGenerator.Next(0, 100) <= 50;
				}

                if (pickPositive)
                {
                    int index = CoreModel.Instance.RandomGenerator.Next(0, toolboxPositiveIons.Count);
                    next = toolboxPositiveIons[index];
                }
                else
                {
                    int index = CoreModel.Instance.RandomGenerator.Next(0, toolboxNegativeIons.Count);
                    next = toolboxNegativeIons[index];
                }
            }
            else
            {
                // Puzzle mode
                if (this.IsOutOfAtoms())
                {
                    // Triggers game over
                    return Atom.NONE;
                }
                else
                {
                    // Go in order and respect quantity
					if (this._atoms.Count > 0)
					{
						next = this._atoms[0];
						int quantity = this._atomQuantities[0];

						if (quantity == 1)
						{
							this._atoms.Remove(next);
							this._atomQuantities.RemoveAt(0);
						}
						else
						{
							// Decrement quantity.
							this._atomQuantities[0]--;
						}
					}
                }
            }

            return next;
        }
    }
}
