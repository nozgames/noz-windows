using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NoZ.Platform.Windows {
    static class Win32 {
        private static class ExternDll {
            public const string User32 = "user32.dll";
            public const string Gdi32 = "gdi32.dll";
            public const string Kernel32 = "kernel32.dll";
        }

        public delegate IntPtr WndProc(IntPtr hWnd, WindowMessage msg, uint wParam, uint lParam);

        [Flags]
        public enum ClassStyles : int {
            ByteAlignClient = 0x1000,
            ByteAlignWindow = 0x2000,
            ClassDC = 0x40,
            DoubleClicks = 0x8,
            DropShadow = 0x20000,
            GlobalClass = 0x4000,
            HorizontalRedraw = 0x2,
            NoClose = 0x200,
            OwnDC = 0x20,
            ParentDC = 0x80,
            SaveBits = 0x800,
            VerticalRedraw = 0x1
        }

        public enum WindowMessage : uint {
            Create = 0x0001,
            Destroy = 0x0002,
            Size = 0x0005,
            Paint = 0x000F,
            Quit = 0x0012,
            EraseBackground =0x0014,
            WindowPositionChanged = 0x0047,
            MouseMove = 0x0200,
            LButtonDown = 0x0201,
            LButtonUp = 0x0202,
            LButtonDoubleClick = 0x0203,
            RButtonDown = 0x0204,
            RButtonUp = 0x0205,
            MouseWheel = 0x020A,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105,
            KeyDown = 0x100,
            KeyUp = 0x101,
            SetCursor = 0x20,
            Activate = 0x06,
            NonClientHitTest = 0x84,
            NonClientDrawCaption = 0xAE,
            GetIcon = 0x7F,
            SetText = 0x0C
        }

        public enum VirtualKey : byte {
            Return = 0x0D,
            Backspace = 0x08,
            Control = 0x11,
            LeftControl = 0xA2,
            RightControl = 0xA3,
            Shift = 0x10,
            Space = 0x20,
            Left = 0x25,
            Up = 0x26,
            Right = 0x27,
            Down = 0x28,
            Home = 0x24,
            End = 0x23,
            Delete = 0x2E,
            OemComma = 0xBC,
            OemPeriod = 0xBE,
            Escape = 0x1B
        }

        [Flags]
        public enum WindowStyles : uint {
            Overlapped = 0x00000000,
            MaximizeBox = 0x00010000,
            MinimizeBox = 0x00020000,
            ThickFrame = 0x00040000,
            SysMenu = 0x00080000,
            Caption = 0x00C00000,
            Visible  = 0x10000000,
            OverlappedWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox
        }


        [Flags]
        internal enum SetWindowPosFlags : int {
            NoSize = 0x0001,
            NoMove = 0x0002,
            NoZOrder = 0x0004,
            NoRedraw = 0x0008,
            NoActivate = 0x0010,
            FrameChanged = 0x0020,
            ShowWindow = 0x0040,
            HideWindow = 0x0080,
            NoCopyBits = 0x0100,
            NoOwnerZOrder = 0x0200,
            NoSendChanging = 0x0400,
            DrawFrame = FrameChanged,
            NoReposition = NoOwnerZOrder,
            DeferErase = 0x2000,
            AsyncWindowPosition = 0x4000
        }

        public const uint IDI_APPLICATION = 32512;

        public enum CursorName {
            Arrow = 32512,
            IBeam = 32513
        }

        public const uint COLOR_WINDOW = 5;

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEX {
            public uint cbSize;
            public ClassStyles style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
		public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy) {
                this.cx = cx;
                this.cy = cy;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MSG {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowPosition {
            internal IntPtr hwnd;
            internal IntPtr hwndInsertAfter;
            internal int x;
            internal int y;
            internal int cx;
            internal int cy;
            [MarshalAs(UnmanagedType.U4)]
            internal SetWindowPosFlags flags;
        }

        internal enum ShowWindowCommand {
            Hide = 0,
            ShowNormal = 1,
            Normal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinimizedNoActivate = 7,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        public static ushort LOWORD(uint v) { return ((ushort)((v) & 0xffff)); }
        public static ushort HIWORD(uint v) { return ((ushort)(((v) >> 16) & 0xffff)); }
        public static int GET_X_LPARAM(uint lparam) { return (short)LOWORD(lparam); }
        public static int GET_Y_LPARAM(uint lparam) { return (short)HIWORD(lparam); }

        [DllImport(ExternDll.User32, SetLastError=true)]
        public static extern IntPtr CreateWindowEx (
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport(ExternDll.User32, SetLastError = true)]
        internal static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern sbyte GetMessage(out MSG msg, IntPtr hwnd, uint msgFilterMin, uint msgFilterMax);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern sbyte PeekMessage(out MSG msg, IntPtr hwnd, uint msgFilterMin, uint msgFilterMax, uint removeMessage);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern IntPtr DispatchMessage(ref MSG lpmsg);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern void SetCursor(IntPtr hcursor);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage uMsg, uint wParam, uint lParam);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport(ExternDll.User32)]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError=true)]
        internal extern static bool GetClientRect(IntPtr hwnd, out RECT clientRectangle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true)]
        internal extern static bool GetWindowRect(IntPtr hwnd, out RECT windowRectangle);

        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllName);

        [DllImport(ExternDll.User32, SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool ShowWindow(IntPtr hwnd, ShowWindowCommand show);

        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport(ExternDll.User32)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport(ExternDll.User32)]
        public static extern bool ScreenToClient(IntPtr hwnd, out POINT lpPoint);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32)]
        public static extern bool MoveWindow (IntPtr hwnd, int x, int y, int width, int height, bool repaint);

    }
}
