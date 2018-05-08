## DEPLOYMENT

- Core projesinin deployment süreci basit olarak 3 temel adımda gerçekleşir:
    1. Projenin dizine publish edilmesi ve barındıracak host üzerine gönderilmesi.
        - Publish işlemi, projenin derlenip, Release sürümünün statik dosyalarla birlikte bir dizine veya ftp üzerinden belirlenen bir alana çıkarılması işlemidir.
    2. Host üzerinde, servis oluşturulması
        - Dotnet Core projesi, temelinde bir konsol uygulaması olduğundan, server reboot veya programın çökmesi durumda tekrar başlatılması gerekmektedir.
        - Host üzerinde bu işlemler için bir servis yazıp, yukarıdaki durumlarda uygulamamızın tekrar başlatılmasını sağlamamız gerekmektedir.
    3. Reverse Proxy kurulumu
        - Core uygulaması, kendi içinde `Kestel` adlı bir web server bulundurmaktadır.
        - Kestel, tüm web server işlemlerini ve statik dosya yönlendirmesi gibi işlemleri kendi içinde oluşturmaktadır.
        - Bununla birlikte, 5000 portundan dinleme yapan Kestel'i, herhangi bir proxy aracılığıyla istediğimiz bir portu (web için genellikle 80) dinlemeye ayarlamamız gerekmektedir.
        - Bunun için genellikle kullandığımız Reverse Proxy Server'lar:
            - IIS
            - Nginx
            - Apache

### 01 - Projeyi Yayınlama Yöntemleri

Dotnet Core projeleri temel olarak iki şekilde yayınlanabilir:

- **Framework Bağımlı Yayınlama (Framework-dependent deployments - FDD)**
    - Sadece uygulama ve 3. parti bağımlılıkların yayınlanmasına dayanır.
    - .NET Core runtime bağımlılığı vardır, bu yüzden deploy ettiğimiz hedef makinede .NET Core kütüphanesinin ilgili sürümü kurulu olması gerekmektedir.
    - İşletim sistemi belirtmeye gerek yoktur. Deploy ettikten sonra, her işletim kendi işletim sistemine özgü Core runtime üzerinden çalıştırılabilir haldedir.
    - Deployment paketi, sadece gerekli olan frameworkleri barındırdığı için boyutu küçüktür.

- **Framework Bağımsız Yayınlama (Self-contained deployments - SCD)**
    - Uygulama, 3. parti bağımlılıklar ve dotnet core kütüphanelerinin birlikte deploy edilmesi yöntemidir.
    - Hedef kaynakta dotnet core kurulu olmasına gerek yoktur.
    - Dotnet core kütüphaneleri için işletim sistemi altyapıları farklı olduğundan, deploy ederken hangi işletim sistemi için deploy etmemiz gerektiğini belirtmemiz gerekmektedir.
        - [Runtime IDentifier Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)
    - Deployment paketi, FDD'ye göre daha büyük boyutludur.

### 02 - Projenin Yayınlanması

- Proje, 4 farklı şekilde yayınlanabilir:
    - Framework-dependent deployment
    - Framework-dependent deployment with third-party dependencies
    - Self-contained deployment
    - Self-contained deployment with third-party dependencies

#### i. CLI Araçları (Command-Line Interface Tools) ile yayınlama

- Projeyi CLI ile yayınlarken `dotnet publish` komutunu kullanırız.
    - Bu komutun ayrıntılı opsiyonları aşağıdaki gibidir:

