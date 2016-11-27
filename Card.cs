using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace UNO
{
    public enum COLOR { BLUE, GREEN, RED, YELLOW, WILD };
    public enum CARD { ZERO, ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, DRAW_2, REVERSE, SKIP, WILD, DRAW_4 };

    public class Card
    {
        //------------------------------
        // Variables
        //------------------------------             

        // The global scale factor for all card images
        internal static double cardScale = 0.75;

        internal Image image;

        public COLOR color;
        public CARD value;

        //------------------------------
        // Functions
        //------------------------------

        public Card(string path)
        {
            image = Shared.LoadImage(path, 100 * cardScale, 150 * cardScale);

            // Pull color & value from the file name
            string name = Path.GetFileNameWithoutExtension(path);
            LoadType(name);            
        }

        void LoadType(string name)
        {
            if (name == "wild" || name == "draw4")
            {
                color = COLOR.WILD;
                
                if (name == "draw4")
                    value = CARD.DRAW_4;
                else
                    value = CARD.WILD;
            }
            else
            {
                var pair = name.Split('_');

                if (pair.Length == 2)
                {
                    // Color
                    switch (pair[0])
                    {
                        case "blue":
                            color = COLOR.BLUE; break;
                        case "green":
                            color = COLOR.GREEN; break;
                        case "red":
                            color = COLOR.RED; break;
                        case "yellow":
                            color = COLOR.YELLOW; break;
                        default:
                            throw new Exception("Unexpected card color!");
                    }

                    // Value
                    switch (pair[1])
                    {
                        case "draw2":
                            value = CARD.DRAW_2; break;
                        case "reverse":
                            value = CARD.REVERSE; break;
                        case "skip":
                            value = CARD.SKIP; break;
                        default:
                            int n;

                            if (int.TryParse(pair[1], out n))
                                value = (CARD)n;
                            else
                                throw new Exception("Unexpected card value!");

                            break;
                    }
                }
                else
                    throw new Exception("Unexpected card name!");
            }
        }
    }
}
