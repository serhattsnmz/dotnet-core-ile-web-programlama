## ASP.NET CORE GENEL BİLGİLER VE YAŞAM DÖNGÜSÜ

### 01 - Genel Bilgiler

- ASP.NET Core; cross-platform, yüksek performanslı, açık kaynak kodlu bir framework'tür.
- .NET Core ve .NET Framework üzerinde çalıştırılabilir.
    - ASP.NET Framework, yalnızca Windows işletim sisteminde, .NET Framework üzerinde çalışmaktadır.
- ASP.NET Core uygulaması, `Main` metodu içinde web server yaratıp kullanan bir ***console uygulamasıdır.***

```cs
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace aspnetcoreapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
```

- `WebHost.CreateDefaultBuilder()` metodu, uygulamamızı üzerinde çalıştıracağımız bir host yaratır ve varsa ayarlamalar yapmamızı sağlar.
    - Bu ayarlamalar, `UseStartup()`, `UseKestrel()` gibi metotlarla yapılandırılır.
    - Default builder ayarlamsında Kestel web server otomatik olarak seçilmiş olur.
    - `Build()` ve `Run()` metotlarıyla `IWebHost` türünde bir host oluşturulup, HTTP istekleri dinlenmeye başlanır.

### 02 - Application Startup

- Main metot içinde, Host oluştururken (WebHostBuilder) kullandığımız `UseStartup()` metodu, `Request Handling Pipeline` düzenlemeleri yapmamızı sağlayan, external bir class oluşturmamızı sağlar.

- Startup class'ı public olmalı ve aşağıdaki iki metodu içermelidir.

```cs
public class Startup
{
    // This method gets called by the runtime. Use this method
    // to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
    }

    // This method gets called by the runtime. Use this method
    // to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
    }
}
```

- **ConfigureServices** : Uygulamanın kullandığı servislerin (örn; ASP.NET Core MVC, Entity Framework Core, Identity) tanımlandığı yerdir.
    - Tanımlanması opsiyoneldir.
- **Configure** : Request pipeline için kullanılacak ara katmanların (middleware) belirtildiği yerdir.
    - Tanımlanması zorunludur.
- Bu metotlar, uygulama çalıştığı zaman `runtime` anında çalışırlar.

...

- Startup sınıfı kullanılacağı zaman, host tarafından sağlanan bazı bağımlılıklar enjenkte edilebilir. Bunlardan en çok kullanılanları:
    - `IHostingEnvironment` : Servisleri geliştirme ortamlarına göre özelleştirmeyi sağlar.
    - `IConfiguration` : Startup boyunca uygulama ayarları yapmamızı sağlar.

```cs
public class Startup
{
    public Startup(IHostingEnvironment env, IConfiguration config)
    {
        HostingEnvironment = env;
        Configuration = config;
    }

    public IHostingEnvironment HostingEnvironment { get; }
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        if (HostingEnvironment.IsDevelopment())
        {
            // Development configuration
        }
        else
        {
            // Staging/Production configuration
        }

        // Configuration is available during startup. Examples:
        // Configuration["key"]
        // Configuration["subsection:suboption1"]
    }
}
```

- `IHostingEnvironment` ve `ILoggerFactory`, alternatif olarak Startup metotlarına parametre olarak da verilebilir(conventions-based approach).

```cs
public class Startup
{
    public void Configure(IApplicationBuilder app, IHostingEnvironment hosting)
    {
        // ...
    }
}
```

#### ConfigureServices Metodu

- Opsiyoneldir.
- Web host tarafından, `Configure` motodu çağırılmadan önce çağırılır ve uygulamanın kullanıcı tanımlı servis ayalarının yapılmasını sağlar.
- `ConfigureServices` metodu içine yeni bir servis eklemek, bu servisi uygulama ve `Configure` için kullanılabilir yapar.
    - Bu servisler uygulama içinde `dependency injection` veya `IApplicationBuilder.ApplicationServices` aracılığıyla kullanılabilir.
