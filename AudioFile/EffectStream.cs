using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace AudioFile
{
    //This class takes WaveStream and expose uncompressed pcm data
    public class EffectStream : WaveStream
    {
        public override WaveFormat WaveFormat
        {
            get { return SourceStream.WaveFormat; }
        }

        public override long Length
        {
            get { return SourceStream.Length; }
        }

        public override long Position
        {
            get
            {
                return SourceStream.Position;
            }
            set
            {
                SourceStream.Position = value;
            }
        }

        public WaveStream SourceStream { get; set; }
        public List<IEffect> Effects { get; set; }
        public EffectStream(WaveStream waveStream)
        {
            SourceStream = waveStream;
            Effects = new List<IEffect>();
        }
        private int channel = 0; 
        public override int Read(byte[] buffer, int offset, int count)
        {
            Console.WriteLine($"Received bytes {count}");
            int read = SourceStream.Read(buffer, offset, count);
            for (int i = 0; i < read / 4; i++)
            {
                //Below line convert 4bytes=32bits into single floating point number
                float sample = BitConverter.ToSingle(buffer, i * 4);
                //sample = sample * 0.5F;

                if (Effects.Count == WaveFormat.Channels)
                {
                    sample = Effects[channel].ApplyEffect(sample);
                    channel = (channel + 1) % WaveFormat.Channels;
                }

                //bytes.CopyTo(buffer,i*4);
                //Below line convert back sample with effect into 4 bytes
                byte[] bytes = BitConverter.GetBytes(sample);
                buffer[i * 4 + 0] = bytes[0];
                buffer[i * 4 + 1] = bytes[1];
                buffer[i * 4 + 2] = bytes[2];
                buffer[i * 4 + 3] = bytes[3];

            }
            return read;
        }
    }
}
