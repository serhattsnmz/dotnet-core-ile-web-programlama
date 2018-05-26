using Microsoft.AspNetCore.Mvc;
using Project.Data.Abstract;
using Project.Entities.Data;
using Project.Services.Abstract;

namespace Project.WebUI.Controllers
{
	public class HomeController : Controller
	{
		private IDatabaseService database;
		private ILogService log;

		public HomeController(IDatabaseService _database, ILogService _log)
		{
			database = _database;
			log = _log;
		}

		public IActionResult Index()
		{
			database.People.Add(new Person
			{
				Name = "serhat"
			});
			database.SaveChanges();
			return View();
		}
	}
}