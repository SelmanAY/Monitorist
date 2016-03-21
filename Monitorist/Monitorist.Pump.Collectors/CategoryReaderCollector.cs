using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Collectors
{
    public class CategoryReaderCollector : Monitorist.Pump.Core.ICollector
    {
        public void Collect()
        {
            throw new NotImplementedException();
        }

        public void Initialize(string senderConfiguration)
        {
            throw new NotImplementedException();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
