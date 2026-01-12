using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManager.Domain.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string message);
        Task PublishObjectAsync<T>(T obj);
    }
}
