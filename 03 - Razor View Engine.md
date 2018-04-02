## RAZOR VIEW ENGINE

### 01 - Razor Syntax
- View Engine nedir?
    - View’lerinizi HTML çıktısı olarak render etmek için kullanılan bir mekanizma/teknoloji’dir.
- Razor View Engine nedir?
- Razor blogu açma ve Razor içinde C# kodlarının kullanımı
- Razor blogu içindeki değişkenlere Razor dışından ulaşım
- Razor açıklama satırı
- Parantez içinde iki değişkenin birleştirilmesi
- View içinde Razor ile döngülerin ve karar yapılarının kullanımı

#### Razor içinde html yapısı kullanırken oluşan hatalar
- Razor yapılarından biri kullanıldığında, hemen içine normal yazı yazıldığında bunu C# dili olarak algılar. Eğer yazdığımız bu kısım C# değilse bize hata verecektir.

```cs
@if (true)
{
    Lorem Ipsum // Bu kısım hata verecektir!
}
```

- Bu hatayı vermemesini sağlamak için bir kaç yöntem vardır:
    - Bir html tagı kullanmak
    - `@:` Razor hata bastırma kodu kullanmak
    - `<text> </text>` taglarını kullanmak.

```html
@if (true)
{
    <span> Lorem Ipsum </span>
    
    @: Lorem Ipsum
    
    <text>
        Lorem Ipsum
        Lorem Ipsum
    </text>
}
```

### 02 - HTML Helpers 
- Html Helper'lar adından da anlaşılacağı gibi, html kodlarını kısa yoldan, metotlarla oluşturmamız için bize yardımcı olur.
- Yazdığımız bu metotlar daha sonrasında render edilirken, normal html kodlarına çevilir.
- **NOT! :** Yazdığımız tüm html helper'ların son çıktısı html'dir.
    - **Bu nedenle helpers'lar ile yapılan her şey, normal html ile de yapılabilir!**
- **NOT2! :** ASP .NET Core ile Html Helper yapıları kısmen bulunsa da artık bu yapılar kullanılmamaktadır. Bunların yerine şu yeni yapılar kullılmaktadır:
    - TagHelpers
    - ViewComponents 

#### General Helpers
- Html.ActionLink()
- Html.Partial()

#### URL Helpers
- Html.ActionLink()
- Url.Action()
- Url helperların otomatik link oluşturması ve bunun önemi
    - .NET Core içinde bunlar yerine tag helperlar kullanılıyor.

#### System.Net.WebUtility Helpers
- .HtmlEncode() 
- .HtmlDecode()
- .UrlEncode() 
- .UrlDecode()
- Html.Raw()

#### Form Helpers
- Form elemanlarını oluşturmak için kullandığımız helper metotları.
- Her elemanın aynı zamanda bir de model ile kullanıma olanak tanıyan `-for` son ekli başka bir metodu da var.
- Form helpers'ların yerini Core 2.0'da `tag helpers`'lar almıştır.
- Form helperslar:
    - Html.BeginForm()
    - Html.TextBox() - Html.TextBoxFor()
    - Html.Label() - Html.LabelFor()
    - Html.AntiForgeryToken()
    - ...

### 03 - Custom Html Helpers
- `IHtmlHelper` sınıfı içerisine extension metotlar yazarak istediğimiz custom helper'ları ekleyebiliriz.
- Bunun için yapılması gereken adımlar şunlardır:
    - Bir klasör içinde ( Library ) class dosyası ( extensions ) oluşturulur.
        - Bu kısım zorunlu değil, fakat dosyaların düzgün bir düzende olması için böyle yapılması daha uygun olur.
    - Model static yapılmak zorunda
    - Class içine statik bir fonksiyon oluşturulur.
    - Fonksiyonun return’ü -> `IHtmlContent`
    - Fonksiyonun kalıtımı -> `this IHtmlHelper helper`
    - String olarak html hazırlanır.
    - `return new HtmlString( <string html metni> )`
    - Kullanılacak sayfada using ile Library klasörüne link verilir.

```cs
namespace Project.Library.Helpers
{
    public static class CustomHelper
    {
        public static IHtmlContent Submit(this IHtmlHelper helper, string value)
        {
            string html = $"<input type='submit' value='{ value }' />";
            return new HtmlString(html);
        }
    }
}
```

- - Html içinde kullanımı:

```html
<!-- using ifadesi eklenir -->
@using WebApplication1.Library.Helpers
...
<!-- Html fonksiyonu kullanılır. -->
<form method="post">
    @Html.Submit("Kaydet")
</form>
```