using System;
using System.Runtime.InteropServices;

namespace NoZ.Platform.Windows {
    public class ComObject : IDisposable {
        public IntPtr Instance {
            get; private set;
        } 

        public ComObject(IntPtr instance) {
            Instance = instance;
        }

        public virtual void Dispose() {
            if (Instance != IntPtr.Zero) {
                Marshal.Release(Instance);
                Instance = IntPtr.Zero;
            }
        }

        protected unsafe T GetDelegateForVTable<T>(int vtableOffset) {
            IntPtr funcPtr = (IntPtr)((void**)(*(void**)Instance))[vtableOffset];
            return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
        }
    }
}
