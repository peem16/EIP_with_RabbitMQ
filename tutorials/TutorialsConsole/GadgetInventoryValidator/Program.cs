﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadgetInventoryValidator
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var exchange = "new_orders";
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange, "topic");
                channel.ExchangeDeclare("gadget", "topic");

                channel.ExchangeBind("gadget", exchange, "*.gadget");

                var queueName = channel.QueueDeclare("gadget", true, false, true, null).QueueName;
                 
                channel.QueueBind(queueName, "gadget", "#");
                
                Console.WriteLine(" [*] Waiting for validate new order. " +
                                  "To exit press CTRL+C");

                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queueName, true, consumer);

                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Validated '{0}' ", message);
                }
            }
        }
    }
}
