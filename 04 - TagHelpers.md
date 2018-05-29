## TAGHELPERS

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

```
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

- `asp-append-version` : Bool
    - Normalde tarayıcılar, statik dosyaları ismine göre lokal cache üzerinde saklarlar.
    - Örneğin;
        - `<img src="img/01.jpg" />` yoluna sahip bir resim dosyası olsun.
        - Bu html kısmı, tarayıcı üzerinde ilk karşılaştırıldığında, server üzerine tekrar bir istek gönderip resmi çeker ve local cache üzerine kaydeder.
        - Bundan sonra aynı yol üzerinde (`img/01.jpg`) olan bir istekle karşılaştığında, tekrar istek göndermez ve local cache üzerinden çeker.
    - Fakat bu durumda şöyle bir sıkıntıyla karşılaşılır : 
        - Eğer dosyanın adı değiştirilmeden içeriği değiştirilirse, tarayıcı sadece dosya yoluna ve ismine baktığından dolayı bu dosyayı değişmiş olarak algılamaz ve eski resmi cache üzerinden çekmeye devam eder. 
        - Bunun sonucunda server tarafında dosya değişmiş olsa da, client bu değişimi görmeyecektir.
    - Bunu engellemek için `asp-append-version` tag helper'ı kullanılabilir.
    - Bu helper `true` olarak işaretlenmişse, otomatik olarak dosya yolunun sonuna QueryString parametresi (`v`) ekler ve buna değer olarak, dosyanın `Sha512` ile alınmış HASH kodunu ekler.
    - Bunun sonucunda eğer dosya ismi değişmeden dosya içeriği değişmişse bile, HASH kodu değişeceğinden, tarayıcı bu değişikliği algılayıp, cache'ten çekmek yerine, tekrar istek gönderip yeni dosyayı çeker.

```html
<!-- img kullanımı -->
<img src="~/img/01.jpg" asp-append-version="true"/>

<!--
    RESULT:
    <img src="/img/01.jpg?v=cWg7C0N0mccb9F22TSxwFYQLlNru6a-18JsAfCFBAsY" />

    Eğer dosya ismi değişmeden içeriği değişirse;
    <img src="/img/01.jpg?v=JXe4q9gMIJ5w6NVcd0PtJRnHQZBac2HInNCJpqviHVA" />
-->
```

### 06 - Partial Tag Helper

- Dikkat edilmesi gerekenler : 
    - Bu helper **ASP.NET Core 2.1** sürümünden sonra kullanılabilir.
    - Partial View'leri **asekron** olarak yüklemeye yarar.
    - Alternatif olarak şu yöntemler de kullanılabilir : 
        - `@await Html.PartialAsync`
        - `@await Html.RenderPartialAsync`
        - `@Html.Partial`
        - `@Html.RenderPartial`

```html
<partial 
    name="_ProductViewDataPartial"
    asp-for="Product"
    view-data="@ViewData" />
```

Temel olarak 3 elemanı bulunur : 

- `name` : string - Required
    - Partial View yolunun verildiği alandır.
- `asp-for` : Model
    - Sayfa modeli üzerinden gönderilen bir elemanın partial üzerine aktarılmasını sağlar.
    - Örneğin `asp-for="Product"` anlam olarak `@Model.Product` demektir.
    - Bu gönderilen model, partial üzerinde model olarak kullanılır.
- `view-data` : ViewData elemanı
    - `ViewDataDictionary` olarak bilgi aktarılmasını sağlar.
    - Sayfaya controller üzerinden gelen herhangi bir `ViewData` aktarılabileceği gibi, sayfa üzerinde o anda üretilip de gönderilebilir.

### 07 - Custom Tag Helpers

- `Microsoft.AspNetCore.Mvc.TagHelpers` kütüphanesinin içinde gelen tag helper'lar dışında kendimiz de custom olarka tag helper oluşturabiliriz.
- Bunun için ;
    - Öncelikle bir class dosyası oluşturuyoruz.
    - Bu sınıfa `TagHelper` sınıfından kalıtım veriyoruz.
    - İçinde `Process` adlı bir override metot oluşturuyoruz.
    - Düzenlemeleri yaptıktan sonra View içine bu tag helper dll yolunu ekliyoruz.

#### İsimlendirme

- Helper isimlendirmesi yaparken iki yöntem kullanabiliriz:
    - Sonuna `-TagHelper` ekleyebiliriz.
    - Sadece ismini girebiliriz.
- Her iki durumda da kullanım değişmeyecektir.

```cs
public class CustomMailTagHelper : TagHelper {}
// > CustomMail > custom-mail

