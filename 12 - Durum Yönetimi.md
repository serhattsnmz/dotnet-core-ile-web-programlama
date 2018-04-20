## DURUM YÖNETİMİ

- HTTP, stateless bir protokoldür.
- Kullanılan web server, her bir request'i ayrı ve birbirinden bağımsız olarak oluşturur ve önceki request üzerindeki veriler ve user bilgileri kullanılamaz.
- Bu yüzden, belirli bir kullanıcıya ait bilgileri o kullanıcıya özel saklamak veya tüm isteklerin kullanabileceği bir havuz yapısı oluşturmak için, Session ve Cookie bağımlı yapılar kullanarak durum yönetimi yapabiliriz.

### 01 - Session

- Sunucunun REMinde tutulur.
- Her kullanıcı için bir ID oluşturur ve tutulacak verileri bu ID üzerinden saklar.
- Bu oluşturulan ID’ler şifrelenerek kullanıcının bilgisayarında cookie olarak tutulur.
- Farklı tarayıcılarda farklı kullanıcı gibi davranır.
- Belirli bir süresi vardır, ayarlanabilir. Default : 20dk
    - Bu süre son request'ten sonrası için geçerlidir. (Sliding time)
- Session nesnesi içine eskiden obje atayabilirken, Core 2.0 üzerinde sadece primitive değişkenler saklayabiliyoruz(int, string vb)
    - Bu durumu aşmak için model yapıları json nesnesine çevrilip saklanabilir.
    - Bu durumlar için bir kütüphane oluşturulup kullanılabilir.
- .NET Core içinde session kullanılacaksa bunu aktifleştirmek gerekir. Bunu Startup.cs dosyası içinden;
    - Aktifleştirme yapılırken `UseSession` yapısının `UseMvc` yapısından önce kullanılmasına dikkat edilmelidir.
    - Aksi halde hata alınır.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache();
    services.AddSession();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSession();
    app.UseMvcWithDefaultRoute();
}
```

- Aktifleştime yapılırken, Session ayarlamaları da eklenebilir:
    - Buradaki `IdleTimeout` ile, Session süresi ayarlanabilir.
        - `TimeSpan` olarak belirtilir.
        - Session middleware'i üzerinden geçen her request bu zamanı sıfırlar.
    - `options.Cookie` üzerinden, session'ın oluşturduğu cookie ile ilgili ayarlamalar yapılabilir.

```cs
services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = new TimeSpan(0, 0, 10);
    options.Cookie.Name = ".AdventureWorks.Session";
});
```

#### Session kullanımı

- Session oluşturma:
    - `HttpContext.Session.Set      (string key, byte[] value)`
    - `HttpContext.Session.SetString(string key, string value)`
    - `HttpContext.Session.SetInt32 (string key, int value)`
- Session çekme:
    - `byte[]   HttpContext.Session.Get(string key)`
    - `bool     HttpContext.Session.TryGetValue(string key, out byte[] value)`
    - `string   HttpContext.Session.GetString(string key)`
    - `int?     HttpContext.Session.GetInt32(string key)`
- Sessionların silinmesi
    - `void     HttpContext.Session.Remove(string key)`
    - `void     HttpContext.Session.Clear()`

#### Modellerin Json yapısına çevrilip session yönetiminde kullanılması

```cs
// Library/Extensions/SessionExtensions.cs
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    public static void SetJson<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) :
                                JsonConvert.DeserializeObject<T>(value);
    }
}
```

```cs
// Controller
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Library.Extensions;
using Project.Models;
public class SessionController : Controller
{
    public IActionResult Index()
    {
        // FakeData ile örnek bir model listesi oluşturma
        List<Person> list = new List<Person>();

        for (int i = 0; i < 20; i++)
            list.Add(new Person {
                Name = FakeData.NameData.GetFirstName(),
                Surname = FakeData.NameData.GetSurname()
            });

        // Listeyi session üzerine kaydetme
        HttpContext.Session.SetJson("people", list);

        return RedirectToAction("Index2");
    }

    public IActionResult Index2()
    {
        // Session'ı çekme ve modele dönüştürme
        // Eğer gelen değer null ile boş bir liste gönderme
        List<Person> list = HttpContext.Session.GetJson<List<Person>>("people") 
            ?? new List<Person>();
        return View(list);
    }
}
```

### 02 - Cookie

- Cookie'ler kullanıcının bilgisayarında saklanır.
    - Kullanıcının bilgisayarında saklandığı ve görülebildiği için hassas verilerin saklanmaması gereklidir.
    - Eğer önemli veriler saklanacaksa ve bu verilerin görünmesi istenmiyorsa, şifrelenerek gönderilebilir.
- Her request ve response üzerinde taşınır.
    - Bu taşınmanın performansının düşmemesi için cookie boyutunu minimum tutmak önemlidir.
    - Genellikle kullanıcıya özel bilgiler server üzerinde barındırılıp, bunu tanımlayan unique bir değer cookie ile kullanıcıya gönderilir.
- Çoğu browser, cookie boyutunu 4096 byte olarak sınırlandırmıştır.

#### Cookie Kullanımı

- Cookie oluşturma:
    - `void Response.Cookies.Append(string key, string value);`
    - `void Response.Cookies.Append(string key, string value, CookieOptions options);`

```cs
Response.Cookies.Append("name", "serhat", new CookieOptions
{
    HttpOnly = true,
    Expires = DateTime.Now.AddDays(1)
});
```

### Diğer Durum Yönetim Yöntemleri

- Yukarıdaki yöntemler dışında, bilgileri sayfalar arasında aktarma işlemleri yaparken kullanabileceğimiz yöntemler şunlardır :
    - TempData
    - QueryString
    - PostData içinde Hidden alanlar
    - Statik değişkenler kullanma
    - Dosyalar ve Database