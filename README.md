# Fikir Havuzu - Kurumsal İnovasyon Yönetim Platformu

Fikir Havuzu, çalışanların şirket içi yeni fikir ve önerilerini paylaşabildiği, yöneticilerin bu fikirleri puanlayarak değerlendirebildiği ve başarılı bulunan fikirlerin vitrinde sergilendiği bir web uygulamasıdır.

Bu proje, staj çalışması kapsamında ASP.NET Core 8.0 MVC ve 4 katmanlı mimari kullanılarak geliştirilmiştir.

## Mimari Yapı ve Teknolojiler

Proje 4 ana katmandan oluşmaktadır:

- **FikirHavuzu.Entity:** Veritabanı model ve varlık sınıfları (Kullanıcı, Fikir, Kategori, Değerlendirme vb.).
- **FikirHavuzu.DataAccess:** Veritabanı bağlantısı, DbContext ve veri erişim kodları.
- **FikirHavuzu.Business:** İş kuralları, kontroller ve servisler (Kullanıcı Servisi, Fikir Servisi, E-Posta Servisi).
- **FikirHavuzu.Web:** Kullanıcı arayüzü, sayfa kontrolleri (MVC Controller & View).

### Kullanılan Teknolojiler

- C# / .NET 8.0 ASP.NET Core MVC
- MS SQL Server & Entity Framework Core 8.0 (Code-First)
- Custom Cookie Authentication & BCrypt (Güvenli Şifreleme)
- Özel Yetki Filtreleri (Rol Bazlı Erişim)
- SmtpClient (Gmail E-Posta Entegrasyonu)
- Bootstrap 5, HTML5, CSS3, JavaScript

> **Not:** E-posta bildirimlerini test etmek isterseniz, `FikirHavuzu.Web/appsettings.json` dosyasındaki `EmailSettings` alanına Gmail adresinizi ve Uygulama Şifrenizi tanımlayabilirsiniz.

## Hazır Test Hesapları

Veritabanında hazır tanımlanmış test hesapları:

| Rol                      | Sicil No / Giriş Adı | Şifre          | E-Posta                       |
| ------------------------ | -------------------- | -------------- | ----------------------------- |
| **Sistem Yöneticisi**    | `adm001`             | `Password123!` | admin@fikirhavuzu.com         |
| **Personel Kullanıcısı** | `PER260901001`       | `Password123!` | mehmet.yilmaz@fikirhavuzu.com |
| **Personel Kullanıcısı** | `PER260901002`       | `Password123!` | ayse.kaya@fikirhavuzu.com     |
| **Personel Kullanıcısı** | `PER260901003`       | `Password123!` | ali.demir@fikirhavuzu.com     |

_(Diğer tüm personel hesapları `PER260901004` ile `PER260901009` arasında olup hepsinin varsayılan şifresi `Password123!` olarak belirlenmiştir)._

## Temel Özellikler ve Modüller

### 1. Yetki Yönetimi ve Güvenlik Koruması

Kullanıcılara modül bazlı yetkiler tanınır. Yöneticinin kendi yetkisini yanlışlıkla kaldırıp sistemi kilitlemesini önleyen güvenlik mekanizması mevcuttur.

### 2. Otomatik Sicil Numarası ile Çalışan Ekleme

Sisteme yeni personel eklenirken `PER26...` formatında otomatik sicil numarası üretilir.

### 3. Fikir Takip Listesi ve Durum Rozetleri

Eklenen fikirler durumlarına göre (Olumlu, Olumsuz, Taslak, Uygulandı) renkli rozetlerle listelenir ve takip edilir.

### 4. Kullanıcı Profili ve Şifre İşlemleri

Kullanıcılar profil sayfalarında inovasyon puanlarını, aktif yetkilerini görebilir ve şifrelerini güncelleyebilirler.

## Veritabanı Tabloları

Veritabanında (FikirHavuzuDb) ilişkisel 9 temel tablo bulunur:
Users, Permissions, UserPermissions, Categories, Ideas, IdeaDocuments, Evaluations, IdeaEditHistories, Notifications.
