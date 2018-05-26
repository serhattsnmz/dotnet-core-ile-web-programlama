using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Entities.Logger
{
    public class ExceptionLog : ILog
    {
		public ExceptionLog() => this.Date = DateTime.Now;

		public DateTime Date { get; set; }
		public string ExceptionInfo { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
	}
}
