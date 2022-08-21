using NAudio.Wave;
using System;
using System.Windows.Forms;

namespace AudioFile
{
    public partial class GiawaVideoPlaylist : Form
    {
        
        WaveOut waveOut;

        //.wav file is uncompressed pcm data
        //.mp3 is compressed pcm data
        //Below stream convert uncompressed pcm data from mp3 to pcm. This pcm is large number of blocks to be provided to speaker.
        BlockAlignReductionStream stream;

        public GiawaVideoPlaylist()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Media Files|*.wav;*.mp3;*.m4a;*.WAV;*.AVI;*.MP3;*.MOV;*.3GP;*.3GPP;*.M4A;";
            ofd.ShowDialog();
            string fileName = ofd.FileName;

            if(fileName.EndsWith(".mp3"))
            {
                Mp3FileReader mp3FileReader = new Mp3FileReader(fileName);
                WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3FileReader);
                stream = new BlockAlignReductionStream(pcm);
            }
            else if(fileName.EndsWith(".wav"))
            {
                WaveStream pcm = new WaveChannel32(new WaveFileReader(fileName));
                stream = new BlockAlignReductionStream(pcm);
            }
            
            waveOut = new WaveOut();
            waveOut.Init(stream);
            waveOut.Play();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Pause();
            }
            else if(waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Play();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 file|*.mp3";
            openFileDialog.ShowDialog();
            string mp3FileName = openFileDialog.FileName;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV file|*.wav";
            saveFileDialog.ShowDialog();
            string wavFileName = saveFileDialog.FileName;

            using(Mp3FileReader mp3FileReader = new Mp3FileReader(mp3FileName))
            {
                using(WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3FileReader))
                {
                    WaveFileWriter.CreateWaveFile(wavFileName, pcm);
                }
            }
        }





        private void button4_Click(object sender, EventArgs e)
        {
            WaveTone waveTone = new WaveTone(double.Parse(textBox1.Text), double.Parse(textBox2.Text));
            stream = new BlockAlignReductionStream(waveTone);

            waveOut = new WaveOut();
            waveOut.Init(stream);
            waveOut.Play();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            waveOut.Stop();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio file|*.wav;*.mp3";
            openFileDialog.ShowDialog();
            string wavFileName = openFileDialog.FileName;

            WaveFileReader waveFileReader = new WaveFileReader(wavFileName);   
            WaveChannel32 waveChannel32 = new WaveChannel32(waveFileReader);
            EffectStream effectStream = new EffectStream(waveChannel32);
            stream = new BlockAlignReductionStream(effectStream);

            for(int i = 0; i < waveChannel32.WaveFormat.Channels; i++)
            {
                effectStream.Effects.Add(new Echo());
            }

            waveOut = new WaveOut();
            waveOut.Init(stream);
            waveOut.Play();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if(waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Pause();
            }
            else if(waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Play();
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            new Form1().ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wav file|*.wav";
            openFileDialog.ShowDialog();
            string waveFile = openFileDialog.FileName;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV file|*.wav";
            saveFileDialog.ShowDialog();
            string waveFileNew = saveFileDialog.FileName;

            WaveFileReader waveFileReader = new WaveFileReader(waveFile);
            WaveFileWriter waveFileWriter = new WaveFileWriter(waveFileNew, new WaveFormat(44100, 1));

            for (int i = 0; i < waveFileReader.SampleCount; i++)
            {
                float[] sample = waveFileReader.ReadNextSampleFrame();
                waveFileWriter.WriteSample(sample[0]);
                waveFileWriter.Flush();

            }
            waveFileWriter.Close();

            WaveChannel32 waveChannel32 = new WaveChannel32(waveFileReader);            
            EffectStream effectStream = new EffectStream(waveChannel32);
            stream = new BlockAlignReductionStream(effectStream);

            for (int i = 0; i < waveChannel32.WaveFormat.Channels; i++)
            {
                effectStream.Effects.Add(new Echo());
            }
            stream.CopyTo(waveFileWriter);
            waveOut = new WaveOut();
            waveOut.Init(stream);
            waveOut.Play();

        }
    }



    public class WaveTone : WaveStream
    {
        private double frequency;
        private double amplitude;
        private double time;

        public WaveTone(double f, double a)
        {
            time = 0;
            frequency = f;
            amplitude = a;
        }
        public override WaveFormat WaveFormat
        {
            get { return new WaveFormat(44100,16,1); }
        }

        public override long Length
        {
            get { return long.MaxValue; }
        }

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //here count is 16 bit depth
            int samples = count / 2;
            //create sine from each sample
            for(int i = 0; i < samples; i++)
            {
                double sine = amplitude * Math.Sin(Math.PI * 2 * frequency * time);
                time += 1.0 / 44100;
                short truncated = (short)Math.Round(sine * (Math.Pow(2, 15) - 1));
                buffer[i * 2] = (byte)(truncated & 0x00ff);
                buffer[i * 2 +1] = (byte)(truncated & 0xff00 >> 8);

            }
            return count;
        }
    }
}
