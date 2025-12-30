
using Prism.Commands;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using voskwpf.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace voskwpf.ViewModels
{
	public class VoskViewModel : INotifyPropertyChanged
	{
		private VoskModel model;
		private Mutex model_mutex = new Mutex();
		private List<string> words = null;
		private string word = "";
		private string cmdDelete = "delete";
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _recognisedText;

		public string RecognisedText
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
			model_mutex.WaitOne();
			try
			{
				words.Clear();
				RecognisedText = "";
				if (model.IsWorking)
				{
					model.Stop();
					words.Clear();
					model.Start();
				}
				else
				{
					model.Start();
				}
			}
			finally
			{
				model_mutex.ReleaseMutex();
			}
		}

		private void VoskEventHandler(object sender, PartialDataEventArgs args)
		{
			string aword = args.PartialData.Trim();
			if (aword != word)
			{
				word = aword;
				if ( string.Compare(aword, cmdDelete,true) == 0)
				{
					RecognisedText = "";
					words.Clear();


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
		public VoskViewModel()
		{
			words = new List<string>();
			model_mutex.WaitOne();
			try
			{
				this.model = App.Services.GetService<VoskModel>();


				this.RecognisedText = "sample";
				model.PartialDataReady += VoskEventHandler;
			}
			finally { 
				model_mutex.ReleaseMutex(); 
			}
			//Record = new WPFCommand((obj) => { DoRecord(obj); }); 
		}
	}
}
