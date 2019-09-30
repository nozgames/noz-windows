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
using System.Runtime.InteropServices;

namespace NoZ.Platform.Windows
{
    class XAudio2Clip : AudioClip, IDisposable
    {
        internal short[] _buffer;
        internal GCHandle _bufferHandle;

        public XAudio2Clip()
        {
        }

        public XAudio2Clip(int samples, AudioChannelFormat channelFormat, int frequency) :
            base(samples, channelFormat, frequency)
        {
            _buffer = new short[samples * (int)channelFormat];
            _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_bufferHandle.IsAllocated)
                _bufferHandle.Free();
        }

        public override void GetData(short[] data, int offset)
        {
            Array.Copy(_buffer, offset, data, 0, Math.Min(data.Length - offset, _buffer.Length));
        }

        public override void SetData(short[] data, int offset)
        {
            // During resource reading the data isnt available at construction time
            // so the buffer will be null until the first call to SetData.  It is assumed
            // that the first call to set data will define the actual audio data.
            if (null == _buffer)
            {
                if (SampleCount != data.Length)
                    throw new ArgumentException("Uninitialized AudioClip cannot be initialized with data with a different size than the sample count");

                if (offset != 0)
                    throw new ArgumentException("Uninitialized AudioClip cannot be initialized with a data offset");

                _buffer = new short[data.Length];
                _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            }

            Array.Copy(data, offset, _buffer, 0, Math.Min(_buffer.Length - offset, data.Length));
        }
    }
}
