using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UNO
{
    public static class Shared
    {
        public static Image LoadImage(string path, double w, double h)
        {           
            // load the image
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(path);
            src.EndInit();

            return new Image { Source = src, Width = w, Height = h };
        }

        // strip functions are intended to strip away image/label data for JSON serialization
        public static Card Strip(Card card)
        {
            return card == null ? card : new Card { value = card.value, color = card.color };
        }

        public static List<Card> Strip(List<Card> playerHand)
        {
            var cards = new List<Card>();

            foreach (var card in playerHand)
                cards.Add(Strip(card));

            return cards;
        }

        public static List<Player> Strip(List<Player> playerList)
        {
            var players = new List<Player>();

            foreach (var player in playerList)
                players.Add(new Player { name = player.name, IP = player.IP, isComputer = player.isComputer, hand = Strip(player.hand) });

            return players;
        }

        // unstrip functions are intended to restore image/label data
        public static Card Unstrip(Card card, Card[,] clientCards)
        {
            return clientCards[(int)card.color, (int)card.value];
        }

        public static List<Card> Unstrip(List<Card> hand, Card[,] clientCards)
        {
            var cards = new List<Card>();

            foreach (var card in hand)
                cards.Add(Unstrip(card, clientCards));

            return cards;
        }
    }
}
