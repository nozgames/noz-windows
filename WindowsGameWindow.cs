using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

#if false
namespace NoZ.Platform.Windows {
    class WindowsGameWindow : GameWindow {
        private static readonly string ClassName = Assembly.GetExecutingAssembly().GetName().Name + "Class";

        private IntPtr _hdc;
        private IntPtr _hwnd;
        private IntPtr _hglrc;
        private Win32.SIZE _clientSize;
        private Win32.WNDCLASSEX _wcex;
        private Stopwatch _stopwatch;
        private Stopwatch _stopwatch2;
        private Cursor _cursor;
        private string _title;

        public override Vector2Int Size => new Vector2Int(_clientSize.cx, _clientSize.cy);

        public WindowsGameWindow (WindowsProgramContext context) {
            RegisterClass();
            CreateWindow(context);
            _stopwatch = new Stopwatch();
            _stopwatch2 = new Stopwatch();
        }

        private bool RegisterClass() {
            _wcex = new Win32.WNDCLASSEX();
            _wcex.style = Win32.ClassStyles.OwnDC;
            _wcex.cbSize = (uint)Marshal.SizeOf(_wcex);
            _wcex.lpfnWndProc = WndProc;
            _wcex.cbClsExtra = 0;
            _wcex.cbWndExtra = 0;
            _wcex.hIcon = Win32.LoadIcon(IntPtr.Zero, (IntPtr)Win32.IDI_APPLICATION);
            _wcex.hCursor = Win32.LoadCursor(IntPtr.Zero, (int)Win32.CursorName.Arrow);
            _wcex.hIconSm = IntPtr.Zero;
            _wcex.hbrBackground = (IntPtr)(Win32.COLOR_WINDOW + 1);
            _wcex.lpszMenuName = null;
            _wcex.lpszClassName = ClassName;
            if (Win32.RegisterClassEx(ref _wcex) == 0) {
                return false;
            }
            return true;
        }

