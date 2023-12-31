﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Backend;
using CsvHelper;
using CsvHelper.Configuration;

namespace Backend
{
    public class TestOptimizationAlgorithm
    {
        public static void RunTests(List<object> testFunctions, object optimizationAlgorithm, Dictionary<string, double[]> paramsDict, Type delegateFunction)
        {
            List<object> testResults = new List<object>();

            // tutaj tworze tablice kombinacji parametrów przygotowane już do testów
            List<Dictionary<string, double>> paramsDictForTest = GetParamsDict(paramsDict);

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

                foreach (var parameters in paramsDictForTest)
                {
                    double[,] bestData = new double[dim + 1, 10];

                    object[] solveParameters = new object[] { calculate, domain, parameters };

                    for (int i = 0; i < 10; i++)
                    {
                        solve.Invoke(optimizationAlgorithm, solveParameters);

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

                    dynamic testResult = new ExpandoObject() ;
                    
                    testResult.OptimiazationAlgorithm = optimiazationAlgorithmName;
                    testResult.TestFunction = testFunctionName;
                    testResult.NumberOfSearchedParameters = dim;
                    foreach(var parameter in parameters)
                    {
                        ((IDictionary<string, object>)testResult).Add(parameter.Key, parameter.Value);
                    }
                    testResult.FoundMinimum = str_minParameters;
                    testResult.StandardDeviationOfFoundMinimum = str_stdDevForParameters;
                    testResult.ObjectiveFunctionValue = minFunction.ToString("F5", CultureInfo.InvariantCulture);
                    testResult.StandardDeviationOfObjectiveFunctionValue = stdDevForFunction.ToString("F5", CultureInfo.InvariantCulture);
                    testResult.NumberOfObjectiveFunctionCalls = numberOfEvaluationFitnessFunction;
                    
                    testResults.Add(testResult);
                }
            }

            string reportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(reportsFolder);

            string reportFilePath = Path.Combine(reportsFolder, "Report.csv");

            int suffix = 1;
            while (File.Exists(reportFilePath))
            {
                reportFilePath = Path.Combine(reportsFolder, $"Report ({suffix}).csv");
                suffix++;
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

        static List<Dictionary<string, double>> GetParamsDict(Dictionary<string, double[]> paramsDict)
        {
            var combinations = new List<Dictionary<string, double>>();
            GetParamsDictHelper(paramsDict, 0, new Dictionary<string, double>(), combinations);
            return combinations;
        }

        static void GetParamsDictHelper(Dictionary<string, double[]> paramsDict, int index,
            Dictionary<string, double> currentCombination, List<Dictionary<string, double>> combinations)
        {
            if (index == paramsDict.Count)
            {
                // Dodawanie skompletowanej kombinacji do listy
                combinations.Add(new Dictionary<string, double>(currentCombination));
                return;
            }

            string paramName = paramsDict.Keys.ElementAt(index);
            Console.WriteLine(paramName);
            double[] paramValues = paramsDict[paramName];

            foreach (double paramValue in paramValues)
            {
                currentCombination[paramName] = paramValue;
                GetParamsDictHelper(paramsDict, index + 1, currentCombination, combinations);
            }
        }
    }
}
