## BAĞLI TABLOLARIN ÇEKİLMESİ İÇİN KULLANILAN ORM DESENLERİ

- Entity Framework Core ile database üzerinde sorgu işlemleri yapılırken, bağlı tablolar otomatik olarak gelmez 
- Kullanılan desenler 3 tanedir:
    - **Eager Loading**
        - Sorgu içinde bağlı tabloların gelmesini sağlar.
    - **Explicit Loading**
        - Bağlı tabloların daha sonra sorgu içine yüklenmesini sağlar
    - **Lazy Loading**
        - Bağlı tabloların sanal olarak yüklenmesini ve istendiğinde dahil edilmesini sağlar.

### 01 - Eager Loading

#### Bağlı tabloyu eklemek

- Sorgu içine bağlı tabloları da dahil etmek için `Include` fonksiyonu kullanılır.
- Bu fonksiyon .NET Core üzerinde yazılırken otomatik olarak çıkmayacağından aşağıdaki using ifadesine ihtiyaç duyar.
    - `using Microsoft.EntityFrameworkCore;`

```cs
using (var context = new BloggingContext())
{
    var blogs = context.Blogs
        .Include(blog => blog.Posts)
        .ToList();
}
``` 

- Birden fazla `Include()` ifadesi aynı sorgu içinde kullanılabilir.

```cs
using (var context = new BloggingContext())
{
    var blogs = context.Blogs
        .Include(blog => blog.Posts)
        .Include(blog => blog.Owner)
        .ToList();
}
```

#### Bağlı tabloların bağlı tablolarını eklemek

- Sorgumuzu çektiğimiz tablonun bağlı tablolarının da bağlı olduğu tabloları çekmek istiyorsak, kullanmamız gereken yapı `ThenInclude()` metodudur.

```cs
using (var context = new BloggingContext())
{
    var blogs = context.Blogs
        .Include(blog => blog.Posts)
            .ThenInclude(post => post.Author)
        .ToList();
}
```

- İstenilen tablo yapısına ulaşana kadar, istenilen kadar `ThenInclude()` metodu kullanılabilir.

```cs
using (var context = new BloggingContext())
{
    var blogs = context.Blogs
        .Include(blog => blog.Posts)
            .ThenInclude(post => post.Author)
                .ThenInclude(author => author.Photo)
        .ToList();
}
```

- `Include()` ve `ThenInclude()` metotları birlikte combine edilebilir.

```cs
using (var context = new BloggingContext())
{
    var blogs = context.Blogs
        .Include(blog => blog.Posts)
            .ThenInclude(post => post.Author)
            .ThenInclude(author => author.Photo)
        .Include(blog => blog.Owner)
            .ThenInclude(owner => owner.Photo)
        .ToList();
}
```

#### Sonradan tablo bağlama

- Bir sorgu yaptıktan sonra, bağlı tablolar istenildiği zamanda çekilebilir. Bu duruma `ayrık sorgulama` denir.
- Ayrık sorgulama yapılırken, ihtiyaç olduğu yerde, lazım olan tabloların sorgulaması yapılıp, context nesnesi üzerine `Load()` metoduyla eklenir.
- EF bu eklemeyi otomatik olarak algılayıp tabloları bağlar.

```cs
// Single Query
var addresses = _context.Addresses.Include(k => k.Persons);
foreach (var address in addresses)
{
    foreach (var person in address.Persons)
    {
        liste.Add(person.Name);
    }
}
```

```cs
// Separated Query
var addresses = _context.Addresses;
foreach (var address in addresses)
{
    _context.Persons.Where(k => k.AddressID == address.ID).Load();
    foreach (var person in address.Persons)
    {
        liste.Add(person.Name);
    }
}
```

