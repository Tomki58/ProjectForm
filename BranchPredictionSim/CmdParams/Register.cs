﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim.CmdParams
{
    class Register : CmdParam<int>
    {
        public override int Value { get; set; }
        public Register(int data)
        {
            Value = data;
        }
    }
}
