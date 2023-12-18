namespace Backend
{
    public class AlgorithmParameters
    {
        public string OptimizationAlgorithmName { get; set; }
        public string[] TestFunctionNames { get; set; }
        public int[] T { get; set; }
        public int[] N { get; set; }
        public int Dim { get; set; }
    }
}