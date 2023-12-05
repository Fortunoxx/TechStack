using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Queries;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> logger;
        private readonly IRequestClient<TestQuery> testQueryClient;

        public TestController(ILogger<TestController> logger, IRequestClient<TestQuery> testQueryClient)
        {
            this.logger = logger;
            this.testQueryClient = testQueryClient;
        }

        [HttpGet("{id:int}", Name = "GetSomeData")]
        public async Task<IActionResult> GetSomeData(int id)
        {
            var query = new TestQuery(id);
            var result = await testQueryClient.GetResponse<TestResult>(query);

            return Ok(result.Message);
        }
    }
}
