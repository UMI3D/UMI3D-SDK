/*
Copyright 2019 - 2022 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Voice noise reducer based on RNNoise lib <see cref="https://github.com/xiph/rnnoise"/> and based on tkmn0's work
    /// on github <see cref="https://github.com/tkmn0/Caress.Unity"/>.
    /// </summary>
    public class NoiseReducer
    {
        private IntPtr _ptr;

        public NoiseReducer(NoiseReducerConfig config)
        {
            NativeMethods.CreateNoiseReducer(ref config, out var result);
            if (result.Error.Code != (byte)1)
            {
                throw new Exception(result.Error.Data.StringValue());
            }

            _ptr = result.Ptr;
        }

        ~NoiseReducer()
        {
            Destroy();
        }

        public void ReduceNoise(short[] pcm, int channel)
        {
            if (_ptr == IntPtr.Zero) return;
            NativeMethods.ReduceNoise(_ptr, pcm, pcm.Length, channel);
        }

        public void ReduceNoiseFloat(float[] pcm, int channel)
        {
            if (_ptr == IntPtr.Zero) return;
            NativeMethods.ReduceNoiseFloat(_ptr, pcm, pcm.Length, channel);
        }

        public void SetAttenuation(double value)
        {
            if (_ptr == IntPtr.Zero) return;
            NativeMethods.SetMaxAttenuation(_ptr, value);
        }

        public void ChangeRnnModel(RnNoiseModel model)
        {
            if (_ptr == IntPtr.Zero) return;
            NativeMethods.ChangeRnnModel(_ptr, model);
        }

        public void Destroy()
        {
            if (_ptr == IntPtr.Zero) return;
            var data = new Data()
            {
                Ptr = _ptr
            };
            NativeMethods.DestroyNoiseReducer(ref data);
            _ptr = data.Ptr;
        }

        public struct NoiseReducerConfig
        {
            public int NumChannels;
            public int SampleRate;
            public double Attenuation;
            public RnNoiseModel Model;
        }

        public enum RnNoiseModel : byte
        {
            General = 0,
            GeneralRecording = 1,
            Voice = 2,
            VoiceRecording = 3,
            Speech = 4,
            SpeechRecording = 5,
            None = 6,
        }

        public struct ApiError
        {
            public byte Code;
            public Data Data;
        }

        public struct PointerResult
        {
            public IntPtr Ptr;
            public ApiError Error;
        }

        internal static class NativeMethods
        {
            private const string DLLName = "libcaress";

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void CreateNoiseReducer(ref NoiseReducerConfig config, out PointerResult result);

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void ReduceNoise(IntPtr ptr, short[] pcm, int pcmLength, int channel);

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void ReduceNoiseFloat(IntPtr ptr, float[] pcm, int pcmLength, int channel);

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetMaxAttenuation(IntPtr ptr, double attenuation);

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void ChangeRnnModel(IntPtr ptr, RnNoiseModel model);

            [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DestroyNoiseReducer(ref Data data);
        }

        public struct Data
        {
            public IntPtr Ptr;
            public uint Length;

            public string StringValue()
            {
                if (Ptr == IntPtr.Zero) return null;
                var buff = ByteValue();
                return Encoding.UTF8.GetString(buff);
            }

            public byte[] ByteValue()
            {
                if (Ptr == IntPtr.Zero) return null;
                var buff = new byte[Length];
                Marshal.Copy(Ptr, buff, 0, buff.Length);
                return buff;
            }

            public Data(string value)
            {
                var buff = Encoding.UTF8.GetBytes(value);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(buff.Length);
                Marshal.Copy(buff, 0, unmanagedPointer, buff.Length);
                this.Ptr = unmanagedPointer;
                this.Length = (uint)buff.Length;
            }

            public Data(byte[] value)
            {
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(value.Length);
                Marshal.Copy(value, 0, unmanagedPointer, value.Length);
                this.Ptr = unmanagedPointer;
                this.Length = (uint)value.Length;
            }

            public void Free()
            {
                if (this.Ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.Ptr);
                }
            }
        }
    }
}