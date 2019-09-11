using System;
using System.IO;
using System.Drawing;

using NoZ.Platform.OpenGL;

namespace NoZ.Platform.Windows {
    class WindowsGraphicsDriver : IGraphicsDriver {
        public GraphicsContext CreateContext() {
            return new OpenGLRenderContext();
        }

        public Image CreateImage() {
            return new OpenGLImage();
        }

        public Image LoadImage(Stream stream) {
            throw new NotImplementedException();
        }

        public Image CreateImage(int width, int height, PixelFormat format) {
            return new OpenGLImage(width, height, format);
        }

        public Cursor CreateCursor(Image image) {
            return null;
        }

        public Cursor CreateCursor(SystemCursor systemCursor) {
            return new WindowsCursor(systemCursor);
        }
    }
}
