using Project.Logger.Abstarct;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Services.Abstract
{
    public interface ILogService
    {
		IGeneralLogRepository GeneralLog { get; }
		IExceptionLogRepository ExceptionLog { get; }
	}
}
