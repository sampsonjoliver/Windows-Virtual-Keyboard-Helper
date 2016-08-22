/* References
 * global mouse coords: stackoverflow.com/questions/4226740/how-do-i-get-the-current-mouse-screen-coordinates-in-wpf
 * virtual keyboard start/close: http://stackoverflow.com/questions/16601424/after-killing-the-process-for-tabletkeyboardtabtip-exe-application-doesnt-bri
 * 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace KeyboardTestApp.Input
{
    /// <summary>
    /// Helper library for managing user input and interactions, such as the virtual keyboard and touch injection.
    /// </summary>
    public static class InputHelper
    {
        private static VirtualKeyboard m_virtualKeyboard;
        private static MouseHookListener m_mouseHookManager;
        private static TouchInject m_touchInjector;

        /// <summary>
        /// Initialise the Input Helper's static variables.
        /// </summary>
        public static void Initialise()
        {
            m_virtualKeyboard = new VirtualKeyboard();
            m_mouseHookManager = new MouseHookListener();
            //m_touchInjector = new TouchInject();
        }

        /// <summary>
        /// Open the virtual keyboard.
        /// </summary>
        public static void LaunchKeyboard()
        {
            m_virtualKeyboard.Launch();
        }

        /// <summary>
        /// Close the virtual keyboard.
        /// </summary>
        public static void CloseKeyboard()
        {
            m_virtualKeyboard.Close();
        }

        /// <summary>
        /// Captures mouse events and replaces them with touch events
        /// </summary>
        public static void EnableTouchInjection()
        {
            m_mouseHookManager.MouseDown += ReplaceWithTouch;
        }

        private static async void ReplaceWithTouch(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("Mouse left click: " + e.GetPosition(this).ToString());
            e.Handled = true;
            await m_touchInjector.SimulateTouch();
            //await m_touchInjector.SimulateTouch();
        }

        public static void RegisterMouseMoveEvent(MouseEventHandler e)
        {
            m_mouseHookManager.MouseMove += e;
        }
        public static void RegisterMouseDownEvent(MouseButtonEventHandler e)
        {
            m_mouseHookManager.MouseDown += e;
        }
        public static void RegisterMouseUpEvent(MouseButtonEventHandler e)
        {
            m_mouseHookManager.MouseUp += e;
        }
    }
}
