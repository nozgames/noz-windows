/*
  NozEngine Library

  Copyright(c) 2015 NoZ Games, LLC

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
using NoZ.Platform.OpenGL;

namespace NoZ.Platform.Windows {
    public static partial class WindowsProgram {

        public static void Main(WindowsProgramContext context) {
            // Start the game
            Game.Start(new GameContext {
                GraphicsDriver = new WindowsGraphicsDriver(),
                AudioDriver = NoZ.Platform.XAudio2.XAudio2.Create(),
                Args = context.Args,
                Resources = new ResourceDatabase(context.Archives),
                Window = new WindowsGameWindow(context),
                GameResourceName = context.GameResourceName,
                Name = context.Name
            });

            // Main message loop:
            Win32.MSG msg = new Win32.MSG();

            bool done = false;
            while (!done) {
                while (Win32.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0x0001) > 0) {
                    if (msg.message == (int)Win32.WindowMessage.Quit) {
                        done = true;
                        break;
                    }

                    Win32.TranslateMessage(ref msg);
                    Win32.DispatchMessage(ref msg);
                }

                Game.Instance.Frame();
            }

            Game.Exit();
        }
    }
}

