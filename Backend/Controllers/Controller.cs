using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Net.Http.Headers;
using System.Net;
using iTextSharp.text.pdf.parser;
using System.IO.Compression;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        private readonly ILogger<Controller> _logger;

        public Controller(ILogger<Controller> logger)
        {
            _logger = logger;
        }

        [HttpPost("UploadTestFunctionDLL")]
        public IActionResult UploadTestFunctionDLL(string testFunctionDLL)
        {
            if (testFunctionDLL == null)
            {
                return NotFound();
            }
            if (!System.IO.File.Exists(testFunctionDLL))
            {
                return BadRequest("File does not exist.");
            }
            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            string testFunctionPath = System.IO.Path.Combine(testFunctionsFolder, System.IO.Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                return BadRequest($"File {System.IO.Path.GetFileName(testFunctionPath)} already exists.");
            }
            else
            {
                System.IO.File.Copy(testFunctionDLL, testFunctionPath);
                Console.WriteLine($"File {System.IO.Path.GetFileName(testFunctionPath)} has been copied.");
            }

            return Ok(System.IO.Path.GetFileNameWithoutExtension(testFunctionDLL));
        }

        [HttpPost("UploadOptimizationAlgorithmDLL")]
        public IActionResult UploadOptimizationAlgorithmDLL(string optimizationAlgorithmDLL)
        {
            if (optimizationAlgorithmDLL == null)
            {
                return NotFound();
            }
            if (!System.IO.File.Exists(optimizationAlgorithmDLL))
            {
                return BadRequest("File does not exist.");
            }

            string optimizationAlgorithmFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            Directory.CreateDirectory(optimizationAlgorithmFolder);

            string optimizationAlgorithmPath = System.IO.Path.Combine(optimizationAlgorithmFolder, System.IO.Path.GetFileName(optimizationAlgorithmDLL));

            if (System.IO.File.Exists(optimizationAlgorithmPath))
            {
                return BadRequest($"File {System.IO.Path.GetFileName(optimizationAlgorithmPath)} already exists.");
            }
            else
            {
                System.IO.File.Copy(optimizationAlgorithmDLL, optimizationAlgorithmPath);
                Console.WriteLine($"File {System.IO.Path.GetFileName(optimizationAlgorithmPath)} has been copied.");
            }

            return Ok(System.IO.Path.GetFileNameWithoutExtension(optimizationAlgorithmDLL));
        }

        [HttpGet("GetAllTestFunctionsNames")]
        public IActionResult GetAllTestFunctionsNames()
        {
            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");

            try
            {
                if (!Directory.Exists(testFunctionsFolder))
                {
                    return BadRequest($"Folder {testFunctionsFolder} does not exist.");
                }

                // Get all DLL files in the TestFunctions folder
                var dllFiles = Directory.GetFiles(testFunctionsFolder, "*.dll");

                // Extract file names without extension
                var testFunctionNames = dllFiles.Select(file => System.IO.Path.GetFileNameWithoutExtension(file)).ToList();

                return Ok(testFunctionNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetAllAlgorithmsNames")]
        public IActionResult GetAllAlgorithmsNames()
        {
            string algorithmsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            try
            {
                if (!Directory.Exists(algorithmsFolder))
                {
                    return BadRequest($"Folder {algorithmsFolder} does not exist.");
                }

                // Get all DLL files in the TestFunctions folder
                var dllFiles = Directory.GetFiles(algorithmsFolder, "*.dll");

                // Extract file names without extension
                var algorithmsNames = dllFiles.Select(file => System.IO.Path.GetFileNameWithoutExtension(file)).ToList();

                return Ok(algorithmsNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetParamsInfoForAlgorithm")]
        public IActionResult GetParamsInfoForAlgorithm(string optimizationAlgorithmName)
        {
            if (optimizationAlgorithmName == null)
            {
                return NotFound();
            }

            string optimizationAlgorithmsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            try
            {
                if (!Directory.Exists(optimizationAlgorithmsFolder))
                {
                    return BadRequest($"Folder {optimizationAlgorithmsFolder} does not exist.");
                }

                string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmName }, optimizationAlgorithmsFolder)[0];

                var paramsInfo = GetParams.GetParamsForAlgorithm(optimizationAlgorithmDLL);

                return Ok(paramsInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("RunAlgorithm")]
        public IActionResult RunAlgorithm(AlgorithmRunParameters algorithmRunParameters)
        {
            if (algorithmRunParameters == null)
            {
                return NotFound();
            }

            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            string optimizationAlgorithmsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            if (algorithmRunParameters.ParamsDict.Count == 0)
            {
                Dictionary<string, List<ParamForAlgorithm>> ParamsDict = new Dictionary<string, List<ParamForAlgorithm>>();

                foreach (var optimizationAlgorithmName in algorithmRunParameters.OptimizationAlgorithmNames)
                {
                    string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmName }, optimizationAlgorithmsFolder)[0];
                    var paramsInfo = GetParams.GetParamsForAlgorithm(optimizationAlgorithmDLL);
                    
                    List<ParamForAlgorithm> paramsList = new List<ParamForAlgorithm>();

                    foreach (var  param in paramsInfo)
                    {
                        ParamForAlgorithm paramForAlgorithm = new ParamForAlgorithm();

                        paramForAlgorithm.Name = param.Name;
                        paramForAlgorithm.UpperBoundry = param.UpperBoundry;
                        paramForAlgorithm.LowerBoundry = param.LowerBoundry;
                        paramForAlgorithm.Step = param.Step;

                        paramsList.Add(paramForAlgorithm);
                    }

                    ParamsDict[optimizationAlgorithmName] = paramsList;
                }

                algorithmRunParameters.ParamsDict = ParamsDict;
            }

            string stateFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "State");

            string resultsStatePath = System.IO.Path.Combine(stateFolder, "ResultsState.json");
            string testStatePath = System.IO.Path.Combine(stateFolder, "TestState.json");
            string algStatePath = System.IO.Path.Combine(stateFolder, "AlgorithmState.json");

            if (System.IO.File.Exists(resultsStatePath))
            {
                System.IO.File.Delete(resultsStatePath);
            }

            if (System.IO.File.Exists(testStatePath))
            {
                System.IO.File.Delete(testStatePath);
            }

            if (System.IO.File.Exists(algStatePath))
            {
                System.IO.File.Delete(algStatePath);
            }

            Directory.CreateDirectory(stateFolder);

            string isFinishedPath = System.IO.Path.Combine(stateFolder, "IsFinished.txt");
            bool isFinished = true;

            string lastQueryPath = System.IO.Path.Combine(stateFolder, "LastQuery.json");
            string json = JsonConvert.SerializeObject(algorithmRunParameters, Formatting.Indented);
            System.IO.File.WriteAllText(lastQueryPath, json);

            string reportsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Reports");

            // usuwanie folderu z raportami żeby nie zwracało poprzednich
            if (Directory.Exists(reportsFolder))
            {
                Directory.Delete(reportsFolder, true);
            }

            // int[] T = { 5, 10, 20, 40, 60, 80 };
            // int[] N = { 10, 20, 40, 80 };
            // int dim = 2;

            string[] optimizationAlgorithmNames = algorithmRunParameters.OptimizationAlgorithmNames;
            string[] testFunctionNames = algorithmRunParameters.TestFunctionNames;
            int dim = algorithmRunParameters.Dim;

            // string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            // string optimizationAlgorithmsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OptimizationAlgorithms");
            // czy na pewno w tym miejscu? a nie w katalogu bin z plikiem .exe?

            //if there are multiple optimization algorithms and one test function, then run all optimization algorithms for this test function
            if (testFunctionNames.Length == 1 && optimizationAlgorithmNames.Length != 1)
            {
               //for each optimization algorithm run test function
                    TestState testState = new TestState();
                    string testFunctionDLL = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder)[0];
                    string[] optimizationAlgorithmDLLs = SearchDLLs.SearchDLLsInDirectory( optimizationAlgorithmNames , optimizationAlgorithmsFolder);

                                    //testing started
                    isFinished = false;

                    using (StreamWriter writer = new StreamWriter(isFinishedPath))
                    {
                        writer.WriteLine(isFinished);
                    }  
                    var dllsAndNames = optimizationAlgorithmDLLs.Zip(optimizationAlgorithmNames, (d, n) => new { dll = d, name = n });
                    //write OptimizationAlgorithmNames and dlls to console
                    for (int iA = 0; iA < optimizationAlgorithmDLLs.Length; iA++)
                    {
                        var dn = dllsAndNames.ToArray()[iA];
                        List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.ParamsDict[dn.name];

                        var (returnedMinimum, returnedparams) = Backend.RunAlgorithm.Run(dn.dll, new string[] { testFunctionDLL }, dim, paramsForAlgorithm);
                        //write csv with returned minimum and parameters

                        string returnedMinimumAndParamsFilePath = System.IO.Path.Combine(reportsFolder, $"ReturnedMinimumAndParams ({dn.name}).csv");

                        dynamic testResult = new System.Dynamic.ExpandoObject() ;
                        
                        testResult.BestMinimum = returnedMinimum;
                        foreach(var parameter in returnedparams)
                        {
                            ((IDictionary<string, object>)testResult).Add(parameter.Key, parameter.Value);
                            Console.WriteLine($"Parametr: {parameter.Key}, wartość: {parameter.Value}");
                        }
                        List<object> testResults = new List<object>
                        {
                            testResult
                        };
                        
                        var config = new CsvHelper.Configuration.CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
                        // var float_options = new CsvHelper.TypeConversion.TypeConverterOptions { 
                        // Formats = new[] { ".##" } };
                        //TODO: float formating in csv

                        using (var writer = new StreamWriter(returnedMinimumAndParamsFilePath))
                        using (var csv = new  CsvHelper.CsvWriter(writer, config))
                        {
                            // csv.Configuration.TypeConverterOptionsCache.AddOptions<float>(float_options);
                            // csv.Configuration.TypeConverterOptionsCache.AddOptions<double>(float_options);
                            csv.WriteRecords(testResults);
                        }

                        Console.WriteLine($"Zapisano wyniki do pliku: {returnedMinimumAndParamsFilePath}");
                        SetTestState(testState, iA);

                        string json2 = JsonConvert.SerializeObject(testState, Formatting.Indented);
                        System.IO.File.WriteAllText(testStatePath, json2);
                    }
                                    System.IO.File.Delete(testStatePath);

                    //testing finished
                    isFinished = true;

                    using (StreamWriter writer = new StreamWriter(isFinishedPath))
                    {
                        writer.WriteLine(isFinished);
                    }
                    // foreach(var dn in dllsAndNames)
                    // {
                    //     Console.WriteLine($"Nazwa algorytmu: {dn.name}, ścieżka do pliku: {dn.dll}");
                    // }
                    // foreach (var optimizationAlgorithmDLL in optimizationAlgorithmDLLs)
                    // {
                    //     Console.WriteLine($"Nazwa algorytmu: {optimizationAlgorithmDLL}");
                    // }
                    // foreach (var optimizationAlgorithmName in optimizationAlgorithmNames)
                    // {
                    //     Console.WriteLine($"Nazwa algorytmu: {optimizationAlgorithmName}");
                    // }


                var reportFiles = Directory.GetFiles(reportsFolder, "*.csv");

                string zipFileName = "Reports.zip";
                string zipFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), zipFileName);

                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var reportFile in reportFiles)
                    {
                        zipArchive.CreateEntryFromFile(reportFile, System.IO.Path.GetFileName(reportFile));
                    }
                }

                byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);

                System.IO.File.Delete(zipFilePath);

                var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = zipFileName
                };

                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(zipFileBytes, "application/zip");
            }
            else 
            {
                List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.ParamsDict[optimizationAlgorithmNames[0]];
                string[] testFunctionDLLs = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder);
                string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmNames[0] }, optimizationAlgorithmsFolder)[0];

                //testing started
                isFinished = false;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }

                Backend.RunAlgorithm.Run(optimizationAlgorithmDLL, testFunctionDLLs, dim, paramsForAlgorithm);

                //testing finished
                isFinished = true;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }

                string reportFile = Directory.GetFiles(reportsFolder, "*.csv").FirstOrDefault();

                var stream = new FileStream(reportFile, FileMode.Open, FileAccess.Read);
                Response.ContentType = new MediaTypeHeaderValue("application/octet-stream").ToString();

                return new FileStreamResult(stream, "text/csv") { FileDownloadName = "Report.csv" };
            }
        }

        [HttpGet("IfCalculationsFinished")]
        public IActionResult IfCalculationsFinished()
        {
            string stateFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "State");
            Directory.CreateDirectory(stateFolder);

            string isFinishedPath = System.IO.Path.Combine(stateFolder, "IsFinished.txt");

            if (System.IO.File.Exists(isFinishedPath))
            {
                // Otwórz plik do odczytu
                using (StreamReader reader = new StreamReader(isFinishedPath))
                {
                    // Odczytaj i zwróć wartość boolowską z pliku
                    string line = reader.ReadLine();
                    if (bool.TryParse(line, out bool result))
                    {
                        return Ok(result);
                    }
                }
            }

            return Ok(true);
        }

        [HttpGet("GetResumedAlgorithmParams")]
        public IActionResult GetResumedAlgorithmParams()
        {
            string stateFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "State");
            Directory.CreateDirectory(stateFolder);

            string lastQueryPath = System.IO.Path.Combine(stateFolder, "LastQuery.json");
            AlgorithmRunParameters algorithmRunParameters = new AlgorithmRunParameters();

            if (System.IO.File.Exists(lastQueryPath))
            {
                string json = System.IO.File.ReadAllText(lastQueryPath);
                algorithmRunParameters = JsonConvert.DeserializeObject<AlgorithmRunParameters>(json);
            }
            else
            {
                return NotFound();
            }

            return Ok(algorithmRunParameters);
        }


        [HttpPost("ResumeAlgorithm")]
        public IActionResult ResumeAlgorithm()
        {
            string stateFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "State");
            Directory.CreateDirectory(stateFolder);

            string isFinishedPath = System.IO.Path.Combine(stateFolder, "IsFinished.txt");
            bool isFinished;

            string lastQueryPath = System.IO.Path.Combine(stateFolder, "LastQuery.json");
            AlgorithmRunParameters algorithmRunParameters = new AlgorithmRunParameters();

            if (System.IO.File.Exists(lastQueryPath))
            {
                string json = System.IO.File.ReadAllText(lastQueryPath);
                algorithmRunParameters = JsonConvert.DeserializeObject<AlgorithmRunParameters>(json);
            }
            else
            {
                return NotFound();
            }
            
            string reportsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Reports");

            string[] optimizationAlgorithmNames = algorithmRunParameters.OptimizationAlgorithmNames;
            string[] testFunctionNames = algorithmRunParameters.TestFunctionNames;
            int dim = algorithmRunParameters.Dim;

            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            string optimizationAlgorithmsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            
            if (testFunctionNames.Length == 1 && optimizationAlgorithmNames.Length != 1)
            {
                int iAStart = 0;
                
                string testStatePath = System.IO.Path.Combine(stateFolder, "TestState.json");
                TestState testState = new TestState();

                if (System.IO.File.Exists(testStatePath))
                {
                    string json2 = System.IO.File.ReadAllText(testStatePath);
                    testState = JsonConvert.DeserializeObject<TestState>(json2);
                    iAStart = testState.AlgorithmIterator;
                }

                string testFunctionDLL = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder)[0];
                string[] optimizationAlgorithmDLLs = SearchDLLs.SearchDLLsInDirectory(optimizationAlgorithmNames, optimizationAlgorithmsFolder);

                //testing started
                isFinished = false;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }
                var dllsAndNames = optimizationAlgorithmDLLs.Zip(optimizationAlgorithmNames, (d, n) => new { dll = d, name = n });
                for (int iA = iAStart; iA < optimizationAlgorithmDLLs.Length; iA++)
                {
                    var dn = dllsAndNames.ToArray()[iA];
                    List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.ParamsDict[dn.name];

                    var (returnedMinimum, returnedparams) = Backend.RunAlgorithm.Run(dn.dll, new string[] { testFunctionDLL }, dim, paramsForAlgorithm);

                    string returnedMinimumAndParamsFilePath = System.IO.Path.Combine(reportsFolder, $"ReturnedMinimumAndParams ({dn.name}).csv");
                    dynamic testResult = new System.Dynamic.ExpandoObject() ;
                    testResult.BestMinimum = returnedMinimum;
                    foreach(var parameter in returnedparams)
                    {
                        ((IDictionary<string, object>)testResult).Add(parameter.Key, parameter.Value);
                        Console.WriteLine($"Parametr: {parameter.Key}, wartość: {parameter.Value}");
                    }
                    List<object> testResults = new List<object>
                    {
                        testResult
                    };
                    var config = new CsvHelper.Configuration.CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
                    // var float_options = new CsvHelper.TypeConversion.TypeConverterOptions { 
                    // Formats = new[] { ".##" } };
                    //TODO: float formating in csv
                    using (var writer = new StreamWriter(returnedMinimumAndParamsFilePath))
                    using (var csv = new  CsvHelper.CsvWriter(writer, config))
                    {
                        // csv.Configuration.TypeConverterOptionsCache.AddOptions<float>(float_options);
                        // csv.Configuration.TypeConverterOptionsCache.AddOptions<double>(float_options);
                        csv.WriteRecords(testResults);
                    }

                    Console.WriteLine($"Zapisano wyniki do pliku: {returnedMinimumAndParamsFilePath}");
                    SetTestState(testState, iA);

                    string json2 = JsonConvert.SerializeObject(testState, Formatting.Indented);
                    System.IO.File.WriteAllText(testStatePath, json2);
                }

                System.IO.File.Delete(testStatePath);

                //testing finished
                isFinished = true;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }

                var reportFiles = Directory.GetFiles(reportsFolder, "*.csv");

                string zipFileName = "Reports.zip";
                string zipFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), zipFileName);

                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var reportFile in reportFiles)
                    {
                        zipArchive.CreateEntryFromFile(reportFile, System.IO.Path.GetFileName(reportFile));
                    }
                }

                byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);

                System.IO.File.Delete(zipFilePath);

                var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = zipFileName
                };

                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(zipFileBytes, "application/zip");
            }
            else
            {
                List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.ParamsDict[optimizationAlgorithmNames[0]];
                string[] testFunctionDLLs = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder);
                string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmNames[0] }, optimizationAlgorithmsFolder)[0];

                //testing started
                isFinished = false;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }

                Backend.RunAlgorithm.Run(optimizationAlgorithmDLL, testFunctionDLLs, dim, paramsForAlgorithm);

                //testing finished
                isFinished = true;

                using (StreamWriter writer = new StreamWriter(isFinishedPath))
                {
                    writer.WriteLine(isFinished);
                }

                string reportFile = Directory.GetFiles(reportsFolder, "*.csv").FirstOrDefault();

                var stream = new FileStream(reportFile, FileMode.Open, FileAccess.Read);
                Response.ContentType = new MediaTypeHeaderValue("application/octet-stream").ToString();

                return new FileStreamResult(stream, "text/csv") { FileDownloadName = "Report.csv" };
            }
        }

        private void SetTestState(TestState testState, int iA)
        {
            testState.AlgorithmIterator = iA + 1;
            testState.TestFuncIterator = 0;
            testState.ParamIterator = 0;
            testState.Iterator = 0;
            testState.BestData = null;
        }
    }
}