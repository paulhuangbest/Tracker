using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Entity;

namespace Core.Controllers
{
    public class CoreController : Controller
    {
        // GET: Core
        public ActionResult Index()
        {
            return View();
        }

        
        public void Join()
        {
            Task.Factory.StartNew(() =>
            {
                
                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");
                        var queueName = channel.QueueDeclare("q_system", true, false, false, null);



                        //foreach (var severity in args)
                        {
                            channel.QueueBind(queue: queueName,
                                              exchange: "direct_logs",
                                              routingKey: "info");
                        }



                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            var routingKey = ea.RoutingKey;

                        };

                        channel.BasicConsume(queue: queueName,
                                             noAck: true,
                                             consumer: consumer);

                        while (true)
                        {
                            

                            Thread.Sleep(5000);
                        }
                        


                    }
                    
            });

            
        }

        public void Split()
        {
            string[] types = Enum.GetNames(typeof(LogType));

            
            

            foreach (string type in types)
            {
                Task t = Task.Factory.StartNew(() =>
                {
                    string queue = "", severity = "";

                    switch (type)
                    {
                        case "ExceptionLog":
                            queue = "q_exception";
                            severity = "exception";
                            break;

                        case "OperateLog":
                            queue = "q_operate";
                            severity = "operate";
                            break;

                        case "SystemLog":
                            queue = "q_system";
                            severity = "system";
                            break;

                        case "Normal":
                            queue = "q_normal";
                            severity = "normal";
                            break;
                    }



                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");
                        var queueName = channel.QueueDeclare(queue, true, false, false, null);



                        //foreach (var severity in args)
                        {
                            channel.QueueBind(queue: queueName,
                                              exchange: "direct_logs",
                                              routingKey: severity);
                        }



                        var consumer = new EventingBasicConsumer(channel);


                        switch (type)
                        {
                            case "ExceptionLog":
                                //EventHandler<BasicDeliverEventArgs> handler = new EventHandler<BasicDeliverEventArgs>(test);
                                consumer.Received += new EventHandler<BasicDeliverEventArgs>(test);
                                break;

                            default:

                                consumer.Received += (model, ea) =>
                                {
                                    var body = ea.Body;
                                    var message = Encoding.UTF8.GetString(body);
                                    var routingKey = ea.RoutingKey;

                                };
                                break;
                        }
                        

                        

                        channel.BasicConsume(queue: queueName,
                                             noAck: true,
                                             consumer: consumer);

                        while (true)
                        {


                            Thread.Sleep(5000);
                        }



                    }

                });


            }
            

            
        }

        private void test(object send, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
        }
        public void remove (string queuename)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDelete("q_system", true, true);
                    }

        }

        // POST: Core/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Core/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Core/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Core/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Core/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
