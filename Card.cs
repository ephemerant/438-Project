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
    enum COLOR { BLUE, GREEN, RED, YELLOW, WILD };
    enum CARD { ZERO, ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, DRAW_2, REVERSE, SKIP, WILD };

    class Card
    {
        // The global scale factor for all card images
        internal static double cardScale = 0.75;

        internal Image image;

        COLOR color;
        CARD value;

        public Card(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            // Pull color & value from the file name
            LoadType(name);

            // Load the image
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(path);
            src.EndInit();

            image = new Image { Source = src, Width = 100 * cardScale, Height = 150 * cardScale };
        }

        void LoadType(string name)
        {
            if (name == "wild" || name == "draw4")
            {
                color = COLOR.WILD;
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
