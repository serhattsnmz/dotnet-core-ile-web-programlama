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

- Update işlemleri yapılırken, Crate html dosyasından farklı olarak, ID property'sinin hidden olarak alınacağı unutulmamalıdır.
    - Update yapılırken, update yapılacak db elemanı bu gelen ID'ye göre bulunur.
    - Eğer ID kısmını hidden olarak formdan göndermezsek, ID = 0 olarak gidecektir ve güncelleme yapılmayacaktır.
- Update işleminde post edilen nesne içinde, sadece form içinde çağırılan elemanların gönderileceği unutulmamalıdır.
    - Örneğin `CreatedDate`, `IsActive` gibi property'ler varsa ve biz bunları form içinden göndermezsek, bu kısımların default değeri alınarak gönderilen nesne oluşturulur ve update yapıldığında eski değerler yerine bu default değerler gelir.
    - Bu durumda bizim db üzerinde istenmeyen değişiklikler yapılmasına neden olur.
    - Bu sorunu çözmek için iki yöntem vardır:
        1. Form içinine bu parametreler hidden olarak gönderilir, böylece yeni nesne oluşurken, bu bilgiler eskisiyle aynı olarak gönderilecektir.
        2. Update yapılırken `_context.Update(person);` fonksiyonunu kullanmak yerine, gelen ID'ye göre db elemanı çekilip sadece ilgili alanlar replace edilerek tekrar db'ye gönderilir.

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

#### SQL Sorgusu Çalıştırma

- .NET üzerinde, LinQ ile yapılan işlemler sınırlı kaldığında, direk SQL komutları çalıştırılabilir.
- Bunun için `<DbContext>.Database.ExecuteSqlCommand()` metodunu kullanıyoruz.
- Metot, geri dönüş olarak `int` tipinde, üzerinde değişiklik yapılan `row` sayısını döndürür.
- Hata olması durumunda `SqlException` fırlatır.
- **NOT:** Bu yöntem kullanılırken `SQLInjection` açığı yaratmamasına dikkat edilmelidir! 

```cs
public int DeletePersonWithSQLCommand(int personID)
{
    string cmd = $"DELETE FROM Persons WHERE ID={personID}";
    return efProjectContext.Database.ExecuteSqlCommand(cmd);
}
```