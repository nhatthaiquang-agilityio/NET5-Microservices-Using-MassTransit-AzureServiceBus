using Azure.Messaging.ServiceBus.Administration;
using MassTransit;
using MassTransit.Definition;
using Messages;
using Messages.Commands;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Consumers;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);

            var host = hostBuilder.Build();

            Task.Run(async () => await host.RunAsync()).GetAwaiter().GetResult();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(configureMassTransit =>
                    {
                        configureMassTransit.AddConsumer<PushNotificationConsumer>(configureConsumer =>
                        {
                            configureConsumer.UseConcurrentMessageLimit(2);
                        });

                        if(Boolean.Parse(configuration["UsingAzureServiceBus"]))
                        {
                            configureMassTransit.UsingAzureServiceBus((context, configure) =>
                            {
                                ServiceBusConnectionConfig.ConfigureNodes(configuration, configure, "AzureServiceBus");

                                configure.ReceiveEndpoint(configuration["Queue"], receive =>
                                {
                                    receive.ConfigureConsumeTopology = false;

                                    // all of these are optional!!
                                    receive.PrefetchCount = 4;

                                    // number of "threads" to run concurrently
                                    receive.MaxConcurrentCalls = 3;

                                    receive.ConfigureConsumer<PushNotificationConsumer>(context);

                                    receive.Subscribe<INotification>(configuration["Topic"], x =>
                                    {
                                        x.Filter = new SqlFilter("NotificationType = 'Push'");
                                    });
                                });
                            });
                        }
                        else
                        {
                            configureMassTransit.UsingRabbitMq((context, configure) =>
                            {
                                configure.PrefetchCount = 4;

                                // Ensures the processor gets its own queue for any consumed messages
                                configure.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(true));

                                ServiceBusConnectionConfig.ConfigureNodes(configuration, configure, "MessageBus");

                                configure.ReceiveEndpoint(configuration["Queue"], receive =>
                                {
                                    // turns off default fanout
                                    receive.ConfigureConsumeTopology = false;

                                    // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                                    receive.SetQuorumQueue();

                                    // enables a lazy queue for more stable cluster with better predictive performance.
                                    // Please note that you should disable lazy queues if you require really high performance, if the queues are always short, or if you have set a max-length policy.
                                    receive.SetQueueArgument("declare", "lazy");

                                    receive.ConfigureConsumer<PushNotificationConsumer>(context);

                                    receive.Bind(configuration["BindTopic"], eventMessage =>
                                    {
                                        eventMessage.RoutingKey = configuration["Topic"];
                                        eventMessage.ExchangeType = ExchangeType.Topic;
                                    });
                                });

                            });
                        }
                    });

                    services.AddMassTransitHostedService();
                });
        }
    }
}
