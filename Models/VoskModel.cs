
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

	public class PartialDataEventArgs
	{
		public string PartialData { get; set; }
		public PartialDataEventArgs(string data) {
			this.PartialData = data;
		}
	}
	public class VoskModel
	{
		// синхронизировать!
		static WaveFileWriter? writer;
		//private static object writer_lock = new object(); 
		private static Mutex writer_mutex = new Mutex();
		static VoskRecognizer? recognizer;

		//private bool isWorking = true;
		Thread voskLoop;

		public event EventHandler<PartialDataEventArgs>? PartialDataReady;

		public void OnPsrtialDataReady(string partial)
		{
			if (PartialDataReady != null)
			{
				PartialDataEventArgs args = new PartialDataEventArgs(partial);
				PartialDataReady.Invoke(this, args);
			}
		}

		public void Start()
		{

			if (!IsWorking)
			{
				IsWorking = true;
				voskLoop = new Thread(() => Init(this));
				voskLoop.Start();
			}
		}

		public bool IsWorking { get; private set; } 
		public void Stop()
		{
			IsWorking = false;

			voskLoop.Join();
			Debug.WriteLine("End thread: " + voskLoop.ManagedThreadId);

			//voskLoop.Interrupt();
			
		}



		private void SomeRecorded(object? sender, WaveInEventArgs e)
		{
			
				Console.WriteLine("Event:\n");
				
				writer_mutex.WaitOne();
				try { 
				
					writer?.Write(e.Buffer, 0, e.BytesRecorded);

					//recognizer?.Reset();
					recognizer?.AcceptWaveform(e.Buffer, e.BytesRecorded);
				}
				finally
				{
					writer_mutex.ReleaseMutex();
				}

				try { 

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
		}
		protected async Task Init(VoskModel model)
		{
			Debug.WriteLine("start thread: " + Thread.CurrentThread.ManagedThreadId);

			Model dict = new /*Model(@"C:\Sound\vosk-model-small-en-us-0.15");*/ Model(@"C:\sound\vosk-model-en-us-0.22");
			recognizer = null;
			recognizer = new VoskRecognizer(dict, 16000f);
			WaveInEvent waveIn = new WaveInEvent();
			waveIn.DataAvailable += model.SomeRecorded;
			waveIn.WaveFormat = new WaveFormat(16000, 1);
			await Task.Delay(1000);
			writer_mutex.WaitOne();
			try
			{
				writer = new WaveFileWriter(@"C:\sound\test.wav", waveIn.WaveFormat);
				waveIn.StartRecording();

			}
			finally
			{
				writer_mutex.ReleaseMutex();
			}

			while (IsWorking)
			{
				//Thread.Sleep(1000);
				bool inner = IsWorking;
				await Task.Delay(250);
			}
			Debug.WriteLine("end thread: " + Thread.CurrentThread.ManagedThreadId);

			waveIn.StopRecording();

			return;
		}

		public VoskModel() 
		{
			IsWorking = false;
			//voskLoop = new Thread(() => Init(this));
			//voskLoop.Start();

		}
	}
}