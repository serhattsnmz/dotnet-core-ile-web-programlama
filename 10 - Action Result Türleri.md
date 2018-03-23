## ACTION RESULT TÜRLERİ

- Tüm Result türleri `IActionResult`’tan türemiştir.
- Bu nedenle `IActionResult` altında tüm result türlerini return edebiliriz.
- Bir action içinde birden fazla farklı dönüş tipleri olabileceği için, genel olarak `IActionResult` kullanmak daha mantıklıdır.

### ViewResult - View()
- Tanımlı bir View içeriğini render edip döndürmeyi sağlar.
- Metot boş kullanılırsa, Controller ile aynı ismi taşıyan klasör içindeki Action ile aynı ismi taşıyan cshtml belgesini döndürür. 
- Farklı bir view döndürülmek isteniyorsa, metot içine ismi yazılabilir.

### PartialViewResult - PartialView()
- Tanımlanmış bir partial görünümünün render edilip döndürülmesini sağlar.
- Partial, Controller ile aynı isimli klasör içinde veya `Shared` klasörü içindeyse, sadece ismi girilerek partial döndürülebilir. Aksi durumda tam yol girilmesi gereklidir.
- Partial tek başına döndürülebileceği gibi, bir model ile birlikte render edilerek döndürülebilir.

### ContentResult - Content()
- Kullanıcı tarafına view kullanmadan içerik döndürür.
- String parametre alır.

### RedirectResult - Redirect()
- Url yönlendirmesi yapar
- Dış siteye bağlantı verilecekse `http://` kullanılması unutulmamalıdır.
- `Redirect` => 302 döndürür.
- `RedirectPermanent` => 301 döndürür.

### RedirectToActionResult - RedirectToAction()
- Farklı bir action metoduna yönlendirme yapar.
- Sadece action ismi verilirse, aynı controller üzerinde bu action'ı arar.
- Farklı bir controller üzerindeki action'a yönlendirme yapılabilir.
- Yönlendirme yapılırken bilgi taşınacaksa, `routeValues` parametresi kullanılabilir.

### JsonResult - Json()
- Verilen bir objeyi serialize edip Json formatına dönüştürür ve döndürür.
- Özellikle Ajax işlemlerinde kullanılır.
- İkinci parametre olarak serialize özellikleri girilebilir.

### FileResult - File()
- Dosya döndürmek için kullanılır.
- Dosya şu türlerde verilebilir:
    - Virtual Path (`VirtualFileResult`)
    - Byte dizisi (`FileContentResult`)
    - Stream nesnesi (`FileStreamResult`)
- Burada dosya türü olarak `MimeType` verilmelidir.
    - Bu bilgi raw response'un headers kısmında iletilir.
    - Bu bilgi sayesinde tarayıcı gelen dosyanın türünü anlar ve ona göre işlem yapar.

### StatusCodeResult
- `StatusCode()`
    -`int` türünde verilen durum kodunu döndürür.
- `NotFound()` - (`NotFoundResult`)
    - Aranılan içeriğin bulunmadığını belirten sayfayı döndürür.
    - Durum kodu : 404
- `Unauthorized()` - (`UnauthorizedResult`)
    - Yetkisizi işlem yaptığınıa dair bir durum kodu ve mesajı döndürür.
    - Durum kodu : 401

### ObjectResult - StatusCode()
- Durum koduyla birlikte ikinci bir parametre olarak bir object döndürür.
- Bu object raw request'in body kısmında taşınır.

### Diğer Result Türleri
- `EmptyResult`
- `ChallengeResult`
- `ForbitResult`
- `SignInResult`
- `SignOutResult`
- `PageResult`
- `ViewComponentResult`