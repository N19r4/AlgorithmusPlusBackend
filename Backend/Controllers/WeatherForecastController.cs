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
    public class UploadFilesController : ControllerBase
    {
        private readonly ILogger<UploadFilesController> _logger;

        public UploadFilesController(ILogger<UploadFilesController> logger)
        {
            _logger = logger;
        }

        [HttpPost("PostTestFunction")]
        public IActionResult UploadTestFunction(FunctionUpload testFunction)
        {

            if (testFunction == null)
            {
                return NotFound();
            }

            string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            // Zapisanie DLL z funkcj� testow� do katalogu TestFunctions
            string testFunctionDLL = $@"{testFunction.DLLPath}";
            string testFunctionPath = Path.Combine(testFunctionsFolder, Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta� nadpisany.");

            }
            else
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta� skopiowany.");
            }
            System.IO.File.Copy(testFunctionDLL, testFunctionPath, true);

            string fileNameList = "";
            fileNameList += $"{testFunction.Name}:{testFunction.DLLPath}\n";

            string path = Path.Combine(Directory.GetCurrentDirectory(), "testFunctionList.txt");
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(fileNameList);
            }

            return Ok(testFunction);
        }

        [HttpPost("PostAlgorithmFunction")]
        public IActionResult UploadAlgorithmFunction(FunctionUpload uploadedFunction)
        {

            if (uploadedFunction == null)
            {
                return NotFound();
            }

            string algorithmsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlgorithmFunctions");
            Directory.CreateDirectory(algorithmsFolder);

            // Zapisanie DLL z funkcj� testow� do katalogu TestFunctions
            string testFunctionDLL = $@"{uploadedFunction.DLLPath}";
            string testFunctionPath = Path.Combine(algorithmsFolder, Path.GetFileName(testFunctionDLL));

            if (System.IO.File.Exists(testFunctionPath))
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta� nadpisany.");

            }
            else
            {
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta� skopiowany.");
            }
            System.IO.File.Copy(testFunctionDLL, testFunctionPath, true);

            string fileNameList = "";
            fileNameList += $"{uploadedFunction.Name}:{uploadedFunction.DLLPath}\n";

            string path = Path.Combine(Directory.GetCurrentDirectory(), "AlgorithmFunctionList.txt");
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(fileNameList);
            }

            return Ok(uploadedFunction);
        }
    }

    
}