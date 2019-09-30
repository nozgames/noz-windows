using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoZ.Platform.Windows
{
    public static class WindowsApplication 
    {
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
