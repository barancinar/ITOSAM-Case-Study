# 📚 ITOSAM Case Study

Bu proje, **İstanbul Ticaret Odası (İTO)** iş görüşmesi kapsamında verilen Case çalışması için geliştirilmiştir. Proje iki ana modülden oluşmaktadır:

1.  **Veri Kazıma (Data Scraping) Modülü**: Python ile otomatik veri toplama.
2.  **Admin Paneli & Web Uygulaması**: ASP.NET Core MVC ve Onion Architecture ile geliştirilmiş yönetim sistemi.

---

## 🏗️ Bölüm 1: Veri Kazıma (Data Scraping)

Belirlenen e-ticaret sitesinden (bkmkitap.com) kategori bazlı kitap verilerini toplamak için geliştirilmiştir.

-   **Teknoloji:** Python (Selenium)
-   **Kaynak:** `https://www.bkmkitap.com/kategori-listesi`
-   **Hedef Kategoriler:**
    -   En Çok Satanlar (Aksiyon, Tarihi Roman, Araştırma)
    -   Ekonomi (Turizm, Türkiye Ekonomisi)
-   **Toplanan Veriler:**
    -   Kitap Adı, Yazar, Yayınevi
    -   Fiyat, Satış Rakamları
    -   Sayfa Sayısı, Basım Yılı, ISBN, Kağıt Tipi vb.
-   **Çıktı:** `Ito_Case_Kitaplari.xlsx` (Bu dosya Admin paneline import edilmek üzere hazırlanır.)

---

## 💻 Bölüm 2: ASP.NET Core Admin Panel

Toplanan verilerin yönetildiği, güvenli ve modern web arayüzü.

### 🧱 Teknoloji ve Mimari Yığın

-   **Platform:** ASP.NET Core MVC (.NET 9.0)
-   **Veritabanı:** MSSQL Server (Entity Framework Core)
-   **Mimari:** **Onion Architecture** (Soğan Mimarisi)
    -   `Core` (Entities, Interfaces, DTOs)
    -   `Infrastructure` (Data Access, Context)
    -   `Service` (Business Logic, Mapping)
    -   `Web` (Controllers, Views)
-   **Önyüz:** Bootstrap 5, jQuery, **DataTables (Server-side)**, Chart.js

### 🔐 Yetkilendirme ve Roller (Identity)

Projede **Role-Based Authorization (RBAC)** katı bir şekilde uygulanmıştır:

1.  **Admin:** Tam yetkilidir. Kullanıcı ekleme, veri silme (`Delete`) ve tüm veriyi temizleme (`Clear All`) işlemlerini yapabilir.
2.  **Uzman:** Veri ekleme (`Create`), düzenleme (`Edit`) ve Excel yükleme (`Import`) yetkisine sahiptir. Silme işlemi yapamaz.
3.  **Analist / Kullanıcı:** Sadece raporları ve listeleri görüntüleme yetkisine sahiptir.

### ✨ Temel Özellikler

1.  **Excel Entegrasyonu:**
    -   Scraper modülünden çıkan Excel dosyası, Admin panel üzerinden sisteme toplu olarak (Bulk Insert) yüklenir.
2.  **Gelişmiş Listeleme (DataTables):**
    -   **Server-side Processing:** Veriler sunucu taraflı sayfalama, sıralama ve arama ile yönetilir. Milyonlarca kayıt olsa bile performans kaybı yaşanmaz.
    -   **Gelişmiş Filtreleme:** Kategori, Yazar, Fiyat Aralığı (Min-Max) ve Satış Adedi kriterlerine göre detaylı sorgulama yapılabilir.
3.  **Dinamik Dashboard:**
    -   Kullanıcı seçimine göre (Bar, Pie, Line) değişen grafikler.
    -   **Strategy Pattern:** Grafik verileri `IChartStrategy` arayüzü üzerinden esnek bir yapıda sunulur.
4.  **Güvenlik:**
    -   DTO (Data Transfer Object) kullanımı ile API güvenliği sağlanmıştır.
    -   Yetkisiz erişim denemeleri engellenir.

---

## 📂 Proje ve Klasör Yapısı (Onion Architecture)

Mimarinin temiz kod prensiplerine uygunluğu aşağıdaki klasör yapısında görülebilir:

```text
ItoCaseApp/
├── ItoCase.Core/             # 1. Merkez Katman (Bağımlılık Yok)
│   ├── Entities/             # Veritabanı Varlıkları (Book, AppUser)
│   ├── Interfaces/           # Soyutlamalar (IRepository, IService)
│   └── DTOs/                 # Veri Transfer Nesneleri
├── ItoCase.Infrastructure/   # 2. Altyapı Katmanı
│   ├── Data/                 # DbContext Konfigürasyonu
│   ├── Repositories/         # Veri Erişim Kodları (Generic Repo)
│   └── UnitOfWork/           # Transaction Yönetimi
├── ItoCase.Service/          # 3. Servis Katmanı
│   ├── Services/             # İş Mantığı (BookService, UserService)
│   ├── Strategies/           # Grafik Hesaplama Stratejileri (Design Pattern)
│   └── Mappings/             # AutoMapper Profilleri
└── ItoCase.Web/              # 4. Sunum Katmanı
    ├── Controllers/          # İstek Karşılama
    ├── Views/                # Arayüz (.cshtml)
    └── wwwroot/              # CSS/JS Kaynakları
```

---

## 📐 Mimari Kararlar ve Tasarım Desenleri

Projede kullanılan desenlerin **uygulama noktaları (Implementation Points)** aşağıdadır:

1.  **Strategy Pattern (Strateji Deseni):**
    -   **Nerede:** `ItoCase.Service/Strategies`
    -   **Amaç:** Grafik verilerini çekerken _Kategori Bazlı_ veya _Çok Satanlar_ gibi farklı algoritmaları çalışma zamanında (Runtime) değiştirebilmek. `ChartService` sınıfı `IChartStrategy` arayüzünü kullanır.
2.  **Unit of Work & Repository Pattern:**

    -   **Nerede:** `ItoCase.Infrastructure`
    -   **Amaç:** `BookService` içinde birden fazla veritabanı işlemi (Ekleme, Silme) yapılırken, tek bir `SaveChanges` ile transaction bütünlüğü sağlamak.

3.  **Dependency Injection (DI):**
    -   **Nerede:** `ItoCase.Web/Program.cs`
    -   **Amaç:** Katmanlar arası bağımlılığı (Coupling) en aza indirmek için servisler DI Container üzerinden yönetilir.

---

## 🚀 Kurulum ve Çalıştırma

### 1- Scraper (Veri Kazıma)

```bash
cd Scraper
pip install -r requirements.txt
python main.py
```

_Bu işlem sonucunda klasörde bir Excel dosyası oluşacaktır._

### 2- Web Uygulaması

1.  `ItoCase.Web/appsettings.json` dosyasındaki bağlantı dizesini (Connection String) düzenleyin.
2.  Terminali açın ve veritabanını oluşturun:
    ```bash
    dotnet ef database update
    ```
3.  Projeyi ayağa kaldırın:
    ```bash
    dotnet run
    ```

### 🔑 Giriş Bilgileri (Varsayılan)

-   **Admin:** `admin@itocase.com` / `Admin123!`

---

**Geliştirici:** Baran Çınar
