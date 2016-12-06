using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DBPSim.SimulationGUI.ModelDrawer
{
    public class SimulationModelInstanceElementHelper
    {

        public static object GetStartElement()
        {
            Border border = new Border();
            border.Width = 15;
            border.Height = 15;
            border.CornerRadius = new CornerRadius(80);
            border.Background = Brushes.Black;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);

            return border;
        }


        public static object GetActionElement(string title)
        {
            Border border = new Border();

            border.Height = 30;
            border.MinWidth = 80;
            border.CornerRadius = new CornerRadius(8);
            border.Background = Brushes.Yellow;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);

            TextBlock text = new TextBlock();
            text.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            text.Margin = new Thickness(3, 0, 3, 0);
            text.Text = title;
            text.Foreground = Brushes.Black;

            border.Child = text;

            return border;
        }


        public static object GetFinishElement()
        {
            Border border = new Border();
            border.Width = 15;
            border.Height = 15;
            border.CornerRadius = new CornerRadius(80);
            border.Background = Brushes.White;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(2);

            Border borderInside = new Border();
            borderInside.Width = 7;
            borderInside.Height = 7;
            borderInside.CornerRadius = new CornerRadius(80);
            borderInside.Background = Brushes.Black;
            borderInside.BorderBrush = Brushes.Black;
            borderInside.BorderThickness = new Thickness(1);

            border.Child = borderInside;

            return border;
        }

    }
}
