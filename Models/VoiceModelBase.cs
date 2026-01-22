using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vosk;

namespace voskwpf.Models
{
	public class VoiceModelBase : IVoiceModel 
	{ 
		public class RecordingStateEventArgs
		{
			public bool IsRecording { get; set; }
			public RecordingStateEventArgs(bool isRecording)
			{
				this.IsRecording = isRecording;
			}
		}
			// синхронизировать!
			protected static WaveFileWriter? writer = null;
			//private static object writer_lock = new object(); 
			protected static Mutex writer_mutex = new Mutex();
			protected WaveInEvent? waveIn = null;
			//private static Model? dictionary = null;

			//private bool isWorking = true;
			protected Thread? recogniseLoop;

			public event EventHandler<PartialDataEventArgs>? PartialData;
			public event EventHandler<RecordingStateEventArgs>? RecordingStateChanged;

			protected virtual void OnPartialDataReady(string partial)
			{
				if (PartialData != null)
				{
					PartialDataEventArgs args = new PartialDataEventArgs(partial);
					PartialData.Invoke(this, args);
				}
			}

			public void OnRecordingState(bool isrecording)
			{
				Debug.WriteLine("Recording state: " + isrecording);
				if (RecordingStateChanged != null)
					if (RecordingStateChanged != null)
					{
						RecordingStateEventArgs args = new RecordingStateEventArgs(isrecording);
						RecordingStateChanged.Invoke(this, args);
					}
			}

		protected virtual Task InitAsync(IVoiceModel model) { return Task.CompletedTask;  }

			public virtual void Start()
			{

				if (!IsWorking)
				{
					IsWorking = true;
					IsRecording = false;
					//recogniseLoop = new Thread(() => InitAsync(this));
					//recogniseLoop.Start();
				}
			}

			public bool IsWorking { get; private set; }

			public bool IsRecording
			{
				get;
				private set;
			}
			public virtual void Stop()
			{
				IsWorking = false;

				recogniseLoop.Join();
				waveIn?.StopRecording();
				writer?.Dispose();
				writer = null;
				Debug.WriteLine("stop recording\n");
				IsRecording = false;
				OnRecordingState(false);
				Debug.WriteLine("End thread: " + recogniseLoop.ManagedThreadId);

				//voskLoop.Interrupt();

			}

		protected virtual void SomeRecorded(object? sender, WaveInEventArgs e){}


		public virtual void OnRecordingStopped(object? sender, StoppedEventArgs e){}
		

		
		
			/*{

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

					//recognizer?.Reset();
					//recognizer?.AcceptWaveform(e.Buffer, e.BytesRecorded);

					//Recognise
					string? data = "";//recognizer?.PartialResult();
					//Json? json = JsonConvert.DeserializeObject<Json>(data ?? "");
					//if ((json?.Partial ?? "") != "")
					//{
						this.OnPartialDataReady(data);
					//}

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
			protected virtual async void InitAsync(VoiceModelBase model)
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
			}*/

			public VoiceModelBase()
			{
				IsWorking = false;
				//Model dict = new Model(@"C:\Sound\vosk-model-small-en-us-0.15");
				//Model dict = new Model(@"C:\sound\vosk-model-en-us-0.22-lgraph");

				//recognizer = new VoskRecognizer(dict, 16000f);
				//waveIn = new WaveInEvent();
				//waveIn.DataAvailable += SomeRecorded;
				//waveIn.RecordingStopped += StateOfRecording;
				//waveIn.WaveFormat = new WaveFormat(16000, 1);


				//voskLoop = new Thread(() => Init(this));
				//voskLoop.Start();
				
			}
		}
	}

