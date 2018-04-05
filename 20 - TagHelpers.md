## TAGHELPERS

- https://dannyvanderkraan.wordpress.com/2016/04/19/asp-net-core-1-0-goodbye-html-helpers-and-hello-taghelpers/
- Tag helper'lar, .NET Core (Core 1.0 - MVC 6) ile birlikte, Html helper kullanımlarının yerini almış yapılardır.
- Daha esnek ve html dostu bir yazım sağladığı için, bu yapıları html helper yapılarına tercih ediyoruz.
- Temel amacı, html kodlarını hızlıca yamamıza olanak sağlamasıdır.
- **Html Helper yapılarından farklı olarak, genel olarak tüm elementi değil, sadece attribute'leri oluşturur.**

```html
<!-- Html Helpers -->
@Html.TextBoxFor(k => k.Name, new { @class = "caption" })

<!-- Tag Helpers -->
<input class="caption" asp-for="Name" />
```

#### Projeye tag helper'ları ekleme

- Tag helperlar ayrı ayrı view sayfalarına eklenebileceği gibi, `_ViewImports.cshtml` dosyasına da eklenebilir. Bu sayede tüm view'larda kullanabilmiş oluruz.

```cs
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- Projeye eklenen tag helperlar herhangi bir view içerisinde kullanılmak istenmiyorsa, `@removeTagHelper` kullanılarak buradan kaldırılabilir.
- Eğer element düzeyinde tag helper'ların kaldırılması isteniyorsa, `!` ünlem işareti kullanılabilir.
    - Bu işaretin hem başlangıç hem de kapanış tagında kullanılması gerekmektedir.

```html
<!span asp-validation-for="Email" class="text-danger"></!span>
```

- Eğer tag helper'ları sadece bazı alanlarda kullanmamız gerekiyorsa, bu alanlar için bir prefix tanımlayıp, sadece bu prefix tanımlandığında çalışmasını sağlayabiliriz. 
    - Bunun için öncelikle `_ViewImports.cshtml` içine `@tagHelperPrefix <prefix>` satırlarının tanımlanması gerekmektedir.

```html
<!-- _ViewImports.cshtml -->
@tagHelperPrefix th:

<!-- View -->
<select asp-items="ViewBag.list"></select>  => Tag Helper çalışmaz.
<th:input asp-for="Name" />                 => Tag Helper çalışır.
```

### 01 - Anchor Tag Helper

- **asp-action**
    - Bir action için yol oluşturur.
- **asp-controller**
    - Bir controller için yol oluşturur.
- **asp-area**
    - Area için yol oluşturur.
- **asp-route-{key}**
    - Route değerleri göndermeyi sağlar.
    - Sonuna eklenen ek kısmını key, verilen değeri value olarak alır.

```html
<a asp-action="Index" asp-controller="Product" asp-route-id="5">Computers</a>
<!-- 
    RESULT:
    <a href="/Product/Index/5">Computers</a>
-->

<a asp-action="Index" asp-controller="Product" asp-route-product="computer">Computers</a>
<!-- 
    RESULT:
    <a href="/Product?product=computer">Computers</a>
-->
```

- **asp-route**
    - Route ayarlarında tanımlanan route ismi girildiğinde, ilgili yolu verir.

```cs
// Route config
routes.MapRoute(
    name: "product",
    template: "Product/Home/All",
    defaults: new { controller = "Product", action = "Index" });
```

```html
<a asp-route="product">Products</a>
<!-- 
    RESULT:
    <a href="/Product/Home/All">Products</a>
-->
```

- **asp-page**
    - `href` attribute gibi davranır.

```html
<a asp-page="/product/new/5">Products</a>
<!-- 
    RESULT:
    <a href="/product/new/5">Products</a>
-->
```

- **asp-all-route-data**
    - Route üzerinden gönderilecek tüm dataların `Dictionary` yapısında value olarak alıp, bunu ilgili formata çeviren elemandır.

```html
@{
var parms = new Dictionary<string, string>
    {
        { "speakerId", "11" },
        { "currentYear", "true" }
    };
}

