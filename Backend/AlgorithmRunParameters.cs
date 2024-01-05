namespace Backend
{
    public class AlgorithmRunParameters
    {
        public string OptimizationAlgorithmName { get; set; }
        public string[] TestFunctionNames { get; set; }
        public int Dim { get; set; }
        public List<ParamForAlgorithm> paramsForAlgorithm { get; set; }
    }
}