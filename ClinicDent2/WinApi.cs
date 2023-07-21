using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClinicDent2
{
    public static class WinApi
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowA", SetLastError = true)]
        public static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SetFocus", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SetActiveWindow", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_RESTORE = 9;
        public const uint BM_CLICK = 0x00F5;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        public static void LeftMouseClickInWindow(IntPtr hWnd, int x, int y)
        {
            POINT point = new POINT { X = x, Y = y };

            // Convert the local window coordinates to screen coordinates
            if (ClientToScreen(hWnd, ref point))
            {
                // Move the cursor to the screen coordinates
                SetCursorPos(point.X, point.Y);

                // Perform the left mouse click
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const byte VK_CONTROL = 0x11;
        public const byte VK_RETURN = 0x0D; //ENTER key

        public const byte VK_V = 0x56;
        public const byte VK_A = 0x41;

        public static void PressCtrlV()
        {
            // Press the Ctrl key
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);

            // Press the V key
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);

            Thread.Sleep(50);

            // Release the V key
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // Release the Ctrl key
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        public static void PressCtrlA()
        {
            // Press the Ctrl key
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);

            // Press the A key
            keybd_event(VK_A, 0, 0, UIntPtr.Zero);

            // Release the V key
            keybd_event(VK_A, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // Release the Ctrl key
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        public static void PressEnter()
        {
            keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero);
            keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        public static void GetWindowSize(IntPtr hWnd, out int width, out int height)
        {
            RECT windowRect;
            width= 0; height = 0;
            if (GetWindowRect(hWnd, out windowRect))
            {
                width = windowRect.Right - windowRect.Left;
                height = windowRect.Bottom - windowRect.Top;
            }
        }
    }
}
