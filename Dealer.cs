using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNO
{
    class Dealer
    {
        static Random rng = new Random();

        List<Card> deck = new List<Card>();

        internal void AddToDeck(Card card)
        {
            deck.Add(card);
        }

        internal void Shuffle()
        {
            var i = deck.Count;

            while (i-- > 1)
            {
                int k = rng.Next(i + 1);

                Card swapped = deck[k];

                deck[k] = deck[i];
                deck[i] = swapped;
            }
        }

        // Deal n cards to player
        internal void Deal(Player player, int n)
        {
            while (n-- > 0)
            {
                player.AddToHand(deck.Last());
                deck.Remove(deck.Last());
            }
        }
        internal Card Deal()
        {
            Card removeCard = deck.Last();
            deck.Remove(removeCard);
            return removeCard;
        }
    }
}
