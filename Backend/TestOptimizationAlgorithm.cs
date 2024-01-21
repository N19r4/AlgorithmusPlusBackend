using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Backend;
using CsvHelper;
using CsvHelper.Configuration;
using iTextSharp.text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Backend
{
    public class TestOptimizationAlgorithm
    {
        public static bool stopCalcFlag { get; set; }

        public static (string, double, Dictionary<string, double>) RunTests(List<object> testFunctions, object optimizationAlgorithm, Dictionary<string, double[]> paramsDict, Type delegateFunction)
        {
            string stateFolder = Path.Combine(Directory.GetCurrentDirectory(), "State");
            Directory.CreateDirectory(stateFolder);

            string resultsStatePath = Path.Combine(stateFolder, "ResultsState.json");
            List<dynamic> testResults = new List<dynamic>();

            string testStatePath = Path.Combine(stateFolder, "TestState.json");
            TestState testState = new TestState(0, 0);
            
            int iStart = 0; 
            int iPStart = 0;
            int iFStart = 0;

            if (File.Exists(testStatePath))
            {
                testState = LoadFromFile(testStatePath);
                iStart = testState.Iterator;
                iPStart = testState.ParamIterator;
                iFStart = testState.TestFuncIterator;

                File.Delete(testStatePath);
            }

            if(File.Exists(resultsStatePath))
            {
                testResults = LoadListFromFile(resultsStatePath);

                File.Delete(resultsStatePath);
            }

            // tutaj tworze tablice kombinacji parametrów przygotowane już do testów
            List<Dictionary<string, double>> paramsDictForTest = GetParamsDict(paramsDict);

            var optimiazationAlgorithmName = PropertyValue.GetPropertyValue<string>(optimizationAlgorithm, "Name");
            var numberOfEvaluationFitnessFunction = PropertyValue.GetPropertyValue<int>(optimizationAlgorithm, "NumberOfEvaluationFitnessFunction");
            var solve = optimizationAlgorithm.GetType().GetMethod("Solve");
            double returnedObjFuncVal = double.MaxValue;
            string returnedMinimum = "";
            double minStdDev = double.MaxValue;
            int returnedParamsIndex = 0;

            for (int iF = iFStart; iF < testFunctions.Count; iF++)
            {
                var testFunction = testFunctions[iF];
                var testFunctionName = PropertyValue.GetPropertyValue<string>(testFunction, "Name");
                var dim = PropertyValue.GetPropertyValue<int>(testFunction, "Dim");
                var domain = PropertyValue.GetPropertyValue<double[,]>(testFunction, "Domain");
                var calculateMethodInfo = testFunction.GetType().GetMethod("Calculate");
                var calculate = Delegate.CreateDelegate(delegateFunction, testFunction, calculateMethodInfo);
                int currentParamIndex = 0;
                
                for (int iP = iPStart; iP < paramsDictForTest.Count; iP++)
                {  
                    var parameters = paramsDictForTest[iP];
                    double[,] bestData = new double[dim + 1, 10];

                    if (testState.BestData != null)
                    {
                        bestData = testState.BestData;
                    }

                    object[] solveParameters = new object[] { calculate, domain, parameters };

                    for (int i = iStart; i < 10; i++)
                    {
                        if (!stopCalcFlag)
                        {
                            solve.Invoke(optimizationAlgorithm, solveParameters);
                        }
                        else
                        {
                            return ("", 0.0, null);
                        }

                        var XBest = PropertyValue.GetPropertyValue<double[]>(optimizationAlgorithm, "XBest");
                        var FBest = PropertyValue.GetPropertyValue<double>(optimizationAlgorithm, "FBest");
                        numberOfEvaluationFitnessFunction = PropertyValue.GetPropertyValue<int>(optimizationAlgorithm, "NumberOfEvaluationFitnessFunction");

                        for (int j = 0; j < dim; j++)
                        {
                            bestData[j, i] = XBest[j];
                        }

                        bestData[dim, i] = FBest;

                        testState.BestData = bestData;
                        testState.Iterator = i + 1;
                        SaveToFile(testState, testStatePath);
                    }

                    iStart = 0;
                    testState.Iterator = 0;
                    SaveToFile(testState, testStatePath);

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

                    string str_minParameters = "(" + string.Join("; ", minParameters.Select(x => x.ToString("F5"))) + ")";

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

                    string str_stdDevForParameters = "(" + string.Join("; ", stdDevForParameters.Select(x => x.ToString("F5"))) + ")";
                    string str_varCoeffForParameters = "(" + string.Join("; ", varCoeffForParameters.Select(x => x.ToString("F5"))) + ")";

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
                    //testResult.ObjectiveFunctionValue = minFunction.ToString("F5", CultureInfo.InvariantCulture);
                    testResult.ObjectiveFunctionValue = Math.Round(minFunction, 5);
                    testResult.StandardDeviationOfObjectiveFunctionValue = Math.Round(stdDevForFunction, 5);
                    testResult.NumberOfObjectiveFunctionCalls = numberOfEvaluationFitnessFunction;
                    
                    testResults.Add(testResult);
                    if (minFunction < returnedObjFuncVal)
                    {
                        if(stdDevForFunction < minStdDev)
                        {
                            returnedParamsIndex = currentParamIndex;
                            returnedObjFuncVal = minFunction;
                            returnedMinimum = str_minParameters;
                        }
                    }

                    currentParamIndex++;
                    
                    SaveListToFile(testResults, resultsStatePath);
                    testState.ParamIterator = iP + 1;
                    SaveToFile(testState, testStatePath);
                }
                
                iPStart = 0;
                testState = new TestState(testState.AlgorithmIterator, iF + 1);
                SaveToFile(testState, testStatePath);
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
            var config = new CsvConfiguration(new CultureInfo("pl-PL"));
            using (var writer = new StreamWriter(reportFilePath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(testResults);
            }
            //success:
            Console.WriteLine($"Zapisano wyniki do pliku: {reportFilePath}");

            File.Delete(testStatePath);
            File.Delete(resultsStatePath);

            return (returnedMinimum, returnedObjFuncVal, paramsDictForTest[returnedParamsIndex]);
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
            double[] paramValues = paramsDict[paramName];

            foreach (double paramValue in paramValues)
            {
                currentCombination[paramName] = paramValue;
                GetParamsDictHelper(paramsDict, index + 1, currentCombination, combinations);
            }
        }

        static void SaveToFile(TestState testState, string filePath)
        {
            using (var fileStream = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.Serialize(fileStream, testState);
                fileStream.Close();
            }
        }

        static TestState LoadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                TestState testState = new TestState(0, 0);

                using (var fileStream = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    testState = (TestState)serializer.Deserialize(fileStream, typeof(TestState));
                    fileStream.Close();
                }

                return testState;
            }
            return new TestState(0, 0);
        }

        static void SaveListToFile(List<dynamic> results, string filePath)
        {
            using (var fileStream = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.Serialize(fileStream, results);
            }
        }

        static List<dynamic> LoadListFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var fileStream = File.Open(filePath, FileMode.Open))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        string json = reader.ReadToEnd();
                        List<dynamic> list = JsonConvert.DeserializeObject<List<dynamic>>(json);

                        List<dynamic> testResults = new List<dynamic>();

                        foreach (var element in list)
                        {
                            dynamic testResult = new ExpandoObject();

                            foreach (var property in element.Properties())
                            {
                                ((IDictionary<string, object>)testResult)[property.Name] = property.Value.ToObject<object>();
                            }

                            testResults.Add(testResult);
                        }

                        return testResults;
                    }
                }
            }
            return new List<dynamic>();
        }
    }
}