        private void CreateWindow(WindowsProgramContext context) {
            _title = context.Name;

            var style = Win32.WindowStyles.OverlappedWindow;
            if (!context.AllowWindowResize)
                style &= ~Win32.WindowStyles.ThickFrame;

            _hwnd = Win32.CreateWindowEx(0, ClassName, context.Name, (uint)style,
                            100, 100, 800, 600, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if(context.WindowSize.x > 0 && context.WindowSize.y > 0)
                Resize(context.WindowSize);

            _hdc = Win32.GetDC(_hwnd);

            Win32.RECT clientRect = new Win32.RECT();
            Win32.GetClientRect(_hwnd, out clientRect);
            _clientSize = new Win32.SIZE(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);

            // Set the pixel format for this DC
            var pfd = new Win32.PixelFormatDescriptor {
                Size = (short)Marshal.SizeOf<Win32.PixelFormatDescriptor>(),
                Version = 1,
                Flags = Win32.PixelFormatDescriptorFlags.DRAW_TO_WINDOW |
                        Win32.PixelFormatDescriptorFlags.SUPPORT_OPENGL |
                        Win32.PixelFormatDescriptorFlags.DOUBLEBUFFER,
                PixelType = Win32.PixelType.Rgba,
                ColorBits = 32,
                RedBits = 0,
                RedShift = 0,
                GreenBits = 0,
                GreenShift = 0,
                BlueBits = 0,
                BlueShift = 0,
                AlphaBits = 0,
                AlphaShift = 0,
                AccumBits = 0,
                AccumRedBits = 0,
                AccumGreenBits = 0,
                AccumBlueBits = 0,
                AccumAlphaBits = 0,
                DepthBits = 32,
                StencilBits = 8,
                AuxBuffers = 0,
                LayerType = 0,
                LayerMask = 0,
                DamageMask = 0
            };

            int id = Win32.ChoosePixelFormat(_hdc, ref pfd);
            Win32.SetPixelFormat(_hdc, id, ref pfd);
            _hglrc = Win32.CreateContext(_hdc);
            Win32.MakeCurrent(_hdc, _hglrc);

            // Disable V-Sync ?
            GL.Imports.wglSwapIntervalEXT(0);
        }

        protected override void Show ( ) {
            Win32.ShowWindow(_hwnd, Win32.ShowWindowCommand.ShowNormal);
        }

        protected override void OnBeginFrame() {
            _stopwatch.Start();
            _stopwatch2.Restart();
            //Win32.MakeCurrent(_hdc, _hglrc);            
        }

        private double lastFPS = 60;
        private int frameCount = 0;
        private double minElapsed = 0;
        private double maxElapsed = 0;
        private double avgElapsed = 0;

        protected override void OnEndFrame() {
            Win32.SwapBuffers(_hdc);

            _stopwatch2.Stop();

            if (_stopwatch.ElapsedMilliseconds > 1000) {
                _stopwatch.Stop();

                lastFPS = lastFPS * 0 + (frameCount / (_stopwatch.ElapsedMilliseconds / 1000.0)) * 1;

                avgElapsed = _stopwatch.Elapsed.TotalMilliseconds / frameCount;

                if (lastFPS >= 1000)
                    Win32.SetWindowText(_hwnd, $"{_title}  (1000+ FPS  {minElapsed:0.00}ms < {avgElapsed:0.00}ms < {maxElapsed:0.00}ms  {Node.TotalNodes} Nodes) ");
                else
                    Win32.SetWindowText(_hwnd, $"{_title}  ({(int)lastFPS} FPS  {minElapsed:0.00}ms < {avgElapsed:0.00}ms < {maxElapsed:0.00}ms  {Node.TotalNodes} Nodes)");

                _stopwatch.Restart();
                frameCount = 0;
                minElapsed = 100000;
                maxElapsed = 0;
                avgElapsed = 0;
            } else {
                
                frameCount++;
                minElapsed = Math.Min(minElapsed, _stopwatch2.Elapsed.TotalMilliseconds);
                maxElapsed = Math.Max(maxElapsed, _stopwatch2.Elapsed.TotalMilliseconds);
                avgElapsed += _stopwatch2.Elapsed.TotalMilliseconds;
            }
        }

        private static readonly KeyCode[] virtualKeyToKeyCode;
        private static readonly KeyCode[] charToKeyCode;

        static WindowsGameWindow() {
            virtualKeyToKeyCode = new KeyCode[255];
            charToKeyCode = new KeyCode[255];

            for (int i = 'A'; i <= 'Z'; i++)
                virtualKeyToKeyCode[i] = (KeyCode)((int)KeyCode.A + i - 'A');
            for (int i = 'a'; i <= 'z'; i++)
                virtualKeyToKeyCode[i] = (KeyCode)((int)KeyCode.A + i - 'a');
            for (int i = '0'; i <= '9'; i++)
                virtualKeyToKeyCode[i] = (KeyCode)((int)KeyCode.D0 + i - '0');

            virtualKeyToKeyCode[(int)Win32.VirtualKey.Control] = KeyCode.Control;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.LeftControl] = KeyCode.Control;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.RightControl] = KeyCode.Control;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Shift] = KeyCode.Shift;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Space] = KeyCode.Space;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Backspace] = KeyCode.Backspace;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Delete] = KeyCode.Delete;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Left] = KeyCode.Left;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Right] = KeyCode.Right;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Up] = KeyCode.Up;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Down] = KeyCode.Down;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Home] = KeyCode.Home;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.End] = KeyCode.End;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Return] = KeyCode.Enter;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.OemComma] = KeyCode.Comma;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.OemPeriod] = KeyCode.Period;
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Return] = KeyCode.Enter;

            // Initialize the character to keycode mapping
            for (int i = 'A'; i <= 'Z'; i++)
                charToKeyCode[i] = (KeyCode)((int)KeyCode.A + i - 'A');
            for (int i = 'a'; i <= 'z'; i++)
                charToKeyCode[i] = (KeyCode)((int)KeyCode.A + i - 'a');
            for (int i = '0'; i <= '9'; i++)
                charToKeyCode[i] = (KeyCode)((int)KeyCode.D0 + i - '0');

            charToKeyCode['-'] = KeyCode.Minus;
            charToKeyCode[' '] = KeyCode.Space;
            charToKeyCode['.'] = KeyCode.Period;
            charToKeyCode[','] = KeyCode.Comma;
            charToKeyCode[';'] = KeyCode.Semicolon;
            charToKeyCode[':'] = KeyCode.Colon;
            charToKeyCode['\r'] = KeyCode.Return;
        }

        private static KeyCode GetKeyCode(char c) => charToKeyCode[c];
        private static KeyCode GetKeyCode(byte virtualKey) => virtualKeyToKeyCode[virtualKey];

        protected override void SetCursor(Cursor cursor) {
            _cursor = cursor;
            if (null == cursor)
                Win32.SetCursor(IntPtr.Zero);
            else
                Win32.SetCursor((cursor as WindowsCursor).Handle);
        }

        public override void Resize(in Vector2Int size) {
            Win32.GetWindowRect(_hwnd, out var windowRect);
            Win32.GetClientRect(_hwnd, out var clientRect);

            Win32.MoveWindow(
                _hwnd,
                windowRect.left,
                windowRect.top,
                ((windowRect.right-windowRect.left) - (clientRect.right-clientRect.left)) + size.x,
                ((windowRect.bottom- windowRect.top) - (clientRect.bottom- clientRect.top)) + size.y,
                true
                );
        }

        private IntPtr WndProc(IntPtr hwnd, Win32.WindowMessage message, uint wparam, uint lparam) {
            switch (message) {
                case Win32.WindowMessage.Paint: {
                    Win32.PAINTSTRUCT ps = new Win32.PAINTSTRUCT();
                    Win32.BeginPaint(hwnd, out ps);
                    Win32.EndPaint(hwnd, ref ps);
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.EraseBackground: {
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.Destroy: {
                    Win32.PostQuitMessage(0);
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.WindowPositionChanged: {
                    unsafe {
                        Win32.WindowPosition* position = (Win32.WindowPosition*)lparam;
                        if (position->hwnd == _hwnd) {
                            if (_clientSize.cx != position->cx ||
                                _clientSize.cy != position->cy) {
                                Win32.RECT clientRect = new Win32.RECT();
                                Win32.GetClientRect(_hwnd, out clientRect);
                                _clientSize = new Win32.SIZE(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);
                            }
                        }

                        Raise(Resized, Size);
                    }
                    break;
                }

                case Win32.WindowMessage.LButtonDown: {
                    SendMouseButtonDownEvent(MouseButton.Left);
                    break;
                }

                case Win32.WindowMessage.LButtonUp: {
                    SendMouseButtonUpEvent(MouseButton.Left);
                    break;
                }

                case Win32.WindowMessage.RButtonDown: {
                    SendMouseButtonDownEvent(MouseButton.Right);
                    break;
                }

                case Win32.WindowMessage.RButtonUp: {
                    SendMouseButtonUpEvent(MouseButton.Right);
                    break;
                }

                case Win32.WindowMessage.MouseMove: {
                    SendMouseMoveEvent(new Vector2(Win32.GET_X_LPARAM(lparam), Win32.GET_Y_LPARAM(lparam)));
                    break;
                }

                case Win32.WindowMessage.MouseWheel: {
                    SendMouseWheelEvent(((short)Win32.HIWORD(wparam)) / 120f);
                    break;
                }

                case Win32.WindowMessage.SysKeyDown:
                case Win32.WindowMessage.KeyDown: {
                    SendKeyDownEvent(GetKeyCode((byte)wparam));
                    break;
                }

                case Win32.WindowMessage.SysKeyUp:
                case Win32.WindowMessage.KeyUp: {
                    SendKeyUpEvent(GetKeyCode((byte)wparam));
                    break;
                }

                case Win32.WindowMessage.SetCursor: {
                    if(_cursor != null) {
                        Win32.SetCursor(((WindowsCursor)_cursor).Handle);
                        return (IntPtr)1;
                    }
                    break;
                }

                case Win32.WindowMessage.Activate: {
                    if(_hglrc != IntPtr.Zero && wparam == 1)
                        Win32.MakeCurrent(_hdc, _hglrc);
                    break;
                }

                default:
                break;
            }

            return Win32.DefWindowProc(hwnd, message, wparam, lparam);
        }
    }
}
#endif
