using Project.Data.Abstract;
using Project.Data.Concrete.EntityFramerwork;
using Project.Data.Concrete.EntityFramerwork.DAL;
using Project.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Services.Concrete.Data
{
	public class EfDatabaseService : IDatabaseService
	{
		// Database
		private readonly EfProjectContext context;

		public EfDatabaseService()
			=> context = new EfProjectContext();

		// Fields
		private IPersonRepository _people;
		private IAddressRepository _address;

		// Properties
		public IPersonRepository People
			=> _people ?? (_people = new EfPersonRepository(context));

		public IAddressRepository Addresses
			=> _address ?? (_address = new EfAddressRepository(context));

		public int SaveChanges()
			=> context.SaveChanges();

		public void Dispose()
			=> context.Dispose();
	}
}
