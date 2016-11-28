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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace UNO
{
    public partial class Game : Screen
    {
        //------------------------------
        // Variables
        //------------------------------

        // The image (card) currently being dragged by the mouse
        Image draggedImage;

        // The mouse's last position, used to prevent "jumping" during image dragging
        Point mousePosition;

        int draggedOffset; // This will keep track of where a card should return to if it is to be put back in players inventory

        Card draggedCard; // The value of the card being dragged
        Card currentCard; // The current card in play

        bool clickedDraw = false; //this is used to determine if player pressed down on deck

        Dealer dealer;

        public Player player; // The actual player
        Player currentPlayer; // The player whose turn it is
        int currentPlayerNumber = 0;//the number of the current player.
        bool turnsReversed = false;

        MainWindow window;
        bool isClient = false;

        List<Card> cardsByID;

        int turnCount;
        int seed;

        //------------------------------
        // Functions
        //------------------------------

        public void Unload()
        {
            window.canvas.Children.Clear();
            window.hand.Children.Clear();
            window.arrows.Clear();
            window.players.Children.Clear();
            window.playerList.Clear();
            window.inPlay.Children.Clear();
        }

        public void Load(MainWindow window, Message message = null)
        {
            this.window = window;
            turnCount = 0;

            // Arrows
            var arrow = Shared.LoadImage(Path.Combine(window.resourcesPath, "arrow-right.png"), 25, 45);
            arrow.Opacity = 0.5;
            Canvas.SetTop(arrow, 45);
            Canvas.SetRight(arrow, 10);
            window.hand.Children.Add(arrow);
            window.arrows.Add(arrow); // right arrow

            arrow.MouseEnter += ArrowBeginHover;
            arrow.MouseLeftButtonUp += RightArrowLeftButtonUp;
            arrow.MouseLeave += ArrowEndHover;

            arrow = Shared.LoadImage(Path.Combine(window.resourcesPath, "arrow-left.png"), 25, 45);
            arrow.Opacity = 0.5;
            Canvas.SetTop(arrow, 45);
            Canvas.SetLeft(arrow, 10);
            window.hand.Children.Add(arrow);
            window.arrows.Add(arrow); // left arrow

            arrow.MouseEnter += ArrowBeginHover;
            arrow.MouseLeftButtonUp += LeftArrowLeftButtonUp;
            arrow.MouseLeave += ArrowEndHover;

            draggedOffset = 0;

            window.players.Visibility = Visibility.Visible;
            window.hand.Visibility = Visibility.Visible;
            window.inPlay.Visibility = Visibility.Visible;
            window.DrawDeck.Visibility = Visibility.Visible;
            window.turnDirection.Visibility = Visibility.Visible;

            dealer = new Dealer();

            cardsByID = new List<Card>();

            foreach (var path in Directory.GetFiles(window.imagesPath))
            {
                // Create two of each card and store them
                for (var i = 0; i <= 1; ++i)
                {
                    Card tempcard = new Card(path);
                    tempcard.ID = cardsByID.Count;
                    cardsByID.Add(tempcard);
                    dealer.AddToDeck(tempcard);
                }
            }

            if (message == null)
            {
                var rng = new Random();

                seed = rng.Next();
            }
            else
            {
                seed = int.Parse(message.Extra);
            }

            Dealer.rng = new Random(seed);
            dealer.Shuffle();

            if (message == null)
                LoadHost();
            else
                LoadClient(message);
        }

        public void LoadClient(Message message)
        {
            isClient = true;

            window.playerList = message.PlayerList;

            // Load players
            int offset = 0;

            foreach (Player thisplayer in window.playerList)
            {
                var labelName = new Label { Content = thisplayer.name, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(labelName, offset);
                Canvas.SetLeft(labelName, 10);
                window.players.Children.Add(labelName);

                thisplayer.labelName = labelName;

                thisplayer.hand = Shared.Unstrip(thisplayer.hand, cardsByID);

                var labelCards = new Label { Foreground = Brushes.White, FontSize = 14 };

                Canvas.SetTop(labelCards, offset + 25);
                Canvas.SetLeft(labelCards, 10);
                window.players.Children.Add(labelCards);

                thisplayer.labelCards = labelCards;
                thisplayer.UpdateLabel();

                thisplayer.IsActive(false); // Dim their labels

                offset += 50;
            }

            turnsReversed = false;

            // Load the current card
            currentCard = Shared.Unstrip(message.Card, cardsByID);

            currentPlayerNumber = 0;
            currentPlayer = window.playerList[0];
            currentPlayer.IsActive(true); // Brighten their labels            

            loadCurrentCard();

            // Determine which player we are
            player = Shared.GetPlayer(window.UserID, window.playerList);

            reloadHand();

            // listen for moves
            var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForMove));
            threadListen.Start();
        }

        public void LoadHost()
        {
            isClient = false;

            // Load players
            int offset = 0;

            foreach (Player thisplayer in window.playerList)
            {
                var labelName = new Label { Content = thisplayer.name, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(labelName, offset);
                Canvas.SetLeft(labelName, 10);
                window.players.Children.Add(labelName);

                thisplayer.labelName = labelName;

                dealer.Deal(thisplayer, 7);

                var labelCards = new Label { Foreground = Brushes.White, FontSize = 14 };

                Canvas.SetTop(labelCards, offset + 25);
                Canvas.SetLeft(labelCards, 10);
                window.players.Children.Add(labelCards);

                thisplayer.labelCards = labelCards;
                thisplayer.UpdateLabel();

                thisplayer.IsActive(false); // Dim their labels

                offset += 50;
            }

            turnsReversed = false;

            currentCard = dealer.Deal();

            currentPlayerNumber = 0;
            currentPlayer = window.playerList[0];
            currentPlayer.isComputer = false; // Keep us human

            player = currentPlayer;
            player.IsActive(true); // Brighten their labels

            loadCurrentCard();

            reloadHand();

            // let players know that we've started
            if (window.lobby.HostID == window.UserID)
                window.udpConnect.SendMessage(new Message { HostID = window.UserID, Action = "begin", PlayerName = window.lobby.clientName, PlayerList = Shared.Strip(window.playerList), Card = Shared.Strip(currentCard), Extra = seed.ToString() });
        }

        //public void BroadcastBegin()
        //{
        //    var oldTimeout = window.udpConnect.udpResponse.Client.ReceiveTimeout;
        //    window.udpConnect.udpResponse.Client.ReceiveTimeout = 50;

        //    var responses = new Dictionary<string, bool>();

        //    foreach (var player in window.playerList)
        //        if (player.IP != null)
        //            responses.Add(player.ID, false);

        //    if (responses.Count > 0)
        //    {
        //        var counter = 0;

        //        window.udpConnect.SendMessage(new Message { HostID = window.UserID, Action = "begin", PlayerName = window.lobby.clientName, PlayerList = Shared.Strip(window.playerList), Card = Shared.Strip(currentCard), Extra = seed.ToString() });

        //        while (counter <= 3)
        //        {
        //            try
        //            {
        //                IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

        //                var data = window.udpConnect.udpResponse.Receive(ref recvEp);

        //                var text = Encoding.ASCII.GetString(data);

        //                var message = new Message(text);

        //                if (message.Action == "beginAck")
        //                {
        //                    responses[message.PlayerID] = true;
        //                    window.udpConnect.SendMessage(new Message { HostID = window.UserID, Action = "beginAckAck" });
        //                }
        //            }
        //            catch
        //            {
        //                if (++counter <= 3)
        //                    window.udpConnect.SendMessage(new Message { HostID = window.UserID, Action = "begin", PlayerName = window.lobby.clientName, PlayerList = Shared.Strip(window.playerList), Card = Shared.Strip(currentCard), Extra = seed.ToString() });
        //            }
        //        }
        //    }

        //    // kick anyone who hasn't repsonded
        //    foreach (var player in window.playerList)
        //        if (player.IP != null)
        //            heardFromAll = heardFromAll && (responses[player.ID]);

        //    window.udpConnect.udpResponse.Client.ReceiveTimeout = oldTimeout;
        //}

        // A key was pressed
        public override void KeyUpHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D: // Scroll to the right
                    RightArrowLeftButtonUp(null, null); break;
                case Key.A: // Scroll to the left
                    LeftArrowLeftButtonUp(null, null); break;
                case Key.S: // Draw a card
                    DrawDeckLeftButtonDown(null, null);
                    DrawDeckLeftButtonUp(null, null);
                    break;
            }
        }

        private void loadCurrentCard()
        {
            Canvas.SetLeft(currentCard.image, 85); // Center within inPlay
            Canvas.SetTop(currentCard.image, 60); // TODO: Use variable numbers instead of fixed

            window.inPlay.Children.Add(currentCard.image);
        }

        private void ArrowEndHover(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var image = (Image)e.Source;
                image.Opacity = 0.5;
            }
        }

        private void ArrowBeginHover(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var image = (Image)e.Source;
                image.Opacity = 0.9;
            }
        }

        // Scroll to the right
        void RightArrowLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (player.handOffset + 7 < player.hand.Count)
            {
                player.handOffset += 1;
                reloadHand();
            }
        }

        // Scroll to the left
        void LeftArrowLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (player.handOffset > 0)
            {
                player.handOffset -= 1;
                reloadHand();
            }
        }

        /// <summary>
        /// Scales "draggedImage" based on "scale", while also adjusting "mousePosition" to be in the same relative position
        /// </summary>
        /// <param name="scale">A value greater than 1 will grow the image, a value of 1 will reset its size, and a value less than 1 will shrink it</param>
        void ScaleDraggedImage(double scale)
        {
            // The coordinates of the image's center, pre-scale
            double centerX = Canvas.GetLeft(draggedImage) + draggedImage.Width / 2;
            double centerY = Canvas.GetTop(draggedImage) + draggedImage.Height / 2;

            // Relative offset of the mouse from the center
            double relativeOffsetX = (centerX - mousePosition.X) / draggedImage.Width;
            double relativeOffsetY = (centerY - mousePosition.Y) / draggedImage.Height;

            draggedImage.Width = 100 * Card.cardScale * scale;
            draggedImage.Height = 150 * Card.cardScale * scale;

            BringToFront(window.canvas, draggedImage);

            // The coordinates of the image's center, post-scale
            double scaledCenterX = Canvas.GetLeft(draggedImage) + draggedImage.Width / 2;
            double scaledCenterY = Canvas.GetTop(draggedImage) + draggedImage.Height / 2;

            // Use relative offset in combination with the new width and height
            mousePosition.X = scaledCenterX - draggedImage.Width * relativeOffsetX;
            mousePosition.Y = scaledCenterY - draggedImage.Height * relativeOffsetY;
        }

        public override void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePosition = e.GetPosition(window.canvas);

            // Was a card clicked?
            if (player == currentPlayer && mousePosition.Y > 369 && e.Source != null && window.canvas.CaptureMouse())
            {
                // Begin drag                
                draggedImage = (Image)e.Source;

                // Make dragged image larger
                ScaleDraggedImage(1.2);

                // Redraw with updated coordinates
                CanvasMouseMove(sender, e);
                if (mousePosition.Y > 369 && mousePosition.X < 645 && mousePosition.X >= 50)//if mouse if inside the 'hand' canvas
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (mousePosition.X >= 50 + 85 * i && mousePosition.X < 135 + 85 * i) // if mouse is dragging the (i+1)th card
                        {
                            draggedOffset = i;
                            draggedCard = player.hand[player.handOffset + draggedOffset];
                        }
                    }
                }
            }
        }

        public override void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedImage != null)
            {
                // End drag
                window.canvas.ReleaseMouseCapture();

                // Reset dragged image's size
                ScaleDraggedImage(1);

                var point = e.GetPosition(window.canvas);

                if (pointWithinBounds(point))
                {
                    bool result = isValidPlay(draggedCard); // Check to see if card play is valid

                    if (result) // The card can be played; play it and move to next player.
                    {
                        playCard(draggedCard, true);
                    }
                    else // Card cannot be played, move card back to hand
                    {
                        // Redraw with updated coordinates
                        CanvasMouseMove(sender, e);
                        // Return to hand
                        returnImageToHand();
                    }
                }
                else
                {
                    returnImageToHand();
                }

                draggedImage = null;
            }
        }

        public void Process(Message message)
        {
            // if it's a begin message or from another host, or if the turn is out of order, discard it
            if (message.Action == "begin" || message.HostID != window.lobby.HostID || int.Parse(message.TurnCount) <= turnCount)
            {
                // Wait for the remote player to make a move                
                var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForMove));
                threadListen.Start();

                return;
            }

            turnCount = int.Parse(message.TurnCount);

            if (message.Action == "draw")
            {
                dealer.Deal(currentPlayer, 1);
                currentPlayer.UpdateLabel();

                // Wait for the remote player to make a move                
                var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForMove));
                threadListen.Start();
            }
            else if (message.Action == "card_played")
            {
                playCard(Shared.Unstrip(message.Card, cardsByID), false);
            }
        }

        internal void playCard(Card card, bool propagate)
        {
            // Add the previous card played back to the deck
            // TODO: It would probably be optimal to only do this after the deck runs out, so all cards will be drawn throughout a deck's life
            window.inPlay.Children.Remove(currentCard.image);
            dealer.AddToDeck(currentCard, true);

            // Play the card
            currentCard = card;
            currentPlayer.hand.Remove(card); // Remove it from the hand

            window.canvas.Children.Remove(currentCard.image); // Remove it from the playable canvas
            loadCurrentCard(); // Add it to the played canvas

            BringToFront(window.inPlay, currentCard.image); // Make sure it is on top

            if (currentPlayer.handOffset + 7 >= currentPlayer.hand.Count && currentPlayer.handOffset > 0) // Decrease the hand offset if necessary
                currentPlayer.handOffset--;

            currentPlayer.UpdateLabel();

            if (winCheck())
            {
                MessageBox.Show(string.Format("{0} wins!", currentPlayer.name));
                // Go back to the menu screen
                window.unloadMainScreen();
                window.Window_Loaded(null, null);
                return;
            }

            // Handle skip & reverse
            switch (card.value)
            {
                case CARD.REVERSE:
                    turnsReversed = !turnsReversed;
                    if (turnsReversed)
                    {
                        window.turnDirectionReverse.Visibility = Visibility.Visible;
                        window.turnDirection.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        window.turnDirection.Visibility = Visibility.Visible;
                        window.turnDirectionReverse.Visibility = Visibility.Hidden;
                    }
                    break;
                case CARD.SKIP:
                    nextPlayer(); break;
            }

            nextPlayer(); // Move to the next player                                                        

            // Handle draw 2 & draw 4
            switch (card.value)
            {
                case CARD.DRAW_2:
                    dealer.Deal(currentPlayer, 2); break;
                case CARD.DRAW_4:
                    dealer.Deal(currentPlayer, 4); break;
            }

            reloadHand(); // Refresh the hand     

            if (propagate)
                BroadcastMove("card_played", currentPlayer, card);

            if (currentPlayer.isComputer)
            {
                currentPlayer.UpdateLabel();

                while (canDraw(currentPlayer)) // Draw until the computer can make a move
                {
                    BroadcastMove("draw", currentPlayer);

                    window.Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate); // Refresh the UI
                    Thread.Sleep(250);

                    dealer.Deal(currentPlayer, 1);
                    currentPlayer.UpdateLabel();
                }

                window.Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate); // Refresh the UI
                Thread.Sleep(250);

                foreach (var playableCard in currentPlayer.hand)
                    if (isValidPlay(playableCard))
                    {
                        // Play the card
                        playCard(playableCard, true);
                        break;
                    }
            }
            else if (currentPlayer.IP != player.IP)
            {
                // Wait for the remote player to make a move                
                var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForMove));
                threadListen.Start();
            }
        }

        private void BroadcastMove(string action, Player player, Card card = null)
        {
            turnCount += 1;

            // player: The player whose turn it now is
            window.udpConnect.SendMessage(new Message { HostID = window.lobby.HostID, Action = action, PlayerName = player.ID, PlayerList = Shared.Strip(window.playerList), Card = Shared.Strip(card), TurnCount = turnCount.ToString(), Extra = turnCount.ToString() });
        }

        private bool pointWithinBounds(Point point)
        {
            double width = window.Width - window.inPlay.Margin.Right - window.inPlay.Margin.Left;
            double height = window.Height - window.inPlay.Margin.Bottom - window.inPlay.Margin.Top;

            return point.X > window.inPlay.Margin.Left && point.X < window.inPlay.Margin.Left + width && point.Y > window.inPlay.Margin.Top && point.Y < window.inPlay.Margin.Top + height;
        }

        private void nextPlayer()
        {
            currentPlayer.IsActive(false);

            if (turnsReversed == false) // Determine the next player
            {
                if (++currentPlayerNumber >= window.playerList.Count)
                    currentPlayerNumber = 0;
            }
            else if (--currentPlayerNumber < 0)
                currentPlayerNumber = window.playerList.Count - 1;

            currentPlayer = window.playerList[currentPlayerNumber];
            currentPlayer.IsActive(true);

            if (!currentPlayer.isComputer && currentPlayer.IP == player.IP)
                player = currentPlayer; // Shared computer
        }

        private bool winCheck()
        {
            return currentPlayer.hand.Count == 0;
        }

        void returnImageToHand()
        {
            Canvas.SetTop(draggedImage, 385);
            Canvas.SetLeft(draggedImage, 50 + (draggedOffset * 85));
        }

        public override void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedImage != null)
            {
                var position = e.GetPosition(window.canvas);
                var offset = position - mousePosition;

                mousePosition = position;

                // Move the image
                Canvas.SetLeft(draggedImage, Canvas.GetLeft(draggedImage) + offset.X);
                Canvas.SetTop(draggedImage, Canvas.GetTop(draggedImage) + offset.Y);
            }
        }

        public override void DrawDeckLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (canDraw())
            {
                clickedDraw = true;
            }
        }

        private bool canDraw(Player player = null)
        {
            if (player == null)
                player = this.player;

            if (player != currentPlayer)
                return false;

            foreach (var card in player.hand)
                if (isValidPlay(card))
                    return false;

            return true;
        }

        public override void DrawDeckLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (clickedDraw == true && canDraw())
            {
                BroadcastMove("draw", currentPlayer);

                dealer.Deal(player, 1);
                clickedDraw = false;

                // Scroll so we can see the last card
                if (player.hand.Count > 7)
                    player.handOffset = player.hand.Count - 7;

                reloadHand();
            }
        }

        bool isValidPlay(Card card)
        {
            return card.value == currentCard.value || card.color == currentCard.color || card.color == COLOR.WILD || currentCard.color == COLOR.WILD;
        }

        void reloadHand()
        {
            Console.WriteLine("\n" + currentPlayer.name);
            foreach (var card in currentPlayer.hand)
                Console.WriteLine(card);
            Console.Write("\n");

            // Unload all the shown cards and reload the cards in the hand
            window.canvas.Children.Clear();

            int counter = 0;
            int offset = 50;

            while (counter < 7 && player.handOffset + counter < player.hand.Count)
            {
                var card = player.hand[player.handOffset + counter];
                Image placeCard = card.image;

                if (player == currentPlayer && isValidPlay(card))
                    card.image.Opacity = 1;
                else
                    card.image.Opacity = 0.5;

                Canvas.SetTop(placeCard, 385);
                Canvas.SetLeft(placeCard, offset);

                window.canvas.Children.Add(placeCard);

                offset += 85;
                counter++;
            }

            if (player == currentPlayer && canDraw())
                window.DrawDeck.Opacity = 1;
            else
                window.DrawDeck.Opacity = 0.5;

            if (window.arrows.Count > 0)
            {
                // Hide right arrow
                if (player.handOffset + 7 >= player.hand.Count)
                    window.arrows[0].Visibility = Visibility.Hidden;
                else
                    window.arrows[0].Visibility = Visibility.Visible;

                // Hide left arrow
                if (player.handOffset <= 0)
                    window.arrows[1].Visibility = Visibility.Hidden;
                else
                    window.arrows[1].Visibility = Visibility.Visible;
            }

            foreach (var player in window.playerList)
                player.UpdateLabel();
        }

        void BringToFront(Canvas canvas, Image image)
        {
            // Move the image to the end of the children
            canvas.Children.Remove(image);
            canvas.Children.Add(image);

            int i = 0;

            // Set Z-index based on position in Children
            foreach (UIElement c in canvas.Children)
                Panel.SetZIndex(c, i++);
        }

        bool sendTurnInfo()
        {
            //window.network.SendMove(currentCard.value, currentCard.color, player.hand.Count);
            return true;// this will return whether the send was successful, for now just default to true
        }
    }
}
