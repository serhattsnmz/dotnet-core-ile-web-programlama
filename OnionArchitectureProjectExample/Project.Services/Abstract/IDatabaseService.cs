using Project.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Services.Abstract
{
    public interface IDatabaseService : IDisposable
    {
		IPersonRepository People { get; }
		IAddressRepository Addresses { get; }

		int SaveChanges();
    }
}
