using BranchPredictionSim.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const string RETURN_CMD = "RET";
        private const string CALL_CMD = "call";

        private List<List<string>> asmCodeLines;
        //linenum: [(realExec, predictExec), ...]
        public Dictionary<int, List<KeyValuePair<bool, bool>>> predictorStats { get; private set; } = new Dictionary<int, List<KeyValuePair<bool, bool>>>();
        public Dictionary<string, int> labelDict { get; private set; } = new Dictionary<string, int>();
        // Impossible to update info // public ObservableCollection<KeyValuePair<string, float>> Data { get; set; } = new ObservableCollection<KeyValuePair<string, float>>();
        public List<Data> stats { get; private set; } = new List<Data>();
        public Stack<int> EIP = new Stack<int>();
        private IBranchPredictor predictor;
        public int currentLineNum = 0;
        public int CurrentAddr { get; private set; } = 0;
        public int CommandLength { get; private set; } = 0;

        public float eax = 0f;
        public float ebx = 0f;
        public float ecx = 0f;
        public float edx = 0f;
        public float zf = 0f;
        public float sf = 0f;
        public float pf = 0f;

        public Executor(string[] asmCodeStr, IBranchPredictor predictor)
        {
            asmCodeLines = ParseAsmCode(asmCodeStr);
            this.predictor = predictor;
            InitLabels();
            stats.Add(new Data("eax", eax));
            stats.Add(new Data("ebx", ebx));
            stats.Add(new Data("ecx", ecx));
            stats.Add(new Data("edx", edx));
            stats.Add(new Data("zf", zf));
            stats.Add(new Data("sf", sf));
            stats.Add(new Data("pf", pf));
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
            if (operand.Equals("SF", StringComparison.CurrentCultureIgnoreCase))
                return ref sf;
            if (operand.Equals("PF", StringComparison.CurrentCultureIgnoreCase))
                return ref pf;
            if (operand.Equals("ZF", StringComparison.CurrentCultureIgnoreCase))
                return ref zf;
            throw new FormatException("Нет такого регистра " + operand);
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
            if (ASMFunctions.cmdDict.ContainsKey(oper))
            {
                var tempList = new List<string>(cmdAndArgs); // leave only operands
                tempList.RemoveAt(0);
                if (!ASMFunctions.cmdDict[oper](tempList, this))
                    throw new Exception("Команда не сработала");
            }
            else if (ASMFunctions.jumpDict.ContainsKey(oper))
            {
                //real execution
                bool execJump = ASMFunctions.jumpDict[oper](this);
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
            } else if (oper.Equals(RETURN_CMD, StringComparison.CurrentCultureIgnoreCase))
            {
                if (EIP.Count == 0)
                    throw new EndOfCodeException("Управление передано ОС");
                lineNum = EIP.Pop();
            } else if (oper.Equals(CALL_CMD, StringComparison.CurrentCultureIgnoreCase))
            {
                EIP.Push(lineNum);
                lineNum = labelDict[cmdAndArgs[1]];
            }
            else
                throw new Exception("No such command " + cmdAndArgs[0]);
        }

        public void Step()
        {
            if (currentLineNum >= asmCodeLines.Count)
                throw new EndOfCodeException("Конец кода, управление передано ОС");
            Step(ref currentLineNum);
            CurrentAddr += CommandLength;
            CommandLength = asmCodeLines[currentLineNum].Aggregate((accum, next) => accum + next).Length;
            currentLineNum++;
        }

        public void updateStats(string reg, float value)
        {
            foreach (var check in stats)
            {
                if (reg == check.regFlag)
                    check.value = value;
            }
        }
    }
}

//            Console.WriteLine(String.Join(" ", codeLine) +
//$"\neax {eax}, ebx {ebx}, ecx {ecx}, edx {edx}" +
//$"\ncf {cf}, zf {zf}");