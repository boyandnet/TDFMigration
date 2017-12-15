using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TDFMgration.Models
{
    public class Result
    {
        public DFDealer DFDealer { get; set; }
        public FTDealer FTDealer { get; set; }
        public ResultType ResultType { get; set; }
    }

    public enum ResultType
    {
        DIFF,
        OnlyInDF,
        OnlyInFT
    }
}
