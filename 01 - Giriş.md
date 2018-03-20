## ASP .NET Core MVC PROGRAMLAMAYA GİRİŞ

### 1) .NET Core ile .NET Framework Arasındaki Farklar

- Eklenecek

### 2) Core Geliştirme Ortamları

- Windows
    - Visual Studio 2017 veya 2015
        - 2017 kurarken dil paketinin EN seçilmesine dikkat edilmelidir.
        - 2015 sürümünde default Core 1.1 gelmektedir, bunun upgrade edilmesi gerekir.
    - VS Code
    - NET Core 2.0 SDK
        - https://www.microsoft.com/net/download/Windows/build
    - Version görüntüleme
        - `dotnet --version`
    - VS üzerinden yeni proje açarken Core 2.0 seçeneği geliyorsa işlemler başlarıyla gerçekleşmiştir.

- Mac ve Linux
    - VS Code
        - Kurulması tavsiye edilen eklentiler:
            - ASP.NET Core Snippets
            - C#
    - NET Core 2.0 SDK
        - https://www.microsoft.com/net/download/Windows/build

<p align="center">
    <img src="assets/02.png" style="max-height:250px" />
</p>

### 3) MVC Pattern Nedir?

- Design Pattern
- Kurallar düzenidir / Kalıptır.
- Belirli kurallar ve düzenleri sabitleyerek projenin işleyişini ve - takım çalışmasını kolaylaştırır.
- İlk olarak 1979 yılında Trygve Reenskaug ortaya atılmıştır. - (Microsoft’un kurulumu 1975)
- MVC nasıl çalışır?

<p align="center">
    <img src="assets/01.png" style="max-height:250px" />
</p>

### 4) İlk Projeyi Oluşturma

- Visual Studio ile örnek proje açma
- VS Code ile proje oluşturma (Terminal üzerinden)
    - `dotnet new --help`
        - Şablonları gösterir.
    - `dotnet new <şablon ismi>`
        - Seçili şablon dosyalarını oluşturur.
    - `dotnet run`
        - Projeyi çalıştırır.
- Projenin dosya düzeni
    - Eklenecek
- İlk projenin ayarlamalarının yapılması ve basit bir MVC sayfası
- İlk ayarlamada `Startup.cs` dosyasının içeriği aşağıdaki gibi olmalıdır:

```cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
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
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
```