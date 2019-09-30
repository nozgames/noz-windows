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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NoZ.Platform.Windows
{
    public class XAudio2Driver : ComObject, IAudioDriver
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int StartEngineDelegate(IntPtr instance);
        public StartEngineDelegate StartEngineNative;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate int CreateMasteringVoiceDelegate(void* instance, out IntPtr voice, int inputChannels, int inputSampleRate, int flags, void* deviceId, void* effectChain, int streamCategory);
        public CreateMasteringVoiceDelegate CreateMasteringVoiceNative;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate int CreateSourceVoiceDelegate(void* instance, out IntPtr voice, void* sourceFormat, int flags, float maxFrequencyRatio, void* callback, void* sendList, void* effectChain);
        public CreateSourceVoiceDelegate CreateSourceVoiceNative;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate int SetVolumeDelegate(void* instance, float volume, int operation);
        public SetVolumeDelegate SetVolumeNative;

        public const string Library = "xaudio2_8.dll";
        [DllImport(Library, EntryPoint = "XAudio2Create", CallingConvention = CallingConvention.StdCall)]
        public unsafe static extern int XAudio2Create(void* arg0, int arg1, int arg2);

        private float _volume = 1.0f;
        public float Volume {
            get => _volume;
            set {
                _volume = value;
                _masterVoice.Volume = _volume ;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct PCMWaveFormat
        {
            public static readonly int SizeInBytes = Marshal.SizeOf<PCMWaveFormat>();

            public ushort format;
            public ushort channels;
            public uint samplesPerSecond;
            public uint avgBytesPerSecond;
            public ushort blockAlign;
            public ushort bitsPerSample;
        }

        private XAudio2MasteringVoice _masterVoice;
        private List<XAudio2SourceVoice> _voices;

        private unsafe XAudio2Driver(IntPtr instance) : base(instance)
        {
            CreateSourceVoiceNative = GetDelegateForVTable<CreateSourceVoiceDelegate>(5);
            CreateMasteringVoiceNative = GetDelegateForVTable<CreateMasteringVoiceDelegate>(7);
            StartEngineNative = GetDelegateForVTable<StartEngineDelegate>(8);            

            _voices = new List<XAudio2SourceVoice>();

            // Create the mastering voice
            IntPtr voice = IntPtr.Zero;
            if (Failed(CreateMasteringVoiceNative(Instance.ToPointer(), out voice, 0, 0, 0, null, null, 6)))
                throw new ApplicationException("Failed to initialize XAudio2");

            _masterVoice = new XAudio2MasteringVoice(voice);
            _masterVoice.Volume = 1.0f;
        }

        public unsafe static XAudio2Driver Create()
        {
            IntPtr instance = IntPtr.Zero;
            if (Failed(XAudio2Create(&instance, 0, 0x00000001)))
                throw new ApplicationException("Failed to initialize XAudio2");

            return new XAudio2Driver(instance);
        }

        private static bool Failed(int hresult)
        {
            return hresult < 0;
        }

        private unsafe XAudio2SourceVoice CreateVoice(AudioChannelFormat channelFormat, int frequency)
        {
            int channels = (int)channelFormat;

            PCMWaveFormat fmt = new PCMWaveFormat
            {
                format = 1,
                channels = (ushort)channels,
                bitsPerSample = 16,
                blockAlign = (ushort)(channels * sizeof(short)),
                samplesPerSecond = (ushort)frequency,
                avgBytesPerSecond = (uint)(channels * sizeof(ushort) * frequency)
            };

            GCHandle fmtPinned = GCHandle.Alloc(fmt, GCHandleType.Pinned);
            IntPtr voice = IntPtr.Zero;
            CreateSourceVoiceNative(Instance.ToPointer(), out voice, fmtPinned.AddrOfPinnedObject().ToPointer(), 0, 2f, null, null, null);
            fmtPinned.Free();

            var x2voice = new XAudio2SourceVoice(voice, channelFormat, frequency);
            x2voice.Id = (uint)_voices.Count + 1;
            x2voice.PlayId = 1;
            _voices.Add(x2voice);

            return x2voice;
        }

        private Voice AllocVoice(AudioClip clip)
        {
            foreach (var x2voice in _voices)
            {
                if (x2voice.IsPlaying)
                    continue;
                if (x2voice.Frequency != clip.Frequency)
                    continue;
                if (x2voice.ChannelFormat != clip.ChannelFormat)
                    continue;

                x2voice.PlayId++;

                return Voice.Create(x2voice.Id, x2voice.PlayId);
            }

            var voice = CreateVoice(clip.ChannelFormat, clip.Frequency);
            return Voice.Create(voice.Id, voice.PlayId);
        }

        public Voice Play(AudioClip clip)
        {
            Voice voice = AllocVoice(clip);
            if (voice.Id == 0)
                return voice;

            if (Failed(_voices[(int)voice.Id - 1].Play((XAudio2Clip)clip)))
                return Voice.Error;

            return voice;
        }

        public override void Dispose()
        {
            // TODO: cleanup voices
            base.Dispose();
        }

        public AudioClip CreateClip()
        {
            return new XAudio2Clip();
        }

        public AudioClip CreateClip(int samples, AudioChannelFormat channelFormat, int frequency)
        {
            return new XAudio2Clip(samples, channelFormat, frequency);
        }

        public bool IsPlaying(Voice voice)
        {
            if (voice.Id > _voices.Count) return false;

            var x2voice = _voices[(int)voice.Id];
            if (x2voice.PlayId != voice.Instance)
                return false;

            return x2voice.IsPlaying;
        }

        public void DoFrame()
        {
        }
    }
}
