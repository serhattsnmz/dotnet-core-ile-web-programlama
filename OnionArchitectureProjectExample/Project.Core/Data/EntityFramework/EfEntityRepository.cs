using Microsoft.EntityFrameworkCore;
using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Project.Core.Data.EntityFramework
{
	public class EfEntityRepository<T> : IEntityRepository<T>
		where T : class, IEntity, new()
	{
		// Database Injection
		// We inject db instead of create, because we will create only one db object for all entity repositories with Unit Of Work Architecture.
		protected readonly DbContext context;

		public EfEntityRepository(DbContext _context)
			=> context = _context;

		// Get Single Entity
		public T Get(Expression<Func<T, bool>> filter)
			=> context.Set<T>().FirstOrDefault(filter);

		public T GetByID(int ID)
			=> context.Set<T>().Find(ID);

		// Get Entity List
		public IQueryable<T> GetAll()
			=> context.Set<T>();

		public IQueryable<T> GetList(Expression<Func<T, bool>> filter)
			=> context.Set<T>().Where(filter);

		// Create
		public void Add(T entity)
			=> context.Set<T>().Add(entity);

		// Update
		public void Update(T entity)
			=> context.Set<T>().Update(entity);

		// Delete
		public void Delete(T entity)
			=> context.Set<T>().Remove(entity);

		// Save Changes
		public void SaveChanges()
			=> context.SaveChanges();
	}
}
