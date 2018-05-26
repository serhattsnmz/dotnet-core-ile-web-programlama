using Project.Core.Logger;
using Project.Entities.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Logger.Abstarct
{
    public interface IExceptionLogRepository : ILogRepository<ExceptionLog>
    {
		// Custom operations
	}
}
