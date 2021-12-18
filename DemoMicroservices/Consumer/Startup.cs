using Consumer.Consumers;
using Consumer.Services;
using MassTransit;
using MassTransit.Definition;
using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Consumer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddRazorPages();

            services.AddTransient<OrderService>();

            services.AddMassTransit(configureMassTransit =>
            {
                configureMassTransit.AddConsumer<OrderConsumer>(configureConsumer =>
                {
                    configureConsumer.UseConcurrentMessageLimit(2);
                });

                if(Boolean.Parse(configuration["UsingAzureServiceBus"]))
                {
                    configureMassTransit.UsingAzureServiceBus((context, configure) =>
                    {
                        ServiceBusConnectionConfig.ConfigureNodes(configuration, configure, "AzureServiceBus");

                        // setup Azure queue consumer
                        configure.ReceiveEndpoint(configuration["Queue"], endpoint =>
                        {
                            // all of these are optional!!
                            endpoint.PrefetchCount = 4;

                            // number of "threads" to run concurrently
                            endpoint.MaxConcurrentCalls = 3;

                            endpoint.ConfigureConsumer<OrderConsumer>(context);
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
                    });
                }
            });

            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
