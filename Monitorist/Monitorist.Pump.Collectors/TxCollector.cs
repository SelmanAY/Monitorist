using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Collectors
{
    public class TxCollector : Monitorist.Pump.Core.ICollector
    {
        public void Collect()
        {
            log.Info("DLL içinden çağırmayı başardık log4net i ");
        }

        public void Initialize(string senderConfiguration)
        {   
            throw new NotImplementedException();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
