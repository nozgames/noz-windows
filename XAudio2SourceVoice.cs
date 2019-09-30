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
    internal class XAudio2SourceVoice : ComObject
    {
        public AudioChannelFormat ChannelFormat {
            get; private set;
        }

        public int Frequency {
            get; private set;
        }

        public bool IsPlaying {
            get {
                return GetState().buffersQueued > 0;
            }
        }

        public uint Id {
            get; set;
        }

        public uint PlayId {
            get; set;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct VoiceState
        {
            public IntPtr currentBufferContext;
            public uint buffersQueued;
            public ulong samplesPlayed;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetStateDelegate(void* instance, out VoiceState state, int flags);
        private GetStateDelegate GetStateNative;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SubmitSourceBufferDelegate(IntPtr instance, IntPtr buffer, void* bufferWMA);
        private SubmitSourceBufferDelegate SubmitSourceBufferNative;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int StartDelegate(IntPtr instance, uint flags, uint operationSet);
        private StartDelegate StartNative;


        public XAudio2SourceVoice(IntPtr instance, AudioChannelFormat channelFormat, int frequency) : base(instance)
        {
            ChannelFormat = channelFormat;
            Frequency = frequency;
            StartNative = GetDelegateForVTable<StartDelegate>(19);
            SubmitSourceBufferNative = GetDelegateForVTable<SubmitSourceBufferDelegate>(21);
            GetStateNative = GetDelegateForVTable<GetStateDelegate>(25);
        }

        private unsafe VoiceState GetState()
        {
            VoiceState result = new VoiceState();
            GetStateNative(Instance.ToPointer(), out result, 0);
            return result;
        }

        public unsafe int Play(XAudio2Clip clip)
        {
            if (clip._buffer == null)
                return 0;

            XAudio2Buffer x2buffer = new XAudio2Buffer
            {
                flags = 0x0040,
                audioBytes = (uint)(clip._buffer.Length * sizeof(short)),
                audioData = clip._bufferHandle.AddrOfPinnedObject(),
                playBegin = 0,
                playLength = 0,
                loopBegin = 0,
                loopLength = 0,
                loopCount = 0,
                context = IntPtr.Zero
            };

            SubmitSourceBufferNative(Instance, new IntPtr(&x2buffer), null);
            return StartNative(Instance, 0, 0);
        }
    }
}
