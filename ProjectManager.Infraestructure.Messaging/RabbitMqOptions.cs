using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Infraestructure.Messaging
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "rabbitmq";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string QueueName { get; set; } = "my_queue";
        public int Port { get; set; } = 5672;
    }
}
