using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Consumer.Services
{
    public class OrderService
    {
        private readonly ILogger<OrderService> _logger;

        public OrderService(ILogger<OrderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task ProcessOrder(Guid orderId, double orderAmount, string orderNumber)
        {
            _logger.LogInformation(
                "Process Order {orderId}, {orderAmount}, {orderNumber}", orderId, orderAmount, orderNumber);

            Task.Delay(1000);

            _logger.LogInformation("Process Order End.");

            return Task.CompletedTask;
        }
    }
}
