using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifoia.Module.AIVideoCoach.Shared.Interfaces
{
    public interface IChatClientFactory<T>
    {
        Task<T> CreateAsync();
        T Create();
    }
}
