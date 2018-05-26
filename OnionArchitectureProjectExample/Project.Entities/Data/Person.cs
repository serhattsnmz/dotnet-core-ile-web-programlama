using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Entities.Data
{
    public class Person : IEntity
    {
		public int ID { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public int Age { get; set; }

		public int? AddressID { get; set; }
		public Address Address { get; set; }
	}
}
