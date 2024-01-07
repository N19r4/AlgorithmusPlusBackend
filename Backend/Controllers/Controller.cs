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

            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            string testFunctionPath = System.IO.Path.Combine(testFunctionsFolder, System.IO.Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                System.IO.File.Copy(testFunctionDLL, testFunctionPath, true);
                Console.WriteLine($"File {System.IO.Path.GetFileName(testFunctionPath)} has been overwritten.");
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

            string optimizationAlgorithmFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            Directory.CreateDirectory(optimizationAlgorithmFolder);

            string optimizationAlgorithmPath = System.IO.Path.Combine(optimizationAlgorithmFolder, System.IO.Path.GetFileName(optimizationAlgorithmDLL));

            if (System.IO.File.Exists(optimizationAlgorithmPath))
            {
                System.IO.File.Copy(optimizationAlgorithmDLL, optimizationAlgorithmPath, true);
                Console.WriteLine($"File {System.IO.Path.GetFileName(optimizationAlgorithmPath)} has been overwritten.");
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
                            LowerBoundry = PropertyValue.GetPropertyValue<double>(paramInfo, "LowerBoundry")
                        }
                        );
                }

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

            // usuwanie folderu z raportami żeby nie zwracało poprzednich
            string reportsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Reports");

            try
            {
                Directory.Delete(reportsFolder, true);
                Console.WriteLine($"Usunięto folder: {reportsFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas usuwania folderu: {ex.Message}");
            }

            // int[] T = { 5, 10, 20, 40, 60, 80 };
            // int[] N = { 10, 20, 40, 80 };
            // int dim = 2;

            string[] optimizationAlgorithmNames = algorithmRunParameters.OptimizationAlgorithmNames;
            string[] testFunctionNames = algorithmRunParameters.TestFunctionNames;
            int dim = algorithmRunParameters.Dim;
            List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.paramsForAlgorithm;

            // string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            // string optimizationAlgorithmsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OptimizationAlgorithms");
            // czy na pewno w tym miejscu? a nie w katalogu bin z plikiem .exe?

            string testFunctionsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            string optimizationAlgorithmsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            //if there are multiple optimization algorithms and one test function, then run all optimization algorithms for this test function
            if (testFunctionNames.Length == 1 && optimizationAlgorithmNames.Length != 1)
            {
                string testFunctionDLL = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder)[0];
                string[] optimizationAlgorithmDLLs = SearchDLLs.SearchDLLsInDirectory( optimizationAlgorithmNames , optimizationAlgorithmsFolder);
                
                foreach (var oneOptimizationAlgorithmDLL in optimizationAlgorithmDLLs)
                {
                    Backend.RunAlgorithm.Run(oneOptimizationAlgorithmDLL, new string[] { testFunctionDLL }, dim, paramsForAlgorithm);
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

                // Pobierz dane binarne pliku ZIP
                byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);

                // Usuń plik ZIP po dodaniu do odpowiedzi
                System.IO.File.Delete(zipFilePath);

                // Zwróć plik ZIP jako odpowiedź HTTP
                var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = zipFileName
                };

                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(zipFileBytes, "application/zip");
            }
            else 
            {
                string[] testFunctionDLLs = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder);
                string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmNames[0] }, optimizationAlgorithmsFolder)[0];

                Backend.RunAlgorithm.Run(optimizationAlgorithmDLL, testFunctionDLLs, dim, paramsForAlgorithm);

                string reportFile = Directory.GetFiles(reportsFolder, "*.csv").FirstOrDefault();

                var stream = new FileStream(reportFile, FileMode.Open, FileAccess.Read);
                Response.ContentType = new MediaTypeHeaderValue("application/octet-stream").ToString();

                return new FileStreamResult(stream, "text/csv") { FileDownloadName = "Report.csv" };
            }
        }
    }
}