## CONTROLER VE VİEW ARASI İLETİŞİM

### 1) Controller'dan View'e Veri Gönderme
- 3 Yöntem vardır:
    - `ViewData`
        - Daha sonra daha kolay kullanım için `ViewBag` yapısına geçmiştir.
        - `ViewDataDictionary` yapısındadır.
    - `ViewBag`
        - Controller'dan bilgilerin View üzerine gönderilmesi için kullanılır.
        - Arka planda `ViewData` kullanır.
        - `Dynamic` yapıdadır.
            - Okunduğu zaman veri tipini kazanır.
            - Bu yapısı nedeniyle kullanıldığı zaman tür dönüşümü yapılması *zorunlu değildir.*
        - Süresi yoktur, sadece view'a kadar ilerleyebilir.
    - `TempData`
        - `ITempDataDictioanry` yapısındadır.
        - Bilgileri, okunana kadar saklar.
        - Okuma yapıldıktan sonra `silinecek` olarak işaretlenip, **request sonunda** silinir.
        - `Silinecek` olarak işaretlenmesini engellemek için `Peek(<key>)`, işaretlendikten sonra işaretini kaldırmak için `Keep(<key>)` metotları kullanılabilir.
        - Saklanması: 
            - `CookieTempDataProvider` nesnesi yaratılır.
            - Bu nesne `Base64UrlTextEncoder` ile encode edilir.
            - Encode edilen nesne default olarak `Cookie`'de saklanır.
        - `Cookie-Based TempData Provider` default gelir, istenilirse bu yapı, `Session-Based TempData Provider` olarak değiştirilebilir.

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Session-Based
    services.AddMvc().AddSessionStateTempDataProvider();

    // Cookie-Based (Default)
    // services.AddMvc().AddCookieTempDataProvider();

    services.AddSession();
}
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSession();
    app.UseMvcWithDefaultRoute();
}

```cs
//second request, PEEK value so it is not deleted at the end of the request
object value = TempData.Peek("value");

//third request, read value and mark it for deletion
object value = TempData["value"];
```

```cs
//second request, get value marking it from deletion
object value = TempData["value"];
//later on decide to keep it
TempData.Keep("value");

//third request, read value and mark it for deletion
object value = TempData["value"];
```

### 2) View’dan Controller’a Veri Gönderme
- View'dan Controller'a veri gönderim 3 şekilde olur (Request):
    - 1. Route Values : `/Course/Details/2`
    - 2. Query String : `/Course/Details?ID=2`
    - 3. Form Data : `ID=2`
- GET ve POST Yöntemleri
    - GET Metodu:
        - Raw request içinde url içinde taşınır.
        - Query String yapısındadır.
    - POST Metodu:
        - Raw request içinde body kısmında taşınır.
        - Form Data yapısındadır.
- GET ve POST ile gönderilen veriler, action üzerinden nasıl alınır.
    - Her iki metotla gönderilen veri de, parametre olarak alınabilir.
    - Parametre olarak almanın dışında her iki metotla gelen bilgi de `Request` üzerinde taşındığı için, bu sınıf içinden alınabilir.
        - GET -> `Request.Query["key"]`
        - POST -> `Request.Form["key"]`
        - Route -> `RouteData.Values["key"]`

```html
<!-- HTML PART -->
<form method="post" action="/Home/GetInfo">
    <input type="text" name="name" /> <br />
    <input type="text" name="surname" /> <br />
    <input type="text" name="age" /> <br />
    <input type="submit" value="Send" />
</form>
``` 

```cs
// Metot 01
[HttpPost]
public ActionResult GetInfo()
{
    string name = Request.Form["name"];
    string surname = Request.Form["surname"];
    int age = int.Parse(Request.Form["age"]);
    return RedirectToAction("Index");
}
```

```cs
// Metot 02
[HttpPost]
public ActionResult GetInfo(string name, string surname, int age)
{
    return RedirectToAction("Index");
}
```

- If( IsPost ) Kullanımı
- Using ( Html.BeginForm ) Kullanımı
- Başka bir controller'a veri gönderme yöntemi

#### AntiForgeryToken Kontrolü
- [XSRF/CSRF](https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-2.0) zaafiyetlerini engellemek için kullanılan yöntemdir.
- Bu yöntem kullanıldığında, form client tarafına gönderilirken bir token oluşturulur ve bu token session üzerinde kaydedilir.
- Kullanıcı formu post ettiğinde, bu token app tarafına gönderilir.
- Controller çalışmadan önce bu token'ın, session üzerinde kaydedilen token ile eşleştirilmesi yapılır. Eğer token uyumsuz ise geçişe izin verilmez.
- Böylece başka bir sayfadan veya harici olarak form gönderimi engellenmiş, sadece uygulamının render edip gönderdiği html üzerindeki form gönderimi kabul edilmiş olunur.
- Bu yöntemi kullanmak için yapılması gereken şudur:
    - Öncelikle html tarafındaki form içine bir `__RequestVerificationToken` adında bir token üretilmelidir. 
        - Eğer projeye `TagHelpers`'lar dahilse, form tagları açılıp **form metodu post olarak işaretlenirse** bu token otomatik olarak üretilir.
        - Eğer TagHelper'lar kullanılmıyorsa, htmlde form taglarının arasına `@Html.AntiForgeryToken()` yazılarak, Html Helper'lar yardımıyla bu token üretilebilir.  
    - Daha sonra bu formun post edildiği controller üzerinde attribute olarak şu satır eklenir. Bu attribute'ün amacı, token kontrolü yapıp, uyumsuzluk durumda controller'a erişimi engellemektir.
        - `[ValidateAntiForgeryToken]`