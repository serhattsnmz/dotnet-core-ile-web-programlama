## MODELS VE MODEL BINDING

### Controller'dan View'e Model ile Veri Taşıma
- Model klasörü içinde yeni bir model oluşturma
- Controller içinde boş bir model türetmesi oluşturma ve doldurma
- Modeli View’a gönderme
- Model içinde modeli çağırma ve sayfada gösterme
- Tek model gönderme
- Model listesi gönderme
- Birden fazla model gönderme

### View'den Controller'a Model ile Veri Taşıma
- View’dan Controller’a model gönderme
- View’da Controller’a kaç farklı şekilde veri gönderilir?
- Düz html form yapısı ile model gönderme arasındaki farklar

### ViewModel Oluşturma
- Birleşik Model oluşturma
- Birleşik Modelleri Controller’dan View’a gönderme
- Birleşik Modelleri View’dan Controller’a gönderme

### Partial View'ı Model Yapısı Kullanarak Gönderme
- Partial View yapısı ikinci parametre olarak bir model alarak dinamik bir hale bürünür.
- Bu yapıyı html içinde `Html.Partial(<partial>, <model>)` olarak kullanabileceğimiz gibi, ActionResult üzerinden `PartialView(<partial>, <model>)` return ederek de kullanabiliriz.
- Core MVC yapısında bu yapılar yerine `View Components` yapıları gelmiştir.(bkz. ilgili ders notu)

<p align="center">
    <img src="assets/03.png" style="max-width:900px" />
</p>

<p align="center">
    <img src="assets/04.png" style="max-width:900px" />
</p>