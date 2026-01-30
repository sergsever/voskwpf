using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace voskwpf.Models
{
	public class PartialDataEventArgs
	{
		public string PartialData { get; set; }
		public PartialDataEventArgs(string data)
		{
			this.PartialData = data;
		}
	}

	public class RecordingStateChangeEventArgs
	{
		public bool IsRecording { get; set; }
		public RecordingStateChangeEventArgs(bool isRecording)
		{
			this.IsRecording = isRecording;
		}
	}
		public interface IVoiceModel
		{
			public event EventHandler<PartialDataEventArgs>? PartialData;
			public event EventHandler<RecordingStateChangeEventArgs>? RecordingStateChange;
			public bool IsWorking { get; }
			public void Start();
			public void Stop();

		}
	}

