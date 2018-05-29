## MIDDLEWARE

- Oluşturulan Middleware'ler, `Request`'ten `Response`'a kadar olan döngü arasına yazılan ve bu ikisi arasında manipülasyon işlemleri yapan elemanlardır.
- Her bir component;
    - Pipeline üzerinde, sonraki elemana geçiş yapılmasını sağlayabilir veya geçişi kesebilir.
    - Sonra gelen component öncesinde ve sonrasında işlem yapılmasını sağlayabilir. (Encapsulation)
- `RequestDelegate`, Request pipeline kurulmasında görevlidir.
    - Her gelen HTTP isteğiyle `RequestDelegate` ilgilenir.
    - `RequestDelegate` için tanımlanan `Run`, `Map` ve `Use` extension metotlarıyla, `In-Line` veya `Reusable Class` kullanılarak middleware eklemesi yapılabilir.
    - Sonraki pipeline'a geçiş işlemleri bu sınıf üzerinden yapılır. 
- Her middleware, pipeline üzerinde kendisinden sonra gelen componeti çalıştırmak veya `short-circuiting` ile direk response cevabı vermek ile görevlidir.

### 01 - IApplicationBuilder ile Middleware Pipeline oluşturma

<p align="center">
    <img src="assets/18.png">
</p>

- Core üzerinde middleware'ler, `IApplicationBuilder` ile düzenlenir.
- Bu yapının düzenlenmesi, `Starup.cs` içindeki `Configure` metodunda olur.
- Yukarıdaki şekilden de görüleceği üzere, her middleware, kendisinden sonra gelen yapıdan önce ve sonra çalışır (kapsar).
- Ayrıca her middleware, kendisinden sonra gelecek yapının çalışıp çalışmayacağına karar verir.
    - Bu karar yapısı, `next()` fonksiyonunun çalışıp çalışmamasıyla alakalıdır.
    - Eğer yapıda short-circuit isteniyorsa, next metodu çalıştırılmaz ve başka bir yönlendirme yapılır.

#### Middleware Ekleme : IApplicationBuilder.Run()

- `Run()` metoduyla eklenen middleware, pipeline'ı sonlandırır.
- Bundan dolayı içinde `next()` metodu bulundurmaz.

```cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            await context.Response.WriteAsync("Hello, World!");
        });
    }
}

```

#### Middleware Ekleme : IApplicationBuilder.Use()

- Birden fazla middleware'i birbirine bağlamak için kullanılır.
- `next()` metoduyla kendinden sonra gelen componenti çalıştırabilir.

```cs
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Do work that doesn't write to the Response.
            await next.Invoke();
            // Do logging or other work that doesn't write to the Response.
        });

        app.Run(async context =>
        {
            await context.Response.WriteAsync("Hello from 2nd delegate.");
        });
    }
}
```

- Buradaki `next` parametresi öncesinde yazılan kodlar, sonra gelecek olan component çalıştırılmadan önce; `next` parametresi çalıştrıldıktan sonra yazılan kodlar, sonraki component çalıştırıldıktan sonra çalışır.
    - Başka bir deyişle **next parametresi öncesindeki kodlar `request` oluştuktan sonra, next parametresi sonrasındakiler ise `response` oluştuktan sonra** çalışır.
    - Bu yapıyla, önceki middleware'in, sonrakini **kapsadığını** anlayabiliriz.
    - Örnek vermek gerekirse;

```cs
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            Fn.LogYaz("Before Middleware 1");
            await next.Invoke();
            Fn.LogYaz("After Middleware 1");
        });

        app.Use(async (context, next) =>
        {
            Fn.LogYaz("Before Middleware 2");
            await next.Invoke();
            Fn.LogYaz("After Middleware 2");
        });
    }
}
```

```
# ÇIKTI
Startup sınıfı oluşturuldu.
ConfigureServices metodu çalıştı.
Configure metodu çalıştı.
Main metodu çalıştı.

Before Middleware 1
Before Middleware 2
After Middleware 2
After Middleware 1
```

> **NOT** : Middleware'ler next parametreleriyle en son middleware'e ulaşınca `Response` oluşur ve middleware üzerinden geriye doğru tekrar geçer. Bu geriye geçişte Response üzerinde değişiklik yapılamaz. Yapılırsa değişiklik değişmez veya hata alınır. Bunu kontrol etmenin en iyi yolu `context.Response.HasStarted` ile kontrol sağlamaktır.

```cs
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            Fn.LogYaz("Before Middleware");

            if (context.Response.HasStarted)
                Fn.LogYaz("Response oluştu!");
            else
                Fn.LogYaz("Response bekliyor.");

            context.Response.Headers.Add("exp1", "value1");

            await next.Invoke();

            Fn.LogYaz("After Middleware");

            if (context.Response.HasStarted)
                Fn.LogYaz("Response oluştu!");
            else
                Fn.LogYaz("Response bekliyor!");

            context.Response.Headers.Add("exp2", "value2");
        });
    }
}
```

```
# ÇIKTI
Before Middleware
Response bekliyor.
After Middleware
Response oluştu!

# HEADERS
exp1 : value1
```

#### Middleware Ekleme : IApplicationBuilder.Map() ve IApplicationBuilder.MapWhen()

- `Map*`, verilen request url path ile, birbirinden farklı middleware çalıştırmamıza olanak sağlayan bir yapıdır.
- Verilen url path kısmı ile **başlayan** tüm url isteklerinde, belli bir fonksiyonun middleware olarak kullanılması sağlanabilir.

