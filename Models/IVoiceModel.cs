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
	public interface IVoiceModel
	{
		public event EventHandler<PartialDataEventArgs>? PartialData;
		public bool IsWorking { get; }
		public void Start();
		public void Stop();

	}
}
