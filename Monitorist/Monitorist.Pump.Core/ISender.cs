using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Core
{
    public interface ISender
    {
        void Initialize(string senderConfiguration, System.Threading.Tasks.Dataflow.BufferBlock<CounterValueModel> inputBlock);
    }
}
