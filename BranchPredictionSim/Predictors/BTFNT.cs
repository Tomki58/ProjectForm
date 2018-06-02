using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//done
namespace BranchPredictionSim.Predictors 
{
    //backwards taken, forwards not taken
    class BTFNT : IBranchPredictor
    {
        public void notfyJump(int lineNum, bool jmpTaken)
        {
            return;
        }

        public bool shouldJump(List<string> codeLine, int lineNumber, Executor executor)
        {
            return (executor.labelDict[codeLine[1]] < lineNumber);
        }
    }
}