<a asp-route="speakerevalscurrent" asp-all-route-data="parms">Speaker Evaluations</a>
<!-- 
    RESULT:
    <a href="/Speaker/EvaluationsCurrent?speakerId=11&currentYear=true">Speaker Evaluations</a>
-->
```

- **asp-protocol**
    - Url'i bir protokol üstünden gitmeye zorlar.

```html
<a asp-route="product" asp-protocol="https">Products</a>
<!-- 
    RESULT:
    <a href="https://localhost/Product/Home/All">Products</a>
-->
```

- **asp-host**
    - Url'i bir host üstünden gitmeye zorlar.

```html
<a asp-route="product" asp-protocol="https" asp-host="example_host">Products</a>
<!-- 
    RESULT:
    <a href="https://example_host/Product/Home/All">Products</a>
-->
```

- **asp-fragment**
    - Url'i bir host üstünden gitmeye zorlar.

```html
<a asp-fragment="about">About</a>
<!-- 
    RESULT:
    <a href="/Product#about">About</a>
-->
```

### 02 - Form ve Input Tag Helper

Form içinde, input elementleri için tag helperlar kullanılırsa, model özellikleri elemente otomatik olarak atanır. Bunlardan en temeli; *type*, *name*, *id* olmakla beraber, aynı zamanda model üzerinde validation işlemleri için tanımlanmış meta tagları da elemente eklenir. Bu şekilde, client-side validation için gerekli olan attribute'ler otomatik eklenmiş olur.

- **asp-for**
    - View içinde kullanılan model yapısından otomatik veri alıp inputları doldurmaya yarar.
    - Html Helper yapsındaki `Html.#for` ifadeleriyle aynı yapıdadır.
    - Model üzerinden veri türünü de aldığından dolayı, input elementinin `type` attribute'ünü de otomatik oluşturur.
        - Eğer otomatik oluşturulan type kısmı uygun değilse, input elementinin içine type girilerek override edilebilir.
    - Bu helper, label yapılarında da kullanılabilir.
- **asp-items**
    - Select işlemi yaparken, içine girdiğimiz option elemanlarını otomatik olarak tanımlamamızı sağlar.
    - Value olarak, `List<SelectListItem>` ister.
        - Controller kısmından herhangi bir yapı ile bunu gönderebiliriz.

```cs
// Controller
public IActionResult Index()
{
    ViewBag.list = new List<SelectListItem>
    {
        new SelectListItem { Text="Istanbul", Value="34" },
        new SelectListItem { Text="Ankara", Value="06" }
    };
    return View();
}
```

```html
<form>
    <select asp-items="ViewBag.list"></select>
</form>

<!-- 
    RESULT:
    <form>
        <select>
            <option value="34">Istanbul</option>
            <option value="06">Ankara</option>
        </select>
    </form>
-->
```

- **asp-validation-for**
    - Validation hatalarını göstermede kullanılır.
    - Validation bölümünda ayrıntılı açıklanacaktır.
- **asp-validation-summary**
    - Validation hatalarını göstermede kullanılır.
    - Validation bölümünda ayrıntılı açıklanacaktır.


### 03 - Cache Tag Helper

- Html yapısının bir kısmını cache üzerinde tutarak performansı arttırmak için kullanılır.
- Tutulduğu yer : Internal ASP.NET Core cache provider.
    - Server üzerinde tutulduğu için, tüm kullanıcılara aynı şey görünür.
- Eğer süre belirlenmemişse default olarak 20 dk hafızada tutar.
- `<cache>` tagları yardımıyla kullanılır.

```html
<cache>@DateTime.Now</cache>
```

Cache tag helper ile birlikte kullanılacak attribute'ler şunlardır:

- `enabled` : Bool
    - Cache mekanizmasının çalışıp çalışmamasını ayarlar.
    - Default olarak true seçilidir.
    - False seçilirse, cache mekanizması çalışmaz.