### 02 - Explicit Loading
- Sorgulama esnasında bağlı tabloların çekilmeyip, gerektiği yerde sorgunun tekrar yapılıp bağlı tabloların çekilmesine dayalı sorgulama biçimidir.
- Eager Loading'teki ayrık sorgulamaya benzer bir yapıda, db üzerine birden fazla sorgu gönderir. Tek farkı kod yapısındadır. 
- EF Core 1.1 sürümü ve sonrasında desteklenmektedir.

```cs
// Example 1
var addresses = _context.Addresses;
foreach (var address in addresses)
{
    _context.Entry(address).Collection(k => k.Persons).Load();
    foreach (var person in address.Persons)
    {
        liste.Add(person.Name);
    }
}
```

```cs
// Example 2
using (var context = new BloggingContext())
{
    var blog = context.Blogs
        .Single(b => b.BlogId == 1);

    context.Entry(blog)
        .Collection(b => b.Posts)
        .Load();

    context.Entry(blog)
        .Reference(b => b.Owner)
        .Load();
}
```

- Explicit Loading ile bağlı tablolar sorgulandığında, sadece sorgu sonrasında geri dönen değerler ram üzerinde depolanır ve tüm bağlı tablo verileri depolanmaz.

```cs
using (var context = new BloggingContext())
{
    var blog = context.Blogs
        .Single(b => b.BlogId == 1);

    var postCount = context.Entry(blog)
        .Collection(b => b.Posts)
        .Query()
        .Count(); // => Bu işlem sonucunda sadece sayısal değer ataması yapılır.
}
```

```cs
using (var context = new BloggingContext())
{
    var blog = context.Blogs
        .Single(b => b.BlogId == 1);

    var goodPosts = context.Entry(blog)
        .Collection(b => b.Posts)
        .Query()
        .Where(p => p.Rating > 3)
        .ToList(); 
    //  =>  Bu işlem sonucunda sadece değeri 3ten büyük 
    //      olan değerler çekilip ram üzerine yazılır.
}
```

### 03 - Lazy Loading
- Sorgu ilk yapıldığında, bağlı tablolar çekilmez.
- Bağlı tablolara ait property'lere ilk ulaşılmaya çalışıldığı zaman, database sorgusu otomatik olarak gerçekleştirilir ve bağlı tablo bilgileri çekilir.
- Bağlı tablolara her ulaşılmaya çalışıldığı ilk sefer, ayrı bir sorgu ile database sorgusu yapılır.
- Lazy loading kullanmak için [Microsoft.EntityFrameworkCore.Proxies](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Proxies/) paketi projeye dahil edilmeli ve `UseLazyLoadingProxies` aktif edilmelidir.

```cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseLazyLoadingProxies()
        .UseSqlServer(myConnectionString);
```

- Veya AddDbContext kullanılıyorsa;

```cs
.AddDbContext<BloggingContext>(
    b => b.UseLazyLoadingProxies()
        .UseSqlServer(myConnectionString));
```

- Lazy loading kullanıldığında tüm bağlı tablolar `virtual` olarak tanımlanmalıdır.

```cs
public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Post> Posts { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public virtual Blog Blog { get; set; }
}
```


### Performans Kıyaslaması
- Genel olarak Eager Loading kullanılması daha uygundur. Çünkü tüm işlemlerin bir database sorgusu üzerinde yapılması, ayrı ayrı database'e bağlanılıp sorgu yapılmasından daha performanslıdır. Bu yüzden bağlı tablolarla fazla işlem yapılmasının gerekli olmadığı durumda bu yöntem tercih edilir.
- Fakat bazı zamanlarda ayrık sorgulama, tek sorgulamadan daha performanslıdır. Özellikle bağlı tabloların hepsini çekmek yerine bir kısmının çekilmesi durumunda veya bağlı tablolar arasındaki ilişkilerin çok karmaşık olduğu durumda bu yöntemin tercih edilmesi daha uygundur.
- Performansın çok kritik olduğu durumlarda bu iki durumda, o anki duruma uygun olarak mix edilir.

### Kaynaklar 
- https://docs.microsoft.com/en-us/ef/core/querying/related-data
- https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/read-related-data