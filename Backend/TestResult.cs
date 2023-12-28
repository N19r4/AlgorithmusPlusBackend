using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{
    public class TestResult
    {
        public string Algorithm { get; set; }
        public string TestFunction { get; set; }
        public int NumberOfSearchedParameters { get; set; }
        public int NumberOfIterations { get; set; }
        public int PopulationSize { get; set; }
        public string FoundMinimum { get; set; }
        public string StandardDeviationOfSearchedParameters { get; set; }
        public string ObjectiveFunctionValue { get; set; }
        public string StandardDeviationOfObjectiveFunctionValue { get; set; }
        public int NumberOfObjectiveFunctionCalls { get; set; }
        public string ExecutionTime { get; set; }
    }
}
