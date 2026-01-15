using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace voskwpf.Models
{
	public class WhisperModel : IVoiceModel
	{
		public event EventHandler<PartialDataEventArgs> PartialData;
	}
}
