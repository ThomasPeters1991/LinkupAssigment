using Microsoft.AspNetCore.Mvc;
using MQManager.Services;

namespace MQManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueuesController: ControllerBase
    {
        private readonly RabbitMQSetupService _rabbitMQSetupService;

        public QueuesController(RabbitMQSetupService rabbitMQSetupService)
        {
            _rabbitMQSetupService = rabbitMQSetupService;
        }

        [HttpPost]
        [Route("CreateQueues")]
        public IActionResult CreateQueues()
        {
            _rabbitMQSetupService.CreateQueues();
            return Ok("Queues created successfully.");
        }
    }
}
