using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        [HttpPost("PostTestFunction")]
        public IActionResult UploadTestFunction(TestFunction testFunction)
        {
            if (testFunction == null)
            {
                return NotFound();
            }

            string testFunctionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            // Zapisanie DLL z funkcj� testow� do katalogu TestFunctions
            string testFunctionDLL = $"{testFunction.DLLPath}";
            string testFunctionPath = Path.Combine(testFunctionsFolder, Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                Console.WriteLine($"File {Path.GetFileName(testFunctionPath)} has been overwritten.");

            }
            else
            {
                Console.WriteLine($"File {Path.GetFileName(testFunctionPath)} has been copied.");

            }
            System.IO.File.Copy(testFunctionDLL, testFunctionPath, true);

            string fileNameList = "";
            fileNameList += $"{testFunction.Name};{testFunctionPath}";

            string path = Path.Combine(Directory.GetCurrentDirectory(), "testFunctionsList.txt");
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(fileNameList);
            }

            return Ok(testFunction);
        }

        [HttpPost("PostOptimizationAlgorithm")]
        public IActionResult UploadOptimizationAlgorithm(OptimizationAlgorithm optimizationAlgorithm)
        {
            if (optimizationAlgorithm == null)
            {
                return NotFound();
            }

            string optimizationAlgorithmFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            Directory.CreateDirectory(optimizationAlgorithmFolder);

            // Zapisanie DLL z funkcj� testow� do katalogu TestFunctions
            string optimizationAlgorithmDLL = $"{optimizationAlgorithm.DLLPath}";
            string optimizationAlgorithmPath = Path.Combine(optimizationAlgorithmFolder, Path.GetFileName(optimizationAlgorithmDLL));

            if (System.IO.File.Exists(optimizationAlgorithmPath))
            {
                Console.WriteLine($"File {Path.GetFileName(optimizationAlgorithmPath)} has been overwritten.");

            }
            else
            {
                Console.WriteLine($"File {Path.GetFileName(optimizationAlgorithmPath)} has been copied.");

            }
            System.IO.File.Copy(optimizationAlgorithmDLL, optimizationAlgorithmPath, true);

            string fileNameList = "";
            fileNameList += $"{optimizationAlgorithm.Name};{optimizationAlgorithmPath}";

            string path = Path.Combine(Directory.GetCurrentDirectory(), "optimizationAlgorithmsList.txt");
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(fileNameList);
            }

            return Ok(optimizationAlgorithm);
        }

        [HttpGet("GetSelectedTestFunctions")]
        public IActionResult GetSelectedTestFunctions()
        {
            string testFunctionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");

            try
            {
                if (!Directory.Exists(testFunctionsFolder))
                {
                    return BadRequest($"Folder {testFunctionsFolder} does not exist.");
                }

                // Get all DLL files in the TestFunctions folder
                var dllFiles = Directory.GetFiles(testFunctionsFolder, "*.dll");

                // Extract file names without extension
                var testFunctionNames = dllFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();

                return Ok(testFunctionNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");

            }
        }

        [HttpGet("GetSelectedAlgorithms")]
        public IActionResult GetSelectedAlgorithms()
        {
            string algorithmsFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            try
            {
                if (!Directory.Exists(algorithmsFolder))
                {
                    return BadRequest($"Folder {algorithmsFolder} does not exist.");
                }

                // Get all DLL files in the TestFunctions folder
                var dllFiles = Directory.GetFiles(algorithmsFolder, "*.dll");

                // Extract file names without extension
                var algorithmsNames = dllFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();

                return Ok(algorithmsNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");

            }
        }

        [HttpPost("RunAlgorithm")]
        public IActionResult RunAlgorithm(AlgorithmParameters algorithmParameters)
        {
            if (algorithmParameters == null)
            {
                return NotFound();
            }

            // int[] T = { 5, 10, 20, 40, 60, 80 };
            // int[] N = { 10, 20, 40, 80 };
            // int dim = 2;

            string optimizationAlgorithmName = algorithmParameters.OptimizationAlgorithmName;
            string[] testFunctionNames = algorithmParameters.TestFunctionNames;
            int dim = algorithmParameters.Dim;

            // string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            // string optimizationAlgorithmsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OptimizationAlgorithms");
            // czy na pewno w tym miejscu? a nie w katalogu bin z plikiem .exe?

            string testFunctionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            string optimizationAlgorithmsFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            string[] testFunctionDLLs = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder);
            string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmName }, optimizationAlgorithmsFolder)[0];

            TestDLL.RunAlgorithm.Run(optimizationAlgorithmDLL, testFunctionDLLs, dim);

            return Ok();
        }
    }
}