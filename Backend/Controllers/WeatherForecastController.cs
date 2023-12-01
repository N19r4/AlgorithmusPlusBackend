using Microsoft.AspNetCore.Mvc;

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

        [HttpGet(Name = "GetTestFunction")]
        [ProducesResponseType(200, Type = typeof(TestFunction))]
        [ProducesResponseType(400)]
        public IActionResult Get(int id)
        {
            TestFunction testFunction = new TestFunction
            {
                Name = "SampleTestFunction",
                DLLPath = "SampleDLLPath",
                Params = new string[] { "Param1", "Param2" }
            };

            if (testFunction == null)
            {
                return NotFound(); // Zwróæ 404 Not Found, jeœli nie znaleziono funkcji testowej.
            }

            return Ok(testFunction);
        }
    }
}