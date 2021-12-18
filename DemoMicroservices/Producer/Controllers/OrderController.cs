using MassTransit;
using Messages.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IBus _bus;

        public OrderController(ILogger<OrderController> logger, IBus bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("Get Order API");
            return new List<string>();
        }

        [HttpPost]
        public async Task<IActionResult> OrderProduct(Messages.Models.OrderViewModel order)
        {
            _logger.LogInformation("Post Order API");

            if (order != null)
            {
                await _bus.Send(new Order() {
                    OrderId = Guid.NewGuid(),
                    OrderAmount = order.OrderAmount,
                    OrderDate = DateTime.Now,
                    OrderNumber = order.OrderNumber
                }); ;

                _logger.LogInformation("Send to a message {OrderAmount}, {OrderNumber}", order.OrderAmount, order.OrderNumber);

                return Ok("Your order is processing.");
            }

            return BadRequest();
        }
    }
}
