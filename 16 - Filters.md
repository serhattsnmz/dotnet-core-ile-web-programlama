## FILTERS

- Filtreler ile Core MVC, `Request Processing Pipeline`'ı üzerindeki belirli adımların öncesi veya sonrasına kod yazmamızı ve request işlenirken bunların çalıştırılmasını sağlar.
- **NOT** : Bu yapılar, sadece Razor sayfalarından oluşan işlemler için kullanılamaz.
- Bazı kullanım alanları:
    - Authorization
    - Gelen tüm requestlerin HTTPS olup olmadığı kontrolü
    - Response Caching (Request'e cache kullanarak kısa yoldan response oluşturma(short-circuiting))

#### Filtreler nasıl çalışır ?

- Filtre yapıları `MVC action invocation pipeline` içinde çalışır. 
- Görsel olarak geçtiği adımlar aşağıdaki gibidir : 

<p align="center">
    <img src="assets/15.png">
</p>

- Toplam 5 adet filter bulunmaktadır:
    - Authorization filters
    - Resource filters
    - Action filters
    - Exception filters
    - Result filters
- Bu filterların çalışma düzeni aşağıdaki gibidir:

<p align="center">
    <img src="assets/16.png" style="max-height:500px">
</p>

#### Senkron ve Asenkron çalışma

- Filtreler, hem senkronize hem de asenkronize çalışmayı desekler.
- Senkron kullanım yapılacaksa, hem **öncesinde** hem de **sonrasında** çalışacak iki tane metot gelir.

```cs
using FiltersSample.Helper;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiltersSample.Filters
{
    public class SampleActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // do something before the action executes
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
        }
    }
}
```

- Asekron kullanımda ise tek metot gelir.
- Bu metot içinde önce ve sonra yapılacak işlemler, action çağırılmadan önce ve sonrasında kodlanır.

```cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiltersSample.Filters
{
    public class SampleAsyncActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // do something before the action executes

            var resultContext = await next(); // Call the Action

            // do something after the action executes; resultContext.Result will be set
        }
    }
}
```

### 01 - Filter Oluşturma

- Filter'lar oluştururken, `Attriube` sınıfından kalıtım almalı ve ilgili interface implament edilmelidir.

```cs
// Senkron Filter
public class ExampleFilter : Attribute, IActionFilter
{
    string filePath = Directory.GetCurrentDirectory() + "/logs.txt";

    public void OnActionExecuted(ActionExecutedContext context)
    {
        File.AppendAllText(filePath, "Executed" + Environment.NewLine);
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        File.AppendAllText(filePath, "Executing" + Environment.NewLine);
    }
}
```

```cs
// Asenkron Filter
public class ExampleFilter : Attribute, IAsyncActionFilter
{
    string filePath = Directory.GetCurrentDirectory() + "/logs.txt";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await File.AppendAllTextAsync(filePath, "Executing" + Environment.NewLine);
        await next();
        await File.AppendAllTextAsync(filePath, "Executed" + Environment.NewLine);
    }
}
```

- Alternatif olarak `*FilterAttribute` sınıfları da kalıtım olarak verilebilir.
    - Bu sınıflar, `Attribute` sınıfını, filtrenin senkron ve asenkron interfacelerinin tümünü içerir.
    - Bu sınıf kullanılırsa, metotları kullanmak için `override` yapılması gerekir.

```cs
public class ExampleFilter : ActionFilterAttribute
{
    string filePath = Directory.GetCurrentDirectory() + "/logs.txt";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await File.AppendAllTextAsync(filePath, "Executing" + Environment.NewLine);
        await next();
        await File.AppendAllTextAsync(filePath, "Executed" + Environment.NewLine);
    }
}
```

### 02 - Filter Tanımlaması ve Kullanımı

- Filter'lar 3 yöntemle tanımlanabilir:
    1. Action Attribute
    2. Controller Attribute
    3. Global Attribute
- Oluşturulan filter'lar, action veya controller üzerinde *attribute* olarak eklenip kullanılabilir.
    - Action attribute olarak kullanılırsa, sadece o action'a özel,
    - Controller attribute olarak kullanılırsa, controller altındaki tüm actionlar için kullanılmış olur.
- Action veya Controller Attribute olarak kullanılacaksa iki yöntemle yazılabilir:
    - By Instance : `[ExampleFilter]`
    - By Type : `[TypeFilter(typeof(ExampleFilter))]`

```cs
// Action Attribute olarak kullanımı
public class FilterController : Controller
{
    [ExampleFilter]
    public IActionResult Index()
    {
        return View();
    }
}

// Controller Attribute olarak kullanımı
[ExampleFilter]
public class FilterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

- Global olarak tanımlamak için `ConfigureService` içinde `MvcOptions.Filters` olarak eklenebilir.
    - Bu global tanımlama 2 şekilde yapılabilir:
        1. Yeni bir nesne türetilerek (Instance)
        2. Type olarak verilerek

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(options =>
    {
        options.Filters.Add(new ExampleFilter1()); // an instance
        options.Filters.Add(typeof(ExampleFilter2)); // by type
    });
}
```

#### Filter çalışma sırası

- Filterlar çalışırken bir önceki filter grubu, önceki filter grubunu kapsar (Russion Doll).
    - Dıştan içe olarak;
        - Global Attribute
        - Controller Attribute
        - Action Attribute

```
1 - The **before** code of filters applied globally
2 -     The **before** code of filters applied to controllers
3 -         The **before** code of filters applied to action methods
4 -         The **after** code of filters applied to action methods
5 -     The **after** code of filters applied to controllers
6 - The **after** code of filters applied globally
```

- Eger filter'lar asenkron olarak kullanılmışsa, tüm filterlar **aynı seviyede** çalışır.
- Filter çalışma sıralaması manuel olarak da ayarlanabilir.
    - Filter kullanılırken, `Order` parametresine `0,1,2` değerleri atanarak çalışma sırası ayarlanabilir.
    - Küçük parametre atanan;
        - `Before` metodu, kendisinden yüksek değer atanan attribute'ün `Before` metodundan **önce** çalışır.
        - `After` metodu, kendisinden yüksek değer atanan attribute'ün `After` metodundan **sonra** çalışır.
    - Kısaca, küçük parametre alan, büyük parametre alan attribute değerini kapsamış olur.

```cs
[MyFilter(Name = "Controller Level Attribute", Order=1)]
```



### 03 - Filtrelere Parametre Ekleme (Build-in Filter Attributes)

- Basit olarak, const metotlar kullanılarak, ilgili filtre içine dışarıdan parametre alınabilir.

```cs
using Microsoft.AspNetCore.Mvc.Filters;
namespace FiltersSample.Filters
{
    public class AddHeaderAttribute : ResultFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;

        public AddHeaderAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(
                _name, new string[] { _value });
            base.OnResultExecuting(context);
        }
    }
}
```

```cs
[AddHeader("Author", "Steve Smith @ardalis")]
public class SampleController : Controller
{
    public IActionResult Index()
    {
        return Content("Examine the headers using developer tools.");
    }
}
```

### 04 - Filter Cancellation and Short Circuiting

- Filter metotları kullanılırken, herhangi bir metot içindeki `context.Result` üzerine müdahale edilerek, yönlendirme yapılabilir.
- Bu yönlendirme yapıldığında, pipeline üzerinde sonra gelen tüm filtreler by-pass edilerek direk sonuca gidilir ve bu filtreler çalışmaz.
- Örnek vermek gerekirse, aşağıdaki kod bloğunda, sadece `ResourceFilter`'ın `OnResourceExecuting` metodu çalışır.
    - `ActionFilter`'ın çalışmamasının nedeni, pipeline üzerinde `ResourceFilter`'ın `ActionFilter`'dan daha önce çalışmasıdır.
    - `ResourceFilter`'ın `OnResourceExecuting` metodu çalıştıktan sonra `context.Result` üzerinden by-pass yapılmış ve diğer filtrelere uğramadan direk sonuca gitmiştir.

```cs
// Filters
public class ExampleFilter : ActionFilterAttribute
{
    string filePath = Directory.GetCurrentDirectory() + "/logs.txt";

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        File.AppendAllText(filePath, "Executing" + Environment.NewLine);
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
        File.AppendAllText(filePath, "Executed" + Environment.NewLine);
    }
}

public class ShortCircuit : Attribute, IResourceFilter
{
    string filePath = Directory.GetCurrentDirectory() + "/logs.txt";

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        File.AppendAllText(filePath, "Executing Resource" + Environment.NewLine);
        context.Result = new RedirectToActionResult("Index", "Home", null);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        File.AppendAllText(filePath, "Executed Resource" + Environment.NewLine);
    }
}

// Controller
[ExampleFilter]
public class FilterController : Controller
{
    [ShortCircuit]
    public IActionResult Index()
    {
        return View();
    }
}
```

### 05 - Filter'lar ile Dependency Injection Kullanımı

- Filter'lar `type` ve `instance` olarak iki şekilde eklenir.
    - `Instance` olarak eklendiğinde, yaratılan bu nesne her request için ortak kullanılır.
    - `Type` olarak eklendiğinde, her request için ayrı bir instance üretilip kullanılır (type-activated).
    - **DI kullanımı için, type olarak eklemek gereklidir.**

```cs
// DI kullanan bir filter örneği
public class FilterWithDI : Attribute, IActionFilter
{
    private IUnitOfWork db;
    public FilterWithDI(IUnitOfWork _db)
        => db = _db;

    public void OnActionExecuted(ActionExecutedContext context)
    {
        db.Logs.Add(new Log { Area1 = "Exuceted" });
        db.SaveChanges();
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        db.Logs.Add(new Log { Area1 = "Exuceting" });
        db.SaveChanges();
    }
}
```

- DI kullanan bir filter'a ulaşmak için 3 yöntem kullanılabilir:
    - `ServiceFilterAttribute`
    - `TypeFilterAttribute`
    - `IFilterFactory`

#### Global Attribute ile DI

- Global olarak filter tanımlaması, daha önce anlatıldığı gibi 2 çeşitti. 
    - Instance > DI kullanılmaz!
    - Type > DI kullanılabilir.
- Global Attribute kullanımı aşağıdaki anlatılan yöntemlerden bağımsızdır.

```cs
// Global Attribute olarak kullanma
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(option => {
        option.Filters.Add(typeof(FilterWithDI));
    });
    services.AddScoped<FilterWithDI>();
}
```

#### ServiceFilterAttribute

- Kullanacağımız filter'ı bir servis olarak ekleyip, kullanacağımız zaman servis yapısından çekip kullanma işlemidir.
- Bunun için;
    - Öncelikle servis olarak eklememiz lazım.
    - Sonrasında `ServiceFilter` ile istediğimiz yerde kullanabiliriz.
> **NOT:** Servis olarak eklenmediği zaman hata verecektir. O yüzden buna dikkat etmek gerekir.

```cs
// Servis olarak ekleme
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<FilterWithDI>();
}
```

```cs
// Controller veya Action Attribute olarak kullanma
[ServiceFilter(typeof(FilterWithDI))]
public class FilterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

#### TypeFilterAttribute

- Servis olarak eklemeye gerek olmadan direk olarak DI mekanizmasını çalıştırır.
- ServiceFilter yapısından farklı olarak, parametre verilebilir.

```cs
// Controller veya Action Attribute olarak kullanma
[TypeFilter(typeof(FilterWithDI))]
public class FilterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

```cs
[TypeFilter(typeof(AddHeaderAttribute), Arguments = new object[] { "Author", "serhattsnmz" })]
public IActionResult Hi(string name)
{
    return Content($"Hi {name}");
}
```

#### Kendi ismini kullanan TypeFilterAttribute oluşturma

- Eğer oluşturulmak istenen filtre;
    - Herhangi bir parametre istemiyorsa,
    - DI bağımlılıkları varsa,
- `[TypeFilter(typeof(FilterType))]` yerine kendi ismini kullanan (`[FilterType]`) bir filter yazılabilir.

```cs
public class TypeFilterWithDI : TypeFilterAttribute
{
    public TypeFilterWithDI(): base(typeof(FilterWithDI2)) { }

    public class FilterWithDI2 : Attribute, IActionFilter
    {
        private IUnitOfWork db;
        public FilterWithDI2(IUnitOfWork _db)
            => db = _db;

        public void OnActionExecuted(ActionExecutedContext context)
        {
            db.Logs.Add(new Log { Area1 = "Exuceted2" });
            db.SaveChanges();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            db.Logs.Add(new Log { Area1 = "Exuceting2" });
            db.SaveChanges();
        }
    }
}
```

```cs
// Kullanımı
[TypeFilterWithDI]
public class FilterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

### 06 - Filter Tipleri

#### i. Authorization filters

- Action metotları erişimi kontrol etmek için kullanılır.
- Filter Pipeline üzerinde ilk çalışan filtredir.
- Sadece `before` metodu bulunur.
- Erişimi kontrol ederken, izni olmayan girişler by-pass yönlendirmesi yapılıp ulaşım kısıtlanabilir.

```cs
public class AuthFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        throw new NotImplementedException();
    }
}
```

#### ii. Resource filters

- `before` metodu, Filter pipeline üzerindeki 2. filtredir.
- `after` metodu, Filter Pipeline üzerinde son çalışan metotdur.
- Genellikle requestler üzerinde `short-circuit` yapmak için kullanılır. 
    - Örneğin cache üzerine alınmış bir yapıyı, diğer yapıları çalıştırmadan direk by-pass ederek cevap vermek için kullanılır.

```cs
public class ResFilter : Attribute, IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        throw new NotImplementedException();
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        throw new NotImplementedException();
    }
}
```

#### iii. Action filters

- Action metodun çalışmadan öncesinde ve çalıştıktan sonrasında (Result öncesi) çalışır.

```cs
public class SampleActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // do something before the action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // do something after the action executes
    }
}
```

- `ActionExecutingContext` içinde bulunan;
    - `ActionArguments` : Action içindeki inputları manipüle etmeye yarar.
    - `Controller` : Controller nesnesini manipüle etmeye yarar. 
    - `Result` : Dönen cevabı by-pass etmek için kullanılır. 
- `ActionExecutedContext ` yukarıdakilere ek olarak;
    - `Canceled` : Başka bir filtre tarafından result by-pass edilecekse, `true` olarak işaretlenir.
    - `Exception` : Hataları handle etmeyi veya ekleme yapmayı sağlar.
        - Eğer `non-nullable` bir değer atanırsa, exception mekanizması çalışır.
        - `Null` olarak ayarlanırsa, önceki hatalar handle edilmiş olur.

#### iv. Exception filters

- Sadece hata olduğunda çalışır!
- Genel olarak `error handling` kuralları yazmamızı sağlar.
- Sadece bir tane metodu bulunur.
- Controller çalışması, Model Binding, Action Filters veya Action Metot hatalarını yakalar.
- Resource Filter, Result Filter veya MVC Result execution hataları yakalanmaz!

```cs
public class ExpFilter : Attribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        throw new NotImplementedException();
    }
}
```

- `ExceptionContext.Exception` ile hata alınıp işlenebilir.
- İki yöntem ile exception handle edilebilir:
    - `ExceptionContext.ExceptionHandled = true`
    - `ExceptionContext.Exception = null`
- Hata handle edildikten sonra, hata kısmına kadar gelen yerler çalışır, eğer response gönderilmişse, kullanıcıya sayfa gösterilir. Client hata sayfasını görmez.

```cs
public class ExpFilter : Attribute, IExceptionFilter
{
    private IUnitOfWork db;
    public ExpFilter(IUnitOfWork _db) => db = _db;

    public void OnException(ExceptionContext context)
    {
        // Get exception
        Exception exp = context.Exception;

        // Tell it handled exeption
        context.ExceptionHandled = true;

        db.Logs.Add(new Log { Area2 = exp.Message });
        db.SaveChanges();

        // Return bad request
        context.Result = new BadRequestResult();
    }
}
```

#### v. Result filters

- Action metodunun result olarak döndürdüğü kısmın (ActionResults) öncesinde ve sonrasında çalışır.
- Özellikle response üzerinde işlem yapılacağı zaman kullanılır.
- Result filtreleri, **sadece başarılı result**'larda çalışır! Exception durumlarında çalışmaz.

```cs
public class ResultFilter : Attribute, IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
```