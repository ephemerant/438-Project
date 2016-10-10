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

namespace UNO_WPF
{
    public partial class MainWindow : Window
    {
        string imagesPath;

        // The image (card) currently being dragged by the mouse
        Image draggedImage;
        // The mouse's last position, used to prevent "jumping" during image dragging
        Point mousePosition;
        // The global scale factor of all cards
        double cardScale = 0.75;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // first path is for general distribution, second is for when debugging
            string[] possibleDirectories = { @"resources\cards", @"..\..\resources\cards" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    imagesPath = Path.GetFullPath(dir);
                    break;
                }

            string[] cards = new string[] { "blue_0.png", "red_5.png", "wild.png", "green_reverse.png", "yellow_skip.png" };

            int offSet = 0;

            foreach (var cardName in cards)
            {
                // For now, display each card on top of the previous, shifted down and to the right by 30px in the X and Y directions
                offSet += 30;

                // Load the card's image
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(Path.Combine(imagesPath, cardName));
                src.EndInit();

                Image i = new Image { Source = src, Width = 100 * cardScale, Height = 150 * cardScale };

                // Position the card on the screen
                Canvas.SetLeft(i, offSet);
                Canvas.SetTop(i, offSet);

                // Display the card by adding it to the canvas
                canvas.Children.Add(i);
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

            draggedImage.Width = 100 * cardScale * scale;
            draggedImage.Height = 150 * cardScale * scale;

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
