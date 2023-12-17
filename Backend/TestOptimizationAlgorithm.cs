using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

//delegate double Function(params double[] x);

namespace TestDLL
{
    public class TestOptimizationAlgorithm
    {
        public static void RunTests(int[] T, int[] N, Type optimizationAlgorithmType, List<object> testFunctions, Type delegateFunction)
        {
            string reportFilePath = "report.csv";

            List<TestResult> testResults = new List<TestResult>();

            foreach (var testFunction in testFunctions)
            {
                var testFunctionName = testFunction.GetType().GetProperty("Name").GetValue(testFunction);
                var dimension = testFunction.GetType().GetProperty("Dim").GetValue(testFunction);
                int dim = (int)dimension;
                var xmin = testFunction.GetType().GetProperty("Xmin").GetValue(testFunction);
                var xmax = testFunction.GetType().GetProperty("Xmax").GetValue(testFunction);
                var calculateMethodInfo = testFunction.GetType().GetMethod("Calculate");
                var calculate = Delegate.CreateDelegate(delegateFunction, testFunction, calculateMethodInfo);

                foreach (var t in T)
                {
                    foreach (var n in N)
                    {
                        var optimizationAlgorithm = Activator.CreateInstance(
                            optimizationAlgorithmType,
                            n,
                            t,
                            calculate,
                            dim,
                            xmin,
                            xmax
                            );

                        var optimizationAlgorithmName = optimizationAlgorithm.GetType().GetProperty("Name").GetValue(optimizationAlgorithm);
                        var numberOfEvaluationFitnessFunction = (int)optimizationAlgorithm.GetType().GetProperty("NumberOfEvaluationFitnessFunction").GetValue(optimizationAlgorithm);
                        var solve = optimizationAlgorithm.GetType().GetMethod("Solve");

                        double[,] bestData = new double[dim + 1, 10];
                        string executionTime = "";

                        for (int i = 0; i < 10; i++)
                        {
                            //log progress
                            // Console.WriteLine($"Algorytm: {optimizationAlgorithmName}, Funkcja testowa: {testFunctionName}, Liczba szukanych parametrów: {dim}, Liczba iteracji: {t}, Rozmiar populacji: {n}, Iteracja: {i + 1}");
                            var watch = System.Diagnostics.Stopwatch.StartNew();
                            var FBest = (double)solve.Invoke(optimizationAlgorithm, new object[] {});
                            watch.Stop();

                            var elapsedMs = watch.ElapsedMilliseconds;
                            executionTime = elapsedMs.ToString();

                            var XBest = (double[])optimizationAlgorithm.GetType().GetProperty("XBest").GetValue(optimizationAlgorithm);
                            numberOfEvaluationFitnessFunction = (int)optimizationAlgorithm.GetType().GetProperty("NumberOfEvaluationFitnessFunction").GetValue(optimizationAlgorithm);

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

                        string str_minParameters = "(" + string.Join(", ", minParameters.Select(d => d.ToString("F5", CultureInfo.InvariantCulture))) + ")";

                        double[] stdDevForParameters = new double[dim];
                        double[] varCoeffForParameters = new double[dim];

                        for (int i = 0; i < dim; i++)
                        {
                            double[] parameters = new double[10];
                            for (int j = 0; j < 10; j++)
                            {
                                parameters[j] = bestData[i, j];
                            }

                            double avg = parameters.Average();
                            double sum = parameters.Sum(x => Math.Pow(x - avg, 2));
                            double stdDev = Math.Sqrt(sum / 10);
                            double varCoeff = 0;
                            if (stdDev != 0)
                                varCoeff = (stdDev / avg) * 100;
                            else
                                varCoeff = 0;

                            stdDevForParameters[i] = stdDev;
                            varCoeffForParameters[i] = varCoeff;
                        }

                        string str_stdDevForParameters = "(" + string.Join(", ", stdDevForParameters.Select(d => d.ToString("F5", CultureInfo.InvariantCulture))) + ")";
                        string str_varCoeffForParameters = "(" + string.Join(", ", varCoeffForParameters.Select(d => d.ToString("F5", CultureInfo.InvariantCulture))) + ")";

                        TestResult testResult = new TestResult
                        {
                            Algorytm = optimizationAlgorithmName.ToString(),
                            FunkcjaTestowa = testFunctionName.ToString(),
                            LiczbaSzukanychParametrów = dim,
                            LiczbaIteracji = t,
                            RozmiarPopulacji = n,
                            ZnalezioneMinimum = str_minParameters,
                            OdchylenieStandardowePoszukiwanychParametrów = str_stdDevForParameters,
                            WartośćFunkcjiCelu = minFunction.ToString("F5", CultureInfo.InvariantCulture),
                            OdchylenieStandardoweWartościFunkcjiCelu = stdDevForFunction.ToString("F5", CultureInfo.InvariantCulture),
                            LiczbaWywołańFunkcjiCelu = numberOfEvaluationFitnessFunction,
                            CzasEgzekucji = executionTime
                        };

                        testResults.Add(testResult);
                    }
                }
            }

            var config = new CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
            using (var writer = new StreamWriter(reportFilePath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(testResults);
            }
            //success:
            Console.WriteLine($"Zapisano wyniki do pliku: {reportFilePath}");
        }
    }
}
