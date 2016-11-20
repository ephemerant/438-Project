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
using System.Windows.Threading;

namespace UNO
{
    public partial class MainWindow : Window
    {
        string resourcesPath;
        string imagesPath;
        string screen; // The current screen being shown        

        List<Image> menuButtons = new List<Image>();//the list of button images
        List<Image> arrows = new List<Image>();
        List<Player> playerList = new List<Player>();

        // The image (card) currently being dragged by the mouse
        Image draggedImage;

        // The mouse's last position, used to prevent "jumping" during image dragging
        Point mousePosition;

        int draggedOffset; // This will keep track of where a card should return to if it is to be put back in players inventory

        Card draggedCard; // The value of the card being dragged
        Card currentCard; // The current card in play

        bool clickedDraw = false;//this is used to determine if player pressed down on deck

        Dealer dealer;

        Player player; // The actual player
        Player currentPlayer; // The player whose turn it is
        int numberOfPlayers = 10;
        int currentPlayerNumber = 0;//the number of the current player.
        bool turnsReversed = false;

        Action emptyDelegate = delegate () { };

        public MainWindow()
        {
            InitializeComponent();

            Keyboard.AddKeyUpHandler(this, KeyUpHandler);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (screen == "Main") // In-game inputs
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
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            screen = "Menu";

            players.Visibility = Visibility.Hidden;
            hand.Visibility = Visibility.Hidden;
            inPlay.Visibility = Visibility.Hidden;
            DrawDeck.Visibility = Visibility.Hidden;
            turnDirection.Visibility = Visibility.Hidden;
            turnDirectionReverse.Visibility = Visibility.Hidden;

            dealer = new Dealer();

            draggedOffset = 0;

            // First path is for general distribution, second is for when debugging
            string[] possibleDirectories = { @"resources", @"..\..\resources" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    resourcesPath = Path.GetFullPath(dir);
                    imagesPath = Path.Combine(resourcesPath, "cards");
                    break;
                }

            foreach (var path in Directory.GetFiles(imagesPath))
            {
                // Create two of each card
                for (var i = 1; i <= 2; ++i)
                {
                    Card tempcard = new Card(path);
                    dealer.AddToDeck(tempcard);
                }
            }

            //load host button
            var hostButton = Shared.LoadImage(Path.Combine(resourcesPath, "hostGame.png"), 395, 81);
            Canvas.SetTop(hostButton, 160);
            Canvas.SetLeft(hostButton, 200);
            canvas.Children.Add(hostButton);
            menuButtons.Add(hostButton);

            hostButton.MouseLeftButtonUp += hostButtonClick;
            hostButton.MouseEnter += ButtonBeginHover;
            hostButton.MouseLeave += ButtonEndHover;

            //load join button
            var joinButton = Shared.LoadImage(Path.Combine(resourcesPath, "joinGame.png"), 395, 85);
            Canvas.SetTop(joinButton, 250);
            Canvas.SetLeft(joinButton, 200);
            canvas.Children.Add(joinButton);
            menuButtons.Add(joinButton);

            joinButton.MouseLeftButtonUp += joinButtonClick;
            joinButton.MouseEnter += ButtonBeginHover;
            joinButton.MouseLeave += ButtonEndHover;

            //load quit button
            var quitButton = Shared.LoadImage(Path.Combine(resourcesPath, "quit.png"), 166, 87);
            Canvas.SetTop(quitButton, 340);
            Canvas.SetLeft(quitButton, 200);
            canvas.Children.Add(quitButton);
            menuButtons.Add(quitButton);

            quitButton.MouseLeftButtonUp += quitButtonClick;
            quitButton.MouseEnter += ButtonBeginHover;
            quitButton.MouseLeave += ButtonEndHover;
        }

        private void unloadMenuScreen()
        {
            if (menuButtons.Count != 0)
            {
                for (int i = menuButtons.Count - 1; i >= 0; i--)
                {
                    canvas.Children.Remove(menuButtons[i]);
                    menuButtons.RemoveAt(i);
                }
            }
        }

        private void unloadMainScreen()
        {
            canvas.Children.Clear();
            hand.Children.Clear();
            arrows.Clear();
            players.Children.Clear();
            playerList.Clear();
            inPlay.Children.Clear();
        }

