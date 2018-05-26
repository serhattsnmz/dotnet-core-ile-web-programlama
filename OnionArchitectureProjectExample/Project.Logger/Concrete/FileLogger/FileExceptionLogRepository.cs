using Project.Core.Logger.FileLogger;
using Project.Entities.Logger;
using Project.Logger.Abstarct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Project.Logger.Concrete.FileLogger
{
    public class FileExceptionLogRepository 
		: FileLogRepository<ExceptionLog>, IExceptionLogRepository
    {
		public FileExceptionLogRepository() : base("exceptionLogs.json"){}
	}
}
