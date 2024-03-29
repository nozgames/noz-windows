﻿/*
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

namespace NoZ.Platform.Windows
{
    public static class WindowsApplication 
    {
        public static void Exit()
        {
            Win32.PostQuitMessage(0);
        }

        public static void Run (string title, ApplicationDelegate applicationDelegate)
        {
            Application.Initialize(applicationDelegate);

            // Create the audio driver
            Audio.Driver = XAudio2Driver.Create();

            // Create the window
            var window = new WindowsWindow(title);
            Window.Driver = window;

            applicationDelegate?.Start();

            // Show the window
            window.Show();

            // Main message loop:
            Win32.MSG msg = new Win32.MSG();

            try
            {
                bool done = false;
                while (!done)
                {
                    while (Win32.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0x0001) > 0)
                    {
                        if (msg.message == (int)Win32.WindowMessage.Quit)
                        {
                            done = true;
                            break;
                        }

                        Win32.TranslateMessage(ref msg);
                        Win32.DispatchMessage(ref msg);
                    }

                    // Rather than using WM_MOUSEMOVE we just get the cursor after all events messages
                    // have been processed and create a single event for it.
                    Win32.GetCursorPos(out var point);
                    Win32.ScreenToClient(window._hwnd, out point);
                    Window.MouseMoveEvent.Broadcast(new Vector2(point.X, point.Y));

                    Application.Step();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
