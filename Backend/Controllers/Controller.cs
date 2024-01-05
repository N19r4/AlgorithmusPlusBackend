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

        [HttpPost("UploadTestFunctionDLL")]
        public IActionResult UploadTestFunctionDLL(string testFunctionDLL)
        {
            if (testFunctionDLL == null)
            {
                return NotFound();
            }

            string testFunctionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            string testFunctionPath = Path.Combine(testFunctionsFolder, Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                System.IO.File.Copy(testFunctionDLL, testFunctionPath, true);
                Console.WriteLine($"File {Path.GetFileName(testFunctionPath)} has been overwritten.");
            }
            else
            {
                System.IO.File.Copy(testFunctionDLL, testFunctionPath);
                Console.WriteLine($"File {Path.GetFileName(testFunctionPath)} has been copied.");
            }

            return Ok();
        }

        [HttpPost("UploadOptimizationAlgorithmDLL")]
        public IActionResult UploadOptimizationAlgorithmDLL(string optimizationAlgorithmDLL)
        {
            if (optimizationAlgorithmDLL == null)
            {
                return NotFound();
            }

            string optimizationAlgorithmFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");
            Directory.CreateDirectory(optimizationAlgorithmFolder);

            string optimizationAlgorithmPath = Path.Combine(optimizationAlgorithmFolder, Path.GetFileName(optimizationAlgorithmDLL));

            if (System.IO.File.Exists(optimizationAlgorithmPath))
            {
                System.IO.File.Copy(optimizationAlgorithmDLL, optimizationAlgorithmPath, true);
                Console.WriteLine($"File {Path.GetFileName(optimizationAlgorithmPath)} has been overwritten.");
            }
            else
            {
                System.IO.File.Copy(optimizationAlgorithmDLL, optimizationAlgorithmPath);
                Console.WriteLine($"File {Path.GetFileName(optimizationAlgorithmPath)} has been copied.");
            }

            return Ok();
        }

        [HttpGet("GetAllTestFunctionsNames")]
        public IActionResult GetAllTestFunctionsNames()
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

        [HttpGet("GetAllAlgorithmsNames")]
        public IActionResult GetAllAlgorithmsNames()
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

        [HttpGet("GetParamsInfoForAlgorithm")]
        public IActionResult GetParamsInfoForAlgorithm(string optimizationAlgorithmName)
        {
            if (optimizationAlgorithmName == null)
            {
                return NotFound();
            }

            string optimizationAlgorithmsFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

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
                            UpperBoundary = PropertyValue.GetPropertyValue<double>(paramInfo, "UpperBoundary"),
                            LowerBoundary = PropertyValue.GetPropertyValue<double>(paramInfo, "LowerBoundary")
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

            // int[] T = { 5, 10, 20, 40, 60, 80 };
            // int[] N = { 10, 20, 40, 80 };
            // int dim = 2;

            string optimizationAlgorithmName = algorithmRunParameters.OptimizationAlgorithmName;
            string[] testFunctionNames = algorithmRunParameters.TestFunctionNames;
            int dim = algorithmRunParameters.Dim;
            List<ParamForAlgorithm> paramsForAlgorithm = algorithmRunParameters.paramsForAlgorithm;

            // string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            // string optimizationAlgorithmsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OptimizationAlgorithms");
            // czy na pewno w tym miejscu? a nie w katalogu bin z plikiem .exe?

            string testFunctionsFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestFunctions");
            string optimizationAlgorithmsFolder = Path.Combine(Directory.GetCurrentDirectory(), "OptimizationAlgorithms");

            string[] testFunctionDLLs = SearchDLLs.SearchDLLsInDirectory(testFunctionNames, testFunctionsFolder);
            string optimizationAlgorithmDLL = SearchDLLs.SearchDLLsInDirectory(new string[] { optimizationAlgorithmName }, optimizationAlgorithmsFolder)[0];

            Backend.RunAlgorithm.Run(optimizationAlgorithmDLL, testFunctionDLLs, dim, paramsForAlgorithm);

            return Ok();
        }
    }
}