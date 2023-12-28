using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Backend;
using CsvHelper;
using CsvHelper.Configuration;

//delegate double Function(params double[] x);

namespace TestDLL
{
    public class TestOptimizationAlgorithm
    {
        public static void RunTests(List<object> testFunctions, object optimizationAlgorithm, List<double[]> paramsList, Type delegateFunction)
        {
            string reportFilePath = "report.csv";

            List<TestResult> testResults = new List<TestResult>();

            // tutaj tworze tablice kombinacji parametrów przygotowane już do testów
            List<double[]> paramsListForTest = GetParamsList(paramsList);

            var optimiazationAlgorithmName = PropertyValue.GetPropertyValue<string>(optimizationAlgorithm, "Name");
            var numberOfEvaluationFitnessFunction = PropertyValue.GetPropertyValue<int>(optimizationAlgorithm, "NumberOfEvaluationFitnessFunction");
            var solve = optimizationAlgorithm.GetType().GetMethod("Solve");

            foreach (var testFunction in testFunctions)
            {
                var testFunctionName = PropertyValue.GetPropertyValue<string>(testFunction, "Name");
                var dim = PropertyValue.GetPropertyValue<int>(testFunction, "Dim");
                var domain = PropertyValue.GetPropertyValue<double[,]>(testFunction, "Domain");
                var calculateMethodInfo = testFunction.GetType().GetMethod("Calculate");
                var calculate = Delegate.CreateDelegate(delegateFunction, testFunction, calculateMethodInfo);

                foreach (var parameters in paramsListForTest)
                {
                    double[,] bestData = new double[dim + 1, 10];
                    string executionTime = "";

                    object[] solveParameters = new object[] { calculate, domain, parameters };

                    for (int i = 0; i < 10; i++)
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        solve.Invoke(optimizationAlgorithm, solveParameters);
                        watch.Stop();

                        var elapsedMs = watch.ElapsedMilliseconds;
                        executionTime = elapsedMs.ToString();

                        var XBest = PropertyValue.GetPropertyValue<double[]>(optimizationAlgorithm, "XBest");
                        var FBest = PropertyValue.GetPropertyValue<double>(optimizationAlgorithm, "FBest");
                        numberOfEvaluationFitnessFunction = PropertyValue.GetPropertyValue<int>(optimizationAlgorithm, "NumberOfEvaluationFitnessFunction");

                        for (int j = 0; j < dim; j++)
                        {
                            bestData[j, i] = XBest[j];
                        }

                        bestData[dim, i] = FBest;
                    }

                    double minFunction = bestData[dim, 0];
                    int minFunction_index = 0;
                    for (int i = 1; i < 10; i++)
                    {
                        if (bestData[dim, i] < minFunction)
                        {
                            minFunction = bestData[dim, i];
                            minFunction_index = i;
                        }
                    }

                    double[] allMinFunction = new double[10];

                    for (int i = 0; i < 10; i++)
                    {
                        allMinFunction[i] = bestData[dim, i];
                    }

                    double avgForFunction = allMinFunction.Average();
                    double sumForFunction = allMinFunction.Sum(x => Math.Pow(x - avgForFunction, 2));
                    double stdDevForFunction = Math.Sqrt(sumForFunction / 10);
                    double varCoeffForFunction = 0;
                    if (stdDevForFunction != 0)
                        varCoeffForFunction = (stdDevForFunction / avgForFunction) * 100;
                    else
                        varCoeffForFunction = 0;


                    double[] minParameters = new double[dim];
                    for (int i = 0; i < dim; i++)
                        minParameters[i] = bestData[i, minFunction_index];

                    string str_minParameters = "(" + string.Join("; ", minParameters) + ")";

                    double[] stdDevForParameters = new double[dim];
                    double[] varCoeffForParameters = new double[dim];

                    for (int i = 0; i < dim; i++)
                    {
                        double[] minimums = new double[10];
                        for (int j = 0; j < 10; j++)
                        {
                            minimums[j] = bestData[i, j];
                        }

                        double avg = minimums.Average();
                        double sum = minimums.Sum(x => Math.Pow(x - avg, 2));
                        double stdDev = Math.Sqrt(sum / 10);
                        double varCoeff = 0;
                        if (stdDev != 0)
                            varCoeff = (stdDev / avg) * 100;
                        else
                            varCoeff = 0;

                        stdDevForParameters[i] = stdDev;
                        varCoeffForParameters[i] = varCoeff;
                    }

                    string str_stdDevForParameters = "(" + string.Join("; ", stdDevForParameters) + ")";
                    string str_varCoeffForParameters = "(" + string.Join("; ", varCoeffForParameters) + ")";

                    TestResult testResult = new TestResult
                    {
                        Algorithm = optimiazationAlgorithmName,
                        TestFunction = testFunctionName,
                        NumberOfSearchedParameters = dim,
                        NumberOfIterations = (int)parameters[0],
                        PopulationSize = (int)parameters[1],
                        FoundMinimum = str_minParameters,
                        StandardDeviationOfSearchedParameters = str_stdDevForParameters,
                        ObjectiveFunctionValue = minFunction.ToString("F5", CultureInfo.InvariantCulture),
                        StandardDeviationOfObjectiveFunctionValue = stdDevForFunction.ToString("F5", CultureInfo.InvariantCulture),
                        NumberOfObjectiveFunctionCalls = numberOfEvaluationFitnessFunction,
                        ExecutionTime = executionTime
                    };

                    testResults.Add(testResult);
                }
            }

            // Zapisz CSV
            var config = new CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
            using (var writer = new StreamWriter(reportFilePath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(testResults);
            }
            //success:
            Console.WriteLine($"Zapisano wyniki do pliku: {reportFilePath}");
        }

        static List<double[]> GetParamsList(List<double[]> paramsList, int currentIndex = 0, double[] currentCombination = null)
        {
            if (currentCombination == null)
                currentCombination = new double[paramsList.Count];

            List<double[]> parameters = new List<double[]>();

            if (currentIndex == paramsList.Count)
            {
                parameters.Add(currentCombination.ToArray());
            }
            else
            {
                foreach (var value in paramsList[currentIndex])
                {
                    currentCombination[currentIndex] = value;

                    parameters.AddRange(GetParamsList(paramsList, currentIndex + 1, currentCombination));
                }
            }

            return parameters;
        }
    }
}
