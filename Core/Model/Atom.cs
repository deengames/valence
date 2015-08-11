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
using System.Numerics;

namespace DeenGames.Valence.Model
{
    public class Atom
    {
        public static Atom NONE = new Atom("None", "NONE", 0, 0, "No atom.");

        // Normal atoms
        public static Atom Hydrogen = new Atom("Hydrogen", "H", 1, 22, "The main 'stuff' of the universe, over 75% of the universe is made up of hydrogen.");
        public static Atom Lithium = new Atom("Lithium", "Li", 1, 10, "Soft and silvery-white, it's used a lot in batteries.");
        public static Atom Beryllium = new Atom("Beryllium", "Be", 2, 16, "A steel-grey, strong, lightweight metal. Used in rocket-nozzles and windows for particle physics experiments. Dust is very toxic.");
        public static Atom Boron = new Atom("Boron", "B", 3, 20, "A trivalent metalloid element and essential plant nutrient. Sometimes brown, sometimes black.");
        public static Atom Carbon = new Atom("Carbon", "C", -4, 26, "Hard and black, the stuff that you find coal made of. Compressed, it becomes diamond.");
        public static Atom Nitrogen = new Atom("Nitrogen", "N", -3, 30, "A colourless, odorless, tasteless gas, it makes up 78% of the Earth's atmosphere.");
        public static Atom Oxygen = new Atom("Oxygen", "O", -2, 34, "The stuff of life. Makes up 21% of air. Too much is toxic, though.");
        public static Atom Flourine = new Atom("Flourine", "Fl", -1, 40, "THE undisputed king of Electronegativity. Forms a pale, yellow-brown gas that's highly reactive. And poisonous.");
        public static Atom Chlorine = new Atom("Chlorine", "Cl", -1, 32, "Used to make salt, if you mix it with sodium. And clean pools.");
        public static Atom Sodium = new Atom("Sodium", "Na", 1, 9, "A soft, silvery-white, highly reactive metal, it's soluble in water and thus, exists in large quantities in oceans.");
        public static Atom Calcium = new Atom("Calcium", "Ca", 2, 10, "A soft gray earth metal. Appears in the Earth's crust, seawater, and bones.");
        public static Atom Aluminium = new Atom("Aluminium", "Al", 3, 16, "A silvery-white ductile metal, the most abundant metal in the Earth's crust. Usually appears combined in minerals. Resists corrosion.");
        public static Atom Iodine = new Atom("Iodine", "I", -1, 27, "A rare, metal-grey solid and violet gas; used commonly in dyes and medicine.");
        public static Atom Sulphur = new Atom("Sulphur", "S", -2, 26, "A bright lemon-yellow crystaline solid. It's essential for life, and used in fertilizers, gunpowder, and insecticides. Also known as brimstone.");
        public static Atom Potassium = new Atom("Potassium", "P", -3, 8, "A soft, silvery-white metal that oxidizes rapidly in are and is very reactive with water. Necessary for the function of all living cells. Appears in fruits.");

        // (Some) Metals
        public static Atom CopperI = new Atom("Copper", "Cu(I)", 1, 19, "A reddish-orange metal, used for thousands of years. The world may soon run out of it.");
        public static Atom CopperII = new Atom("Copper", "Cu(II)", 2, 19, "A commonly-oxidizing version of copper. Usually becomes a turqoise colour; very soluble in water, where it can kill bacteria.");
        public static Atom MercuryI = new Atom("Mercury", "Hg(I)", 1, 20, "Forms a dense, white or yellow-white odorless solid, if combined with chlorine.");
        public static Atom MercuryII = new Atom("Mercury", "Hg(II)", 2, 20, "Also known as quicksilver, it's one of the few elements that's liquid at room-temperature. Very poisonous eaten, and present in fish.");
        public static Atom LeadII = new Atom("Lead", "Pb(II)", 2, 23, "A soft, malleable poor metal, it's used in batteries, bullets, weights, solder, and radiation shields.");
        public static Atom LeadIV = new Atom("Lead", "Pb(IV)", 4, 23, "A bluish-white colour when freshly cut, it tarnishes grey when exposed to air. It looks chrome silver when melted into a liquid.");
        public static Atom IronII = new Atom("Iron", "Fe(II)", 2, 18, "One of the most common metals, it alloys to make steel. Most of it comes from meteorites.");
        public static Atom IronIII = new Atom("Iron", "Fe(III)", 3, 18, "A common ion, Fe(3) has a reddish, copper-like appearance; it can even make water appear copper-coloured.");

        // Noble Gases
        public static Atom Helium = new Atom("Helium", "He", 0, 0, "The second most abundant element in universe, and accounts for 24% of the elemental mass of our galaxy. A noble gas, it doesn't react and has no electronegativity.");
        public static Atom Neon = new Atom("Neon", "Ne", 0, 0, "Common in the universe but rare on Earth; used in neon signs. A noble gas, it doesn't react and has no electronegativity.");
        public static Atom Xenon = new Atom("Xenon", "Xe", 0, 0, "Exists in over 40 unstable isotopes, which are used to study the solar system's history. A noble gas, it doesn't react and has no electronegativity.");

        private string _element = "";
        private int _ionCharge = 0;
        // Pauling's revised Electronegativity, multiplied by 10 and rounded.
        private int _electronegativity = 0;
        private string _name = "";
        private string _description = "";

        public Atom(string name, string element, int ionCharge, int electronegativity, string description)
        {
            this._name = name;
            this._element = element;
            this._ionCharge = ionCharge;
            this._electronegativity = electronegativity;
            this._description = description;
        }

        public static IList<Atom> AllAtoms
        {
            get
            {
                IList<Atom> toReturn = new List<Atom>();

                toReturn.Add(Atom.NONE);
                toReturn.Add(Atom.Flourine);
                toReturn.Add(Atom.Hydrogen);

                toReturn.Add(Atom.Beryllium);
                toReturn.Add(Atom.Boron);

                toReturn.Add(Atom.Chlorine);

                toReturn.Add(Atom.Carbon);

                toReturn.Add(Atom.Lithium);

                toReturn.Add(Atom.Nitrogen);

                toReturn.Add(Atom.Oxygen);
                toReturn.Add(Atom.Calcium);
                toReturn.Add(Atom.Aluminium);

                toReturn.Add(Atom.Sodium);
                toReturn.Add(Atom.Iodine);


                toReturn.Add(Atom.CopperII);
                toReturn.Add(Atom.IronII);
                toReturn.Add(Atom.IronIII);
                toReturn.Add(Atom.LeadII);
                toReturn.Add(Atom.LeadIV);

                toReturn.Add(Atom.CopperI);
                toReturn.Add(Atom.MercuryI);

                toReturn.Add(Atom.MercuryII);

                toReturn.Add(Atom.Potassium);
                toReturn.Add(Atom.Sulphur);
                toReturn.Add(Atom.Calcium);

                toReturn.Add(Atom.Helium);
                toReturn.Add(Atom.Neon);
                toReturn.Add(Atom.Xenon);

                return toReturn;
            }
        }

        public string Element { get { return this._element; } }
        public int IonCharge { get { return this._ionCharge; } }
        public int Electronegativity { get { return this._electronegativity; } }
        public string Name { get { return this._name; } }
        public string Description { get { return this._description; } }
        public int Points
        {
            get
            {
                if (this.IonCharge == 0)
                {
                    // Inert atoms are like 5*5; we double!
                    return 50;
                }
                else
                {
                    return this.IonCharge * this.IonCharge;
                }
            }
        }
        // for debugging
        public override string ToString()
        {
            return this.Element;
        }
    }
}
