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
	internal interface IVoiceModel
	{
		public event EventHandler<PartialDataEventArgs>? PartialData;
	}
}
