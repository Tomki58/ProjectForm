using BranchPredictionSim.CmdParams;
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
            {"pop", ASMFunctions.pop },
            {"push", ASMFunctions.push },
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
            {"jns", ASMFunctions.jns},
            {"jmp", ASMFunctions.jmp},
            {"loop", ASMFunctions.loop},
        };
        private const string FIRST_OP_IMM = "Первый операнд не может быть числом";
        //fix next addr increment
        //todo some cmds
        //todo add more predictors

        public static bool push(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            executor.stack.Push(firstOp.Value);
            return true;
        }

        public static bool pop(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            firstOp.Value = executor.stack.Pop();
            return true;
        }

        public static bool mov(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            var secondOp = executor.ParseOperand(cmd[1]);

            firstOp.Value = secondOp.Value;
            return true;
        }

        public static bool add(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            var secondOp = executor.ParseOperand(cmd[1]); 

            firstOp.Value = firstOp.Value + secondOp.Value;
            //upd flags
            executor.zf.Flag = (firstOp.Value) == 0 ? true : false;
            executor.sf.Flag = (firstOp.Value) < 0 ? true : false;
            executor.pf.Flag = (firstOp.Value) % 2 == 0 ? true : false;

            return true;
        }

        public static bool sub(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            var secondOp = executor.ParseOperand(cmd[1]);

            firstOp.Value = firstOp.Value - secondOp.Value;
            //upd flags
            executor.zf.Flag = (firstOp.Value) == 0 ? true : false;
            executor.sf.Flag = (firstOp.Value) < 0 ? true : false;
            executor.pf.Flag = (firstOp.Value) % 2 == 0 ? true : false;

            return true;
        }

        public static bool mul(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            var secondOp = executor.ParseOperand(cmd[1]);

            firstOp.Value = firstOp.Value * secondOp.Value;

            return true;
        }

        public static bool div(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);

            executor.edx.Value = executor.eax.Value % firstOp.Value;
            executor.eax.Value = executor.eax.Value / firstOp.Value;

            return true;
        }
        
        public static bool cmp(List<string> cmd, Executor executor)
        {
            var firstOp = executor.ParseOperand(cmd[0]);
            if (firstOp is Imm)
                throw new FormatException(FIRST_OP_IMM);
            var secondOp = executor.ParseOperand(cmd[1]);

            //upd flags
            executor.zf.Flag = firstOp.Value == secondOp.Value;
            executor.sf.Flag = firstOp.Value < secondOp.Value;
            executor.pf.Flag = (firstOp.Value - secondOp.Value) % 2 == 0;

            return true;
        }

        //jump stuff
        public static bool je(Executor executor)
        {
            return executor.zf.Flag;
        }

        public static bool jne(Executor executor)
        {
            return !executor.zf.Flag;
        }

        public static bool jle(Executor executor)
        {
            return executor.zf.Flag || executor.sf.Flag;
        }

        public static bool jg(Executor executor)
        {
            return !jle(executor);
        }

        public static bool jp(Executor executor)
        {
            return executor.pf.Flag;
        }

        public static bool jnp(Executor executor)
        {
            return !jp(executor);
        }

        public static bool js(Executor executor)
        {
            return executor.sf.Flag;
        }

        public static bool jns(Executor executor)
        {
            return !js(executor);
        }

        public static bool jmp(Executor executor)
        {
            return true;
        }

        public static bool loop(Executor executor)
        {
            if (executor.ecx.Value == 0)
                return false;
            executor.ecx.Value--;
            return true;
        }
    }
}
