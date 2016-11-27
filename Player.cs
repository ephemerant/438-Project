using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UNO
{
    public class Player
    {
        //------------------------------
        // Variables
        //------------------------------

        public List<Card> hand;
        public int CardCount;

        public Label labelName;
        public Label labelCards;

        public string name;

        public int handOffset = 0; // Where in the players 'hand' list the current displayed cards are.

        public bool isComputer = false;        

        public string IP;

        //------------------------------
        // Functions
        //------------------------------

        public Player()
        {

        }

        public Player(string name)
        {
            this.name = name;

            hand = new List<Card>();
        }

        internal void AddToHand(Card card)
        {
            hand.Add(card);
        }

        internal void IsActive(bool active)
        {
            labelName.Opacity = active ? 1 : 0.6;
            labelCards.Opacity = active ? 1 : 0.6;

            UpdateLabel();
        }

        internal void UpdateLabel()
        {
            labelCards.Content = string.Format("{0} card{1}", hand.Count, hand.Count > 1 ? "s" : "");
        }
    }
}
