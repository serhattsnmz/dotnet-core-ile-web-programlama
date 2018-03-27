## DATABASE CRUD İŞLEMLERİ

### Controller

```cs
using Microsoft.AspNetCore.Mvc;
using Project.Models.Entities;
using Project.Models.Interfaces;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
		private IPersonRepository context;

		public HomeController(IPersonRepository repo)
			=> context = repo;

		public IActionResult Index()
			=> View(context.Persons);

		public IActionResult Create()
			=> View();

		[HttpPost]
		public IActionResult Create(Person person)
		{
			context.CreatePerson(person);
			return RedirectToAction("Index");
		}

		public IActionResult Edit(int id)
			=> View(context.GetById(id));

		[HttpPost]
		public ActionResult Edit(Person person)
		{
			context.UpdatePerson(person);
			return RedirectToAction("Index");
		}

		public ActionResult Delete(int id)
		{
			context.DeletePerson(id);
			return RedirectToAction("Index");
		}
    }
}
```

### Views

#### Index.cshtml

```html
@model IEnumerable<Person>

<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>Index</title>
</head>
<body>

    <div>
        <a asp-action="Create">Create</a>
    </div>

   @foreach (var item in Model)
   {
       <div>
           @item.Name - 
           @item.Surname - 
           @item.Age - 
           <a asp-action="Edit" asp-route-id="@item.ID">Edit</a> - 
           <a asp-action="Delete" asp-route-id="@item.ID">Delete</a>
       </div>
   }
</body>
</html>
```

#### Create.cshtml

```html
@model Person

<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>Create</title>
</head>
<body>

    <form method="post">

        <label asp-for="Name">Name : </label>
        <input asp-for="Name" /> <br />

        <label asp-for="Surname">Surname : </label>
        <input asp-for="Surname" /> <br />

        <label asp-for="Age">Age : </label>
        <input asp-for="Age" /> <br />

        <input type="submit" value="Kaydet" />
    </form>

</body>
</html>
```

#### Edit.cshtml

```html
@model Person

<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>Create</title>
</head>
<body>

    <form method="post">

        <input type="hidden" asp-for="ID" />

        <label asp-for="Name">Name : </label>
        <input asp-for="Name" /> <br />

        <label asp-for="Surname">Surname : </label>
        <input asp-for="Surname" /> <br />

        <label asp-for="Age">Age : </label>
        <input asp-for="Age" /> <br />

        <input type="submit" value="Kaydet" />
    </form>

</body>
</html>
```