using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//done
namespace BranchPredictionSim.Predictors
{
    class NBitPredictor : IBranchPredictor
    {
        private int predictMem;
        private int maxHigh;

        public NBitPredictor(int n)
        {
            predictMem = 2 ^ (n - 1);
            maxHigh = 2 ^ n;
        }

        public void notfyJump(int lineNum, bool jmpTaken)
        {
            predictMem = predictMem + (jmpTaken ? 1 : -1);
            if (predictMem < 0)
                predictMem = 0;
            if (predictMem >= maxHigh)
                predictMem = maxHigh - 1;
        }

        public bool shouldJump(List<string> codeLine, int lineNumber, Executor executor)
        {
            return predictMem >= (maxHigh / 2);
        }
    }
}
