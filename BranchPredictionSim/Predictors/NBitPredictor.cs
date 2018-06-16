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
        private int maxHigh;
        private Dictionary<int, int> predictCounter = new Dictionary<int, int>();//linenum:nbitcounter

        public NBitPredictor(int n)
        {
            maxHigh = 2 ^ n;
        }

        public void notfyJump(int lineNum, bool jmpTaken)
        {
            predictCounter[lineNum] = predictCounter[lineNum] + (jmpTaken ? 1 : -1);
            if (predictCounter[lineNum] < 0)
                predictCounter[lineNum] = 0;
            if (predictCounter[lineNum] >= maxHigh)
                predictCounter[lineNum] = maxHigh - 1;
        }

        public bool shouldJump(List<string> codeLine, int lineNumber, Executor executor)
        {
            if (!predictCounter.ContainsKey(lineNumber))
                predictCounter[lineNumber] = maxHigh / 2;
            return predictCounter[lineNumber] >= (maxHigh / 2);
        }
    }
}
