using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("PostTestFunction")]
        public IActionResult UploadTestFunction(TestFunction testFunction)
        {
            //TestFunction testFunction = new TestFunction
            //{
            //    Name = "SampleTestFunction",
            //    DLLPath = "SampleDLLPath",
            //    Params = new string[] { "Param1", "Param2" }
            //}


            if (testFunction == null)
            {
                return NotFound();
            }

            string testFunctionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFunctions");
            Directory.CreateDirectory(testFunctionsFolder);

            // Zapisanie DLL z funkcj¹ testow¹ do katalogu TestFunctions
            string testFunctionDLL = $@"{testFunction.DLLPath}";
            string testFunctionPath = Path.Combine(testFunctionsFolder, Path.GetFileName(testFunctionDLL));

            if (File.Exists(testFunctionPath))
            {
                File.Copy(testFunctionDLL, testFunctionPath, true);
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta³ nadpisany.");
            }
            else
            {
                File.Copy(testFunctionDLL, testFunctionPath);
                Console.WriteLine($"Plik {Path.GetFileName(testFunctionPath)} zosta³ skopiowany.");
            }

            return Ok(testFunction);
        }
    }
}