using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Whisper.net;

namespace voskwpf.Models
{
	public class WhisperModel : IVoiceModel
	{
		private IWaveIn? waveIn = null;
		private WaveFileWriter? writer = null;
		private bool IsWriterInUse = false;

		private Stream memstream;

		private readonly string? DICT;
		private const string WAVE_PATH = @"C:\sound\test.wav";

		protected Thread? whisperLoop = null;

		public event EventHandler<PartialDataEventArgs>? PartialData;
		public event EventHandler<RecordingStateChangeEventArgs>? RecordingStateChange;

		private void OnRecognise(string data)
		{
			PartialDataEventArgs args = new PartialDataEventArgs(data); 
			if (this.PartialData != null)
				this.PartialData.Invoke(this, args);
		}

		private void OnRecordingStateChange(bool isrecording)
		{
			if (RecordingStateChange != null)
			{
				RecordingStateChangeEventArgs args = new RecordingStateChangeEventArgs(isrecording);
				RecordingStateChange.Invoke(this, args);
			}
		}


		public bool IsWorking { get; private set; }

		public bool IsRecording { get; private set; }
		public void Start()
		{
			if (!IsWorking)
			{
				IsWorking = true;

				if (this.waveIn == null)
				{

					//var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

					this.waveIn = new WaveInEvent();//WasapiLoopbackCapture((MMDevice)device);
					waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
					this.writer = new WaveFileWriter(WAVE_PATH, this.waveIn.WaveFormat);
					this.waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(this.OnDataAvailable);
					this.waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(OnRecordingStopped);
				}
				this.waveIn.StartRecording();
				this.IsRecording = true;
				this.OnRecordingStateChange(true);


				//Init(this);
				//whisperLoop = new Thread(() => Init(this));
				//whisperLoop.Start();
				/*recogniseLoop = new Thread(() => 
				
				{ 
					Init(this); 
				});*/
			}
		}

		public virtual void Stop()
		{
			if (this.waveIn != null)
			{
				this.waveIn.StopRecording();
				this.IsRecording = false;
				this.OnRecordingStateChange(false);
			}
		}

		private void OnDataAvailable(object? sender, WaveInEventArgs e)
		{
			Debug.WriteLine("recorded: " + e.BytesRecorded);
			this.writer.Write(e.Buffer, 0, e.BytesRecorded);
		}

		public async void OnRecordingStopped(object? sender, StoppedEventArgs e)
		{
			this.writer.Close();
			this.writer.Dispose();
			this.writer = null;
			this.waveIn.Dispose();
			this.waveIn = null;
			this.IsWorking = false;

			await this.RecogniseAsync();
		}

		private async Task RecogniseAsync()
		{
			using var factory = WhisperFactory.FromPath(DICT);
			using var processor = factory.CreateBuilder()
			.WithLanguage("en").Build();

			using var filestream = File.OpenRead(WAVE_PATH);
			string all = "";
			await foreach (var result in processor.ProcessAsync(filestream))
			{
				if (!string.IsNullOrEmpty(result.Text))
				{
					Debug.WriteLine("result: " + result.Text);
					all += result.Text;
				}
			}

			if (!string.IsNullOrEmpty(all.Trim()))
			{
				Debug.WriteLine("recognise: "  +  all);
				this.OnRecognise(all);
			}
				
		
		}




		public WhisperModel()
		{
			this.memstream = new MemoryStream();
			this.DICT = ConfigurationManager.AppSettings["Whisper_dict"];
		}
	}
}
