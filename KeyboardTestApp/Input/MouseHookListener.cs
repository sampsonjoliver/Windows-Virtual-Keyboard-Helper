using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KeyboardTestApp.Input
{
    public class MouseHookListener
    {
        private LowLevelMouseProc _proc;
        private delegate IntPtr LowLevelMouseProc(int nCode, UIntPtr wParam, IntPtr lParam);
        private IntPtr _hookID = IntPtr.Zero;

        private bool enabled;

        private const int WH_MOUSE_LL = 0xE; //14
        const uint MOUSEEVENTF_MASK = 0xFFFFFF00;
        const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        internal event MouseEventHandler MouseMove;
        internal event MouseButtonEventHandler MouseClick;
        internal event MouseButtonEventHandler MouseDown;
        internal event MouseButtonEventHandler MouseUp;
        internal event MouseButtonEventHandler MouseDoubleClick;
        internal event MouseWheelEventHandler MouseWheel;

        [Flags]
        private enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        public MouseHookListener()
        {
            _proc = HandleHookCallback;
            Start();
        }

        public void Start()
        {
            if (!enabled)
            {
                _hookID = SetHook(_proc);
                enabled = true;
            }
        }

        public void Stop()
        {
            if (enabled)
            {
                UnhookWindowsHookEx(_hookID);
                enabled = false;
            }
        }
 
        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            if (hook == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception();
            }
            
            return hook;
            /*
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
            */
        }

        private IntPtr HandleHookCallback(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                bool shouldProcess = ProcessCallback(wParam, lParam);
                if (!shouldProcess)
                    return (IntPtr)(-1);
            }
            /////////////
            /*
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                Console.WriteLine(@"Left clicked: " + hookStruct.pt.x + @", " + hookStruct.pt.y);
                UnhookWindowsHookEx(_hookID);
                mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
                mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
                _hookID = SetHook(_proc);
            }
             * */
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private bool ProcessCallback(UIntPtr wParam, IntPtr lParam)
        {
            //MouseStruct marshalledMouseStruct = (MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct));
            var mouseInfoHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            var extraInfo = (uint)mouseInfoHookStruct.dwExtraInfo.ToInt32();
            if ((extraInfo & MOUSEEVENTF_MASK) == MOUSEEVENTF_FROMTOUCH)
            {
                if ((extraInfo & 0x80) != 0)
                {
                    //Touch Input
                    return true;
                }
                else
                {
                    //Pen Input
                    return true;
                }
            }

            MouseEventArgs e;

            switch ((uint)wParam)
            {
                case Messages.WM_LBUTTONDOWN:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left);
                    e.RoutedEvent = Mouse.MouseDownEvent;

                    ProcessMouseDown((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_LBUTTONUP:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left);
                    e.RoutedEvent = Mouse.MouseUpEvent;

                    ProcessMouseUp((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_LBUTTONDBLCLK:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    ProcessMouseDoubleClick((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_RBUTTONDOWN:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    ProcessMouseDown((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_RBUTTONUP:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right);
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;

                    ProcessMouseUp((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_RBUTTONDBLCLK:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    ProcessMouseDoubleClick((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_MBUTTONDOWN:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    MouseDown(this, (MouseButtonEventArgs)e);
                    break;
                case Messages.WM_MBUTTONUP:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle);
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;

                    ProcessMouseUp((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_MBUTTONDBLCLK:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    MouseDoubleClick(this, (MouseButtonEventArgs)e);
                    break;
                case Messages.WM_MOUSEWHEEL:
                    e = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, (int)mouseInfoHookStruct.mouseData);
                    e.RoutedEvent = Mouse.PreviewMouseWheelEvent;

                    ProcessMouseWheel((MouseWheelEventArgs)e);
                    break;
                case Messages.WM_MOUSEHWHEEL:
                    e = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, (int)mouseInfoHookStruct.mouseData);
                    e.RoutedEvent = Mouse.PreviewMouseWheelEvent;

                    ProcessMouseWheel((MouseWheelEventArgs)e);
                    break;
                case Messages.WM_XBUTTONDOWN:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, mouseInfoHookStruct.mouseData == 1 ? MouseButton.XButton1 : MouseButton.XButton2);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    ProcessMouseDown((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_XBUTTONUP:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, mouseInfoHookStruct.mouseData == 1 ? MouseButton.XButton1 : MouseButton.XButton2);
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;

                    ProcessMouseUp((MouseButtonEventArgs)e);
                    break;
                case Messages.WM_XBUTTONDBLCLK:
                    e = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, mouseInfoHookStruct.mouseData == 1 ? MouseButton.XButton1 : MouseButton.XButton2);
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;

                    ProcessMouseDoubleClick((MouseButtonEventArgs)e);
                    break;
                default:
                    e = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount);
                    e.RoutedEvent = Mouse.PreviewMouseMoveEvent;

                    ProcessMouseMove((MouseEventArgs)e);
                    break;
            }
            
            return !e.Handled;
        }

        private void ProcessMouseDown(MouseButtonEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(this, e);
            }
        }

        private void ProcessMouseUp(MouseButtonEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(this, e);
            }
        }

        private void ProcessMouseClick(MouseButtonEventArgs e)
        {
            if (MouseClick != null)
            {
                MouseClick(this, e);
            }
        }

        private void ProcessMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (MouseDoubleClick != null)
            {
                MouseDoubleClick(this, e);
            }
        }

        private void ProcessMouseWheel(MouseWheelEventArgs e)
        {
            if (MouseWheel != null)
            {
                MouseWheel(this, e);
            }
        }

        private void ProcessMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null)
            {
                MouseMove(this, e);
            }
        }

        private MouseEventArgs CreateMouseEvent(uint wParam)
        {
            MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount);

            switch (wParam)
            {
                case Messages.WM_LBUTTONDOWN:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_LBUTTONUP:
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;
                    break;
                case Messages.WM_LBUTTONDBLCLK:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_RBUTTONDOWN:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_RBUTTONUP:
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;
                    break;
                case Messages.WM_RBUTTONDBLCLK:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_MBUTTONDOWN:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_MBUTTONUP:
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;
                    break;
                case Messages.WM_MBUTTONDBLCLK:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_MOUSEWHEEL:
                    e.RoutedEvent = Mouse.PreviewMouseWheelEvent;
                    break;
                case Messages.WM_MOUSEHWHEEL:
                    e.RoutedEvent = Mouse.PreviewMouseWheelEvent;
                    break;
                case Messages.WM_XBUTTONDOWN:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                case Messages.WM_XBUTTONUP:
                    e.RoutedEvent = Mouse.PreviewMouseUpEvent;
                    break;
                case Messages.WM_XBUTTONDBLCLK:
                    e.RoutedEvent = Mouse.PreviewMouseDownEvent;
                    break;
                default:
                    e.RoutedEvent = Mouse.PreviewMouseMoveEvent;
                    break;
            }
            
            return e;
        }
    }
}
