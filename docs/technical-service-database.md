# Teknik Servis ve Arıza Takip Sistemi

Bu repository, ASP.NET Core 8 + MSSQL tabanlı teknik servis ve arıza takip uygulamasıdır.

## Eklenen birleşik yapı

- `database/setup.sql`: Mevcut kullanıcı, kategori, talep ve yorum tabloları.
- `database/technical-service-schema.sql`: `Musteriler`, `Cihazlar` ve `ArizaKayitlari` tabloları, ilişkileri, indeksleri ve `vw_ArizaDetaylari` görünümü.
- `wwwroot/styles.css`: Uygulanmış siyah, gri ve bordo arayüz teması.

## MSSQL kurulumu

1. SQL Server / SSMS açın.
2. Önce `database/setup.sql` dosyasını çalıştırın.
3. Ardından `database/technical-service-schema.sql` dosyasını çalıştırın.
4. Uygulamanın `appsettings.json` bağlantı dizesini kendi SQL Server ortamınıza göre güncelleyin.
5. `dotnet restore`, `dotnet build` ve `dotnet run` komutlarını çalıştırın.

Yeni tablo ilişkisi:

```text
Musteriler 1 ---- N Cihazlar 1 ---- N ArizaKayitlari
```
