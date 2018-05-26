using Project.Core.Data;
using Project.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Data.Abstract
{
    public interface IPersonRepository : IEntityRepository<Person>
    {
		// Custom operations
    }
}
