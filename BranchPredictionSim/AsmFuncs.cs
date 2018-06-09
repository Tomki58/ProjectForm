using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{
    class ASMFunctions
    {
        //funcname to func map
        internal static Dictionary<string, Func<List<string>, Executor, bool>> cmdDict = new Dictionary<string, Func<List<string>, Executor, bool>>() {
            {"add", ASMFunctions.add },
            {"sub", ASMFunctions.sub },
            {"mul", ASMFunctions.mul },
            {"div", ASMFunctions.div },
            {"mov", ASMFunctions.mov },
            {"cmp", ASMFunctions.cmp },
        };

        internal static Dictionary<string, Func<Executor, bool>> jumpDict { get; private set; } = new Dictionary<string, Func<Executor, bool>>()
        {
            {"je",  ASMFunctions.je},
            {"jne", ASMFunctions.jne},
            {"jle", ASMFunctions.jle},
            {"jg", ASMFunctions.jg},
            {"jp", ASMFunctions.jp},
            {"jnp", ASMFunctions.jnp},
            {"js", ASMFunctions.js},
            {"jns", ASMFunctions.jns}
        };

        //todo calls
        //todo loops
        //todo retur statement
        //todo some cmds
        //todo code finished catch exception
        public static bool mov(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            float value = Convert.ToInt32(cmd[1]);

            firstOp = value;
            // upadte stats
            executor.updateStats(cmd[0], firstOp);
            return true;
        }

        public static bool add(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]); 

            firstOp = firstOp + secondOp;
            // update stats
            executor.updateStats(cmd[0], firstOp);
            executor.zf = (firstOp) == 0 ? 1 : 0;
            executor.sf = (firstOp) < 0 ? 1 : 0;
            executor.pf = (firstOp) % 2 == 0 ? 1 : 0;
            // upd flags
            executor.updateStats("zf", executor.zf);
            executor.updateStats("sf", executor.sf);
            executor.updateStats("pf", executor.pf);

            return true;
        }

        public static bool sub(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);

            firstOp = firstOp - secondOp;
            // upadte stats
            executor.updateStats(cmd[0], firstOp);
            executor.zf = (firstOp) == 0 ? 1 : 0;
            executor.sf = (firstOp) < 0 ? 1 : 0;
            executor.pf = (firstOp) % 2 == 0 ? 1 : 0;
            // upd flags
            executor.updateStats("zf", executor.zf);
            executor.updateStats("sf", executor.sf);
            executor.updateStats("pf", executor.pf);

            return true;
        }

        public static bool mul(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);

            firstOp = firstOp * secondOp;
            // upadte stats
            executor.updateStats(cmd[0], firstOp);
            return true;
        }

        public static bool div(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);

            executor.edx = (int)executor.eax % (int)firstOp;
            executor.eax = (int) executor.eax / (int) firstOp;
            // upadte stats
            executor.updateStats("edx", executor.edx);
            executor.updateStats("eax", executor.eax);
            return true;
        }
        
        public static bool cmp(List<string> cmd, Executor executor)
        {
            ref float firstOp = ref executor.operandToVar(cmd[0]);
            ref float secondOp = ref executor.operandToVar(cmd[1]);

            executor.zf = (firstOp - secondOp) == 0 ? 1 : 0;
            executor.sf = (firstOp - secondOp) < 0 ? 1 : 0;
            executor.pf = (firstOp - secondOp) % 2 == 0 ? 1 : 0;

            // upd flags
            executor.updateStats("zf", executor.zf);
            executor.updateStats("sf", executor.sf);
            executor.updateStats("pf", executor.pf);

            return true;
        }

        public static bool je(Executor executor)
        {
            return executor.zf == 1 ? true : false;
        }

        public static bool jne(Executor executor)
        {
            return executor.zf == 0 ? true : false;
        }

        public static bool jle(Executor executor)
        {
            if (executor.zf == 1 || executor.sf == 1)
                return true;
            return false;
        }

        public static bool jg(Executor executor)
        {
            return !jle(executor);
        }

        public static bool jp(Executor executor)
        {
            return executor.pf == 1 ? true : false;
        }

        public static bool jnp(Executor executor)
        {
            return !jp(executor);
        }

        public static bool js(Executor executor)
        {
            return executor.sf == 1 ? true : false;
        }

        public static bool jns(Executor executor)
        {
            return !js(executor);
        }


    }
}
