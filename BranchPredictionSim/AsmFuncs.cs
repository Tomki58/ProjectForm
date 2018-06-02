using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{
    class ASMFunctions
    {
        //add some cmds
        public static bool mov(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            float value = Convert.ToInt32(cmd[1]);

            firstOp = value;
            return true;
        }

        public static bool add(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]); 

            firstOp = firstOp + secondOp;
            return true;
        }

        public static bool sub(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);

            firstOp = firstOp - secondOp;
            return true;
        }

        public static bool mul(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);

            firstOp = firstOp * secondOp;
            return true;
        }

        public static bool div(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);

            executor.edx = (int)executor.eax % (int)firstOp;
            executor.eax = (int) executor.eax / (int) firstOp;
            
            return true;
        }
        //todo
        public static bool cmp(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);
            executor.zf = 0;
            if (firstOp == secondOp)
            {
                executor.zf = 1;
            }
            return true;
        }
    }
}
