## DEPENDENCY INJECTION NEDİR?

#### DI Nedir ? 

- Bir sınıfın bağımlı olduğu sınıfların, bu sınıfın içine dışarıdan enjekte edilmesi işlemidir.
- Temel amaç, sınıfın bağımlılığını ortadan kaldırmaktır.
- Nesne tabanlı programanın en önemli prensiplerinden biri olan SOLID ilkesinin içinde bulunur.

#### Var olan bir kodu neden değiştirmek istemeyiz?

- Program içindeki bir kodu zorunlu kalmadıkça değiştirmek istemeyiz. 
- Bunun en büyük nedeni, mevcut kodun uzun zamandır test edilmiş ve sorunları devamlı düzeltilmiş olup, yapacağınız ufak değişiklikler bile, hem kodu hem de bağlı olan yapıları tekrar test etmeyi gerektirecektir.
- Eğer yazdığımız unit testler yeterince kapsamlı değilse, kodun güvenirliliğine emin olmak için, uzun bir süre daha kullanıp hatalarını gidermek gerekecektir.
- Bunun içindir ki SOLID prensiplerinden bir tanesi de Open/Closed Principle (OCP) olarak geçmektedir. OCP kısaca bir kod değişime kapalı ama genişletmeye açık olmalıdır demektir. Yani bir koda yeni şeyler ekleyebilirsiniz, mesela yeni sınıflar gibi. Ama var olan sınıfları ve sınıflar içinde ki methodların içeriğini değiştirmeniz yukarıda açıkladığım sebeplerden dolayı çoğu zaman tavsiye edilmez.
- DI ile bağımlılıklar genel olarak ortadan kaldırıldığı için, genişletilebilirlik ve her genişletmenin bağımsız olarak test edilebilirliği sağlanmış olur.

#### DI Kullanmadan programlama

- Örnek olarak elimizde aşağıdaki gibi bir sınıf olsun.

```cs
public class Creator {

    private Checker _checker = new Checker();
    // işlemler
}

// veya aynı class'ı constructor ile yazarsak;

public class Creator {

    private Checker _checker;

    public Creator() {
        _checker = new Checker();
    }
    // işlemler
}
```

- Yukarıdaki sınıf incelenirse, dışarıdan `Checker` adında başka bir sınıftan türeyen nesneye sahip. Yani bu sınıfa bağlı.
- Eğer ileride farklı bir checker kullanılmak isteniyorsa veya birden fazla müşteri için birden fazla checker yazmak gerekiyorsa, yapılabilecek iki çözüm yolu vardır : 
    1. olarak, Checker adlı sınıf değiştirilebilir. 
        - Böyle bir değişim yapılırsa, eski sınıfın bilgilerinin üzerine override edilmesi gerektiği gibi, yeniden kodun test edilmesi ve bağlı yapılarla uyumunun kontrol edilmesi gerekmektedir. 
        - Üstelik böyle bir düzenlemenin geri dönüşü olmayacaktır, çünkü eski yapı silinip yeni bir yapıya geçilmiştir.
    2. olarak, Checker benzeri başka sınıflar oluşturulabilir ( Öneğin; NetChecker, AISChecker vb. ). 
        - Böyle bir durumda, Creator sınıfı ve buna benzer Checker sınıfından kalıtım alan diğer tüm sınıflar içinde değişiklik yapılmalıdır. 
        - Bu durum önceki duruma göre geri dönüşüme izin verir, fakat birden çok class yapısı etkilendiği için sağlıklı değildir. Üstelik gözden kaçırılan bağımlılıkların olmaması için, tüm bağımlı sınıfların loglarının tutulması da gereklidir.
        - Bunlar dışında oluşturulacak yeni sınıfların belli bir düzenini tutturmak zor olacaktır ve eksik metot veya property ekleme olasılığı artacaktır. Bu da sınıfı kullanan diğer yerlerdeki hata olasılığını arttıracaktır.

#### Interface kullanarak programlama

- Türetilen sınıfların belli bir düzende olmasını ve hepsinde ortak property ve metotların olmasını sağlamak için interface'ler kullanılır.
- Interface'lerin implament edildiği tüm sınıflar aynı düzende olacaktır.

