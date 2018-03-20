## ROUTING

### 01) Routing nedir?
- Routing, gelen url isteklerinin yönledirilmesini düzenleyen kurallardır.
- `Startup.cs` sınıfı içindeki `Configure` metotu içinde tanımlanır.
- Neden routing ayarlarına ihtiyaç var?
    - SEO uyumluluğunu arttırır. Böylece default yapı dışında, SEO uyumlu url yapılarını oluşturmamıza izin verir.
    - Uzun ve anlaşılmaz url yapısı yerine, daha anlaşılır bir yapı kurmamıza olanak tanır. Örn:
        - `site.com/Haberler/Index?haberID=123` gibi bir url yerine,
        - `site.com/Haberler/123` veya `site.com/Haberler/istanbulda-son-durum` gibi daha anlaşılır url yapılarını oluşturabiliriz.
- Microsoft Docs yolu :
    - https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing

### 02) Default Routing Ayarları
```cs
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});
```

### 03) Custom Routing
```cs
// Örnek bir custom routing
routes.MapRoute(
    name: "CustomRoute",
    template: "Haberler/Kategori/{catID}",
    defaults: new { controller = "Haberler", action = "Index"}
);
```

- Parametreler : 
    - **name** : Route yönlendirmemize bir isim vermemizi sağlar. Bu ismin tekrarlanmaması gereklidir.
    - **template** : Kullanıcı tarafından gelecek isteğin url tanımlanması burada yapılır.
    - **defaults** : Gelen isteğin hangi Controller ve Action tarafından karşılanacağını belirttiğimiz kısımdır.

- Template kısmında tanımladığımız dinamik değerler, karşılayan action tarafından parametre olarak alınır.
- Yine bu değerlerin opsiyonel olarak girilmesini istiyorsak yanına `?` işareti bıkakıp opsiyonel yapabiliriz.

### 04) Route Constrainsts ( Rota Kısıtlamaları )

### 05) Attribute Routing
- Attribute Route ayarlaması kullanılan actionlara, `{controller}/{action}` üzerinden artık bağlanılınamaz.
- Attribute Routing kullanılacak action üzerinde aşağıdaki gibi tanımlama yapılır:
    - `[Route(<template>, Name=<name>, Order=<number>)]`
    - **template** : Kullanıcı tarafından gelecek isteğin url tanımlanması burada yapılır.
    - **name** : Route yönlendirmemize bir isim vermemizi sağlar. Bu ismin tekrarlanmaması gereklidir.
    - **order** : Route ayarlarını sıralamamızı sağlar. Birbirine benzeyen yapılardan öncelikle hangisinin sorgulanacağını buradaki sıraya göre yaparız.
- Bir action üzerinde birden fazla attribute routing tanımlaması yapılabilir.

```cs
public class HomeController : Controller
{
    [Route("")]
    [Route("Home")]
    [Route("Home/Index")]
    public IActionResult Index()
    {
        return View();
    }
    [Route("Home/About")]
    public IActionResult About()
    {
        return View();
    }
    [Route("Home/Contact")]
    public IActionResult Contact()
    {
        return View();
    }
}
```

#### Http[Verb] ile Tanımlama
- Özellikle Rest API yapılarında, isimleri aynı fakat metotları farklı end-pointler oluşturmak için kullanılan yöntemdir.
- İki şekilde tanımlaması yapılabilir : 

```cs
// Yöntem 1
[HttpGet]
[Route("Kullanici")]
public IActionResult Goster() => View();

[HttpPost]
[Route("Kullanici")]
public IActionResult Ekle() => View();

// Yöntem 2
[HttpGet("/Kullanici")]
public IActionResult Goster() => View();

[HttpPost("/Kullanici")]
public IActionResult Ekle() => View();
```

#### Kök Yol Tanımlama 
```cs
[Route("Haber")]
public class HaberlerController : Controller
{
    // site/haber
    public IActionResult Anasayfa() => View();

    // site/haber/icerik
    [Route("Icerik")]
    public IActionResult Icerik() => View();

    // site/bagimsiz
    [Route("~/bagimsiz")]
    public IActionResult Bagimsiz() => View();
}
```
- Controller üzerinde route tanımlaması yapılırsa, altındaki tüm action metotlar için bu tanımlama kök route tanımlaması olur.
- Bu controller içinde herhangi bir action üzerinde route tanımlaması yapılmazsa, bu action default olarak ulaşılabilir hale gelir.
    - **NOT** : Kök route tanımlaması bulunan bir controller içinde en fazla bir tane default action boş bırakılabilir. Birden fazla bırakıldığı durumda hata verir.
- Kök route tanımlaması yapılan bir controller içinde `~/` ifadesi kullanılarak bu tanımlama ezilebilir. Böylece ana site yolu üzerinden belirlenen yola erişim sağlanabilir.

#### Route Template Özel isimleri
- O an içinde bulunulan default yolu tanımlamak için kullanılan özel isimlerdir.
- Bu isimlendirmeler 3 tanedir:
    - `[controller]`
    - `[action]`
    - `[area]`

```cs
[Route("[controller]/[action]")]
public class ProductsController : Controller
{
    [HttpGet] // Matches '/Products/List'
    public IActionResult List() {
        // ...
    }

    [HttpGet("{id}")] // Matches '/Products/Edit/{id}'
    public IActionResult Edit(int id) {
        // ...
    }
}
```

#### Multiple Routes
```cs
[Route("Store")]
[Route("[controller]")]
public class ProductsController : Controller
{
    [HttpPost("Buy")]     // Matches 'Products/Buy' and 'Store/Buy'
    [HttpPost("Checkout")] // Matches 'Products/Checkout' and 'Store/Checkout'
    public IActionResult Buy()
}
```

#### Opsiyonel Parametre
- Opsiyonel parametre, yanına soru işareti (?) getirilerek tanımlanır.
- Böylece bu parametre gelmediği durumlarda hata gösterilmez.
```cs
[Route("Icerik/{gelenDeger?}")]
public IActionResult Icerik(string gelenDeger)
{
    ViewBag.gelenDeger = gelenDeger;
    return View();
}
```

#### Parametreye Default Değer Atama
```cs
[Route("Icerik/{gelenDeger=deneme}")]
public IActionResult Icerik(string gelenDeger)
{
    ViewBag.gelenDeger = gelenDeger;
    return View();
}
```

#### Rota Kısıtlamaları
- Girilen route parametrelerine kısıtlama getirmek için kullanılır.
- Referans : https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-template-reference

```cs
[Route("Icerik/{icerikID:int:min(1)?}")]
public IActionResult Icerik(int icerikID)
{
    ViewBag.gelenDeger = icerikID;
    return View();
}
```