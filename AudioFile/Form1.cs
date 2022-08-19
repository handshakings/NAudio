using NAudio.Wave;
using System;
using System.Windows.Forms;
using NAudio.Lame;
using System.Diagnostics;
using System.IO;

namespace AudioFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        WaveIn waveIn;      //api for recording
        WaveOut waveOut;    //api for playback
        BufferedWaveProvider waveProvider; //To store audio samples
        WaveFileWriter waveFileWriter;      //To write bytes to a wave file

        //Below section to record and playback audio
        private void Form1_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var deviceInfo = WaveIn.GetCapabilities(i);
                comboBox1.Items.Add(deviceInfo.ProductName);
            }
            comboBox1.SelectedIndex = 0;

            for(int i = 0;i < WaveOut.DeviceCount; i++)
            {
                var deviceInfo = WaveOut.GetCapabilities(i);
                comboBox2.Items.Add(deviceInfo.ProductName);
            }
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button4.Enabled = true;

            waveIn = new WaveIn();
            waveIn.DeviceNumber = comboBox1.SelectedIndex;

            waveOut = new WaveOut();
            waveOut.DeviceNumber = comboBox2.SelectedIndex;
            waveOut.DesiredLatency = int.Parse(comboBox3.Text); //by default is 300 milliseconds
            waveIn.WaveFormat = new WaveFormat(48000, 1);
            waveProvider = new BufferedWaveProvider(waveIn.WaveFormat);

            waveOut.Volume = volumeSlider1.Volume;
            waveOut.Init(waveProvider);
            waveOut.Play();

            //Handle data available at wavein instance
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();    
        }
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveProvider.AddSamples(e.Buffer,0,e.BytesRecorded);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
            button4.Enabled = false;

            waveIn.StopRecording();
            waveIn.Dispose();
            waveOut.Dispose();
        }



        //Below section to record and save audio.wave file
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            DialogResult result = saveFileDialog.ShowDialog();
            string fileName = saveFileDialog.FileName;

            waveIn = new WaveIn();
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveIn.DeviceNumber = comboBox1.SelectedIndex;
            waveIn.DataAvailable += WaveIn_DataAvailable1;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;
            waveFileWriter = new WaveFileWriter(fileName, waveIn.WaveFormat);
            waveIn.StartRecording();
        }
        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            waveFileWriter.Dispose();
            waveIn.Dispose();
        }
        private void WaveIn_DataAvailable1(object sender, WaveInEventArgs e)
        {
            waveFileWriter.Write(e.Buffer,0,e.BytesRecorded);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            waveIn.StopRecording();

        }




        //Below section is to convert audio from one format to another using NAudio.Lame package
        string inputWaveFile;
        string outputMp3File;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            inputWaveFile = openFileDialog.FileName;


        }
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MP3 files|*.mp3";
            saveFileDialog.ShowDialog();
            outputMp3File = saveFileDialog.FileName;

        }
        private void button7_Click(object sender, EventArgs e)
        {
            //Benefit of using() expression is that any identifier/writer/variable etc declared inside would be 
            //disposed automatically 
            using (AudioFileReader audioFileReader = new AudioFileReader(inputWaveFile))
            using (LameMP3FileWriter lameMP3FileWriter = new LameMP3FileWriter(outputMp3File, audioFileReader.WaveFormat, 128))
            audioFileReader.CopyTo(lameMP3FileWriter);
        }





        //Add desiredLatency (echo) and Play
        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files|*.mp3";
            openFileDialog.ShowDialog();
            string file = openFileDialog.FileName;

            waveOut = new WaveOut();
            waveOut.DesiredLatency = int.Parse(comboBox3.Text);
            waveProvider = new BufferedWaveProvider(new WaveFormat(44100,1));
            waveOut.Volume = volumeSlider1.Volume;
            waveOut.Init(waveProvider);
            waveOut.Play();

            using (AudioFileReader audioFileReader = new AudioFileReader(file))
            {
                while(audioFileReader.CanRead)
                {
                    byte[] buffer = new byte[1024];
                    int rec = audioFileReader.Read(buffer, 0, buffer.Length);
                    Array.Resize(ref buffer, rec);
                    waveProvider.AddSamples(buffer,0, buffer.Length);
                }
            }
            
            
            
            
        }
    }
}