```cs
public class Startup
{
    private static void HandleMapTest1(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            await context.Response.WriteAsync("Map Test 1");
        });
    }

    private static void HandleMapTest2(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            await context.Response.WriteAsync("Map Test 2");
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Map("/map1", HandleMapTest1);

        app.Map("/map2", HandleMapTest2);

        app.Run(async context =>
        {
            await context.Response.WriteAsync("Hello from non-Map delegate. <p>");
        });
    }
}

// RESULT
// Request                  Response
// localhost:1234           Hello from non-Map delegate.
// localhost:1234/map1	    Map Test 1
// localhost:1234/map2	    Map Test 2
// localhost:1234/map3	    Hello from non-Map delegate.
```

> **NOT** : `Map` fonksiyonu kullanıldığında, belirtilen path kısmı, `HttpRequest.Path` üzerinden silinir.

```cs
private static void HandleMapTest1(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        var url = context.Request.Path;
        await context.Response.WriteAsync("URL : " + url);
    });
}

// RESULT
// Request                  Response
// localhost:1234/map1      URL : 
// localhost:1234/map1/exp  URL : /exp      
```

- `MapWhen` fonksiyonu ise, `Map` fonksiyonundan farklı olarak, bellirli bir durumu fonksiyon olarak vermemizi ve bu durum sağlandığı müddetçe, belirtilen middleware eklenmesini sağlar.

```cs
public class Startup
{
    private static void HandleBranch(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            var branchVer = context.Request.Query["branch"];
            await context.Response.WriteAsync($"Branch used = {branchVer}");
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.MapWhen(context => context.Request.Query.ContainsKey("branch"),
                               HandleBranch);

        app.Run(async context =>
        {
            await context.Response.WriteAsync("Hello from non-Map delegate. <p>");
        });
    }
}

// RESULT
// Request                          Response
// localhost:1234                   Hello from non-Map delegate.
// localhost:1234/?branch=master    Branch used = master
```

- `Map` fonksiyonu nesting yapısını destekler:

```cs
app.Map("/level1", level1App => {
    level1App.Map("/level2a", level2AApp => {
        // "/level1/level2a"
        //...
    });
    level1App.Map("/level2b", level2BApp => {
        // "/level1/level2b"
        //...
    });
});
```

### 02 - Hazır Middleware Yapıları

- Dotnet Core içinde gelen hazır middleware yapılarına [burdan](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.0&tabs=aspnetcore2x#built-in-middleware) ulaşılabilir.
- Burda bilinmesi gereken önemli nokta, bu yapılar eklenirken sırasına dikkat edilmesidir.
    - Listede verilen sıralamaya göre eklenmesi gerekir.

### 03 - External Class Yapısında Middleware Oluşturma ve Kullanma

- Middleware olarak kullanılabilecek iki çeşit class yapısı aşağıdaki gibidir:

```cs
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;

namespace Culture
{
    // Create Middleware (Request only)
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            // Request Codes...

            // Call the next delegate/middleware in the pipeline
            return this._next(context);
        }
    }

    // Create Middleware (Request and Response)
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Request Codes...

            // Call the next delegate/middleware in the pipeline
            await this._next(context);

            // Response Codes...
        }
    }

    // Add the middleware to IApplicationBuilder as extension method (optional)
    public static class RequestCultureMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCulture(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestCultureMiddleware>();
        }
    }
}
```

- Middleware oluşturulduktan sonra direk `Configure` içinde kullanılabileceği gibi, `IApplicationBuilder` içine extension metot olarak eklenip de kullanılabilir.
- Bu yapılar : 

```cs
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        // Use directly
        app.UseMiddleware<RequestCultureMiddleware>();

        // Use as extension method
        app.UseRequestCulture();

        app.Run(async (context) =>
        {
            await context.Response.WriteAsync(
                $"Hello {CultureInfo.CurrentCulture.DisplayName}");
        });

    }
}
```

### 04 -  Middleware Yapılarında Dependency Injection

#### Conventional Middleware Activation

- Middleware classları, **program başlangıcında**, configuration ayarları bittikten sonra oluşturulur.
- Oluşum esnasında constructor metotlar çalıştırılıp nesne türetilir, fakat `invoke()` metodu, requestler geldiğinde çalıştırılır.
- Her request için değil, program başlangıcında nesneler türetildiği için, `scoped` ömrüne sahip servisler constructor metotlar ile enjekte edilemez. Bunun yerine `Invoke()` metoda `parametre` olarak verilip alınabilir.

```cs
public class RequestCultureMiddleware
{
    private readonly RequestDelegate _next;

    public RequestCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork db)
    {
        db.Logs.Add(new Models.Log{ LogName = "Example Log" });
        db.SaveChanges();
        
        await _next(context);
    }
}
```

#### Factory-Based Middleware Activation

- `IMiddleware` interface'i ile oluşturulmuş middleware yapılarıdır.
- Avantajları:
    - Her request ile birlikte tekrar oluşturulurlar.
    - Her request oluşturulduğunda constructor metodu tekrar çalıştırılır.
    - `scoped` ömrüne sahip servisler, constructor metot içinde enjekte edilebilirler.
- Factory-Based middleware kullanımı için, kullanmadan önce bu sınıfın servis olarak eklenmesi gerekmektedir.

```cs
// Create Factory-Based Middleware
public class IMiddlewareMiddleware : IMiddleware
{
    private readonly AppDbContext _db;

    public IMiddlewareMiddleware(AppDbContext db)
        => _db = db;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _db.Logs.Add(new Models.Log { Area4 = "Deneme" });
        await _db.SaveChangesAsync();

        await next(context);
    }
}

// Use as extension method (optional)
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseIMiddlewareMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IMiddlewareMiddleware>();
    }
}
```

```cs
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));

    services.AddTransient<IMiddlewareMiddleware>();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseIMiddlewareMiddleware();
}
```