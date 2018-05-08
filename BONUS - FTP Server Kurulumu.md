## FTP SERVER KURULUMU (Google Cloud)

#### ADIM 1 - Google Cloud Hesabından Yeni Bir Sanal Makine Oluşturma ve Bağlanma

- Google Cloud hesabına giriş yapılıp yeni bir proje eklenir.
- `Compute Engine` > `Sanal makine örnekleri` yolundan yeni bir sanal makine eklenir.
    - `Ubuntu 16.04 LTS` versiyonunu seçiyoruz.
    - Giriş için SSH key aktifleştirmesi yapılabilir.
- Kurulum yapıldıktan sonra çıkan ekrandaki `Harici IP` bizim ssh bağlantısını yapacağımız IP adresidir.
- SSH ile giriş yapıyoruz.

```
$ ssh -i .ssh/id_rsa ssonmez@<IP_Adresi>
```

#### ADIM 2 - Kurulum Yapma

- Sistem üzerinde güncelleştirmeleri kontrol ediyoruz.

```
$ sudo apt-get update
$ sudo apt-get upgrade
```

- FTP server kurulumunu gerçekleştiriyoruz

```
$ sudo apt-get install vsftpd -y
```

#### ADIM 3 - Local Firewall Kuralları

- FTP bağlantısında hata olmaması için, server üzerinde bulunan firewall kısmının kapalı veya gerekli izinleri barındırmış olması gerekmektedir.
- Firewall durumunu kontrol etmek için :

```
$ sudo ufw status
```

```bash
# OUTPUT : Firewall kapalı

Status: inactive

# OUTPUT : Firewall açık

Status: active

To Action  From
-- ------  ----
OpenSSH ALLOW   Anywhere
OpenSSH (v6)   ALLOW   Anywhere (v6)
```

- Firewall üzerine kural eklemek için şu komutları yazıyoruz:

```bash
$ sudo ufw allow 20/tcp             # for FTP
$ sudo ufw allow 21/tcp             # for FTP
$ sudo ufw allow 990/tcp            # for TSL
$ sudo ufw allow 40000:50000/tcp    # for Passive Ports
$ sudo ufw status

Status: active

To                         Action      From
--                         ------      ----
OpenSSH                    ALLOW       Anywhere
990/tcp                    ALLOW       Anywhere
20/tcp                     ALLOW       Anywhere
21/tcp                     ALLOW       Anywhere
40000:50000/tcp            ALLOW       Anywhere
OpenSSH (v6)               ALLOW       Anywhere (v6)
20/tcp (v6)                ALLOW       Anywhere (v6)
21/tcp (v6)                ALLOW       Anywhere (v6)
990/tcp (v6)               ALLOW       Anywhere (v6)
40000:50000/tcp (v6)       ALLOW       Anywhere (v6)
```

#### ADIM 4 - Yeni FTP Kullanıcısı Ekleme

- Yeni bir kullanıcı ekleyip bu kullanıcı için bir ftp dizini ayarlıyoruz.

```bash
# Yeni kullanıcı ekleme
$ sudo adduser <username>
```

```bash
# Kullanıcı için ftp dizini oluşturma
$ sudo mkdir /home/<username>/ftp
$ sudo chown nobody:nogroup /home/<username>/ftp
$ sudo chmod a-w /home/<username>/ftp
```

- Son durumda ftp dizininin izinleri aşağıdaki gibi olmalıdır.

```bash
$ sudo ls -la /home/<username>/ftp

total 8
4 dr-xr-xr-x  2 nobody nogroup 4096 Aug 24 21:29 .
4 drwxr-xr-x 3 <username>  <username>   4096 Aug 24 21:29 ..
```

- Daha sonrasında ftp içinde dosyaların yükleneceği ve kullanıcının erişiminin olduğu bir dizin oluşturmamız gerekecektir.

```bash
$ sudo mkdir /home/<username>/ftp/files
$ sudo chown <username>:<username> /home/<username>/ftp/files
```

- Son durumda son eklediğimiz `files` adlı dizinin izinleri şu şekilde olmalıdır.

```bash
$ sudo ls -la /home/<username>/ftp

total 12
dr-xr-xr-x 3 nobody nogroup 4096 Aug 26 14:01 .
drwxr-xr-x 3 <username>  <username>   4096 Aug 26 13:59 ..
drwxr-xr-x 2 <username>  <username>   4096 Aug 26 14:01 files
```

#### ADIM 5 - FTP Ayarlarının Yapılması

- FTP programının ayar dosyaları `/etc/vsftpd.conf` altında bulunmaktadır.
- Öncelikle bu dosyayı yeniden oluşturmak için eskisinin ismini değiştiriyoruz.

