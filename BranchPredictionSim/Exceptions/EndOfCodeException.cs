using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchPredictionSim.Exceptions
{
    class EndOfCodeException : Exception
    {
        public EndOfCodeException()
        {
        }

        public EndOfCodeException(string message)
            : base(message)
        {
        }

        public EndOfCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
