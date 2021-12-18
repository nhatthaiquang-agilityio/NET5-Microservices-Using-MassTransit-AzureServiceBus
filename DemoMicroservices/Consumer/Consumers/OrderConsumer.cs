using Consumer.Services;
using MassTransit;
using Messages.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace Consumer.Consumers
{
    public class OrderConsumer : IConsumer<Order>
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly OrderService _orderService;

        public OrderConsumer(ILogger<OrderConsumer> logger, OrderService orderService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public async Task Consume(ConsumeContext<Order> context)
        {
            _logger.LogInformation("Consume Order Message");

            var data = context.Message;

            _logger.LogInformation(
                "Data: {OrderId}, {OrderAmount}, {OrderNumber}", data.OrderId, data.OrderAmount, data.OrderNumber);

            try
            {
                // TODO: call servive/task
                await _orderService.ProcessOrder(data.OrderId, data.OrderAmount, data.OrderNumber);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to ProcessOrder. CorrelationId {OrderId}, {OrderAmount}, {OrderNumber}",
                    data.OrderId, data.OrderAmount, data.OrderNumber);
            }

            _logger.LogInformation("Consumed Order Message");
        }
    }
}
