using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//done
namespace BranchPredictionSim.Predictors
{
    class AlwaysPredictJump : IBranchPredictor
    {
        public AlwaysPredictJump() { }

        public void notfyJump(int lineNum, bool jmpTaken)
        {
            return;
        }

        public bool shouldJump(List<string> codeLine, int lineNumber, Executor executor)
        {
            return true;
        }
    }
}
