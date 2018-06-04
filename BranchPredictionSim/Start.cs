using BranchPredictionSim.Predictors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim
{
    class Start
    {
        //public static void Main(string[] args)
        //{
        //    string predTypeStr = args[0];
        //    string asmCodePath = args[1];

        //    var asmCodeLines = File.ReadAllLines(asmCodePath);
        //    var predictor = ParsePredictorName(predTypeStr);

        //    //var a = new Executor("MOV EAX 7\nMov ebx 7\nCMP EAX EBX\nADD eax ebx\nSUB ebx eax\nMUL EAX EBX\n");
        //    var executor = new Executor(asmCodeLines, predictor);
        //    executor.RunProgram();
        //    Console.Read();
        //}

        private static IBranchPredictor ParsePredictorName(string predictorName)
        {
            switch (predictorName)
            {
                case "alw":
                    return new AlwaysPredictJump();
                case "btfnt":
                    return new BTFNT();
                default:
                    throw new ArgumentException("Нет такого предиктора");
            }
        }
    }
}
