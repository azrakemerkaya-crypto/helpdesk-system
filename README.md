# Helpdesk System

Teknik Servis ve Talep Takip Sistemi, ASP.NET Core 8 + MSSQL ile geliştirilmiş, kullanıcı dostu bir Helpdesk çözümüdür.

## Özellikler
- Kullanıcı kaydı ve güvenli giriş (JWT)
- Roller: Kullanıcı, Teknik Personel, Yönetici
- Talep oluşturma, listeleme, durum güncelleme
- Cihaz seri numarası takibi
- Yorum ekleme
- Dashboard istatistikleri
- Statik web arayüzü

## Çalıştırma

1. SQL Server kurulu olmalıdır.
2. Veritabanı oluştur:

```sql
CREATE DATABASE HelpdeskDb;
```

3. `appsettings.json` içindeki bağlantı dizesini kontrol edin:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

4. Uygulamayı çalıştırın:

```bash
dotnet restore
dotnet build
dotnet run
```

5. Tarayıcıda açın:

```text
http://localhost:PORT
```

## API örnekleri

Kayıt:

```http
POST /api/auth/register
{
  "fullName": "Admin Kullanıcı",
  "email": "admin@example.com",
  "password": "123456"
}
```

Giriş:

```http
POST /api/auth/login
{
  "email": "admin@example.com",
  "password": "123456"
}
```

Talep oluşturma:

```http
POST /api/tickets
Authorization: Bearer <token>
{
  "title": "Yazıcı sorunu",
  "description": "Yazıcı baskı yapmıyor.",
  "categoryId": 1,
  "priority": "High",
  "deviceSerialNumber": "SN-001"
}
```

## Notlar
- İlk kullanıcı varsayılan olarak `User` rolüne sahiptir.
- Admin rolü için veritabanında `Role` alanını `Admin` olarak güncelleyebilirsiniz.
- Statik arayüz app.js üzerinden API'ye bağlanır.
