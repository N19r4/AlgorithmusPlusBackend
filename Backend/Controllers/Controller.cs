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

            // Zapisanie DLL z funkcj¹ testow¹ do katalogu TestFunctions
            string testFunctionDLL = $"{testFunction.DLLPath}";
            string testFunctionPath = Path.Combine(testFunctionsFolder, Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta³ nadpisany.");
            }
            else
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta³ skopiowany.");
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

            // Zapisanie DLL z funkcj¹ testow¹ do katalogu TestFunctions
            string optimizationAlgorithmDLL = $"{optimizationAlgorithm.DLLPath}";
            string optimizationAlgorithmPath = Path.Combine(optimizationAlgorithmFolder, Path.GetFileName(optimizationAlgorithmDLL));

            if (System.IO.File.Exists(optimizationAlgorithmPath))
            {
                Console.WriteLine($"Plik {Path.GetFileName(optimizationAlgorithmPath)} zosta³ nadpisany.");
            }
            else
            {
                Console.WriteLine($"Plik {Path.GetFileName(optimizationAlgorithmPath)} zosta³ skopiowany.");
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

        [HttpPost("PostSelectedTestFunctions")]
        public IActionResult PostSelectedTestFunctions(List<string> testFunctionsNames)
        {
            if (testFunctionsNames == null)
            {
                return NotFound();
            }

            List<Tuple<string, string>> lines = new List<Tuple<string, string>>();

            try
            {
                string[] fileLines = System.IO.File.ReadAllLines("testFunctionList.txt");

                foreach (string line in fileLines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == 2)
                    {
                        string name = parts[0].Trim();
                        string path = parts[1].Trim();
                        lines.Add(new Tuple<string, string>(name, path));
                    }
                }
            }
            catch (IOException e)
            {
                return BadRequest($"B³¹d odczytu pliku: {e.Message}");
            }

            List<string> testFunctionPaths = new List<string>();

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                string currentName = lines[i].Item1;

                if (testFunctionsNames.Contains(currentName))
                {
                    string currentPath = lines[i].Item2;
                    testFunctionPaths.Add(currentPath);
                }

                if (testFunctionPaths.Count == testFunctionsNames.Count)
                    break;
            }

            return Ok(testFunctionPaths);
        }
    }
}