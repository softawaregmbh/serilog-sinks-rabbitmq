using RabbitMQ.Client;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            const string queueName = "log";
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            }
            var rabbitMQConfig = new RabbitMQConfiguration
            {
                Hostname = connectionFactory.HostName,
                Port = connectionFactory.Port,
                Username = connectionFactory.UserName,
                Password = connectionFactory.Password,
                RouteKey = queueName,
                BatchPostingLimit = 5,
                Period = TimeSpan.FromSeconds(10)
            };
            var logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.Sink(new RabbitMQSink(rabbitMQConfig, new JsonFormatter(), null))
                .CreateLogger();
            using (logger)
            {
                var i = 0;
                var timer = new Timer(_ => { logger.Information($"Running {++i}"); }, null, 0, 1000);
                Console.WriteLine("Press enter to quit");
                Console.ReadLine();
            }
        }
    }
}
