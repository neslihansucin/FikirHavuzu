# Fikir Havuzu 💡

Fikir Havuzu, çalışanların şirket içi yeni fikir ve önerilerini paylaşabildiği, yöneticilerin bu fikirleri puanlayarak değerlendirebildiği ve başarılı bulunan fikirlerin vitrinde sergilendiği bir web uygulamasıdır.

Bu proje staj çalışması kapsamında ilk olarak **ASP.NET Core MVC** ile geliştirilmiş, daha sonrasında modern yazılım geliştirme standartlarına uygun olarak **React (Next.js) + REST API** mimarisine geçirilmiştir.

---

## 🌟 Sistem Rolleri ve Yetkiler

Projede **Role-Based Access Control (RBAC)** mantığıyla çalışan bir yetkilendirme sistemi bulunmaktadır. Temel roller şunlardır:

- **Sistem Yöneticisi:** Tüm panellere tam erişimi olan, kullanıcı tanımlayan ve yetki atayan ana yönetici rolüdür.
- **Fikir Koordinatörü (Jüri):** Sisteme atılan fikirleri değerlendirme, puanlama ve onaylama yetkisine sahip yönetim ekibidir. Tarafsız değerlendirme yapılabilmesi için **fikirleri anonim olarak (fikir sahibinin ismini görmeden)** puanlarlar.
- **Personel Sorumlusu:** Çalışan hesaplarını ve pasif/aktif durumlarını yönetme yetkisine sahip İK/Yönetim rolüdür.
- **Personel (Standart Kullanıcı):** Sisteme giriş yaparak fikir önerebilen, kendi fikirlerinin durumunu takip edebilen standart çalışan rolüdür.

*(Tüm bu yetkiler Sistem Yöneticisi tarafından kullanıcılara dinamik olarak atanıp kaldırılabilmektedir).*

---

## 📸 Projeden Görüntüler

### 1. Sisteme Giriş
Projeye giriş ekranı.

<img width="1919" height="1019" alt="login" src="https://github.com/user-attachments/assets/5e42b956-61fa-4c29-be96-64ecda14738e" />


### 2. Gösterge Paneli (Dashboard)
Yöneticilerin veya personellerin yetkilerine göre dinamik olarak değişen, sistemdeki istatistiklerin tek bakışta göründüğü özet ekranı.

<img width="1919" height="1020" alt="dashboard" src="https://github.com/user-attachments/assets/c52938bf-6e50-44f9-954a-677e86d61609" />


### 3. Yeni Fikir Önerisi
Personellerin yenilikçi fikirlerini, kategorisini belirleyip belgeler ekleyerek havuza attığı form sayfası.

<img width="1919" height="1018" alt="yenifikir" src="https://github.com/user-attachments/assets/ea287568-c883-4c16-98fa-8c683861b66d" />

<img width="1919" height="1017" alt="fikirlerim" src="https://github.com/user-attachments/assets/ef40a33d-2f99-42a4-8d7c-d1399c777c98" />


### 4. Fikir Listeleri ve Yönetimi
Havuzdaki fikirlerin durumlarına (Taslak, Bekleyen, Pırıltılı Fikir vb.) göre takip edildiği ve yöneticiler tarafından değerlendirildiği (onay/red) ekranlar. Jüriler (Koordinatörler) adil bir puanlama için bu listelerde fikir sahiplerini anonim (gizli) olarak görürler. Ancak sistem yöneticileri için böyle bir kısıt yoktur.

<img width="1919" height="1019" alt="fikirler" src="https://github.com/user-attachments/assets/d2b7f439-814f-4ef6-90d5-135fce928c81" />

<img width="1919" height="1018" alt="cekilenfikirler" src="https://github.com/user-attachments/assets/5c7e9607-86ed-44cc-969d-123ddf44227e" />


### 5. Pırıltılı Fikirler Vitrini (İnovasyon Podyumu)
Kuruma değer katan, hayata geçirilmiş fikirlerin sergilendiği ve personellerin topladıkları puanlara göre sıralandığı madalyalı podyum ekranı.

<img width="1919" height="1019" alt="liderlik podyumu" src="https://github.com/user-attachments/assets/277d0659-dc7e-440e-b480-3b9996d98635" />


### 6. Kullanıcı ve Yetki Yönetimi
Sistem yöneticisinin yeni çalışanları ekleyebildiği, hesapları aktif/pasif yapabildiği ve modül bazlı yetkilendirme (Role-Based Access Control) yapabildiği admin paneli.

<img width="1919" height="1018" alt="kullanıcıyönetimi" src="https://github.com/user-attachments/assets/fe3f15e8-3e2e-4ecb-9ff5-86cd9f855c37" />

<img width="1919" height="1020" alt="yetkiyönetimi" src="https://github.com/user-attachments/assets/594c521d-6720-4efe-9633-0329a59ed850" />


### 7. Profil ve Hesap Ayarları
Kullanıcıların kendi yetkilerini görebildiği, profil fotoğraflarını ve şifrelerini güncelleyebildiği hesap sayfası.

<img width="1919" height="1019" alt="profilim" src="https://github.com/user-attachments/assets/0ed2ed68-3195-4d79-a391-0455b69f77b5" />


---

## 🛠 Kullanılan Teknolojiler

**Backend & Veritabanı:**
- C# / .NET 8.0 Web API
- MS SQL Server & Entity Framework Core (Code-First)
- Redis (Önbellekleme)
- Elasticsearch (Arama ve Loglama altyapısı)

**Frontend:**
- React.js (Next.js App Router)
- PrimeReact UI & Sakai Teması
- i18next (Çoklu dil desteği)

**DevOps & Test:**
- Docker (Container altyapısı)
- Jenkins (CI/CD Pipeline)
- SonarQube (Kod kalitesi ve güvenlik analizi)
- Selenium (UI testleri)
