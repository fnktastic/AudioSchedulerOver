using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.DataAccess
{
    public interface IDataContextFactory
    {
        Context Instance { get; }
    }

    class DataContextFactory : IDataContextFactory
    {
        public Context Instance => new Context();
    }
}
