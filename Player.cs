using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNO
{
    class Player
    {
        internal List<Card> hand = new List<Card>();

        internal void AddToHand(Card card)
        {
            hand.Add(card);
        }
    }
}
