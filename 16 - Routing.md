## ROUTING

### 01) Routing nedir?
- `Routing Middleware`, gelen url isteklerinin yönledirilmesini düzenleyen kurallardır.
- `Startup.cs` sınıfı içindeki `Configure` metotu içinde tanımlanır.
- Neden routing ayarlarına ihtiyaç var?
    - SEO uyumluluğunu arttırır. Böylece default yapı dışında, SEO uyumlu url yapılarını oluşturmamıza izin verir.
    - Uzun ve anlaşılmaz url yapısı yerine, daha anlaşılır bir yapı kurmamıza olanak tanır. Örn:
        - `site.com/Haberler/Index?haberID=123` gibi bir url yerine,
        - `site.com/Haberler/123` veya `site.com/Haberler/istanbulda-son-durum` gibi daha anlaşılır url yapılarını oluşturabiliriz.
- Microsoft Docs yolu :
    - https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing

### 02) Default Routing Ayarları
- Default routing ayarları `site/{controller}/{action}` yapısında url yapıları oluşturmak için kullanılır.
- İki şekilde default routing ayarlaması yapılabilir.

```cs
app.UseMvcWithDefaultRoute();
```

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
    - **constraints** : Rota kısıtlamalarını tanımladığımız alandır.
    - **dataTokens** : (Açıklaması ayrı başlık olarak aşagıda yazılmıştır.)

- Template kısmında tanımladığımız dinamik değerler, karşılayan action tarafından parametre olarak alınır.
- Yine bu değerlerin opsiyonel olarak girilmesini istiyorsak yanına `?` işareti bıkakıp opsiyonel yapabiliriz.

#### DataTokens
- Burada tanımlanan değerlere action içinde `RouteData` üzerinden ulaşılarak, actiona hangi route düzeninden ulaşıldığı bilgisi alınabilir. 
- Genellikle, birden fazla route yolunun aynı actiona ulaşması ve bu action içinde bu ayrımın yapılmasına olanak vermek için kullanılır.
- Örnek olarak;

```cs
// Route Ayarlaması
app.UseMvc(routes =>  
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}",
        defaults: null,
        constraints: null,
        dataTokens: new { Name = "default_route" });
});

// Bu değere action içinden ulaşım
public class ProductController : Controller  
{
    public string Index()
    {
        var nameTokenValue = (string)RouteData.DataTokens["Name"];
        return nameTokenValue;
    }
}
```

#### Area Routing
- Routing tanımlamasında default parametre değerine area tanımlaması da yapılabilir.

```cs
defaults: new { area = Blog, controller = Users, action = AddUser }
```

#### RouteData
- `RouteData` request içindeki route parametrelerinin tutulduğu alandır. 
- Bunlar action, controller ve diğer taşınan parametrelerdir.
- RouteData içinde post veya get ile taşınan veriler taşınmaz!
- Route içinde taşınan parametrelerin, action metodu içine parametre olarak alınmasına gerek yoktur. Direk `RouteData.Values` içinden çekilebilir.
    - İstenilirse, RouteData yerine, metot içine parametre olarak da alınabilir.

```cs
[Route("Haberler/Spor/{haberID}")]
public string Index3()
{
    object hID          = RouteData.Values["haberID"];      // 1
    object action       = RouteData.Values["action"];       // Index3
    object controller   = RouteData.Values["controller"];   // Home
    return "Haber Sayfası";
}
```

#### A catch-all parameters
- Route değerlerindeki her bir parametre sadece bir segment alır. Eğer url olarak gönderilen segment sayısı, onu karşılayan route ayarlarındaki segment sayısından fazla olursa, url uyumsuzluğu olduğundan 404 hatası ile karşılaşırız.
- Bazı durumlarda, route üzerinden kaç tane segment geleceğini sınırlandırmak istemeyiz.
- Bu durumda route temasının en sonuna, gelecek tüm segmentleri alabilecek `Catch-All` parametresi ekleriz.
- Bu yapı, metotlardaki `params` ifadesine benzerdir.
- Bunun için parametre isminin önüne `*` işareti getirilir.
- Parametre ismi olarak herhangi bir değer alınabilir.
- Örnek vermek gerekirse;

```cs
routes.MapRoute(
    name: "default",
    template: "{controller=Home}/{action=Index}/{*extra}");
```

- Burada yapılan ayarlamada, ilk iki segmentten sonra gelen tüm segmentler, extra parametresi ile alınır.
- Daha sonra bu parametre `/` işareti üzerinden parse edilebilir.
- Örnek olarak aşağıdaki gibi bir controller yazıp değerleri çekersek;

