using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Queries;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IRequestClient<TestQuery> testQueryClient;

        public TestController(IRequestClient<TestQuery> testQueryClient)
            => this.testQueryClient = testQueryClient;

        [HttpGet("{id:int}", Name = "GetSomeData")]
        public async Task<IActionResult> GetSomeData(int id)
        {
            var query = new TestQuery(id);
            var result = await testQueryClient.GetResponse<TestResult>(query);

            return Ok(result.Message);
        }

        [AllowAnonymous]
        [HttpGet("anonymous/{id:int}", Name = "GetSomeAnonymousData")]
        public async Task<IActionResult> GetSomeAnonymousData(int id)
        {
            var query = new TestQuery(id);
            var result = await testQueryClient.GetResponse<TestResult>(query);

            return Ok(result.Message);
        }
    }
}
