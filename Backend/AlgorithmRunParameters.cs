namespace Backend
{
    public class AlgorithmRunParameters
    {
        public string[] OptimizationAlgorithmNames { get; set; }
        public string[] TestFunctionNames { get; set; }
        public int Dim { get; set; }
        public List<ParamForAlgorithm> paramsForAlgorithm { get; set; }
    }
}