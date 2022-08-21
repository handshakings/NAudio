using System;
using System.Collections.Generic;

namespace AudioFile
{
    public class Echo : IEffect
    {
        public int EchoLength { get; set; }
        public float EchoFactor { get; set; }
        public Queue<float> Samples;
        public Echo(int length = 2000, float factor = 0.5f)
        {
            EchoLength = length;
            EchoFactor = factor;
            Samples = new Queue<float>();
            for(int i = 0; i < length; i++)
            {
                Samples.Enqueue(0.0f);
            }
        }
        public float ApplyEffect(float sample)
        {
            Samples.Enqueue(sample);
            float processedSample = sample + EchoFactor * Samples.Dequeue();
            //To keep sample between -1 and +1
            float OneOrMinusOne = Math.Min(1,Math.Max(-1, processedSample));
            return OneOrMinusOne;
        }
    }
}
