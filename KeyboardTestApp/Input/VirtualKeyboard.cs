using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardTestApp.Input
{
    class VirtualKeyboard
    {
        // TODO: externalise this info to a config file
        private ProcessStartInfo virtualKeyboardInfo = new ProcessStartInfo(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
        private const String virtualKeyboardWindowName = "IPTip_Main_Window";

        private const uint WM_SYSCOMMAND = 0x0112;  // 274
        private const uint SC_CLOSE = 0xF060;       // 61536

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindow(String sClassName, String sAppName);

        internal VirtualKeyboard()
        {
            this.Initialise();
        }

        public void Initialise()
        {
            virtualKeyboardInfo.WindowStyle = ProcessWindowStyle.Hidden;
        }

        internal void Launch()
        {
            Process.Start(virtualKeyboardInfo);
        }

        internal void Close()
        {
            /* This is necessary over simply killing the Process vkb var, as TabTip.exe will automatically get consumed into
            * a system process upon being started in Launch Keyboard. Hence, vkb will be marked as Exited and will hence not
            * refer to any existing process.
           */
            IntPtr KeyboardWnd = FindWindow(virtualKeyboardWindowName, null);

            if (KeyboardWnd != null)
                PostMessage(KeyboardWnd.ToInt32(), WM_SYSCOMMAND, (int)SC_CLOSE, 0);
        }
    }
}
