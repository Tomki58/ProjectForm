using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim.CmdParams
{
    class FlagReg 
    {
        public bool Flag { get; set; }
        public FlagReg(bool flag)
        {
            Flag = flag;
        }
    }
}
