using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim.Predictors
{
    class Gshare : IBranchPredictor
    {
        private int maxHigh;
        private Dictionary<int, int> predictCounter = new Dictionary<int, int>();//linenum:nbitcounter
        private Dictionary<int, List<bool>> predictHistory = new Dictionary<int, List<bool>>();//linenum:history of branching

        public Gshare(int n)
        {
            maxHigh = 2 ^ n;
        }

        public void notfyJump(int lineNum, bool jmpTaken)
        {
            if (!predictHistory.ContainsKey(lineNum))
                predictHistory[lineNum] = new List<bool>();
            predictHistory[lineNum].Add(jmpTaken);

            int predictCounterAddr = predictHistoryAndLineNumIdentifier(lineNum);
            predictCounter[predictCounterAddr] = predictCounter[predictCounterAddr] + (jmpTaken ? 1 : -1);
            if (predictCounter[predictCounterAddr] < 0)
                predictCounter[predictCounterAddr] = 0;
            if (predictCounter[predictCounterAddr] >= maxHigh)
                predictCounter[predictCounterAddr] = maxHigh - 1;
        }

        public bool shouldJump(List<string> codeLine, int lineNumber, Executor executor)
        {
            int predictCounterAddr = predictHistoryAndLineNumIdentifier(lineNumber);
            if (!predictCounter.ContainsKey(predictCounterAddr))
                predictCounter[predictCounterAddr] = maxHigh / 2;
            return predictCounter[predictCounterAddr] >= (maxHigh / 2);
        }

        private int predictHistoryAndLineNumIdentifier(int lineNum)
        {
            string predictHistoryBinary = "";
            if (!predictHistory.ContainsKey(lineNum))
                predictHistory[lineNum] = new List<bool>();
            for (int i = 0; i < 4; i++)
            {
                if (predictHistory[lineNum].Count <= i)
                    return lineNum;
                predictHistoryBinary += predictHistory[lineNum][predictHistory[lineNum].Count - i - 1] ? "1" : "0";
            }
            return lineNum ^ int.Parse(predictHistoryBinary);
        }
    }
}
