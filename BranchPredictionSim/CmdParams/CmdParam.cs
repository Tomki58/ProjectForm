using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{
    abstract class CmdParam<T>
    {
        public abstract T Value { get; set; }
    }
}
