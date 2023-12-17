using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiExampleController : ControllerBase
    {
        private static readonly string name = "ApiExample";
        private static readonly string description = "This is an example of an API endpoint.";

        private readonly ILogger<ApiExampleController> _logger;

        public ApiExampleController(ILogger<ApiExampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetApiExample")]
        public ApiExample Get()
        {
            return new ApiExample
            {
                Name = name,
                Description = description
            };
        }

        [HttpPost(Name = "PostApiExample")]
        public string Post(ApiExample apiExample)
        {
            // new ApiExample
            // {
            //     Name = name,
            //     Description = description
            // };
            //log to console
            _logger.LogInformation("POST request made to ApiExampleController");
            _logger.LogInformation("Name: " + apiExample.Name);
            _logger.LogInformation("Description: " + apiExample.Description);
            return "200 OK";
        }
    }
}