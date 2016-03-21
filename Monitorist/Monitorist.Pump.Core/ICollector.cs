using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Core
{
    public interface ICollector
    {
        void Collect();
        void Initialize(string senderConfiguration);
    }
}
