## VIEW COMPONENTS

- Daha önceki .NET Framework sürümünde kullanılan `Partial View` yapısına benzer fakat biraz daha esnek ve geniş kullanım özellikleri sunan yapıdır.
- `Partial View` kavramı .NET Core sürümünde halen olsa da, daha kullanışlı olması nedeniyle `View Components` yapısını kullanmayı tercih ederiz.
- Buradaki temel mantık, bir kere yazılan html parçasının, modelli veya modelsiz (dinamik veya statik) bir yapıda tekrar tekrar kullanılmasını sağlamaktır. Bu sayede, bu html parçasında yapılan tek bir değişiklik, bu parçanın kullanıldığı tüm yerlerde değişecektir.
- View Componentleri, View yapısının fonksiyonları gibi düşünebiliriz. Parametre alabilen ve geriye html döndüren bir yapısı vardır.
- View Components kullanmak `DRY` prensibi gereği kod tekrarının önleyecektir.
- View Componentler, parametre alımına izin verir, bu Partial View'da olmayan bir özelliktir.
- Componentler, direk olarak `Layout` içinde kullanılabilir. Bu kullanım için, View dosyalarının shared dizini altında bulunması gereklidir.

### 01 - View Component Oluşturma

- Projemiz içinde bir class oluşturuyoruz. 
    - Class'ı bir klasör içinde oluşturmamız projenin toplu durması açısından daha iyi olur.
- Açtığımız class yapısının component olduğunu belirtmek için üç yol vardır : 
    1. Herhangi bir isimle oluşturulup `ViewComponent` sınıfından kalıtım verebiliriz.
    2. Herhangi bir isimle oluşturulup `[ViewComponent]` attribute'ü eklenebilir.
    3. İsimlendirme yaparken isminin sonuna `-ViewComponent` ekleyebiliriz.
    - Her üç durumda da, oluşturduğumuz class artık bir View Component olacaktır.
    - **NOT:** Bu üç durumdan sadece 1. durumda View Component `View()` döndürecektir. Bu sınıfın özelliklerine ulaşabilmek için en doğru olan metot kalıtım vermektir.
- View Component sınıfı, Controller sınıflarında olduğu gibi; public, non-nested ve non-abstract olmalıdır.
- Oluşturduğumuz component sınıfı içinde bir tane `Invoke()` adlı metot oluşturuyoruz.
    - Component'i çağırdığımız zaman çalışan ve bize geri dönüş veren metot bu metottur.

```cs
public class Hello : ViewComponent
{
    public string Invoke()
    {
        return "Hello World!";
    }
}
```

### 02 - Oluşturulan View Component'i View İçinde Kullanma

- View Component'i çağırmak için `Component.InvokeAsync()` metodunu `await` olarak çağırmak yeterlidir.

```cs
@await Component.InvokeAsync("Hello")
```

**NOT:** Componentler, Controller içinden de direk olarak return edilebilirler:

```cs
public IActionResult IndexVC()
{
    return ViewComponent("PriorityList", new { maxPriority = 3, isDone = false });
}
```

### 03 - ViewComponent Result Türleri

- ViewComponent sınıfı içindeki Invoke() metodunun genel geri dönüş tipi `IViewComponentResult`'tur. Bunun dışında aşağıdaki gibi bilinen C# veri türleri döndürlürse, bu tipler de geri dönüş tipi olarak verilebilir.

#### Bilinen türleri döndürme

- Yazılan `Invoke()` metodu sonucunda, C# yapısındaki bilinen türleri döndürebilir. Örn: string, int, double vs.

```cs
public class Hello : ViewComponent
{
    public string Invoke()
    {
        return "Hello World!";
    }
}
```

#### Html Döndürme

- String olarak veri döndürdüğümüzde ve bunu view içinde kullandığımda, html taglarının string olarak algılanıp biçimlendirme yapılmadığını görürüz. Bunların html olarak algılanması için `HtmlString` türünde bir geri dönüş kullanmamız gerekmektedir.

```cs
[ViewComponent]
public class Hello
{
    public HtmlString Invoke()
    {
        return new HtmlString("<b>Hello World!</b>");
    }
}
```

#### ViewComponent ile View Döndürme

- ViewCompoenentler, Invoke metotları içinde statik veya model ile dinamik bir view döndürebilirler.
- Oluşturulacak view dosyalarının aranacağı dizinler :
    - `Views/<controller_name>/Components/<view_component_name>/<view_name>`
    - `Views/Shared/Components/<view_component_name>/<view_name>`
- **NOT:** Eğer return edilen View() metodu içinde, html dosyasının adı belirtilmemişse, default olarak arayacağı view ismi `default.cshtml`'dir.

```cs
public IViewComponentResult Invoke(){ 
    return View(); // Default.cshtml dosyasını arar
}

public IViewComponentResult Invoke(){ 
    return View("foo"); // foo.cshtml dosyasını arar
}
```

- View Component'lerde View ile model kullanımı, Views'larda olduğu gibidir.
- Örnek vermek gerekirse:

```cs
// View Component Sınıfı
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Project.Library.Components
{
    public class SideBar : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new Dictionary<string, string>();
            for (int i = 0; i < 5; i++)
                model.Add($"www.example{i}.com", $"Web Site {i}");

            return View(model);
        }
    }
}
```

```html
<!-- ~/Views/Shared/Components/SideBar/Default.cshtml dosyası -->
@model Dictionary<string, string>

<h3>Side Bar</h3>
<ul>
    @foreach (var key in Model.Keys)
    {
        <li><a href="@key">@Model[key]</a></li>
    }
</ul>
```

```html
<!-- Kullanımı -->
@await Component.InvokeAsync("SideBar")

<!-- 
    Çıktısı

    <h3>Side Bar</h3>
    <ul>
        <li><a href="www.example0.com">Web Site 0</a></li>
        <li><a href="www.example1.com">Web Site 1</a></li>
        <li><a href="www.example2.com">Web Site 2</a></li>
        <li><a href="www.example3.com">Web Site 3</a></li>
        <li><a href="www.example4.com">Web Site 4</a></li>
    </ul>
-->
```

### 04 - ViewComponent Üzerine Parametre Gönderme

```cs
// Component Model
public HtmlString Invoke(string message)
{
    return new HtmlString($"<b><i>{message}</i></b>");
}

// Html
@await Component.InvokeAsync("Hello", new { message = "Test Message" })
```
