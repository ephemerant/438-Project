using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace UNO
{
    public class Screen
    {
        //------------------------------
        // Variables
        //------------------------------

        public Action emptyDelegate = delegate () { };

        //------------------------------
        // Functions
        //------------------------------

        public virtual void KeyUpHandler(object sender, KeyEventArgs e)
        {

        }

        // Main menu buttons
        public void ButtonEndHover(object sender, MouseEventArgs e)
        {
            var image = (Image)e.Source;
            image.Width /= 1.1;
            image.Height /= 1.1;
        }

        // Main menu buttons
        public void ButtonBeginHover(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var image = (Image)e.Source;
                image.Width *= 1.1;
                image.Height *= 1.1;
            }
        }

        public virtual void DrawDeckLeftButtonDown(object sender, MouseEventArgs e)
        {

        }

        public virtual void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        public virtual void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        public virtual void CanvasMouseMove(object sender, MouseEventArgs e)
        {

        }

        public virtual void DrawDeckLeftButtonUp(object sender, MouseEventArgs e)
        {

        }
    }
}
