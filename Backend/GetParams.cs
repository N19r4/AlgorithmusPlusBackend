using System.Reflection;

namespace Backend
{
    public class GetParams
    {
        public static List<ParamInfo> GetParamsForAlgorithm(string optimizationAlgorithmDLL)
        {
            object optimizationAlgorithm = null;

            var assembly = Assembly.LoadFile(optimizationAlgorithmDLL);
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                if (interfaces.Any(i => i.Name == "IOptimizationAlgorithm"))
                {
                    // Znaleziono klasę implementującą IOptimizationAlgorithm
                    Console.WriteLine($"Class found: {type.FullName}");

                    optimizationAlgorithm = Activator.CreateInstance(type);

                    break;
                }
            }

            var paramsInfoArray = PropertyValue.GetPropertyValue<Array>(optimizationAlgorithm, "ParamsInfo");

            List<ParamInfo> paramsInfo = new List<ParamInfo>();

            foreach (var paramInfo in paramsInfoArray)
            {
                paramsInfo.Add(
                    new ParamInfo
                    {
                        Name = PropertyValue.GetPropertyValue<string>(paramInfo, "Name"),
                        Description = PropertyValue.GetPropertyValue<string>(paramInfo, "Description"),
                        UpperBoundry = PropertyValue.GetPropertyValue<double>(paramInfo, "UpperBoundry"),
                        LowerBoundry = PropertyValue.GetPropertyValue<double>(paramInfo, "LowerBoundry"),
                        Step = PropertyValue.GetPropertyValue<double>(paramInfo, "Step"),
                    }
                    );
            }

            return paramsInfo;
        }
    }
}
