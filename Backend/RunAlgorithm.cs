using Backend;
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
        static public void Run(string optimizationAlgorithmDLL, string[] testFunctionDLLs, int dim)
        {
            // Wczytanie funkcji testowych z plików DLL z katalogu TestFunctions
            //TODO: wczytanie tylko podanych funkcji testowych
            List<object> testFunctions = LoadTestFunctions(dim, testFunctionDLLs);
            (var optimizationAlgorithm,var delegateFunction) = LoadOptimizationAlgorithm(optimizationAlgorithmDLL);

            var paramsInfoArray = PropertyValue.GetPropertyValue<Array>(optimizationAlgorithm, "ParamsInfo");

            //tę listę będzie trzeba przekazać na front po wybraniu algorytmu do przetestowania i dopiero po ustawieniu parametrów
            //użytkownik będzie mógł uruchomić testowanie
            List<ParamInfo> paramsInfo = new List<ParamInfo>();

            foreach (var paramInfo in paramsInfoArray)
            {
                paramsInfo.Add(
                    new ParamInfo
                    {
                        Name = PropertyValue.GetPropertyValue<string>(paramInfo, "Name"),
                        Description = PropertyValue.GetPropertyValue<string>(paramInfo, "Description"),
                        UpperBoundary = PropertyValue.GetPropertyValue<double>(paramInfo, "UpperBoundary"),
                        LowerBoundary = PropertyValue.GetPropertyValue<double>(paramInfo, "LowerBoundary")
                    }
                    );
            }

            //dostajemy od użytkownika jego wymagania dot. parametrów tj. upper boundry, lower boundry i step
            //na podstawie tego tworzymy tablice dla parametrów - na razie przykład na sztywno
            List<TestParam> testParams = new List<TestParam>();
            testParams.Add(new TestParam(60, 20, 20));
            testParams.Add(new TestParam(80, 20, 20));

            List<double[]> paramsList = new List<double[]>();

            foreach (var testParam in testParams)
            {
                int size = (int)((testParam.UpperBoundry - testParam.LowerBoundry) / testParam.Step) + 1;

                double[] param = new double[size];

                int i = 0;

                for (var val = testParam.LowerBoundry; val <= testParam.UpperBoundry; val += testParam.Step)
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
                Console.WriteLine("Załadowane funkcje testowe:");

                foreach (var testFunction in testFunctions)
                {
                    var name = PropertyValue.GetPropertyValue<string>(testFunction, "Name");

                    Console.WriteLine($"Nazwa: {name}");
                }
            }
            else
            {
                Console.WriteLine("Brak funkcji testowych do załadowania.");
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
                    Console.WriteLine($"Znaleziono klasę: {type.FullName}");

                    instance = Activator.CreateInstance(type);

                    break;
                }
            }

            delegateFunction = assembly.GetType("fitnessFunction");

            return  (instance, delegateFunction);
        }
    }
}