- Web Host, bazı servisleri Startup metodu çağırılmadan önce düzenleyebilir. [(Ayrıntı)](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/index?view=aspnetcore-2.0)
- Servisleri ekleme : 
    - `IServiceCollection` sınıfı içindeki `Add[Service_Name]` metotları çağırılıp servisler eklenebilir.
    - Servisler eklendiği zaman, varsa ayarları da yapılabilir.

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Add framework services.
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    services.AddMvc();

    // Add application services.
    services.AddTransient<IEmailSender, AuthMessageSender>();
    services.AddTransient<ISmsSender, AuthMessageSender>();
}
```

#### Configure Metodu

- Bu metot, gelen HTTP isteğine nasıl cevap verileceğini ayarlamamızı sağlar.
    - Gelen isteğe cevap verilme sürecine (Request => Response) `Request Handling Pipeline` denir.
- Request pipeline yönetimi, `IApplicationBuilder` nesnesi üzerine `Ara katman (Middleware)` eklenmesiyle düzenlenir.
    - `IApplicationBuilder` web host tarafından üretilir ve direk olarak `Configure` metoduna yönlendirilir.
- Request pipeline, hazır gelen Core middleware'leri (developer exception page, BrowserLink, error pages, static files, and ASP.NET MVC) ile düzenlenebileceği gibi, custom middleware de yazabiliriz.
    - `IApplicationBuilder` üzerine eklenmiş `Use[middleware]` *extension* metotları middleware eklenmesi için kullanılabilir.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseBrowserLink();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller}/{action=Index}/{id?}");
    });
}
```

- Her eklenen middleware, kendisinden sonra gelen middleware'i çalıştırmak ya da kısa devre (short-circuiting) yaparak başka bir yere yönlendirmekle sorumludur.
    - Short-circuiting ile yönlendirilen yer, tekrar middleware'leri en baştan çağırılmasını sağlayabileceği gibi, sonrakileri atlayarak direk response da gönderebilir.

#### Convenience metot

- `ConfigureServices` ve `Configure` metotları, `Startup` sınıfı içinde tanımlanabileceği gibi, web host build edilirken de oluşturulabilir.

```cs
public class Program
{
    public static IHostingEnvironment HostingEnvironment { get; set; }
    public static IConfiguration Configuration { get; set; }

    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }

    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                HostingEnvironment = hostingContext.HostingEnvironment;
                Configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddMvc();
            })
            .Configure(app =>
            {
                if (HostingEnvironment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                }

                // Configuration is available during startup. Examples:
                // Configuration["key"]
                // Configuration["subsection:suboption1"]

                app.UseMvcWithDefaultRoute();
                app.UseStaticFiles();
            })
            .Build();
}
```

#### Startup Filters

- `IStartupFilter` interface'inden implament edilen bir sınıf ile, herhangi bir middleware'i, tüm middleware pipeline'ından önce veya sonra çalıştırılmasını sağlayabiliriz.
- Bu sayede bir middleware'in, `Configure` metodu içinde belirttiğimiz middleware'ler dışında, sistemde default gelen middleware'lerden de önce veya sonra çalıştığından emin olabiliriz.
- `IStartupFilter` interface'i, bir tane `Action<IApplicationBuilder>` döndüren `Configure` adlı metot implament eder.
- Her bir sınıf, bir ya da daha fazla middleware kullanabilir. Kullanılan middleware'ler filtre olarak ekleneceğinden, ayrıca `Startup > Configure` metodu içine eklenmesine gerek yoktur.
- Oluşturulan filtre, `ConfigureServices` içinde aktifleştirilir.
    - Birden fazla filtre veya aynı filtre birden fazla defa kullanılabilir.
    - Yazıldığı yere göre, servisler sırayla aktifleştirilir.

```cs
public class RequestSetOptionsStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.UseMiddleware<RequestSetOptionsMiddleware>();
            next(builder);
        };
    }
}
```

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IStartupFilter, RequestSetOptionsStartupFilter>();
    services.AddMvc();
}
```

### 03 - Dependency injection in ASP.NET Core (Services)

- [Dependency Injection Nedir?](BONUS%20-%20Dependency%20Injection%20Nedir.md)