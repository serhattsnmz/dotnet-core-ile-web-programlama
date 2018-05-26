using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Entities.Data
{
    public class Address : IEntity
	{
		public int ID { get; set; }
		public string FullAddress { get; set; }
		public string City { get; set; }

		public IEnumerable<Person> Persons { get; set; }
	}
}
