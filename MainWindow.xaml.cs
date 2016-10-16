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
    public partial class MainWindow : Window
    {
        string imagesPath;

        // The image (card) currently being dragged by the mouse
        Image draggedImage;
        // The mouse's last position, used to prevent "jumping" during image dragging
        Point mousePosition;

        Dealer dealer;
        Player player;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dealer = new Dealer();
            player = new Player();

            // First path is for general distribution, second is for when debugging
            string[] possibleDirectories = { @"resources\cards", @"..\..\resources\cards" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    imagesPath = Path.GetFullPath(dir);
                    break;
                }

            foreach (var path in Directory.GetFiles(imagesPath))
            {
                // Two of each card
                for (var i = 1; i <= 2; ++i)
                    dealer.AddToDeck(new Card(path));
            }

            dealer.Shuffle();
            dealer.Deal(player, 7);

            int offset = 33;

            foreach (var card in player.hand)
            {
                Canvas.SetTop(card.image, 385);
                Canvas.SetLeft(card.image, offset);

                canvas.Children.Add(card.image);

                offset += 90;
            }

            var names = new string[] { "Player 1", "Dr. Doyle", "Morpheus", "Terminator", "Citizen", "Kane", "Will Smith", "Player 8", "Iron Maiden", "Final Boss" };

            offset = 0;

            var rng = new Random();

            foreach (var name in names)
            {
                var labelName = new Label { Content = name, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(labelName, offset);
                Canvas.SetLeft(labelName, 10);
                players.Children.Add(labelName);

                int numCards = rng.Next(1, 8); // upper bound non-inclusive

                var labelCards = new Label { Content = string.Format("{0} card{1}", numCards, numCards > 1 ? "s" : ""), Foreground = Brushes.White, FontSize = 14 };
                Canvas.SetTop(labelCards, offset + 25);
                Canvas.SetLeft(labelCards, 10);
                players.Children.Add(labelCards);

                offset += 50;
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

            BringToFront(draggedImage);

            // The coordinates of the image's center, post-scale
            double scaledCenterX = Canvas.GetLeft(draggedImage) + draggedImage.Width / 2;
            double scaledCenterY = Canvas.GetTop(draggedImage) + draggedImage.Height / 2;

            // Use relative offset in combination with the new width and height
            mousePosition.X = scaledCenterX - draggedImage.Width * relativeOffsetX;
            mousePosition.Y = scaledCenterY - draggedImage.Height * relativeOffsetY;
        }

        void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Was something clicked?
            if (e.Source != null && canvas.CaptureMouse())
            {
                // Begin drag                
                mousePosition = e.GetPosition(canvas);
                draggedImage = (Image)e.Source;

                // Make dragged image larger
                ScaleDraggedImage(1.2);

                // Redraw with updated coordinates
                CanvasMouseMove(sender, e);
            }
        }

        void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedImage != null)
            {
                // End drag
                canvas.ReleaseMouseCapture();

                // Reset dragged image's size
                ScaleDraggedImage(1);

                // Redraw with updated coordinates
                CanvasMouseMove(sender, e);

                draggedImage = null;
            }
        }

        void CanvasMouseMove(object sender, MouseEventArgs e)
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

        void BringToFront(Image image)
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