public class CustomMail : TagHelper {}
// > CustomMail > custom-mail
```

- İsimlendirme yapılırken dikkat edilmesi gereken bazı noktalar vardır.
    - Öncelikle class isimlendirmesi `CamelCase` düzeninde yapılması önerilir.
    - İsimlendirme sonrasında, html içinde kullanılırken, CamelCase düzeni `kebab-case` düzeninde kullanılır. (Son eklenen TagHelper kelimesinin dikkate alınmadığı unutulmamalıdır.)
        - CamelCase dışındaki isimlendirmeler aynen kullanılacaktır. Sadece büyük harfler küçük harf olarak alınacaktır.
    - Örnek vermek gerekirse;
        - CustomMail > custom-mail
        - NewInputExampleTagHelper > new-input-example
        - Custom_Mail > custom_mail

#### View içinde kullanılması

- Öncelikle View içinde kullanmak için dll yolunu belirtmemiz gerekiyor.
- Burada unutulmaması gereken önemli nokta, **yol verirken *namespace* değil *dll dosya ismini* belirtiyoruz.**

```cs
namespace Project.Library.TagHelpers
{
    public class CustomMail : TagHelper {}
}
```

- Proje derlendikten sonra, tüm yapı `Project.dll` dosyasına çevrileceğinden, kullanımı;

```cs
// proje içindeki tüm tag helperlar
@addTagHelper *, Project 

// veya sadece ilgili namespace içindeki tüm tag helperlar
@addTagHelper Project.Library.TagHelpers.*, Project

// veya sadece ilgili class içindeki tag helper
@addTagHelper Project.Library.TagHelpers.CustomMail, Project
```

- Bu yapılar `_ViewImports.cshtml` dosyası içine eklenirse, tüm View dosyalarında kullanılabilir.

#### Class içeriğinin yazılması : Property

- Clas içinde verilen property'ler, html yazarken belirtilebileceğimiz attribute anlamına gelir.
- Bu property'lere default değer atanarak, html kısmında yazılmadığında default değerin kullanılmasını sağlayabiliriz.
- Property isimleri `CamelCase` düzeninde verildiğinde, kullanırken bu isimler `kebab-case` düzenine çevrilir. (Yukarıda anlatılan kurallar geçerlidir.)

```cs
public class CustomMail : TagHelper
{
    public string MailAddress { get; set; }
}
```

```html
<custom-mail mail-address="example@mail.com"></custom-mail>
```

- Yukarıdaki kullanımdan farklı olarak, oluşlturduğumuz property'nin kendi ismiyle değil de farklı bir isim ile kullanılmasını istiyorsak, `HtmlAttributeName(<attribute_name>)` metodunu kullanabiliriz.

```cs
public class CustomMail : TagHelper
{
    [HtmlAttributeName("mail")]
    public string MailAddress { get; set; }
}
```

```html
<custom-mail mail="example@mail.com"></custom-mail>
```

#### Class içeriğinin yazılması : Fields

- Class içeriğine girilen field'lar private veya public olsalar da tag helper üzerinden erişilemezler.
- Class içinde kullanılacak private değişkenler bu şekilde tanımlanabilir.

#### Class içeriğinin yazılması : Process Method

- Process metot, tag helper çıktısı vermek için override ettiğimiz metottur.
- İçinde iki tane parametre ile gelir :
    - **TagHelperContext:** 
    - **TagHelperOutput:** 

```cs
public class CustomMail : TagHelper
{
    public string MailAddress { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
    }
}
```

- `output.TagName`
    - Tag helper kullanıldığında, çevrilmesini istediğimiz tag kısmını buraya yazıyoruz.

- `output.Attributes`
    - Seçilen tag içinde attribute işlemleri yapmamızı sağlar.
    - `.SetAttribute(<attribute_name>, <value>)`
        - Tag içinde belirtilen attribute değerinin oluşmasını ve value atanmasını sağlar.
    - `.RemoveAll(<attribute_name>)`
        - Seçilen attributleri tag içinden kaldırır.

- `output.Content`
    - Taglar arasında kalan text alanı ile ilgili işlemler yapmayı sağlar.
    - `.SetContent(<text>)`
        - Tag içinde belirtilen text'in eklenmesini sağlar.

```cs
public class CustomMail : TagHelper
{
    public string MailAddress { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.SetAttribute("href", "mailto:" + MailAddress);
        output.Content.SetContent(MailAddress);
    }
}
```

```html
<custom-mail mail-address="example@mail.com"></custom-mail>