```
Usage: dotnet publish [options]

Options:
  -h, --help                            Show help information.
  -o, --output <OUTPUT_DIR>             Output directory in which to place the published artifacts.
  -f, --framework <FRAMEWORK>           Target framework to publish for. The target framework has to be specified in the project file.
  -r, --runtime <RUNTIME_IDENTIFIER>    Publish the project for a given runtime. This is used when creating self-contained deployment. Default is to publish a framework-dependent app.
  -c, --configuration <CONFIGURATION>   Configuration to use for building the project.  Default for most projects is  "Debug".
  --version-suffix <VERSION_SUFFIX>     Defines the value for the $(VersionSuffix) property in the project.
  --manifest <manifest.xml>             The path to a target manifest file that contains the list of packages to be excluded from the publish step.
  --self-contained                      Publish the .NET Core runtime with your application so the runtime doesn't need to be installed on the target machine. Defaults to 'true' if a runtime identifier is specified.
  --no-restore                          Does not do an implicit restore when executing the command.
  -v, --verbosity                       Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
  --no-dependencies                     Set this flag to ignore project to project references and only restore the root project.
  --force                               Set this flag to force all dependencies to be resolved even if the last restore was successful. This is equivalent to deleting project.assets.json.

```

**Framework-dependent deployment**

- Proje bağımlılıklarının güncellenmesi
    - `[proje-adi].csproj` dosyası içinde yazdığımız bağımlılıkların indirilmesi ve güncellenmesi işleminin yapılmasıdır.
    - `dotnet restore` komutu çalıştırılır.
- Projenin derlenmesi
    - `dotnet build` komutu ile projenin son halinin derlenmesi işlemidir.
- Projenin yayınlanması
    - `dotnet publish -f netcoreapp2.0 -c Release -o CorePublish` komutu projenin yayınlanacak dosyalarının oluşturulmasını sağlar.
        - `-o` parametresi ile publish edilecek dosyaların çıkarılacağı konumu belirtiyoruz.
        - `-c` parametresi ile derlenme ayarının belirtiyoruz.
        - `-f` parametresi ile projemizin framework versiyonunu belirtiyoruz.
- Projenin çalıştırılması
    - `dotnet [app_name].dll` komutuyla, ilgili projenin dll dosyası dotnet aracılığıyla çalıştırılır.
    - Proje çalıştırıldığında, `localhost:5000` üzerinden yayın yapmaya başlar.

**Self-contained deployment**

