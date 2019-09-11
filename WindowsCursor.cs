using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoZ.Platform.Windows {

    class WindowsCursor : Cursor, IDisposable {
        public IntPtr Handle { get; private set; }

        public WindowsCursor(SystemCursor systemCursor) {
            switch (systemCursor) {
                default:
                case SystemCursor.Arrow:
                    Handle = IntPtr.Zero;
                    break;

                case SystemCursor.IBeam:
                    Handle = Win32.LoadCursor(IntPtr.Zero, (int)Win32.CursorName.IBeam);
                    break;
            }
        }

        public void Dispose() {            
        }
    }
}
