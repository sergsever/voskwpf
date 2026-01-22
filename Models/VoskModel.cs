
using Microsoft.VisualBasic;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Diagnostics;
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
	public class VoskModel : VoiceModelBase
	{
		// синхронизировать!
		static WaveFileWriter? writer;
		//private static object writer_lock = new object(); 
		private static Mutex writer_mutex = new Mutex();
		static VoskRecognizer? recognizer;
		WaveInEvent? waveIn = null;
		//private static Model? dictionary = null;

		//private bool isWorking = true;
		Thread voskLoop;

		public event EventHandler<PartialDataEventArgs>? PartialData;
		public event EventHandler<RecordingStateEventArgs>? RecordingStateChanged;

		public void OnPsrtialDataReady(string partial)
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

		public virtual void Start()
		{

			if (!IsWorking)
			{
				IsWorking = true;
				IsRecording = false;
				voskLoop = new Thread(() => Init(this));
				voskLoop.Start();
			}
		}

		public bool IsWorking { get; private set; }

		public bool IsRecording { get;
			private set; 
		}
		public void Stop()
		{
			IsWorking = false;

			voskLoop.Join();
			waveIn?.StopRecording();
			writer?.Dispose();
			writer = null;
			Debug.WriteLine("stop recording\n");
			IsRecording = false;
			OnRecordingState(false);
			Debug.WriteLine("End thread: " + voskLoop.ManagedThreadId);

			//voskLoop.Interrupt();
			
		}

			private void StateOfRecording(object? sender, StoppedEventArgs e)
			{
				Debug.WriteLine("Recording: " + e.ToString());
			if (e.Exception != null)
			{
				Debug.WriteLine("Recording exception: " + e.Exception.Message);
			}
				OnRecordingState(false);
			}


		private void SomeRecorded(object? sender, WaveInEventArgs e)
		{

			//Console.WriteLine("Event:\n");


			try
			{
				writer_mutex.WaitOne();
				try
				{

					writer?.Write(e.Buffer, 0, e.BytesRecorded);
				}
				finally
				{
					writer_mutex.ReleaseMutex();
				}

				recognizer?.Reset();
				recognizer?.AcceptWaveform(e.Buffer, e.BytesRecorded);

				string? data = recognizer?.PartialResult();
				Json? json = JsonConvert.DeserializeObject<Json>(data ?? "");
				if ((json?.Partial ?? "") != "")
				{
					this.OnPsrtialDataReady(json?.Partial);
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("exception: " + ex.ToString());
			}
			finally
			{
				//writer_mutex.ReleaseMutex();
			}
		}
		protected async Task Init(VoskModel model)
		{
			try
			{
				Debug.WriteLine("start thread: " + Thread.CurrentThread.ManagedThreadId);
				

				writer_mutex.WaitOne();
				try
				{
					writer = new WaveFileWriter(@"C:\sound\test.wav", waveIn?.WaveFormat);
				}
				finally
				{
					writer_mutex.ReleaseMutex();
				}

				


				await Task.Delay(1000);
				if (!IsRecording)
				{
					waveIn?.StartRecording();
					Debug.WriteLine("start record\n");
					IsRecording = true;
				}
				OnRecordingState(true);
				//IsRecording = true;
				while (IsWorking)
				{
					//Thread.Sleep(1000);
					bool inner = IsWorking;
					await Task.Delay(250);
				}
				
				Debug.WriteLine("end thread: " + Thread.CurrentThread.ManagedThreadId);


			}
			catch (Exception ex)
			{
				Debug.WriteLine("Init ex " + ex.Message);
			}
			finally
			{
				writer_mutex.ReleaseMutex();
			}

			IsRecording = false;

			return;
		}

		public VoskModel() 
		{
			IsWorking = false;
			//Model dict = new Model(@"C:\Sound\vosk-model-small-en-us-0.15");
			Model dict = new Model(@"C:\sound\vosk-model-en-us-0.22-lgraph");

			recognizer = new VoskRecognizer(dict, 16000f);
			waveIn = new WaveInEvent();
			waveIn.DataAvailable += SomeRecorded;
			//waveIn.RecordingStopped += StateOfRecording;
			waveIn.WaveFormat = new WaveFormat(16000, 1);


			//voskLoop = new Thread(() => Init(this));
			//voskLoop.Start();

		}
	}
}