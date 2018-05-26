using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Entities.Logger
{
    public class GeneralLog : ILog
    {
		public GeneralLog() => this.Date = DateTime.Now;

		public DateTime Date { get; set; }
		public string LogName { get; set; }
		public string LogInfo { get; set; }
	}
}
