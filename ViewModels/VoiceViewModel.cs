
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using voskwpf.Models;

namespace voskwpf.ViewModels
{
	public class VoiceViewModel : INotifyPropertyChanged
	{
		private IVoiceModel model;
		private Mutex model_mutex = new Mutex();
		private List<string> words = null;
		private string word = "";
		private string cmdDelete = "delete";
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string? _recognisedText;
		private bool _isRecording = false;


		public bool IsRecording { get { return _isRecording; }
						private set
			{
				if (_isRecording != value) {
					_isRecording = value;
					OnPropertyChanged("IsRecording");
						};
			}
		}
		public string? RecognisedText
		{
			get { return _recognisedText; }
			set
			{
				if (_recognisedText != value)
				{
					_recognisedText = value;
					OnPropertyChanged("RecognisedText");
				}
			}
		}

		public ICommand Record
		{
			get
			{
				return new DelegateCommand(DoRecord);
			}
		}


		public void DoRecord()
		{
			try
			{
				lock (model_mutex)
				{
					words.Clear();
					RecognisedText = "";
					if (model.IsWorking)
					{
						model.Stop();
						words.Clear();
						//model.Start();
						//IsRecording = true;
					}
					else
					{
						model.Start();
						//IsRecording = true;

					}
				}
			}
			catch(Exception ex)
			{
				Debug.WriteLine("Start ex: " + ex.Message);
			}
			finally
			{
				//model_mutex.ReleaseMutex();
			}
		}

		private void VoiceEventHandler(object sender, PartialDataEventArgs args)
		{
			string aword = args.PartialData.Trim();
			if (aword != word)
			{
				word = aword;
				if ( string.Compare(aword, cmdDelete,true) == 0)
				{
					RecognisedText = "";
					words.Clear();

					/*
					model_mutex.WaitOne();
					try
					{
						model.Stop();
						words.Clear();
						model.Start();
					}
					finally
					{
						model_mutex.ReleaseMutex();
					}
					*/
				}

				words.Add(aword);
				string text = "";
				foreach (string word in words)
				{
					text += " " + word;
				}
				RecognisedText = text;
			}
			//RecognisedText += args.PartialData;
		}

		private void RecordingStateEventHandler(object sender, RecordingStateEventArgs args)
		{
			if (args.IsRecording)
			{
				this.IsRecording = true;
				OnPropertyChanged("IsRecording");
			}
			else
			{
				this.IsRecording = false;
				OnPropertyChanged("IsRecording");
			}
		}
				public VoiceViewModel()
				{
					words = new List<string>();
					model_mutex.WaitOne();
					try
					{
				if (ConfigurationSettings.AppSettings["engine"] == "vosk")
					this.model = App.Services.GetService<VoskModel>();
				else
					this.model = App.Services.GetService<WhisperModel>();
						


					this.RecognisedText = "sample";
				//this.IsRecording = false;
						model.PartialData += VoiceEventHandler;
						//model.RecordingStateChanged += RecordingStateEventHandler;
					}
					finally {
						model_mutex.ReleaseMutex();
					}
					//Record = new WPFCommand((obj) => { DoRecord(obj); }); 
				}
	}
}
