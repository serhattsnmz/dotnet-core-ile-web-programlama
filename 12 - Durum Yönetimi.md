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

- String türünde değişkenler saklar.
- Cookie'ler kullanıcının bilgisayarında saklanır.
    - Kullanıcının bilgisayarında saklandığı ve görülebildiği için hassas verilerin saklanmaması gereklidir.
    - Eğer önemli veriler saklanacaksa ve bu verilerin görünmesi istenmiyorsa, şifrelenerek gönderilebilir.
- Ömürleri dolana kadar browser tarafından saklanmaya devam eder 
- Her request ve response üzerinde taşınır.
    - Bu taşınmanın performansının düşmemesi için cookie boyutunu minimum tutmak önemlidir.
    - Genellikle kullanıcıya özel bilgiler server üzerinde barındırılıp, bunu tanımlayan unique bir değer cookie ile kullanıcıya gönderilir.
- Çoğu browser, cookie boyutunu 4096 byte olarak sınırlandırmıştır.
- Eklenme için `Response` okuma için ise `Request` objeleri kullanılır.

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

- CookieOptions : 
    - **Domain** - Cookie ile birleştirilmek istenen domain ismi
    - **Path** - Cookie yolu
    - **Expires** - Cookie silinme zamanı
    - **HttpOnly** - Client-Side scriptlerin cookie kullanıp kullanmayacağının ayarlanması
    - **Secure** - Sadece HTTPS üzerinden çalışmayı desteklemek için true yapılabilir.

- Cookie Okuma:
    - Controller üzerinden : 
        - `Request.Cookies[<key>]`
    - View üzerinden :
        - `Context.Request.Cookies[<key>]`
    - Herhangi bir dış class veya middleware üzerinden : 
        - https://stackoverflow.com/questions/38571032/how-to-get-httpcontext-current-in-asp-net-core

```cs
// External Class : 
public class MyCookieExp
{
    public static string ReadCookie(HttpContext context, string key)
    {
        return context.Request.Cookies[key];
    }
}

// Controller : 
var cookie = MyCookieExp.ReadCookie(HttpContext, "name");
```

- Cookie Silme:
    - `Response.Cookies.Delete(<key>)`

#### Önemli bir not!

> İnternet standartlarına göre request headers içerisinde non-ascii değerlerin taşınması uygun görülmemektedir. Bu tür bir zorunluluk taşıyorsanız ilgili değerleri encode-decode yöntemleri ile işleyebilirsiniz.

- Bu sebepten ötürü, .NET Core Cookies içinde ASCII değerlerine uygun olmayan karakterler taşındığında, request Controller yapısına uğramadan direk 400 hatası döndürür.

### 03 - Cache 

- Caching mekanizması, herhangi bir veriyi cevap olarak daha hızlı döndürmek için, bu veriyi RAM üzerinde saklama ve gerektiğinde geri döndürme işlemidir.
- Globaldir. Her kullanıcı aynı bilgileri görür.
- Statik değişkenlerden farkı, burada süre belirtebiliyoruz.
- İsteğimiz dışımızda, server alan açmaya zorlandığında yine bu cache’ler silinebilir.
    - Bu silinmeler önceliğe göre yapılır.
- ASP.NET Core içinde caching mekanizaması bir servis olarak bulunmaktadır.
- Bu nedenle bu servisi kullanmadan önce `Startup.cs` içine eklemek gerekmektedir.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddMemoryCache();
}
```

- Caching mekanizmasını kullanacağımız herhangi bir yerde, bu servis DI ile enjekte edilip kullanılır. 

```cs
private IMemoryCache cache;
public IDGCacheController(IMemoryCache cache)
{
    this.cache = cache;
}
```

#### Cache oluşturma

```cs
public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value);
public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, DateTimeOffset absoluteExpiration);
public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, IChangeToken expirationToken);
public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions options);
```

#### Cache Options

```cs
public DateTimeOffset? AbsoluteExpiration { get; set; }
//
// Summary:
//     Gets or sets an absolute expiration time, relative to now.
public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
//
// Summary:
//     Gets or sets how long a cache entry can be inactive (e.g. not accessed) before
//     it will be removed. This will not extend the entry lifetime beyond the absolute
//     expiration (if set).
public TimeSpan? SlidingExpiration { get; set; }
//
// Summary:
//     Gets the Microsoft.Extensions.Primitives.IChangeToken instances which cause the
//     cache entry to expire.
public IList<IChangeToken> ExpirationTokens { get; }
//
// Summary:
//     Gets or sets the callbacks will be fired after the cache entry is evicted from
//     the cache.
public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
//
// Summary:
//     Gets or sets the priority for keeping the cache entry in the cache during a memory
//     pressure triggered cleanup. The default is Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal.
public CacheItemPriority Priority { get; set; }
//
// Summary:
//     Gets or sets the size of the cache entry value.
public long? Size { get; set; }
```

#### Cache okuma 

```cs
public static object Get(this IMemoryCache cache, object key);
public static TItem Get<TItem>(this IMemoryCache cache, object key);

public static bool TryGetValue<TItem>(this IMemoryCache cache, object key, out TItem value);

public static TItem GetOrCreate<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory);
[AsyncStateMachine(typeof(CacheExtensions.<GetOrCreateAsync>d__9<>))]
public static Task<TItem> GetOrCreateAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory);
```

- Basit olarak bir cache kontrolü, yoksa oluşturulması, varsa geri döndürülmesi şu şekilde oluşturulur:

```cs
public string GetCacheValue(){
    string key ="IDGKey";
    string obj;

    if (!cache.TryGetValue<string>(key, out obj))
    {
        obj = DateTime.Now.ToString();
        cache.Set<string>(key, obj);
    }

    return obj;
}
```

- Aynı işlemler daha basit olarak `GetOrCreate()` metoduyla da yapılabilir.
- Bu metot varsa çeker, yoksa oluşturup geri döndürür.

```cs
public string Get()
{
    return cache.GetOrCreate<string>(“IDGKey”,
        cacheEntry => {
            return DateTime.Now.ToString();
            });
}
```

#### External class içinde cache kullanımı

- Diğerlerinden olduğu gibi, `IMemoryCache` interface'i parametre olarak verilir ve kullanılıldığı controller üzerinde DI ile enjekte edilip, metoda parametre olarak verilir.

```cs
// External class : 
public class MyCacheExp
{
    public static T GetCache<T>(IMemoryCache cache, string key)
        => cache.Get<T>(key);
}

// Controller :
ViewBag.date = MyCacheExp.GetCache<DateTime>(cache, "date");
```

#### Cache Silme

```cs
public IActionResult Delete()
{
    cache.Remove("date");
    return RedirectToAction("Index");
}
```

### Diğer Durum Yönetim Yöntemleri

- Yukarıdaki yöntemler dışında, bilgileri sayfalar arasında aktarma işlemleri yaparken kullanabileceğimiz yöntemler şunlardır :
    - TempData
    - QueryString
    - PostData içinde Hidden alanlar
    - Statik değişkenler kullanma
    - Dosyalara ve Database'e kayıt