```cs
public interface IChecker
{
  bool Exists(AccountNumber accountNumber);
}

public class DatabaseAccountChecker: IChecker {}

public class AzureAccountChecker: IChecker {}

public class XmlAccountChecker: IChecker {}
```

- Creator sınıfını artık aşağıdaki gibi yazabiliriz:

```cs
public class Creator {

    private IChecker _checker;

    public Creator() {
        _checker = new DatabaseAccountChecker();
    }
    // işlemler
}
```

- Bu durumda sadece ctor içindeki nesneyi değiştirmemiz yeterli olacaktır.
- Bu şekilde yeni sınıflar için düzen sağlanmış ve bağımlılık kısmen azaltılmış olacaktır. 
- Fakat bu durumda bile halen Creator sınıfı IChecker interface'inden türetilen sınıflara bağımlı durumdadır.

#### Dependency Injection kullanarak programlama

- Oluşturacağımız sınıflarda, interface'ler dışında başka hiçbir sınıfa bağımlılık bulunmamalıdır.
- Yukarıdaki kodları aşağıdaki gibi değiştirebilir:

```cs
public class Creator {

    private IChecker _checker;

    public Creator(IChecker checker) {
        _checker = checker;
    }
    // işlemler
}
```

- Son durumda, sınıf içinde interface dışında herhangi bir bağımlılık bulunmamaktadır.
- Interface'ten türetilen sınıflar, Creator sınıfı kullanıldığında (ctor metot parametresi olarak tanımlandığından) dışarıdan zorunlu olarak girilmesi istendiğinden, her kullanıldığı yerde parametre olarak almak zorundadır. 
- Fakat DI kütüphaneleri kullanarak (örn: Ninject DI framework), global olarak bu ilişkilendirme işlemi yapılabilir. Örneğin, her `IChecker` interface'i kullanıldığı zaman `DatabaseAccountChecker` sınıfından bir nesne gönder gibi.
- Bu kütüpnanelere örnek olarak [Ninject](http://www.ninject.org/) verilebilir. Ayarlaması basit olarak aşağıdaki gibidir:

```cs
public class CheckerModule : NinjectModule
{
    public override void Load() 
    {
        this.Bind<IChecker>().To<DatabaseAccountChecker>();
    }
}
``` 

- .NET Core içinde DI otomatik olarak gelir ve `Startup.cs` içinde birleştirme yapılabilir.
- Örnek olarak `IPersonRepository` interface'i ile `SQLPersonRepository` sınıfı birleştirilmek isteniyorsa, aşağıdaki gibi bir kod, `Startup.cs` içine eklenir.

```cs
services.AddTransient<IPersonRepository, SQLPersonRepository>();
```

### Sonuç : DI Faydaları

- Projede somut sınıflar üzerine olan bağımlılığı azaltacaktır ve dolayısıyla genel olarak bakımın daha kolay olmasını sağlayacak.
- Daha düzgün tasarlanmış modüller arası dependency tree’ye sahip olacaksınız.
- Programda istenilen sınıfların rahatlıkla değiştirilmesine yardımcı olacak. Çünkü bağımlılık olmadığı için bir sınıf yerine başka bir sınıf kullanmak istediğinizde değiştirmek zorunda olacağınız kod miktarı ya sıfır yada çok daha az olacak.
- Uygulama çalışma anında sizlere daha rahat configuration yapma şansı tanıyacak. Mesela, XML kullanarak hangi interface ile hangi sınıfın birbirlerine bağlı olacağını kolayca tanımlayabilirsiniz. Bu farklı müşterilen farklı isteklerini kodunuzu değiştirmek zorunda kalmadan rahatlıkla sunabilmeniz demektir.
- Hangi sınıfları kullanacağınızı bir yerden kontrol etmiş olacaksınız.
- Belkide en önemlisi çok daha rahat Unit Test ler yazmanızı sağlamış olacağı. İstediğiniz sınıfı rahatlıkla mock ederek farklı kod kısımlarını test edebileceksiniz.