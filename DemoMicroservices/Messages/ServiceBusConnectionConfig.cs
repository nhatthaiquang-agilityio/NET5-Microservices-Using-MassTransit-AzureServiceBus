using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Messages
{
    /// <summary>
    /// Translates an AMQP connection string and configures MassTransit.RabbitMq
    /// </summary>
    public class ServiceBusConnectionConfig
    {
        private const string DefaultVirtualHost = "/";

        /// <summary>
        /// Configures MassTransit.RabbitMq with properties for successful connections
        /// Supports single node and cluster node implementations
        /// Requires amqp:// protocol in the connection string
        /// Expects connection string to follow RabbitMQ format - https://www.rabbitmq.com/uri-spec.html
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurator">configurator of RabbitMQ</param>
        /// <param name="connectionStringName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ConfigureNodes(
            IConfiguration configuration, IRabbitMqBusFactoryConfigurator configurator, string connectionStringName)
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);

            if (connectionString == null)
            {
                throw new InvalidOperationException($"{connectionString} is not provided in the appsettings.json");
            }

            var nodes = ExtractValuesFromConnectionString(connectionString.Replace("amqp://",""));
            var listNodes = nodes ?? nodes;

            if (listNodes.Skip(1).Any())
            {
                ConfigureRabbitMqForCluster(configurator, listNodes);
                return;
            }

            // single nodes
            var singleInstanceNode = listNodes.Single();

            configurator.Host(singleInstanceNode.HostName, singleInstanceNode.VirtualHost, configure =>
            {
                configure.Username(singleInstanceNode.UserName);
                configure.Password(singleInstanceNode.Password);
            });
        }

        /// <summary>
        /// Configures Azure Service Bus with properties for successful connections
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurator">configurator of Azure Service Bus</param>
        /// <param name="connectionStringName">name of connection string</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ConfigureNodes(
            IConfiguration configuration, IServiceBusBusFactoryConfigurator configurator, string connectionStringName)
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);

            if (connectionString == null)
            {
                throw new InvalidOperationException($"{connectionString} is not provided in the appsettings.json");
            }

            // single nodes
            configurator.Host(connectionString);
        }

        /// <summary>
        /// Extract message bus string(connection string) to list nodes
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static IEnumerable<ServiceBusConnectionConfiguration> ExtractValuesFromConnectionString(string connectionString)
        {
            var nodes = new List<ServiceBusConnectionConfiguration>();
            var nodeUris = connectionString.Split(";", StringSplitOptions.RemoveEmptyEntries);

            foreach (var nodeUri in nodeUris)
            {
                var nodeConfig = TranslateMessageBusUriToNodeConfig(nodeUri);
                nodes.Add(nodeConfig);
            }

            return nodes;
        }

        /// <summary>
        /// Translare uri string to node
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static ServiceBusConnectionConfiguration TranslateMessageBusUriToNodeConfig(string uri)
        {
            var userAndPass = uri.Split('@');

            if (userAndPass.Length < 2)
            {
                throw new InvalidOperationException("couldn't parse username and password from connection string");
            }

            var userAndPassSplit = userAndPass[0].Split(":");
            var username = userAndPassSplit[0];
            var password = userAndPassSplit[1];

            var hostAndPort = userAndPass[1].Split(":");
            var hostname = string.Empty;
            var port = string.Empty;

            if (hostAndPort.Length == 2)
            {
                hostname = hostAndPort[0].TrimEnd('/');
                port = hostAndPort[1];
            }
            else
            {
                hostname = hostAndPort[0].TrimEnd('/');
            }
           
            var virtualHost = DefaultVirtualHost;

            return new ServiceBusConnectionConfiguration
            {
                HostName = hostname,
                UserName = username,
                Password = password,
                Port = port,
                VirtualHost = virtualHost
            };
        }

        /// <summary>
        /// Config host, username, password to cluster if we have multi nodes
        /// </summary>
        /// <param name="configurator">RabbitMQ Bus Configurator</param>
        /// <param name="nodes">List nodes RabbitMQ</param>
        private static void ConfigureRabbitMqForCluster(
            IRabbitMqBusFactoryConfigurator configurator, IEnumerable<ServiceBusConnectionConfiguration> nodes)
        {
            var listNodes = nodes as ServiceBusConnectionConfiguration[] ?? nodes.ToArray();
            var firstNode = listNodes.First();

            configurator.Host(firstNode.HostName, firstNode.VirtualHost, configure =>
            {
                configure.Username(firstNode.UserName);
                configure.Password(firstNode.Password);

                foreach (var node in listNodes)
                {
                    configure.UseCluster(cluster => { cluster.Node(node.HostName); });
                }
            });
        }
    }

    /// <summary>
    /// Representation of a RabbitMQ Connection string
    /// </summary>
    public class ServiceBusConnectionConfiguration
    {
        /// <summary>
        /// Gets or sets the HostName defined in the connection string
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the VirtualHost appended to the HostName
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Gets or sets the UserName prepended to the HostName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password prepended to the HostName
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Port prepended to the HostName
        /// </summary>
        public string Port { get; set; }
    }
}
