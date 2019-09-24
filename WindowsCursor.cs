using System;

#if false

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

#endif