using Project.Core.Data.EntityFramework;
using Project.Data.Abstract;
using Project.Data.Concrete.EntityFramerwork.DAL;
using Project.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Data.Concrete.EntityFramerwork
{
    public class EfAddressRepository : EfEntityRepository<Address>, IAddressRepository
    {
		// Database Injection
		// We inject db instead of create, because we will create only one db object for all entity repositories with Unit Of Work Architecture.
		// After Injection, we send the db object to the base.
		public EfAddressRepository(EfProjectContext context) : base(context) { }
	}
}
