using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Project.Core.Logger
{
    public interface ILogRepository<T>
		where T : class, ILog, new()
    {
		List<T> GetList(Func<T, bool> filter = null);

		void Add(T log);
		void Delete(T log);
    }
}
