using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Data.Concrete.EntityFramerwork.DAL;
using Project.Services.Abstract;
using Project.Services.Concrete.Data;
using Project.Services.Concrete.Logger;

namespace Project.WebUI
{
	public class Startup
    {
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
        {
			services.AddScoped<IDatabaseService, EfDatabaseService>();
			services.AddScoped<ILogService, FileLogService>();

			services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			app.UseStaticFiles();
			app.UseStatusCodePages();
			app.UseMvcWithDefaultRoute();
        }
    }
}