        //Open the main screen
        private void StartMainScreen()
        {
            players.Visibility = Visibility.Visible;
            hand.Visibility = Visibility.Visible;
            inPlay.Visibility = Visibility.Visible;
            DrawDeck.Visibility = Visibility.Visible;
            turnDirection.Visibility = Visibility.Visible;

            string[] possibleDirectories = { @"resources", @"..\..\resources" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    resourcesPath = Path.GetFullPath(dir);
                    imagesPath = Path.Combine(resourcesPath, "cards");
                    break;
                }

            dealer.Shuffle();

            // Arrows
            var arrow = Shared.LoadImage(Path.Combine(resourcesPath, "arrow-right.png"), 25, 45);
            arrow.Opacity = 0.5;
            Canvas.SetTop(arrow, 45);
            Canvas.SetRight(arrow, 10);
            hand.Children.Add(arrow);
            arrows.Add(arrow); // right arrow

            arrow.MouseEnter += ArrowBeginHover;
            arrow.MouseLeftButtonUp += RightArrowLeftButtonUp;
            arrow.MouseLeave += ArrowEndHover;

            arrow = Shared.LoadImage(Path.Combine(resourcesPath, "arrow-left.png"), 25, 45);
            arrow.Opacity = 0.5;
            Canvas.SetTop(arrow, 45);
            Canvas.SetLeft(arrow, 10);
            hand.Children.Add(arrow);
            arrows.Add(arrow); // left arrow

            arrow.MouseEnter += ArrowBeginHover;
            arrow.MouseLeftButtonUp += LeftArrowLeftButtonUp;
            arrow.MouseLeave += ArrowEndHover;

            // Simulate players
            var names = new string[] { "Player 1", "Dr. Doyle", "Morpheus", "Terminator", "Citizen", "Kane", "Will Smith", "Player 8", "Iron Maiden", "Final Boss" };

            int offset = 0;

            foreach (var name in names)
            {
                var player = new Player(name);

                player.isComputer = true; // Test by making them all computers

                var labelName = new Label { Content = name, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(labelName, offset);
                Canvas.SetLeft(labelName, 10);
                players.Children.Add(labelName);

                player.labelName = labelName;

                dealer.Deal(player, 7);

                var labelCards = new Label { Foreground = Brushes.White, FontSize = 14 };
                
                Canvas.SetTop(labelCards, offset + 25);
                Canvas.SetLeft(labelCards, 10);
                players.Children.Add(labelCards);

                player.labelCards = labelCards;
                player.UpdateLabel();

                playerList.Add(player);

                player.IsActive(false); // Dim their labels

                offset += 50;
            }

            turnsReversed = false;

            currentCard = dealer.Deal();

            currentPlayerNumber = 0;
            currentPlayer = playerList[0];
            currentPlayer.isComputer = false; // Keep us human

            player = currentPlayer;
            player.IsActive(true); // Brighten their labels

            loadCurrentCard();

            reloadHand();
        }

        private void loadCurrentCard()
        {
            Canvas.SetLeft(currentCard.image, 85); // Center within inPlay
            Canvas.SetTop(currentCard.image, 60); // TODO: Use variable numbers instead of fixed

            inPlay.Children.Add(currentCard.image);
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

            BringToFront(canvas, draggedImage);

            // The coordinates of the image's center, post-scale
            double scaledCenterX = Canvas.GetLeft(draggedImage) + draggedImage.Width / 2;
            double scaledCenterY = Canvas.GetTop(draggedImage) + draggedImage.Height / 2;

            // Use relative offset in combination with the new width and height
            mousePosition.X = scaledCenterX - draggedImage.Width * relativeOffsetX;
            mousePosition.Y = scaledCenterY - draggedImage.Height * relativeOffsetY;
        }

        void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                mousePosition = e.GetPosition(canvas);

                // Was a card clicked?
                if (player == currentPlayer && mousePosition.Y > 369 && e.Source != null && canvas.CaptureMouse())
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
        }

