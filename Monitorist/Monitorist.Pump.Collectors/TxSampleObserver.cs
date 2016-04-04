using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tx.Windows;

namespace Monitorist.Pump.Collectors
{
	class TxSampleObserver : IObserver<PerformanceSample>
	{
		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnNext(PerformanceSample value)
		{
			throw new NotImplementedException();
		}
	}
}
