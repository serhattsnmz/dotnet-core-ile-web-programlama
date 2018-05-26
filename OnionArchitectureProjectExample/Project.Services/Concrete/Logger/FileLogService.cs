using Project.Logger.Abstarct;
using Project.Logger.Concrete.FileLogger;
using Project.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Services.Concrete.Logger
{
	public class FileLogService : ILogService
	{
		// Fields
		IGeneralLogRepository _generalLog;
		IExceptionLogRepository _exceptionLog;

		public IGeneralLogRepository GeneralLog
			=> _generalLog ?? (_generalLog = new FileGereralLogRepository());

		public IExceptionLogRepository ExceptionLog
			=> _exceptionLog ?? (_exceptionLog = new FileExceptionLogRepository());
	}
}
