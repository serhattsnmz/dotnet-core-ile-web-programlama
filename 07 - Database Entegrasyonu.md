## DATABASE ENTEGRASYONU

### 01 – ORM (Object Relational Mapping) Nedir?
- İlişkisel veritabanı (RDBMS) ile nesneye yönelik programlanın (OOP) arasında bir tür köprü özelliği gören ve ilişkisel veritabanındaki bilgilerimizi yönetmek için, nesne modellerimizi kullandığımız bir tekniktir/metodtur.
- Database işlemleri öncelikle bu katmanda gerçekleştirilir, sonra database üzerine SaveChanges yapılır.
    - Transaction Özelliği
    - Bu sayade verilerin CRUD işlemlerinde herhangi bir hata çıkması sonucu oluşan yarıda kesilme ve verilerin bir kısmının db üzerine kayıt edilmesi sorunu ortadan kalkar.
    - Veriler biz `SaveChanges()` metodunu çalıştırmayana kadar db üzerine kaydedilmez.
    - Bu yüzden işlemler bittiğinde bu metodun çalıştırılması unutulmamalıdır.

<p align="center">
    <img src="assets/05.png" style="max-height:300px" />
</p>

<p align="center">
    <img src="assets/06.png" style="max-height:300px" />
</p>

### 02 - Code First (Önce Kod)
- Projelerde DB kullanırken bunu iki şekilde yapabiliriz.
    1. DB First : Öncelikle db tasarlanır ve ilgili db üzerinde tablolar oluşturulur. Daha sonrasında bu tablo yapıları program içinde çekilerek (ADO.NET ile) üzerinde CRUD işlemleri yapılır.
    2. Code First : DB tasarımı yapıldıktan sonra, bu yapılar OOP yapısına uygun olarak modellenir, daha sonra bu modeller üzerinden db tabloları ilgili database üzerinde oluşturulur.

### 03 - Dependency Injection
- Tanım : 
- Dependency Injection yapısı ile database işlemleri yapmak için izlenmesi gereken yol:
    1. Database tablolarını oluşturacak `Model (Entity)` oluşturulması
    2. Veritabanı bağlantısını sağlayacak `Context` sınıfının oluşturulması
    3. Migration işlemleri
    4. Bu modelleri kullanabilmek için yazılacak `Interface` oluşturulması
    5. Interface'lerin implament edilip db işlemlerini yapan `Repository` oluşturulması
    6. Interface ve Repository bağlantısının `Dependency Injection` ile `Startup.cs` içinde yapılması
    7. Controller içinde DI ile interface'in çekilmesi

###  ADIM 01 - Veritabanı Sınıflarını ( Entities ) Oluşturma
- Tabloları oluştururken models dizini altında yeni bir `Entities` dizini açılıp kullanılabileceği gibi, yeni bir class library projesi de açılarak modellemeler oluşturulabilir.
- Modellemeler oluşturulurken dikkat edilmesi gerekenler:
    - Her modelin bir ID'si olmalıdır.
    - Modeller içinde foreign key bağlamaları varsa, bu foreign key ID'leri ayrıca tekrar yazılmalıdır. Bu işlem CRUD işlemlerinde kolaylık sağlamaktadır.
    - Modeller bittiğinde validation kısımlarının da yazıldığına emin olunulmalıdır.
    - Database üzerine tablolar oluşturulurken tablo isimlerini custom olarak girmek istiyorsak, clas üstüne attribute olarak `[Table(<tablo_adi>)]` yazılabilir.
- Tablolar arasındaki ilişkileri tanımlama
    - Bire bir ilişki – YOK
    - Bire çok ilişki
    - Çoka çok ilişki
- Virtual anlamı ve nullable veri tanımlama

### ADIM 02 - Veritabanı İşlemlerini Yönetecek Sınıfı ( Context ) Oluşturma
- Context sınıfı models dizini altında oluşturulacaksa, `DAL` adlı bir dizin oluşturup içinde oluşturmak, entity modellerle karıştırılmaması açısından daha uygundur. Bunun dışında ayrı bir `class library` projesi eklenerek de context sınıfı burada oluşturulabilir.
- Context sınıfı `DbContext` sınıfından kalıtım almalıdır.
- Context sınıfı oluşturulurken üç adımdan oluştuğuna dikkat edilmelidir.
    1. DB bağlantısını oluşturacak ctor metodun yazılması
    2. İlgili modellerin bağlanması
    3. Bağlantı ayarlarının startup.cs içinde yapılandırılması

