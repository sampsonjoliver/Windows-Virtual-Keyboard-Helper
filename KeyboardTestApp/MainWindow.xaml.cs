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
using System.Windows.Shapes;

using KeyboardTestApp.Input;

namespace KeyboardTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InputHelper.Initialise();
            //InputHelper.EnableTouchInjection();
            //InputHelper.RegisterMouseMoveEvent(UpdateLabel);
            //InputHelper.RegisterMouseDownEvent(ClickEvent);

            // Register a click event to open the keyboard on clicking in a textbox
            InputHelper.RegisterMouseUpEvent(ClickEvent);

        }

        private void UpdateLabel(object sender, MouseEventArgs e)
        {
            lbl1.Content = e.GetPosition(this).ToString();
        }
        /// <summary>
        /// Traces the click event to detect what is hit, and opens the virtual keyboard if a textboxview is found, else closes it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickEvent(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse left click: " + this.PointToScreen(e.GetPosition(this)).ToString());

            KeyboardTestApp.Input.Point p = TouchInject.GetMousePosition();
            IInputElement t = this.InputHitTest(e.GetPosition(this));
            // Check for null first to avoid exceptions
            if (t == null)
            {

            }
            // Checks the type of the object hit. The type of the visual component hit is not the same as the type of the class,
            // and is much harder to find without digging through the class properties. The easiest way of getting other types to add
            // to this list is to simply run the program and Console.Writeline the type of what is hit, then copy this to code.
            else if (t.GetType().ToString() == "System.Windows.Controls.TextBoxView")
                InputHelper.LaunchKeyboard();
            else
                InputHelper.CloseKeyboard();
            //e.Handled = true;
        }

        /// <summary>
        /// Manual method of launching the keyboard, requires adding this callback for every textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TouchDown(object sender, TouchEventArgs e)
        {
           // InputHelper.Instance().LaunchKeyboard();
        }

        private void click_LostFocus(object sender, RoutedEventArgs e)
        {
            InputHelper.CloseKeyboard();
        }

        private /*async*/ void click_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //await InputHelper.SimulateTouch();
            
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void click_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }
    }
}
