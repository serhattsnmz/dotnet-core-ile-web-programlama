using Microsoft.EntityFrameworkCore;
using Project.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Data.Concrete.EntityFramerwork.DAL
{
    public class EfProjectContext : DbContext
	{
		// For using "UseSqlServer", you must add the following line to the "Project.Data.csproj"
		// 
		// <ItemGroup>
		// <PackageReference Include = "Microsoft.EntityFrameworkCore.SqlServer" Version="2.0.2" />
		// </ItemGroup>

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(
				"Server=(localdb)\\mssqllocaldb;Database=ExampleProject;Trusted_Connection=True"
				);
		}

		public DbSet<Person> People { get; set; }
		public DbSet<Address> Addresses { get; set; }
	}
}