```cs
public IActionResult Index()
{
    var model = new Dictionary<string, object>();

    model["controller"] = RouteData.Values["controller"];
    model["action"] = RouteData.Values["action"];
    model["extra"] = RouteData.Values["extra"];

    return View(model);
}
```

- Request Url yapısına göre aşağıdaki değerler elde edilecektir.
- **NOT:** Catch-All parametresi her zaman opsiyoneldir.

```
>> URL : localhost/Route/Index/3/4/5
Controller  : Route
Action      : Index
Extra       : 3/4/5

>> URL : localhost/Route/Index/3
Controller  : Route
Action      : Index
Extra       : 3

>> URL : localhost/Route/Index
Controller  : Route
Action      : Index
Extra       : 
```

### 04) Route Constraints ( Rota Kısıtlamaları )
- Route yollarındaki parametrelerin belirli bir kuralda girilmesi ve bu kurallarda girilmediği takdirde yönlendirme yapılmamasını istediğimiz durumlarda kullanırız.
- Rota kısıtlaması yapılan parametreler opsiyonel olarak kullanılmaz.
- Rota kısıtlamaları iki yolla yapılabilir : 
    1. Inline Constraints
    2. Maproute içinde Contraints argümanı ile

```cs
// Inline Constraint
routes.MapRoute(
    name: "default",
    template: "{controller=Home}/{action=Index}/{id:int:range(1,100)?}");

// Maproute parametresi - Single
routes.MapRoute(
    name: "default",
    template: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index"},
    constraints: new { id = new IntRouteConstraint()});

// Maproute parametresi - Multiple
routes.MapRoute(
    name: "default",
    template: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index"},
    constraints: new { id = new CompositeRouteConstraint(
        new IRouteConstraint[]
        {
            new IntRouteConstraint(),
            new RangeRouteConstraint(1,100)
        }
        )});
```

#### Regex (Regular Expressions - Düzenli İfadeler) ile Rota Kısıtlaması
- (.) -> Bir  karakterin gelecebileceği ve karakteri tanımlamadığımız yerlerde kullanılır.
- (+) -> Kendinden önce gelen karakterin en az bir kere tekrarlanması gerektiğini belirtir.
- (*) -> Kendinden önce gelen karakterin sıfır veya daha fazla tekrarlanması gerektiğini belirtir.
- (?) -> Kendinden önce gelen karakterin en fazla bir kere gelmesi gerektiğini belirtir.
- ([ ]) -> İçindeki karakterlerden birinin geleceğini belirtir.
    - [ab] [0-9] [a-z]
- ({ }) -> Kendinden önce gelen karakterin içinde yazılan sayı kadar tekrar edeceğini belirtir.
- (^) -> Metnin başını ifade eder. Kendinden sonra gelen ifadeyi metnin başında arar.
- ($) -> Metnin sonunu ifade eder. Kendinden önce gelen ifadeyi metnin sonunda arar.
- (\s) -> Belirtilen yerde boşluk olması gerektiğini belirtir.
- (\S) -> Belirtilen yerde boşluk olmaması gerektiğini belirtir.
- (\d) -> Sayısal ifade geleceğini belirtir.
- Örnekler:
    - Cep telefonu düzeni
        - (05)55-444-22-11
        - ^(05)\d{2}-\d{3}-\d{2}-\d{2}$

#### HTTPverb ile Rota Kısıtlaması
- Route düzenine hangi HTTP methodlar ile ulaşılabileceğini belirtmek için kullanırız.
- Birden fazla HTTP method yazılabilir.
- `HttpMethodRouteConstraint` metodu kullanılır.

#### Örnek:
```cs
routes.MapRoute(
    name: "Haber Kategori",
    template: "Haber/Kategori/{catName}/{catID}",
    defaults: new { controller = "Home", action = "Kategori" },
    constraints: new {
        catID   = @"^\d{2}$",   // Sadece iki basamaklı sayı
        catName = @"^[a-z]+$",  // Sadece harflerden oluşacak
        metod   = new HttpMethodRouteConstraint("GET","POST") });
```

#### Custom Route Constraints
- Kendi kuralımızı yazmak için öncellikle `IRouteConstraint` interface'inden türeyen bir sınıfa ihtiyacımız vardır. 
    - Interface'i implement ettiğimizde içinde bir tane `Match` adlı metot gelir ve bu metodun geri dönüş tipi `bool` olmak zorundadır.
    - Bu metot içine kuralımızı yazıp, sonucunda, gelen değerin uyup uymadığını belirtmek için true veya false döndürürüz.
