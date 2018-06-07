using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{
    class Data
    {
        public Data(string regFlag, float value)
        {
            this.regFlag = regFlag;
            this.value = value;
        }
        public string regFlag { get; set; }
        public float value { get; set; }
    }
}
