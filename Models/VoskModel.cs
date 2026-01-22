
using Microsoft.VisualBasic;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Vosk;

namespace voskwpf.Models
{

	

	public class RecordingStateEventArgs
	{
		public bool IsRecording { get; set; }
		public RecordingStateEventArgs(bool isRecording)
		{
			this.IsRecording = isRecording; }
	}
	public class VoskModel : IVoiceModel
	{
		private readonly string WAVE_PATH = @"C:\sound\test.wav";

		// синхронизировать!
		private  WaveFileWriter? writer;
		//private static object writer_lock = new object(); 
		//private static Mutex writer_mutex = new Mutex();
		private VoskRecognizer? recognizer;
		WaveInEvent? waveIn = null;
		//private static Model? dictionary = null;

		//private bool isWorking = true;
		Thread voskLoop;

		public event EventHandler<PartialDataEventArgs>? PartialData;
		public event EventHandler<RecordingStateEventArgs>? RecordingStateChanged;

		public void OnPartialDataReady(string partial)
		{
			if (PartialData != null)
			{
				PartialDataEventArgs args = new PartialDataEventArgs(partial);
				PartialData.Invoke(this, args);
			}
		}

		public void OnRecordingState(bool isrecording)
			{
				Debug.WriteLine("Recording state: " +  isrecording);
			if (RecordingStateChanged != null)
				if(RecordingStateChanged != null)
				{
					RecordingStateEventArgs args = new RecordingStateEventArgs(isrecording);
					RecordingStateChanged.Invoke(this, args);
				}
			}

		public virtual async void Start()
		{
			if (this.waveIn == null)
			{
				this.waveIn = new WaveInEvent();
				this.waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
				this.writer = new WaveFileWriter(WAVE_PATH, this.waveIn.WaveFormat);
				this.waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(this.OnDataAvailable);
				this.waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(this.OnStopRecording);
				this.waveIn.StartRecording();
				this.IsWorking = true;
				this.IsRecording = true;
			}
		}

		public bool IsWorking { get; private set; }

		public bool IsRecording { get;
			private set; 
		}
		public void Stop()
		{
			this.waveIn.StopRecording();
			this.IsWorking = false;
			this.IsRecording = false;
			
		}


		private void OnStopRecording(object? sender, StoppedEventArgs e)
		{
			if(this.waveIn != null)
			{
				this.waveIn.Dispose();
				this.waveIn = null;
				this.writer.Close();
				this.writer.Dispose();
				this.waveIn = null;
				this.Recognise();
			}
		}

		private async void Recognise()
		{
			if(!this.IsRecording)
			{
				using var filestream = File.OpenRead(WAVE_PATH);
				byte[] buffer = new byte[filestream.Length];
				filestream.Read(buffer, 0, buffer.Length);
				this.recognizer.AcceptWaveform(buffer, buffer.Length);
				string result = this.recognizer.FinalResult();

				if(!string.IsNullOrEmpty(result.Trim()))
				{
					Json json = JsonConvert.DeserializeObject<Json>(result);
					Debug.WriteLine("result: " + json.Text.Trim());
					OnPartialDataReady(json.Text.Trim());
				}
			}
		}
		private void OnDataAvailable(object? sender, WaveInEventArgs e)
		{
			this.writer?.Write(e.Buffer, 0, e.BytesRecorded);
		}

		public VoskModel() 
		{
			IsWorking = false;
			//Model dict = new Model(@"C:\Sound\vosk-model-small-en-us-0.15");
			Model dict = new Model(@"C:\sound\vosk-model-en-us-0.22-lgraph");
			recognizer = new VoskRecognizer(dict, 16000f);
		}
	}
}