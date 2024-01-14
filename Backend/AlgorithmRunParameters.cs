namespace Backend
{
    public class AlgorithmRunParameters
    {
        public string[] OptimizationAlgorithmNames { get; set; }
        public string[] TestFunctionNames { get; set; }
        public int Dim { get; set; }
        //map of params for each algorithm
        public Dictionary<string, List<ParamForAlgorithm>> ParamsDict { get; set; }
        // public List<ParamForAlgorithm> paramsForAlgorithm { get; set; }
    }
}