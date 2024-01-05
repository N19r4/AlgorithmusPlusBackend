using Backend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    static class RunAlgorithm
    {
        static public void Run(string optimizationAlgorithmDLL, string[] testFunctionDLLs, int dim, List<ParamForAlgorithm> paramsForAlgorithm)
        {
            // Wczytanie funkcji testowych i algorytmu optymalizacyjnego
            List<object> testFunctions = LoadTestFunctions(dim, testFunctionDLLs);
            (var optimizationAlgorithm,var delegateFunction) = LoadOptimizationAlgorithm(optimizationAlgorithmDLL);

            List<double[]> paramsList = new List<double[]>();

            foreach (var paramForAlgorithm in paramsForAlgorithm)
            {
                int size = (int)((paramForAlgorithm.UpperBoundry - paramForAlgorithm.LowerBoundry) / paramForAlgorithm.Step) + 1;

                double[] param = new double[size];

                int i = 0;

                for (var val = paramForAlgorithm.LowerBoundry; val <= paramForAlgorithm.UpperBoundry; val += paramForAlgorithm.Step)
                {
                    param[i] = val;
                    i++;
                }

                paramsList.Add(param);
            }

            TestOptimizationAlgorithm.RunTests(testFunctions, optimizationAlgorithm, paramsList, delegateFunction);
        }

        static List<object> LoadTestFunctions(int dim, string[] testFunctionDLLs)
        {
            List<object> testFunctions = new List<object>();

            foreach (var dllFile in testFunctionDLLs)
            {
                var assembly = Assembly.LoadFile(dllFile);

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        var instance = Activator.CreateInstance(type, dim);

                        testFunctions.Add(instance);
                    }
                }
            }

            if (testFunctions.Any())
            {
                Console.WriteLine("Loaded test functions:");

                foreach (var testFunction in testFunctions)
                {
                    var name = PropertyValue.GetPropertyValue<string>(testFunction, "Name");

                    Console.WriteLine($"Name: {name}");
                }
            }
            else
            {
                Console.WriteLine("No test functions to load.");
            }

            return testFunctions;
        }

        static (object, Type) LoadOptimizationAlgorithm(string optimizationAlgorithmDLL)
        {
            object instance = null;
            Type delegateFunction = null;

            var assembly = Assembly.LoadFile(optimizationAlgorithmDLL);

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Any(i => i.Name == "IOptimizationAlgorithm"))
                {
                    // Znaleziono klasę implementującą IOptimizationAlgorithm
                    Console.WriteLine($"Class found: {type.FullName}");

                    instance = Activator.CreateInstance(type);

                    break;
                }
            }

            delegateFunction = assembly.GetType("fitnessFunction");

            return  (instance, delegateFunction);
        }
    }
}