- Match metodu içinde toplam 5 tane parametre bulunur : 
    - **HttpContext:** Request, response, session vs gibi, http özel metotlarını bulunduran parametredir.
    - **IRouter:** Constraint'lere ait router metotlarını taşır.
    - **routeKey:** Kontrol edilecek route değerinin alındığı yerdir. Custom Constraint metodumuzu üzerinde çalıştırdığımız route elemanının kendisine bunla ulaşırız.
    - **RouteValueDictionary:** Url route parametrelerinin bulunduğu sözlük yapısıdır.
    - **RouteDirection:** Routing işleminin Http isteğinin URL'inden mi, yoksa custom oluşturulan URL'den mi geldiğini söyler. Enum yapısındadır.
        - IncomingRequest : Client tarafından gelen url isteği.
        - UrlGeneration : Route ayarları sonucu oluşturulan URL isteği.

```cs
public class WeekDaysConstraint : IRouteConstraint
{
    List<string> WeekDays = new List<string>() { "pzt", "sali", "car", "per", "cuma" };

    public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        return WeekDays.Contains(values[routeKey]);
    }
}
```

- MapRoute parametresi olarak kullanımı

```cs
routes.MapRoute(
    name: "default",
    template: "{controller}/{action}/{day}",
    defaults: new { controller = "Home", action = "Index"},
    constraints: new { day = new WeekDaysConstraint()});
```

- Eğer metot inline constraint olarak kullanılacaksa, bunun servis olarak eklenmesi gerekmektedir.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    services.Configure<RouteOptions>(option =>
        option.ConstraintMap.Add("weekday", typeof(WeekDaysConstraint))
    );
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller}/{action}/{day:weekday}",
            defaults: new { controller = "Home", action = "Index" });
    });
}
```


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

#### AreaAttribute Kullanımı
- Bir controller veya areanın başka bir areaya ait davranmasını istiyorsak bu yöntemi kullanabiliriz.

```cs
[Area("Blog")]
public class UsersController : Controller
{
    public IActionResult AddUser()
    {
        return View();
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

### 06 - Roting Constraints Listesi

#### Data Tipi Kontrolü

CONSTRAINT	| INLINE | CLASS | NOTES 
--- | --- | --- | ---
int	|{id:int} | IntRouteConstraint	| Constrains a route parameter to represent only 32-bit integer values
alpha	| {id:alpha}	| AlphaRouteConstraint	| Constrains a route parameter to contain only lowercase or uppercase letters A through Z in the English alphabet.
bool	| {id:bool}	| BoolRouteConstraint	| Constrains a route parameter to represent only Boolean values.
datetime	| {id:datetime}	| DateTimeRouteConstraint	| Constrains a route parameter to represent only DateTime values.
decimal	| {id:decimal}	| DecimalRouteConstraint	| Constrains a route parameter to represent only decimal values.
double	| {id:double}	| DoubleRouteConstraint	| Constrains a route parameter to represent only 64-bit floating-point values
float	| {id:float}	| FloatRouteConstraint	| Matches a valid float value (in the invariant culture - see warning)
guid	| {id:guid}	| GuidRouteConstraint	| Matches a valid Guid value

#### Uzunluk ve İçerik Kontrolü

CONSTRAINT	| INLINE | CLASS | NOTES 
--- | --- | --- | ---
length(length)	| {id:length(12)}	| LengthRouteConstraint	| Constrains a route parameter to be a string of a given length or within a given range of lengths.
maxlength(value)	| {id:maxlength(8)}	| MaxLengthRouteConstraint	| Constrains a route parameter to be a string with a maximum length.
minlength(value)	| {id:minlength(4)}	| MinLengthRouteConstraint	| Constrains a route parameter to be a string with a maximum length.
range(min,max)	| {id:range(18,120)}	| RangeRouteConstraint	| Constraints a route parameter to be an integer within a given range of values.
min(value)	| {id:min(18)}	| MinRouteConstraint	| Constrains a route parameter to be a long with a minimum value.
max(value)	| {id:max(120)}	| MaxRouteConstraint	| Constrains a route parameter to be an integer with a maximum value.

#### Regex Kontrolü

CONSTRAINT	| INLINE | CLASS | NOTES 
--- | --- | --- | ---
regex(expression)	| {ssn:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}}$)}	| RegexRouteConstraint	| Constrains a route parameter to match a regular expression.