#### Ctor Metodunun Oluşturulması ve Modellerin bağlanması
- Database üzerinde tablo olarak bulunmasını istediğimiz ve daha öncesinde tanımladığımız modelleri, Context dosyamız içinde property olarak belirtmemiz lazım.
- Bu property girdilerinin tiplerinin `DbSet<model>` olmasına dikkat edilmelidir.
- Model isimlendirmesi yapılırken, çoğul ifadelerin kullanılması, yazım açısından daha iyi olacaktır.
- Buradaki isimlendirmeler, db üzerindeki tablo isimlendirmesinde kullanılır.

```cs
using Microsoft.EntityFrameworkCore;
using Project.Models.Entities;

namespace Project.Models.DAL
{
    public class ProjectContext : DbContext
    {
        // Database bağlantısının oluşturulması
		public ProjectContext(DbContextOptions<ProjectContext> options)
			:base(options) { }

        // Modellerin tanıtılması
		public DbSet<Person> People { get; set; }
		public DbSet<Address> Addresses { get; set; }
	}
}
```

#### Bağlantı Stringinin Oluşturulması ve Yapılandırılması
- Database bağlantısı için bir `connection string`'e ihtiyacımız vardır.
- MSSQL için bu bağlantı string'i:
    - `"SERVER=<server_adı> ; DATABASE=<db_adı> ; UID=<kullanıcı_adı> ; PWD=<parola>"`
    - Buradaki bilgiler MSSQL kurulurken oluşturulan bilgilerdir.
    - Database girişinde windows auth ile giriş sağlanıyorsa, UID ve PWD yerine `Integrated Security=true` yazılarak giriş yapılabilir.
- `Startup.cs` dosyasına aşağıdaki ayarlamaları giriyoruz.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    var connection = "Server=.;Database=_coreDeneme;UID=sa;PWD=123";
    services.AddDbContext<ProjectContext>(option => option.UseSqlServer(connection));
}
```

### ADIM 03 - Migration İşlemleri
- Migration işlemleri, database ile modellerimizin arasındaki farkın bulunması ve database güncellenmeden önce hangi işlemlerin yapılacağının çözümlendiği işlemdir.
- Migration işlemleri için bir kütüphanenenin kurulması gerekiyor. Bunun için projeye sağ tıklayıp `Edit <project>.csproj` yoluna gelip aşağıdaki kütüphaneyi `ItemGroup` tagları arasına ekliyoruz.
    - `<DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.0"></DotNetCliToolReference>`
- Daha sonrasında projenin ana dizininde `cmd` açılarak veya `Nuget Package Console` ile aşağıdaki kodlar çalıştırılır.
- Yapılacak migration işlemleri ve kodları şu şekildedir : 
    - Yeni bir migration eklemek
        - Komut satırı
            - `dotnet ef migrations add <migration_name>`
        - NPM Console
            - `Add-Migration <migration_name>`
    - Database güncelleme
        - Komut satırı
            - `dotnet ef database update`
        - NPM Console
            - `Update-Database`

### ADIM 04 - Modellere Ait Interface Oluşturulması
- Her tabloya ait CRUD işlemleri için bir interface tanımlanmalıdır.
- Bu tanımlanan interface, kullanılan database teknolojisindan bağımsız olarak, kodlarımızı yazmamızı sağlar.

```cs
public interface IPersonRepository
{
    Person GetById(int personID);
    IQueryable<Person> Persons { get; }
    void CreatePerson(Person person);
    void UpdatePerson(Person person);
    void DeletePerson(int personID);
}
```

### ADIM 05 - Interface'lerden Implament Edilen Repository Oluşturulması
```cs
public class SQLPersonRepository : IPersonRepository
{
    private ProjectContext _context;

    public SQLPersonRepository(ProjectContext context)
    {
        _context = context;
    }

    public Person GetById(int personID)
        => _context.People.FirstOrDefault(k => k.ID == personID);

    public IQueryable<Person> Persons 
        => _context.People;

    public void CreatePerson(Person person)
    {
        _context.People.Add(person);
        _context.SaveChanges();
    }

    public void UpdatePerson(Person person)
    {
        _context.Update(person);
        _context.SaveChanges();
    }

    public void DeletePerson(int personID)
    {
        Person person = GetById(personID);
        _context.Remove(person);
        _context.SaveChanges();
    }
}
```

### ADIM 06 - Dependency Injection Bağlantısı

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Burada bağlantıyı yapıyoruz
    services.AddTransient<IPersonRepository, SQLPersonRepository>();

    var connection = "Server=.;Database=_coreDeneme;UID=sa;PWD=123";
    services.AddDbContext<ProjectContext>(option => option.UseSqlServer(connection));

    services.AddMvc();
}
```