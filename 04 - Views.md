## VİEWS

### 1) Views ve Layout Kullanımı
- Layout page oluşturma
- `RenderBody()` kısmı

### 2) ViewStart Ne İşe Yarar
- Views klasörü altına `_ViewStart.cshtml` tanımlama
- ViewStart için layout tanımlama
- Html sayfalarında layout tanımını kaldırma

### 3) Otomatik Layout Sayfası Oluşturma
- Yeni bir view oluştururken “Use Layout Page” tanımını;
    - Belirtme
    - Boş bırakma
    - Tick kaldırma
- .NET Core yapısında, ilk çalıştırıldığında tick işaretlenirse .NET Frameworkte olduğu gibi otomatik olarak JS ve CSS dosyalarını indirme yoktur.

### 4) Nested (İç İçe) Layout Oluşturma
- View Page with Layout oluşturma

### 5) Section ( Bölüm ) Oluşturma
- RenderSection – True/False
- Default Section tanımlama:

```cs
@if (IsSectionDefined("Footer"))
{
    RenderSection("Footer");
}
else
{
    <span>This is the default yo!</span>
}
```

### 6) Partial View Kullanımı
- Yeni bir Partial View oluşturma
- @Html.Partial(“yol”)
- @{ Html.RenderPartial(“yol”) }
- Yol Belirtme
    - Shared içinde veya kendi klasörünün altında direk isim yazarak çağırılabilir.
    - Diğer klasörlerin altında ise tam yol belirtilmesi zorunludur.
- Partial çekilirken, partial’a bir model gönderilerek de çekilebilir.

### 7) _ViewImports.cshtml Dosyası
- Views içerisinde kullanacağımız using ifadeleri global olarak tanımlamak için kullanılır.
- Views dizini altında oluşturulur ve ismi sabittir.
- İçerisindeki ifadeler başında @ işaretiyle yazılır.

```cs
@using Project.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
...
```