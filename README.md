# Helpdesk System

Teknik Servis ve Talep Takip Sistemi, ASP.NET Core 8 Web API ve Microsoft SQL Server kullanılarak hazırlanmıştır.

## Özellikler
- Kullanıcı kaydı ve JWT ile giriş
- Admin, Teknik Personel ve Standart Kullanıcı rolleri
- Talep oluşturma, listeleme ve detay görüntüleme
- Yönetici tarafından teknik personele talep atama
- Talep durumu ve öncelik güncelleme
- Talep yorumları
- Temel dashboard istatistikleri
- Entity Framework Core ile SQL Server erişimi

## Çalıştırma

1. `appsettings.json` içindeki SQL Server bağlantısını düzenleyin.
2. Terminalde proje klasöründe çalıştırın:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

EF aracı yoksa:

```bash
dotnet tool install --global dotnet-ef
```

İlk kullanıcıyı `/api/auth/register` ile oluşturabilirsiniz. İlk kullanıcıyı yönetici yapmak için veritabanındaki `Users.Role` değerini `Admin` olarak güncelleyin.

## API uçları
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/tickets`
- `GET /api/tickets/{id}`
- `POST /api/tickets`
- `PUT /api/tickets/{id}/status`
- `PUT /api/tickets/{id}/assign`
- `POST /api/tickets/{id}/comments`
- `GET /api/dashboard/summary`

JWT token aldıktan sonra isteklerde `Authorization: Bearer TOKEN` başlığını kullanın.

## 30 Günlük Dokümantasyon
Günlük çalışma planı `docs/30-GUNLUK-CALISMA-PLANI.md` dosyasındadır.
