using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UNO
{
    class Player
    {
        internal List<Card> hand = new List<Card>();

        public Label labelName;
        public Label labelCards;
        public string name;

        public Player(string name)
        {
            this.name = name;
        }

        internal void AddToHand(Card card)
        {
            hand.Add(card);
        }

        internal void IsActive(bool active)
        {
            labelName.Opacity = active ? 1 : 0.6;
            labelCards.Opacity = active ? 1 : 0.6;
        }
    }
}
