using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{
    static class RunAlgorithm
    {
        static public void Run(string optimizationAlgorithmDLL, string[] testFunctionDLLs, int[] T, int[] N, int dim)
        {


            // Wczytanie funkcji testowych z plików DLL z katalogu TestFunctions
            //TODO: wczytanie tylko podanych funkcji testowych
            List<object> testFunctions = LoadTestFunctions(dim, testFunctionDLLs);
            (var types,var delegateFunction) = LoadOptimizationAlgorithms(optimizationAlgorithmDLL);

            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && !typeof(Delegate).IsAssignableFrom(type))
                {
                    Console.WriteLine(type.FullName);
                    TestOptimizationAlgorithm.RunTests(T, N, type, testFunctions, delegateFunction);
                }
            }
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
                Console.WriteLine("Załadowane funkcje testowe:");

                foreach (var testFunction in testFunctions)
                {
                    var name = testFunction.GetType().GetProperty("Name").GetValue(testFunction);
                    //var dimension = testFunction.GetType().GetProperty("Dim").GetValue(testFunction);
                    //var xmin = testFunction.GetType().GetProperty("Xmin").GetValue(testFunction);
                    //var xmax = testFunction.GetType().GetProperty("Xmax").GetValue(testFunction);

                    Console.WriteLine($"Nazwa: {name}");
                }
            }
            else
            {
                Console.WriteLine("Brak funkcji testowych do załadowania.");
            }

            return testFunctions;
        }

        static (Type[], Type) LoadOptimizationAlgorithms(string optimizationAlgorithmPath)
        {
            List<object> optimizationAlgorithms = new List<object>();

            var assembly = Assembly.LoadFile(optimizationAlgorithmPath);

            var types = assembly.GetTypes();
            
            var delegateFunction = assembly.GetType("Function");

            return  (types, delegateFunction);
        }
    }
}