```bash
$ sudo mv /etc/vsftpd.conf /etc/vsftpd.conf.orig
```

- Sonrasında bu dosyayı düzenlenebilir halde yeniden açıyoruz.

```bash
$ sudo nano /etc/vsftpd.conf
```

- Açılan pencerede aşağıdaki satıları ekleyip kaydediyoruz.

```bash
# FTP aktifleştirme
listen=YES

# Bilinmeyen kaynaklara erişimi kapatma
anonymous_enable=NO

# Lokal kaynaklara erişimi açma
local_enable=YES
write_enable=YES
chroot_local_user=YES

# FTP dosya yolu
user_sub_token=$USER
local_root=/home/$USER/ftp

# İzin verilern kullanıcıların çekileceği dosya yolu
userlist_enable=YES
userlist_file=/etc/vsftpd.userlist
userlist_deny=NO

# Passive Port Ayarlamaları
# Buraya hostun public ip adresinin girilmesi gerekmektedir.
pasv_enable=YES
pasv_min_port=40000
pasv_max_port=50000
pasv_address=<ip_address>
```

- Bu ayarları yaptıktan sonra daha önce FTP için oluşturduğumuz kullanıcı adını, FTP erişimine izin vermek için oluşturacağımız yeni bir dosyanın içine ekliyoruz.

```
$ echo "<username>" | sudo tee -a /etc/vsftpd.userlist
```

- Eklediğimiz kullanıcı adının istediğimiz şekilde eklendiğinden emin olmamız gerekmektedir.

```bash
$ cat /etc/vsftpd.userlist
# Çıktı
serhat
```

- Tüm işlemler düzgün ilerlediyse, ftp server'ı baştan başlatmamız gerekmektedir.

```
$ sudo systemctl restart vsftpd
```

#### ADIM 6 - Google Cloud Firewall Ayarlarının Yapılması

- Lokal firewall ayarlarının yanında, Google Cloud Firewall ayarlarından da kullanacağımız portlara izin vermemiz gerekmektedir.
- `VPS Ağı > Güvenlik duvarı kuralları` sekmesinden aşağıdaki ayarlamaları yapıyoruz.

```
AD          :   FTP-Rule-1
Ag          :   Default
Öncelik     :   1000
Yön         :   Giriş
Eylem       :   İzin Ver
Hedefler    :   Ağdaki tüm örnekler
Kaynak filt :   IP Aralıkları
IP Aralık   :   0.0.0.0/0
Protokoller :   tcp:21
---
AD          :   FTP-Rule-2
Ag          :   Default
Öncelik     :   1000
Yön         :   Giriş
Eylem       :   İzin Ver
Hedefler    :   Ağdaki tüm örnekler
Kaynak filt :   IP Aralıkları
IP Aralık   :   0.0.0.0/0
Protokoller :   tcp:20
---
AD          :   FTP-Rule-TSL
Ag          :   Default
Öncelik     :   1000
Yön         :   Giriş
Eylem       :   İzin Ver
Hedefler    :   Ağdaki tüm örnekler
Kaynak filt :   IP Aralıkları
IP Aralık   :   0.0.0.0/0
Protokoller :   tcp:990
---
AD          :   FTP-Rule-PassivePorts
Ag          :   Default
Öncelik     :   1000
Yön         :   Giriş
Eylem       :   İzin Ver
Hedefler    :   Ağdaki tüm örnekler
Kaynak filt :   IP Aralıkları
IP Aralık   :   0.0.0.0/0
Protokoller :   tcp:40000-50000
---
```

#### ADIM 7 - FTP Giriş Kontrolü

- Hızlıca ftp kontrolü yapmak için komut satırından `ftp` komutunu kullanabiliriz.
- Buradaki girişte herhangi bir sorun yoksa, Filezilla vb ftp programlarından da giriş yapabilirsiniz.

```bash
# Google Cloud üzerinden aldığımız public IP adres girilecek
$ ftp -p <ip_address>
Connected to <ip_address>.
220 (vsFTPd 3.0.3)

# FTP kullanıcı adı girilecek
Name (<ip_address>:serhat): serhat
331 Please specify the password.

# Parola giriyoruz
Password:
230 Login successful.
Remote system type is UNIX.
Using binary mode to transfer files.

# ls ile dosyaları listeliyoruz.
# Burada bir sorun varsa Passive Mode ayalarları kontrol edilmelidir.
ftp> ls
227 Entering Passive Mode (35,227,17,158,175,27).
150 Here comes the directory listing.
drwxr-xr-x    2 1002     1003         4096 May 08 14:14 files
226 Directory send OK.

# Çıkış yapıyoruz.
ftp> bye
221 Goodbye.

```