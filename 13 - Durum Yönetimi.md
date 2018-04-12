## DURUM YÖNETİMİ

- Ders notu eklenecektir.
- Session nesnesi içine eskiden obje atayabilirken, Core 2.0 üzerinde sadece primitive değişkenler saklayabiliyoruz(int, string vb)
    - Bu durumu aşmak için model yapıları json nesnesine çevrilip saklanabilir.
    - Bu durumlar için bir kütüphane oluşturulup kullanılabilir.

.NET Core içinde session kullanılacaksa bunu aktifleştirmek gerekir. Bunu Startup.cs dosyası içinden;

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache();
    services.AddSession();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSession();
}
```