using Microsoft.AspNetCore.Mvc;
using MQManager.Services;

namespace MQManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueuesController : ControllerBase
    {
        private readonly RabbitMQSetupService _rabbitMQSetupService;

        public QueuesController(RabbitMQSetupService rabbitMQSetupService)
        {
            _rabbitMQSetupService = rabbitMQSetupService;
        }

        [HttpPost]
        [Route("CreatePredefinedQueues")]
        public IActionResult CreatePredefinedQueues()
        {
            _rabbitMQSetupService.CreateQueues();
            return Ok("Predefined queues created successfully.");
        }

        [HttpDelete]
        [Route("ClearAllQueues")]
        public IActionResult ClearAllQueues()
        {
            _rabbitMQSetupService.ClearQueues();
            return Ok("All queues cleared successfully.");
        }
    }
}
