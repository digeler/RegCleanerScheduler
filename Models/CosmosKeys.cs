using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegCleanerScheduler
{
    internal class CosmosKeys
    {
        public string primaryMasterKey { get; set; }
        public string secondaryMasterKey { get; set; }
        public string primaryReadonlyMasterKey { get; set; }
        public string secondaryReadonlyMasterKey { get; set; }
    }
}
