/*
  NoZ Game Engine

  Copyright(c) 2019 NoZ Games, LLC

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files(the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions :

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using NoZ.Platform.OpenGL;

namespace NoZ.Platform.Windows
{
    public class WindowsWindow : IWindowDriver
    {
        /// <summary>
        /// Interial window class used to create window
        /// </summary>
        private static string ClassName = Assembly.GetEntryAssembly().GetName().Name + "Class";

        private IntPtr _hdc;
        private IntPtr _hwnd;
        private IntPtr _hglrc;
        private Win32.SIZE _clientSize;
        private Win32.WNDCLASSEX _wcex;
        private Stopwatch _stopwatch;
        private Stopwatch _stopwatch2;
        private string _title;

        public Vector2Int Size => new Vector2Int(_clientSize.cx, _clientSize.cy);

        /// <summary>
        /// Title of the window
        /// </summary>
        public string Title {
            get => _title;
            set {
                _title = value;
                Win32.SetWindowText(_hwnd, value);
            }
        }

        public WindowsWindow(string title)
        {
            _title = title;

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
            if (Win32.RegisterClassEx(ref _wcex) == 0)
                throw new Exception("failed to register window class");

            _stopwatch = new Stopwatch();
            _stopwatch2 = new Stopwatch();

            var style = Win32.WindowStyles.OverlappedWindow;
            //            if (!context.AllowWindowResize)
            //                style &= ~Win32.WindowStyles.ThickFrame;

            _hwnd = Win32.CreateWindowEx(0, ClassName, _title, (uint)style,
                            100, 100, 800, 600, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            //if (context.WindowSize.x > 0 && context.WindowSize.y > 0)
            //Resize(context.WindowSize);

            _hdc = Win32.GetDC(_hwnd);

            Win32.RECT clientRect = new Win32.RECT();
            Win32.GetClientRect(_hwnd, out clientRect);
            _clientSize = new Win32.SIZE(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);

            // Use Opengl
            Graphics.Driver = OpenGLDriver.Create();

            // Set the pixel format for this DC
            var pfd = new GL.Win32.PixelFormatDescriptor
            {
                Size = (short)Marshal.SizeOf<GL.Win32.PixelFormatDescriptor>(),
                Version = 1,
                Flags = GL.Win32.PixelFormatDescriptorFlags.DRAW_TO_WINDOW |
                        GL.Win32.PixelFormatDescriptorFlags.SUPPORT_OPENGL |
                        GL.Win32.PixelFormatDescriptorFlags.DOUBLEBUFFER,
                PixelType = GL.Win32.PixelType.Rgba,
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

            _hdc = Win32.GetDC(_hwnd);
            var id = GL.Win32.wglChoosePixelFormat(_hdc, ref pfd);
            GL.Win32.SetPixelFormat(_hdc, id, ref pfd);
            _hglrc = GL.Win32.wglCreateContext(_hdc);
            GL.Win32.wglMakeCurrent(_hdc, _hglrc);
        }

        public void DrawBegin()
        {
        }

        public void DrawEnd()
        {
            GL.Win32.wglSwapBuffers(_hdc);
        }

        /// <summary>
        /// Show the window and run until exited
        /// </summary>
        public void Show()
        {
            Win32.ShowWindow(_hwnd, Win32.ShowWindowCommand.ShowNormal);
        }

        private static readonly KeyCode[] virtualKeyToKeyCode;
        private static readonly KeyCode[] charToKeyCode;

        static WindowsWindow()
        {
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
            virtualKeyToKeyCode[(int)Win32.VirtualKey.Escape] = KeyCode.Escape;

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

        private IntPtr WndProc(IntPtr hwnd, Win32.WindowMessage message, uint wparam, uint lparam)
        {
            switch (message)
            {
                case Win32.WindowMessage.Paint:
                {
                    Win32.PAINTSTRUCT ps = new Win32.PAINTSTRUCT();
                    Win32.BeginPaint(hwnd, out ps);
                    Win32.EndPaint(hwnd, ref ps);
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.EraseBackground:
                {
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.Destroy:
                {
                    Win32.PostQuitMessage(0);
                    return IntPtr.Zero;
                }

                case Win32.WindowMessage.WindowPositionChanged:
                {
                    unsafe
                    {
                        Win32.WindowPosition* position = (Win32.WindowPosition*)lparam;
                        if (position->hwnd == _hwnd)
                        {
                            if (_clientSize.cx != position->cx ||
                                _clientSize.cy != position->cy)
                            {
                                Win32.RECT clientRect = new Win32.RECT();
                                Win32.GetClientRect(_hwnd, out clientRect);
                                _clientSize = new Win32.SIZE(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);
                            }
                        }

                        //Raise(Resized, Size);
                    }
                    break;
                }

                case Win32.WindowMessage.LButtonDown:
                {
                    Window.MouseButtonDownEvent.Broadcast(MouseButton.Left);
                    break;
                }

                case Win32.WindowMessage.LButtonUp:
                {
                    Window.MouseButtonUpEvent.Broadcast(MouseButton.Left);
                    break;
                }

                case Win32.WindowMessage.RButtonDown:
                {
                    Window.MouseButtonDownEvent.Broadcast(MouseButton.Right);
                    break;
                }

                case Win32.WindowMessage.RButtonUp:
                {
                    Window.MouseButtonUpEvent.Broadcast(MouseButton.Right);
                    break;
                }

                case Win32.WindowMessage.MouseMove:
                {
                    Window.MouseMoveEvent.Broadcast(new Vector2(Win32.GET_X_LPARAM(lparam), Win32.GET_Y_LPARAM(lparam)));
                    break;
                }

                case Win32.WindowMessage.MouseWheel:
                {
                    //SendMouseWheelEvent(((short)Win32.HIWORD(wparam)) / 120f);
                    break;
                }

                case Win32.WindowMessage.SysKeyDown:
                case Win32.WindowMessage.KeyDown:
                {
                    Window.KeyDownEvent.Broadcast(GetKeyCode((byte)wparam));
                    break;
                }

                case Win32.WindowMessage.SysKeyUp:
                case Win32.WindowMessage.KeyUp:
                {
                    Window.KeyUpEvent.Broadcast(GetKeyCode((byte)wparam));
                    break;
                }

#if false
                case Win32.WindowMessage.SetCursor:
                {
                    if (_cursor != null)
                    {
                        Win32.SetCursor(((WindowsCursor)_cursor).Handle);
                        return (IntPtr)1;
                    }
                    break;
                }
#endif

                case Win32.WindowMessage.Activate:
                {
                    if (wparam == 1)
                        GL.Win32.wglMakeCurrent(_hdc, _hglrc);
                    break;
                }

                default:
                    break;
            }

            return Win32.DefWindowProc(hwnd, message, wparam, lparam);
        }
    }
}

