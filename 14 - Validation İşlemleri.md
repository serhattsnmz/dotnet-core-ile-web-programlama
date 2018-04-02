## VALIDATION İŞLEMLERİ

- Validation kontrolü yapmanın önemi
    - Web zaafiyetlerinin en büyük nedeni input validation eksikliğinden kaynaklanır.
    - Ayrıca kullanıcıdan alınan bilgilerin bazı kurallara uyması gerekir. Örn: Parolanın minimum uzunluğu gibi.
- .NET kütüphanesi ile, alınan her verinin validation kontrolünün ayrı ayrı yapılma zorluğu ortadan kalkmış ve bu validationların model üzerinden attribute olarak tanımlanması sağlanmıştır.
    - Bu şekilde validation yapıldığıda, kontrol etmek için sadece `ModelState` üzerinde `IsValid` kontrolü yapmak yeterlidir.

> **NOT:** .NET Framework sürümünde, inputa gidilen html kodları otomatik olarak elimine edilip zararlı kod olarak algılanıyordu. .NET Core sürümünde ise bu kontrol yapılmamaktadır. Buna dikkat edilmesi gerekir.

- Validation işlemleri `Model Binding` tarafından yönetilir.
- Model Binding, `ModelStateDictionary` ile çalışır.
    - Bu dictionary içinde 3 temel yapı vardır: 
        1. **AddModelError:** Model içindeki herhangi bir property üzerine error eklemeyi sağlar
        2. **GetValidationStat:** Validasyon durumunu gösterir. Enum değer döndürür.
        3. **IsValid:** Validasyon işlemi sonucunda `true` veya `false` döndürür.

<p align="center">
    <img src="assets/08.png" style="max-height:350px">
</p>

İleriki kısımlarda kullanılacak model ve html yapısı yapısı:
```cs
public class Register
{
    public string UserName { get; set; }
    public string EMail { get; set; }
    public string Password { get; set; }
    public DateTime Birthday { get; set; }
    public bool TermsAccepted { get; set; }
}
```

```html
@model Register

<form method="post">

    <div class="text-danger" asp-validation-summary="ModelOnly"></div>

    <div class="form-group">
        <label asp-for="UserName"></label>
        <input asp-for="UserName" class="form-control" />
        <span asp-validation-for="UserName"></span>        
    </div>

    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" />
        <span asp-validation-for="Password"></span>
    </div>

    <div class="form-group">
        <label asp-for="EMail"></label>
        <input asp-for="EMail" class="form-control" />
        <span asp-validation-for="EMail"></span>
    </div>

    <input type="submit" value="Send!" class="btn btn-success btn-sm" />
</form>
```

### Modele Error Mesaj Ekleme

- Modele error mesaj eklemek için, `ModelState`'in `AddModelError` metodu kullanılır.
- İlk parametre olarak eklenmesini istediğimiz property ismini, ikinci olarak da eklemek istediğimiz mesajı gireriz.
    - Property ismini `string` veya `nameof()` ifadeleriyle verebiliriz.
- **NOT:** Model içindeki herhangi bir property'ye bu metot ile hata mesajı eklendiğinde, `IsValid` işlemi false döner.

```cs
[HttpPost]
public IActionResult Index(Register model)
{
    if (model.Password != null && model.Password.Length < 6)
        ModelState.AddModelError("Password", "Parola en az 6 karakterli olmalıdır.");

    if (!model.EMail.Contains("@"))
        ModelState.AddModelError(nameof(Register.EMail), "Girilen değer email olmalıdır!");

    return View(model);
}
```

### Form Üzerinde Validation Hata Mesajlarını Gösterme

- Bir model üzerinde Validation kontrolü yapıldığında, model üzerine hata kodları da eklenir.
    - Bu nedenle hata mesajlarının gösterilmesi için, *validation kontrolünden geçmiş modelin* tekrar sayfaya gönderilmesi gereklidir.
- Bu model daha sonra tekrar View üzerine gönderildiğinde, inputlar `asp-for` ile tanımlanmışsa, input'un class yapısına `.input-validation-error` isminde bir class daha eklenir.
    - Bu class ismi kullanılarak, hatalı validation hatası yakalanmış inputlar üzerinde css düzenlemesi yapılabilir. Örn, input kırmızı border ile işaretlenebilir.  
- Validation işlemlerini her input için ayrı ayrı göstermek istiyorsak, `asp-validation-for` yapısı kullanılmalıdır.

```html
div class="form-group">
    <label asp-for="EMail"></label>
    <input asp-for="EMail" class="form-control" />
    <span asp-validation-for="EMail"></span>
</div>
```

- Validation mesajlarının tamamının gösterilmesi için, `asp-validation-summary` yapısı kullanılmalıdır. Bu yapının 3 tane enum değeri vardır: 
    - **All:** Property ve model errorların hepsi gösterilir.
    - **ModelOnly:** Property errorları dahil edilmez, sadece model'e manuel eklenen hata mesajları gösterilir.
    - **None:** Herhangi bir hata mesajı gösterilmez.

```html
<div class="text-danger" asp-validation-summary="All"></div>
```