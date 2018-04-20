## AREA KAVRAMI

- Area'lar, farklı namespace'ler ile ayrılmış, kendine ait routing ayarlarına ve MVC yapısına sahip alanlardır.
- Farklı amaçlar için farklı app gruplarına ihtiyacımız olduğu durumlarda kullanırız.
    - Örneğin, bir sitenin ayrı bir müşteri paneli, admin paneli vb varsa, bu yapılar ayrı ayrı arealar oluşturularak kullanılabilir.
- Bir Core projesi içinde istenilen sayıda area açılabilir. 
- Büyük projelerin bağımsız parçalarını birbirinden ayırarak yönetilebilirliği kolaylaştırır.
- Aynı isimle controller açılması, bu controller farklı arealar altında olduğu müddetçe sorun oluşturmaz.
- Core, area içindeki bir view'ı render ederken, default olarak aşağıdaki yollara bakar : 

```cs
/Areas/<Area-Name>/Views/<Controller-Name>/<Action-Name>.cshtml
/Areas/<Area-Name>/Views/Shared/<Action-Name>.cshtml
/Views/Shared/<Action-Name>.cshtml
```

- Bu default yolları değiştirmek mümkündür.
- Bunun için `Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions` üzerinden `AreaViewLocationFormats` kısmı override edilebilir. (Servise yapısı içinde) 
    - Buradaki numaralandırma : 
        - `{0}` > Action Name
        - `{1}` > Controller Name
        - `{2}` > Area Name

```cs
services.Configure<RazorViewEngineOptions>(options =>
{
    options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/Categories/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Categories/{2}/Views/Shared/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});
```

### 01 - Area Ayarlamalarının Yapılması

- Area kullanımı için basit olarak iki adımlı bir ayarlama yapmak gerekmektedir.
    - Route ayarlaması
    - Controllers ayarlaması

#### Route ayarlaması

- Route ayarlaması için, yeni bir routeMap eklenmesi gerekmektedir.
- Bu yeni ayarlamanın diğer alanlarla çakışmaması için onların üstünde olması daha sağlıklıdır.

```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "areaRoute",
        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    routes.MapRoute(
        name: "default",
        template: "{controller}/{action}/{id?}",
        defaults: new { controller = "Home", action = "Index" });
});
```

#### Controllers ayarlaması

- Controller kullanımında, her controller sınıfının üstüne `Area` attribute'ü ile, hangi areaya ait olduğu belirtilmelidir.
    - Eğer bu ayarlama yapılmazsa, root dizinden türetilmiş gibi algılanır ve çakışma yaratır.

```cs
namespace MyStore.Areas.Products.Controllers
{
    [Area("Products")]
    public class HomeController : Controller
    {
        // GET: /Products/Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Products/Home/Create
        public IActionResult Create()
        {
            return View();
        }
    }
}
```

### 02 - Area Link Yaratma

- Program içinde başka bir action'a link yaratırken kullanacağımız yöntemler şunlardır: 
- Örnek olarak gideceğimiz link : `/Products/Home/Index`

- Aynı area ve controller, farklı action : 
    - HtmlHelper:
        - `@Html.ActionLink("Go to Product's Home Page", "Index")`
    - TagHelper:
        - `<a asp-action="Index">Go to Product's Home Page</a>`

- Aynı area, farklı controller ve action : 
    - HtmlHelper:
        - `@Html.ActionLink("Go to Product's Home Page", "Index", "Home")`
    - TagHelper:
        - `<a asp-controller="Home" asp-action="Index">Go to Product's Home Page</a>`

- Farklı area, controller ve action : 
    - HtmlHelper:
        - `@Html.ActionLink("Go to Product's Home Page", "Index", "Home", new {area = "Products"})`
    - TagHelper:
        - `<a asp-area="Products" asp-controller="Home" asp-action="Index">Go to Product's Home Page</a>`