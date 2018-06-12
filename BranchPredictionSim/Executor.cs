using BranchPredictionSim.CmdParams;
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
        //consts
        private const string COMMENT_SYMB = ";";
        private const string LABEL_SYMB = ":";
        private const string HEX_DIGIT_HEADER = "h";
        private const string OCTO_DIGIT_HEADER = "q";
        private const string BYTE_DIGIT_HEADER = "b";
        private const string DECIMAL_DIGIT_HEADER = "d";
        private const string RETURN_CMD = "RET";
        private const string CALL_CMD = "call";

        private List<List<string>> asmCodeLines;
        public List<KeyValuePair<int, KeyValuePair<bool, bool>>> predictorStats { get; private set; } = new List<KeyValuePair<int, KeyValuePair<bool, bool>>>();
        public Dictionary<string, int> labelDict { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, Register> regDict;
        public Dictionary<string, FlagReg> flagDict;
        public Stack<int> EIP = new Stack<int>();
        private IBranchPredictor predictor;
        public int currentLineNum = 0;
        public int CurrentAddr { get; private set; } = 0;
        public int CommandLength { get; private set; } = 0;

        public Register eax = new Register(0);
        public Register ebx = new Register(0);
        public Register ecx = new Register(0);
        public Register edx = new Register(0);
        public FlagReg zf = new FlagReg(false);
        public FlagReg sf = new FlagReg(false);
        public FlagReg pf = new FlagReg(false);
        public Stack<int> stack = new Stack<int>();

        public Executor(string[] asmCodeStr, IBranchPredictor predictor)
        {
            asmCodeLines = ParseAsmCode(asmCodeStr);
            this.predictor = predictor;
            InitLabels();
            regDict = new Dictionary<string, Register>()
            {
                {"EAX", eax },
                {"EBX", ebx },
                {"ECX", ecx },
                {"EDX", edx }
            };
            flagDict = new Dictionary<string, FlagReg>()
            {
                {"ZF", zf },
                {"SF", sf },
                {"PF", pf },
            };
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

        public CmdParam<int> ParseOperand(string operand)
        {
            switch (operand.ToUpper())
            {
                case "EAX":
                    return eax;
                case "EBX":
                    return ebx;
                case "ECX":
                    return ecx;
                case "EDX":
                    return edx;
                default:
                    return new Imm(int.Parse(operand));
            }
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

        public void RunProgram(int maxIter = 500)
        {
            int currIter = 0;
            while (currentLineNum < asmCodeLines.Count && currIter++ < maxIter)
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
            //check for jump
            else if (ASMFunctions.jumpDict.ContainsKey(oper))
            {
                //real execution
                bool execJump = ASMFunctions.jumpDict[oper](this);

                //prediction
                bool jumpPrediction = predictor.shouldJump(cmdAndArgs, lineNum, this);

                //add to stats
                predictorStats.Add(new KeyValuePair<int, KeyValuePair<bool, bool>>(lineNum, new KeyValuePair<bool, bool>(execJump, jumpPrediction)));

                if (execJump)
                {
                    lineNum = labelDict[cmdAndArgs[1]]; // set i to line num where label is
                }
            }
            //check for label
            else if (labelDict.ContainsKey(oper.Substring(0, oper.Length - 1)))
            {
                return;
            }
            //check for 'ret' command
            else if (oper.Equals(RETURN_CMD, StringComparison.CurrentCultureIgnoreCase))
            {
                if (EIP.Count == 0)
                    throw new EndOfCodeException("Управление передано ОС");
                lineNum = EIP.Pop();
            }
            //check for 'call' cmd
            else if (oper.Equals(CALL_CMD, StringComparison.CurrentCultureIgnoreCase))
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
            int prevAddr = currentLineNum;
            Step(ref currentLineNum);
            //fix this shit vnizu
            if (prevAddr == currentLineNum - 1)//linear
            {
                CurrentAddr += CommandLength;

            } else
            {

            }
            CommandLength = asmCodeLines[currentLineNum].Aggregate((accum, next) => accum + next).Length;
            currentLineNum++;
        }

    }
}