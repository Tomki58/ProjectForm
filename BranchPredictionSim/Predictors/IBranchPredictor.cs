using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//domne
namespace BranchPredictionSim
{
    interface IBranchPredictor
    {
        bool shouldJump(List<string> codeLine, int lineNumber, Executor executor);
        void notfyJump(int lineNum, bool jmpTaken);
    }
}
