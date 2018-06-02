using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{

    class Executor
    {
        private const string COMMENT_SYMB = ";";
        private const string LABEL_SYMB = ":";
        private const string HEX_DIGIT_HEADER = "h";
        private const string OCTO_DIGIT_HEADER = "q";
        private const string BYTE_DIGIT_HEADER = "b";
        private const string DECIMAL_DIGIT_HEADER = "d";
        private static string[] JMP_OPS = new string[] { "je", "jne", "jg" };
        //funcname to func map
        private static Dictionary<string, Func<List<string>, Executor, bool>> cmdDict = new Dictionary<string, Func<List<string>, Executor, bool>>() {
            {"add", ASMFunctions.add },
            {"sub", ASMFunctions.sub },
            {"mul", ASMFunctions.mul },
            {"div", ASMFunctions.div },
            {"mov", ASMFunctions.mov },
            {"cmp", ASMFunctions.cmp },
        };

        private List<List<string>> asmCodeLines;
        //linenum: [(realExec, predictExec), ...]
        public Dictionary<int, List<KeyValuePair<bool, bool>>> predictorStats { get; private set; } = new Dictionary<int, List<KeyValuePair<bool, bool>>>();
        public Dictionary<string, int> labelDict { get; private set; } = new Dictionary<string, int>();
        private IBranchPredictor predictor;
        private int currentLineNum = 0;

        public float eax = 0f;
        public float ebx = 0f;
        public float ecx = 0f;
        public float edx = 0f;
        public float cf = 0f;
        public float zf = 0f;

        public Executor(string[] asmCodeStr, IBranchPredictor predictor)
        {
            asmCodeLines = ParseAsmCode(asmCodeStr);
            this.predictor = predictor;
            InitLabels();
        }

        private void InitLabels()
        {
            for (int i = 0; i < asmCodeLines.Count; i++)
            {
                var cmd = asmCodeLines[i];
                if (cmd[0].EndsWith(LABEL_SYMB))
                {
                    labelDict[cmd[0].Remove(cmd[0].Length - 1)] = i; //remove ':' and point to next line
                }
            }
        }

        public ref float operandToVar(string operand)
        {
            if (operand.Equals("EAX", StringComparison.CurrentCultureIgnoreCase))
                return ref eax;
            if (operand.Equals("EBX", StringComparison.CurrentCultureIgnoreCase))
                return ref ebx;
            if (operand.Equals("ECX", StringComparison.CurrentCultureIgnoreCase))
                return ref ecx;
            if (operand.Equals("EDX", StringComparison.CurrentCultureIgnoreCase))
                return ref edx;
            if (operand.Equals("CF", StringComparison.CurrentCultureIgnoreCase))
                return ref cf;
            if (operand.Equals("ZF", StringComparison.CurrentCultureIgnoreCase))
                return ref zf;
            throw new FormatException("чекай дауна такого регистра нет " + operand);
        }

        private static List<List<string>> ParseAsmCode(string[] asmCodeLines)
        {
            var splitCode = new List<List<string>>();

            //split each line by params
            foreach (var line in asmCodeLines)
            {
                if (line.StartsWith(COMMENT_SYMB))
                    continue;

                var lineSplit = line.Split(new[] { " ", ", ", "," },
                    StringSplitOptions.RemoveEmptyEntries);
                splitCode.Add(new List<string>(lineSplit));
            }

            return splitCode;
        }

        public void RunProgram()
        {     
            while (currentLineNum < asmCodeLines.Count)
            {
                Step();
            }
        }

        public void Step(ref int lineNum)
        {
            var cmdAndArgs = asmCodeLines[lineNum];
            var oper = cmdAndArgs[0].ToLower();

            Console.Write("\n" + String.Join(" ", cmdAndArgs));

            //check for normal operation
            if (cmdDict.ContainsKey(oper))
            {
                var tempList = new List<string>(cmdAndArgs); // leave only operands
                tempList.RemoveAt(0);
                if (!cmdDict[oper](tempList, this))
                    throw new Exception("Команда не сработала");
            }
            //todo implement jumps
            else if (JMP_OPS.Contains(oper))
            {
                //real execution
                bool execJump = true;
                switch (oper)
                {
                    //check condition
                }
                if (execJump)
                {
                    lineNum = labelDict[cmdAndArgs[1]]; // set i to line num where label is
                }
                Console.Write(" Real jump: " + execJump);

                //prediction
                bool jumpPrediction = predictor.shouldJump(cmdAndArgs, lineNum, this);
                Console.Write(" Prediction jump: " + jumpPrediction);

                //add to stats
                if (!predictorStats.ContainsKey(lineNum))
                    predictorStats[lineNum] = new List<KeyValuePair<bool, bool>>();
                predictorStats[lineNum].Add(new KeyValuePair<bool, bool>(execJump, jumpPrediction));
            }
            else if (labelDict.ContainsKey(oper.Substring(0, oper.Length - 1)))
            {
                return;
            }
            else
                throw new Exception("No such command " + cmdAndArgs[0]);
            //lineNum++;
        }

        public void Step()
        {
            if (currentLineNum >= asmCodeLines.Count)
                throw new Exception("КОда нет");
            Step(ref currentLineNum);
            currentLineNum++;
        }
    }
}

//            Console.WriteLine(String.Join(" ", codeLine) +
//$"\neax {eax}, ebx {ebx}, ecx {ecx}, edx {edx}" +
//$"\ncf {cf}, zf {zf}");