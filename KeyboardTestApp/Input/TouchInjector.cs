using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TCD.System.TouchInjection;

namespace KeyboardTestApp.Input
{
    class TouchInject
    {
        // TODO: define max input gestures in a .ini
        private PointerTouchInfo[] inputGesture = new PointerTouchInfo[maxInputGestures];
        private static uint maxInputGestures = 1;
        private bool enabled {get; set;}

        #region win32
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        #endregion

        internal TouchInject()
        {
            this.enabled = false;
            TouchInjector.InitializeTouchInjection();
        }

        /// <summary>
        /// Simulates a touch tap gesture at the current mouse global screen coordinates.
        /// </summary>
        /// <returns></returns>
        internal async Task<bool> SimulateTouch()
        {
            Point p = GetMousePosition();
            
            inputGesture[0] = CreatePointerTouchInfo((int)p.X, (int)p.Y, 2, 1);

            bool success = TouchInjector.InjectTouchInput(1, inputGesture);

            inputGesture[0].PointerInfo.PointerFlags = PointerFlags.UP;
            await Task.Delay(3);
            success &= TouchInjector.InjectTouchInput(1, inputGesture);

            return success;
        }

        /// <summary>
        /// Simulates a touch tap gesture at the specified screen coordinates. Note: useful to use PointToScreen(Point p) to get global screen coords.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        internal async Task<bool> SimulateTouch(double xPos, double yPos)
        {
            inputGesture[0] = CreatePointerTouchInfo((int)xPos + 2, (int)yPos + 2, 2, 1);

            bool success = TouchInjector.InjectTouchInput(1, inputGesture);

            inputGesture[0].PointerInfo.PointerFlags = PointerFlags.UP;
            await Task.Delay(3);
            success &= TouchInjector.InjectTouchInput(1, inputGesture);

            return success;
        }

        internal bool SimulateTouchDown()
        {
            Point p = GetMousePosition();
            inputGesture[0] = CreatePointerTouchInfo((int)p.X, (int)p.Y, 2, 1);

            bool success = TouchInjector.InjectTouchInput(1, inputGesture);
            return success;
        }

        internal bool SimulateTouchSwipe()
        {
            Point p = GetMousePosition();
            inputGesture[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            inputGesture[0].Move(inputGesture[0].PointerInfo.PtPixelLocation.X - (int)p.X, inputGesture[0].PointerInfo.PtPixelLocation.Y - (int)p.Y);

            //bool success = TouchInjector.InjectTouchInput(1, inputGesture);
            return true;
        }

        internal bool SimulateTouchRelease()
        {
            Point p = GetMousePosition();
            inputGesture[0] = CreatePointerTouchInfo((int)p.X, (int)p.Y, 2, 1);

            inputGesture[0].PointerInfo.PointerFlags = PointerFlags.UP;
            bool success = TouchInjector.InjectTouchInput(1, inputGesture);
            return success;
        }

        /// <summary>
        /// Creates a PointerTouchInfo struct with the specified properties.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="id"></param>
        /// <param name="orientation"></param>
        /// <param name="pressure"></param>
        /// <returns></returns>
        private PointerTouchInfo CreatePointerTouchInfo(int x, int y, int radius, uint id, uint orientation = 90, uint pressure = 32000)
        {
            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = PointerInputType.TOUCH;
            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;
            contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            contact.PointerInfo.PointerId = id;
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }
    }
}