<!-- 
    RESULT:
    <a href="mailto:example@mail.com">example@mail.com</a>
-->
```

- `output.PreContent`
    - Taglar arasında kalan content kısmının öncesi ile ilgili işlemler yapamızı sağlar.
    - `.SetHtmlContent(<text>)`
        - Taglar arasındaki content kısmının öncesine html text eklenmesini sağlar.

- `output.PostContent`
    - Taglar arasında kalan content kısmının sonrası ile ilgili işlemler yapamızı sağlar.
    - `.SetHtmlContent(<text>)`
        - Taglar arasındaki content kısmının sonrasına html text eklenmesini sağlar.

```cs
[HtmlTargetElement(Attributes="bold")]
public class BoldTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("bold");
        output.PreContent.SetHtmlContent("<strong>");
        output.PostContent.SetHtmlContent("</strong>");
    }
}
```

```html
<p bold>Lorem Ipsum.</p>

<!--
    RESULT:
    <p><strong>Lorem Ipsum.</strong></p>
-->
```

- `output.SuppressOutput()`
    - Çıktının render edilmemesini sağlar.
    - Genellikle belirli bir koşula göre içeriğin gösterilip gösterilmemesi durumda kullanılır.

#### Class Attributes

- Tag helper sınıflarına yazılan attribute'ler ile birçok seçim ve düzenleme yapılabilir.

- `[HtmlTargetElement(<tag>, <Attributes>, <ParentTag>, <TagStructure>)]`
    - Üstüne yazıldığında tag helper'ın hangi durumlarda uygulanacağını belirler.
    - `<tag>` elemanı verildiğinde, belirtilen tag kullanımında bu ayarların uygulanacağını belirtir.
    - `<Attriubutes>`, verilen attribute'ler kullanıldığında tag helper'ın aktifleşmesini sağlar.
    - `<ParentTag>`, verilen tag etiketinin içindeki tagların etkilenmesini sağlar.
    - `<TagStructure>`, tag etiketinin kapanma tagının olup olmayacağının ayarlandığı kısımdır.
- Bu elemanlar ayrı ayrı attribute olarak kullanılabileceği gibi, aynı attribute içinde de yazılabilir.
    - Ayrı ayrı yazılırsa `OR - VEYA` olarak algılanır.
    - Aynı attribute içinde yazılırsa `AND - VE` olarak yazılır.

```cs
[HtmlTargetElement("bold")]
[HtmlTargetElement(Attributes = "bold")]
[HtmlTargetElement("p", ParentTag = "boldThemAll")]
public class BoldTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("bold");
        output.PreContent.SetHtmlContent("<strong>");
        output.PostContent.SetHtmlContent("</strong>");
    }
}
```

```html
<bold>Example</bold>

<p bold>Example</p>

<boldThemAll>
    <p>Example 01</p>
    <p>Example 02</p>
</boldThemAll>

<!-- 
    RESULT:

    <bold><strong>Example</strong></bold>

    <p><strong>Example</strong></p>

    <boldThemAll>
        <p><strong>Example 01</strong></p>
        <p><strong>Example 02</strong></p>
    </boldThemAll>
-->
```

#### Tag helper ile model alma ve gönderme

```cs
public class WebsiteContext
{
    public Version Version { get; set; }
    public int CopyrightYear { get; set; }
    public bool Approved { get; set; }
    public int TagsToShow { get; set; }
}

public class WebsiteInformationTagHelper : TagHelper
{
    public WebsiteContext Info { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "section";
        output.Content.SetHtmlContent(
                $@"<ul><li><strong>Version:</strong> {Info.Version}</li>
            <li><strong>Copyright Year:</strong> {Info.CopyrightYear}</li>
            <li><strong>Approved:</strong> {Info.Approved}</li>
            <li><strong>Number of tags to show:</strong> {Info.TagsToShow}</li></ul>");
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
```

```html
<website-information 
    info="new WebsiteContext() { 
        Version = new Version(1,2,0), Approved = true, CopyrightYear = 2018, TagsToShow = 1 
    }"></website-information>

<!--
    RESULT:
    <section>
        <ul>
            <li><strong>Version:</strong> 1.2.0</li>
            <li><strong>Copyright Year:</strong> 2018</li>
            <li><strong>Approved:</strong> True</li>
            <li><strong>Number of tags to show:</strong> 1</li>
        </ul>
    </section>
-->
```