        void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                if (draggedImage != null)
                {
                    // End drag
                    canvas.ReleaseMouseCapture();

                    // Reset dragged image's size
                    ScaleDraggedImage(1);

                    var point = e.GetPosition(this.canvas);

                    if (pointWithinBounds(point))
                    {
                        bool result = isValidPlay(draggedCard); // Check to see if card play is valid

                        if (result) // The card can be played; play it and move to next player.
                        {
                            playCard(draggedCard);
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
        }

        internal void playCard(Card card)
        {
            // Add the previous card played back to the deck
            // TODO: It would probably be optimal to only do this after the deck runs out, so all cards will be drawn throughout a deck's life
            inPlay.Children.Remove(currentCard.image);
            dealer.AddToDeck(currentCard, true);

            // Play the card
            currentCard = card;
            currentPlayer.hand.Remove(card); // Remove it from the hand

            canvas.Children.Remove(currentCard.image); // Remove it from the playable canvas
            loadCurrentCard(); // Add it to the played canvas

            BringToFront(inPlay, currentCard.image); // Make sure it is on top

            if (currentPlayer.handOffset + 7 >= currentPlayer.hand.Count && currentPlayer.handOffset > 0) // Decrease the hand offset if necessary
                currentPlayer.handOffset--;

            currentPlayer.UpdateLabel();

            if (winCheck())
            {
                MessageBox.Show(string.Format("{0} wins!", currentPlayer.name));
                // Go back to the menu screen
                unloadMainScreen();
                Window_Loaded(null, null);
                return;
            }

            // Handle skip & reverse
            switch (card.value)
            {
                case CARD.REVERSE:
                    turnsReversed = !turnsReversed;
                    if (turnsReversed)
                    {
                        turnDirectionReverse.Visibility = Visibility.Visible;
                        turnDirection.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        turnDirection.Visibility = Visibility.Visible;
                        turnDirectionReverse.Visibility = Visibility.Hidden;
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

            if (currentPlayer.isComputer)
            {
                currentPlayer.UpdateLabel();

                while (canDraw(currentPlayer)) // Draw until the computer can make a move
                {
                    Console.WriteLine("Drawing...");

                    Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate); // Refresh the UI
                    System.Threading.Thread.Sleep(250);

                    dealer.Deal(currentPlayer, 1);
                    currentPlayer.UpdateLabel();
                }

                Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate); // Refresh the UI
                System.Threading.Thread.Sleep(250);

                foreach (var playableCard in currentPlayer.hand)
                    if (isValidPlay(playableCard))
                    {
                        Console.WriteLine("{0} played a {1} {2}", currentPlayer.name, playableCard.color, playableCard.value);

                        // Play the card
                        playCard(playableCard);
                        break;
                    }
            }
            else
                Console.WriteLine("Your turn!");
        }


        private bool pointWithinBounds(Point point)
        {
            double width = this.Width - inPlay.Margin.Right - inPlay.Margin.Left;
            double height = this.Height - inPlay.Margin.Bottom - inPlay.Margin.Top;

            return point.X > inPlay.Margin.Left && point.X < inPlay.Margin.Left + width && point.Y > inPlay.Margin.Top && point.Y < inPlay.Margin.Top + height;
        }

        private void nextPlayer()
        {
            currentPlayer.IsActive(false);

            if (turnsReversed == false) // Determine the next player
            {
                if (++currentPlayerNumber >= numberOfPlayers)
                    currentPlayerNumber = 0;
            }
            else if (--currentPlayerNumber < 0)
                currentPlayerNumber = numberOfPlayers - 1;

            currentPlayer = playerList[currentPlayerNumber];
            currentPlayer.IsActive(true);

            if (!currentPlayer.isComputer)
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

        void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                if (draggedImage != null)
                {
                    var position = e.GetPosition(canvas);
                    var offset = position - mousePosition;

                    mousePosition = position;

                    // Move the image
                    Canvas.SetLeft(draggedImage, Canvas.GetLeft(draggedImage) + offset.X);
                    Canvas.SetTop(draggedImage, Canvas.GetTop(draggedImage) + offset.Y);
                }
            }
        }

        void TableMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {

            }
        }
        void TableMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {

            }
        }
        void TableMouseMove(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {

            }
        }

        void DrawDeckLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                if (canDraw())
                {
                    clickedDraw = true;
                }
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

        void DrawDeckLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                if (clickedDraw == true && canDraw())
                {
                    dealer.Deal(player, 1);
                    clickedDraw = false;

                    // Scroll so we can see the last card
                    if (player.hand.Count > 7)
                        player.handOffset = player.hand.Count - 7;

                    reloadHand();
                }
            }
        }

        void DrawDeckMouseMove(object sender, MouseEventArgs e)
        {
            if (screen.Equals("Main"))
            {
                if (canDraw())
                {

                }
            }
        }

        // Main menu buttons
        void ButtonEndHover(object sender, MouseEventArgs e)
        {
            var image = (Image)e.Source;
            image.Width /= 1.1;
            image.Height /= 1.1;
        }

        // Main menu buttons
        void ButtonBeginHover(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var image = (Image)e.Source;
                image.Width *= 1.1;
                image.Height *= 1.1;
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

        // Host a game
        private void hostButtonClick(object sender, MouseEventArgs e)
        {
            unloadMenuScreen();
            StartMainScreen();
            screen = "Main";
        }

        // Join a game
        private void joinButtonClick(object sender, MouseEventArgs e)
        {
            unloadMenuScreen();
            StartMainScreen();
            screen = "Main";
        }

        private void quitButtonClick(object sender, MouseEventArgs e)
        {
            Close();
        }

        bool isValidPlay(Card card)
        {
            return card.value == currentCard.value || card.color == currentCard.color || card.color == COLOR.WILD || currentCard.color == COLOR.WILD;
        }

        void reloadHand()
        {
            // Unload all the shown cards and reload the cards in the hand
            canvas.Children.Clear();

            int counter = 0;
            int offset = 50;

            while (counter < 7 && player.handOffset + counter < player.hand.Count)
            {
                var card = player.hand[player.handOffset + counter];
                Image placeCard = card.image;

                if (isValidPlay(card))
                    card.image.Opacity = 1;
                else
                    card.image.Opacity = 0.5;

                Canvas.SetTop(placeCard, 385);
                Canvas.SetLeft(placeCard, offset);

                canvas.Children.Add(placeCard);

                offset += 85;
                counter++;
            }

            if (canDraw())
                DrawDeck.Opacity = 1;
            else
                DrawDeck.Opacity = 0.5;

            if (arrows.Count > 0)
            {
                // Hide right arrow
                if (player.handOffset + 7 >= player.hand.Count)
                    arrows[0].Visibility = Visibility.Hidden;
                else
                    arrows[0].Visibility = Visibility.Visible;

                // Hide left arrow
                if (player.handOffset <= 0)
                    arrows[1].Visibility = Visibility.Hidden;
                else
                    arrows[1].Visibility = Visibility.Visible;
            }

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
    }
}
