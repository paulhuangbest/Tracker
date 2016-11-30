using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Common
{
    public class MQInfo
    {
        public string HostName { get; set; }

        public string ExchangeName { get; set; }

        public string ExchangeType { get; set; }

        public string QueueName { get; set; }

        public string RoutingKey { get; set; }

        
    }


    public class ConsumerInfo
    {
        public string HostName { get; set; }

        public string QueueName { get; set; }

        public string ConsumerTag { get; set; }

        public Dictionary<string, string> Args { get; set; }

        public EventHandler<BasicDeliverEventArgs> Handler { get; set; }

        public Func<Dictionary<string,string>, string> Notice { get; set; }
    }


    public class ConsumerTask
    {
        public Task Task { get; set; }

        public CancellationTokenSource TokenSource { get; set; }
    }

    public class RabbitMQHelper
    {
       

        public static void CreateMQ(MQInfo info)
        {
            var factory = new ConnectionFactory() { HostName = info.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: info.ExchangeName,//"direct_" + profile.ProjectKey,
                                            type: info.ExchangeType);


                    var queueName = channel.QueueDeclare(info.QueueName, true, false, false, null);


                    channel.QueueBind(queue: queueName,
                                        exchange: info.ExchangeName,//"direct_" + profile.ProjectKey,
                                        routingKey: info.RoutingKey);


                    //string[] types = Enum.GetNames(typeof(LogType));

                    //foreach (string type in types)
                    //{
                    //    string queue = "", severity = "";

                    //    switch (type)
                    //    {
                    //        case "ExceptionLog":
                    //            queue = "mq_exception_" + profile.ProjectKey;
                    //            severity = "exception";
                    //            break;

                    //        case "OperateLog":
                    //            queue = "mq_operate_" + profile.ProjectKey;
                    //            severity = "operate";
                    //            break;

                    //        case "SystemLog":
                    //            queue = "mq_system_" + profile.ProjectKey;
                    //            severity = "system";
                    //            break;

                    //        case "Normal":
                    //            queue = "mq_normal_" + profile.ProjectKey;
                    //            severity = "normal";
                    //            break;
                    //    }



                    //    var queueName = channel.QueueDeclare(queue, true, false, false, null);


                    //    channel.QueueBind(queue: queueName,
                    //                        exchange: "direct_" + profile.ProjectKey,
                    //                        routingKey: severity);

                    //}

                }
            }
        }

        public static ConsumerTask CreateConsumer(ConsumerInfo info)
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            Task t = Task.Run(() =>
            {
                var factory = new ConnectionFactory() { HostName = info.HostName };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += info.Handler;

                        channel.BasicConsume(queue: info.QueueName,
                             noAck: true,
                             consumerTag: info.ConsumerTag,
                             consumer: consumer);

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                break;
                            }

                            if (info.Notice != null)
                                info.Notice(info.Args);

                            Thread.Sleep(5000);
                        }
                    }
                }

            }, ct);

            return new ConsumerTask() { 
                Task = t,
                TokenSource = tokenSource
            };
        }
    }
}