- `expires-...`
    - Cache bitiş zamanını ayarlamaya yardımcı olur.
    - `expires-on` : Datetime
        - Absolute bir tarih girilerek cache mekanizmasının bitiş zamanı ayarlanır.
    - `expires-after` : TimeSpan
        - İlk istekten belli bir zaman sonra cache mekanizmasının tekrardan başlaması sağlanır.
        - Zaman bittiğinde cache silinir. Sonraki ilk istekte tekrar belirlenen zaman kadar cache tutulur.
    - `expires-sliding` : TimeSpan
        - Expires-after'a benzer, fakat süre bitmeden tekrardan istekle karşılaşırsa süreyi yeniden başlatır.
- `vary-by-...`
    - Cache mekanizmasının belirli bir özelliğe göre tutulmasını sağlar.
    - Özellik belirlendikten sonra, her farklı özellik sahibi, kendi cache yapısına sahip olur.
    - `vary-by-header` : Header tags
        - Belirlenen bir veya daha fazla header'a göre cache tutulması sağlanır. 
        - Böylece her header için ayrı bir cache tutulmuş olur. 
        - Örnek olarak aşağıdaki yapıda, `User-Agent` ve `content-encoding` header'ları farklı geldikçe, farklı farklı cache tutulur.

```html
<cache vary-by-header="User-Agent,content-encoding">
    Current Time Inside Cache Tag Helper: @DateTime.Now
</cache>
```

-   - `vary-by-query` : Query string key
        - Gelen Query string key'in valuesine göre cache tutulmasını sağlar.
    - `vary-by-route` : Route value key
        - Url Route üzerinden gelen key'in valuesine göre cache tutulmasını sağlar.
    - `vary-by-cookie` : Cookie key
        - Cookie elemanınan göre cache tutulmasını sağlar.
    - `vary-by-user` : Bool
        - Giriş yapan kullanıcıya göre cache tutulmasını sağlar.
    - `vary-by` : Model
        - Bir model yapısı verilerek, eğer model yapısı değişirse, cache mekanizmasının resetlenmesini sağlar.
        - Her model değeri için ayrı ayrı cache tutulur.

```cs 
public IActionResult Index(string myParam1,string myParam2,string myParam3)
{
    int num1;
    int num2;
    int.TryParse(myParam1, out num1);
    int.TryParse(myParam2, out num2);
    return View(viewName, num1 + num2);
}
```

```html
<cache vary-by="@Model">
    Current Time Inside Cache Tag Helper: @DateTime.Now
</cache>
```

- `priority` : High-Low-NeverRemove-Normal
    - Hafızanın yetersiz kaldığı yerde, cache'ler otomatik silinir.
    - Bu silinmeler önceliğe göre olmaktadır.
    - İlk silinecek cache'ler, priorty özelliği low olanlardır.

### 04 - Environment Tag Helper

- Belirlenen **hosting ortamına** göre, içindeki öğelerin dahil edilip edilmeyeceğini belirler.
- Örneğin geliştirme ortamında olmasını istediğimiz fakat product sürümünde görünmesini istemediğimiz bir html parçası varsa, environment tag helperlar ile belirlenebilir.

```html
<environment names="Development">
  <strong>HostingEnvironment.EnvironmentName is Development</strong>
</environment>

<environment names="Staging,Production">
  <strong>HostingEnvironment.EnvironmentName is Staging or Production</strong>
</environment>
```

- `names` : Staging - Development - Production
    - Görünmesini istediğimiz host ortamlarının isimleri yazılır.
    - Core 1.0 ve üstü sürümlerinde vardır.
- `include` : Staging - Development - Production
    - Görünmesini istediğimiz host ortamlarının isimleri yazılır.
    - Core 2.0 ve üstü sürümlerinde vardır.
- `exclude` : Staging - Development - Production
    - Görünmesini *istemediğimiz* host ortamlarının isimleri yazılır.
    - Core 2.0 ve üstü sürümlerinde vardır.

### 05 - Image Tag Helper

### Custom Tag Helpers

### Kaynaklar : 

- https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/image-tag-helper
- https://dannyvanderkraan.wordpress.com/2016/04/19/asp-net-core-1-0-goodbye-html-helpers-and-hello-taghelpers/