- Projenin hedef platformlarının belirtilmesi
    - `[app_name].csproj` içine hedef platformların belirtilmesi gereklidir.
    - [Tüm platformlar için bakınız...](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

```xml
<PropertyGroup>
    <RuntimeIdentifiers>win10-x64;osx.10.11-x64</RuntimeIdentifiers>
</PropertyGroup>
```

- Proje bağımlılıklarının güncellenmesi
    - `[proje-adi].csproj` dosyası içinde yazdığımız bağımlılıkların indirilmesi ve güncellenmesi işleminin yapılmasıdır.
    - `dotnet restore` komutu çalıştırılır.
- Projenin derlenmesi
    - `dotnet build` komutu ile projenin son halinin derlenmesi işlemidir.
- Projenin yayınlanması
    - `dotnet publish -r win10-x64 -f netcoreapp2.0 -c Release -o CorePublish` 
    - `dotnet publish -r osx.10.11-x64 -f netcoreapp2.0 -c Release -o CorePublish` 
        - `-r` parametresi ile hedef platformu belirtiyoruz.
        - `-o` parametresi ile publish edilecek dosyaların çıkarılacağı konumu belirtiyoruz.
        - `-c` parametresi ile derlenme ayarının belirtiyoruz.
        - `-f` parametresi ile projemizin framework versiyonunu belirtiyoruz.
- Projenin çalıştırılması
    - Hedef platforma özgün olarak çıkarılmış `[app_name]` veya `[app_name].exe` dosyalarından biri çalıştrırılarak programa ulaşım sağlanır.
    - Proje çalıştırıldığında, `localhost:5000` üzerinden yayın yapmaya başlar.

#### ii. Visual Studio ile Yayınlama

**Framework-dependent deployment**

- Projenin derlenmesi
    - Solution üzerine sağ tıklayıp `Build Solution` tıklanır.
- Projenin yayınlanması
    - Araç çubukları üzerinden config kısmı `Debug > Release` olarak değiştirilir.
    - Proje üzerine sağ tıklanıp `Publish` tıklanır.
    - Buradan `Folder Publish` seçilip, dosyaların çıkarılacağı konum belirlenir.
- Projenin çalıştırılması
    - `dotnet [app_name].dll` komutuyla, ilgili projenin dll dosyası dotnet aracılığıyla çalıştırılır.
    - Proje çalıştırıldığında, `localhost:5000` üzerinden yayın yapmaya başlar.

**Self-contained deployment**

- Projenin hedef platformlarının belirtilmesi
    - `[app_name].csproj` içine hedef platformların belirtilmesi gereklidir.
    - Projeye sağ tıklanıp `Edit [app_name].csproj` tıklanır.
    - [Tüm platformlar için bakınız...](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

```xml
<PropertyGroup>
    <RuntimeIdentifiers>win10-x64;osx.10.11-x64</RuntimeIdentifiers>
</PropertyGroup>
```

- Projenin derlenmesi
    - Solution üzerine sağ tıklayıp `Build Solution` tıklanır.
- Projenin yayınlanması
    - Araç çubukları üzerinden config kısmı `Debug > Release` olarak değiştirilir.
    - Proje üzerine sağ tıklanıp `Publish` tıklanır.
    - Buradan `Folder Publish` seçilip, dosyaların çıkarılacağı konum belirlenir.
    - Alt kısımdaki `Publish` kısmının yanındaki ayarlara tıklayarak, `Create Profile` olarak değiştirilir ve tıklanır.
    - Yeni gelen pencereden `Target Location > Settings > Target Runtime` kısmından ilgili platform seçilir.
    - Pencere kapatılıp `Start` butonuna tıklanır.
- Projenin çalıştırılması
    - Hedef platforma özgün olarak çıkarılmış `[app_name]` veya `[app_name].exe` dosyalarından biri çalıştrırılarak programa ulaşım sağlanır.
    - Proje çalıştırıldığında, `localhost:5000` üzerinden yayın yapmaya başlar.

#### iii. Publish Dosyaları

- `[app-name].deps.json` : 
    - Projenin çalışma zamanındaki bağımlılıklarını bulunduran dosyadır.
    - Kompoentleri ve kütüphaneleri bulundurur.
    - Projenin çalışması için zorunlu dosyadır.
    - [Ayrıntılı Bilgi](https://github.com/dotnet/cli/blob/85ca206d84633d658d7363894c4ea9d59e515c1a/Documentation/specs/runtime-configuration-file.md)
- `[app-name].dll` : 
    - Application'ı bulunduran dosyadır.
    - Yayın yapılırken bu dosya çalıştırılır.
- `[app-name].pdb` : 
    - Debug sembollerinin bulundugu dosyadır.
    - Projenin çalışması için zorunlu değildir.
- `[app-name].runtimeconfig.json` : 
    - Uygulamanın çalışma zamanı ayarlarının bulunduğu dosyadır.
    - Projenin yazıldığı dotnet versiyonunu da barındırır.
    - [Ayrıntılı Bilgi](https://github.com/dotnet/cli/blob/85ca206d84633d658d7363894c4ea9d59e515c1a/Documentation/specs/runtime-configuration-file.md)
- `bundleconfig.json`:
    - Bundle configuration ayarlarını bulundurur.
- `appsettings.json`:
    - Bağlantı stringi, log ayaları vb gibi ayaların bulunduğu json dosyasıdır.
- `wwwroot`
    - Statik dosyaların bulundugu dizindir.

### 03 - Linux Server Ayarlamaları (Ubuntu 16.04)

- **Server üzerine .NET Core SDK kurulumu**
    - [Kurulum Adımları](https://www.microsoft.com/net/download/linux-package-manager/ubuntu16-04/sdk-current)
- **FTP Server kurulumu ve dosyaların FTP üzerinden Hosta yüklenmesi**