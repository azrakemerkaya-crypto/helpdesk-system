USE HelpdeskDb;
GO

/*
    Teknik Servis ve Ariza Takip Sistemi
    Musteriler -> Cihazlar -> ArizaKayitlari
    Bu dosya mevcut HelpdeskDb yapisini bozmadan yeni tablolar ekler.
*/

IF OBJECT_ID(N'dbo.Musteriler', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Musteriler
    (
        MusteriID INT IDENTITY(1,1) NOT NULL,
        Ad NVARCHAR(50) NOT NULL,
        Soyad NVARCHAR(50) NOT NULL,
        Telefon NVARCHAR(20) NOT NULL,
        AlternatifTelefon NVARCHAR(20) NULL,
        Eposta NVARCHAR(150) NULL,
        Adres NVARCHAR(500) NULL,
        Il NVARCHAR(50) NULL,
        Ilce NVARCHAR(50) NULL,
        KayitTarihi DATETIME2(0) NOT NULL CONSTRAINT DF_Musteriler_KayitTarihi DEFAULT SYSDATETIME(),
        Aktif BIT NOT NULL CONSTRAINT DF_Musteriler_Aktif DEFAULT 1,
        CONSTRAINT PK_Musteriler PRIMARY KEY (MusteriID)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Musteriler_Eposta' AND object_id = OBJECT_ID(N'dbo.Musteriler'))
    CREATE UNIQUE INDEX UX_Musteriler_Eposta ON dbo.Musteriler(Eposta) WHERE Eposta IS NOT NULL;
GO

IF OBJECT_ID(N'dbo.Cihazlar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cihazlar
    (
        CihazID INT IDENTITY(1,1) NOT NULL,
        MusteriID INT NOT NULL,
        CihazTuru NVARCHAR(100) NOT NULL,
        Marka NVARCHAR(100) NOT NULL,
        Model NVARCHAR(100) NULL,
        SeriNo NVARCHAR(100) NULL,
        SatinAlmaTarihi DATE NULL,
        GarantiBitisTarihi DATE NULL,
        CihazDurumu NVARCHAR(30) NOT NULL CONSTRAINT DF_Cihazlar_CihazDurumu DEFAULT N'Aktif',
        Aciklama NVARCHAR(500) NULL,
        KayitTarihi DATETIME2(0) NOT NULL CONSTRAINT DF_Cihazlar_KayitTarihi DEFAULT SYSDATETIME(),
        CONSTRAINT PK_Cihazlar PRIMARY KEY (CihazID),
        CONSTRAINT FK_Cihazlar_Musteriler FOREIGN KEY (MusteriID) REFERENCES dbo.Musteriler(MusteriID),
        CONSTRAINT CK_Cihazlar_CihazDurumu CHECK (CihazDurumu IN (N'Aktif', N'Serviste', N'Teslim Edildi', N'Pasif'))
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Cihazlar_SeriNo' AND object_id = OBJECT_ID(N'dbo.Cihazlar'))
    CREATE UNIQUE INDEX UX_Cihazlar_SeriNo ON dbo.Cihazlar(SeriNo) WHERE SeriNo IS NOT NULL;
GO

IF OBJECT_ID(N'dbo.ArizaKayitlari', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ArizaKayitlari
    (
        ArizaID INT IDENTITY(1,1) NOT NULL,
        CihazID INT NOT NULL,
        ArizaNo AS (N'ARZ-' + RIGHT(N'000000' + CONVERT(NVARCHAR(10), ArizaID), 6)) PERSISTED,
        ArizaBasligi NVARCHAR(200) NOT NULL,
        ArizaAciklamasi NVARCHAR(MAX) NOT NULL,
        ServisNotu NVARCHAR(MAX) NULL,
        Oncelik NVARCHAR(20) NOT NULL CONSTRAINT DF_ArizaKayitlari_Oncelik DEFAULT N'Orta',
        Durum NVARCHAR(30) NOT NULL CONSTRAINT DF_ArizaKayitlari_Durum DEFAULT N'Kayit Acildi',
        KayitTarihi DATETIME2(0) NOT NULL CONSTRAINT DF_ArizaKayitlari_KayitTarihi DEFAULT SYSDATETIME(),
        KabulTarihi DATETIME2(0) NULL,
        TahminiTeslimTarihi DATE NULL,
        TamamlanmaTarihi DATETIME2(0) NULL,
        TeslimTarihi DATETIME2(0) NULL,
        IscilikUcreti DECIMAL(12,2) NOT NULL CONSTRAINT DF_ArizaKayitlari_IscilikUcreti DEFAULT 0,
        ParcaUcreti DECIMAL(12,2) NOT NULL CONSTRAINT DF_ArizaKayitlari_ParcaUcreti DEFAULT 0,
        IndirimTutari DECIMAL(12,2) NOT NULL CONSTRAINT DF_ArizaKayitlari_IndirimTutari DEFAULT 0,
        ToplamTutar AS (CASE WHEN IscilikUcreti + ParcaUcreti - IndirimTutari < 0 THEN 0 ELSE IscilikUcreti + ParcaUcreti - IndirimTutari END) PERSISTED,
        GarantiKapsaminda BIT NOT NULL CONSTRAINT DF_ArizaKayitlari_GarantiKapsaminda DEFAULT 0,
        Aciklama NVARCHAR(1000) NULL,
        CONSTRAINT PK_ArizaKayitlari PRIMARY KEY (ArizaID),
        CONSTRAINT FK_ArizaKayitlari_Cihazlar FOREIGN KEY (CihazID) REFERENCES dbo.Cihazlar(CihazID),
        CONSTRAINT CK_ArizaKayitlari_Oncelik CHECK (Oncelik IN (N'Dusuk', N'Orta', N'Yuksek', N'Acil')),
        CONSTRAINT CK_ArizaKayitlari_Durum CHECK (Durum IN (N'Kayit Acildi', N'Inceleniyor', N'Parca Bekleniyor', N'Tamir Ediliyor', N'Tamir Tamamlandi', N'Musteri Onayi Bekleniyor', N'Teslim Edildi', N'Iptal Edildi')),
        CONSTRAINT CK_ArizaKayitlari_Ucretler CHECK (IscilikUcreti >= 0 AND ParcaUcreti >= 0 AND IndirimTutari >= 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ArizaKayitlari_ArizaNo' AND object_id = OBJECT_ID(N'dbo.ArizaKayitlari'))
    CREATE UNIQUE INDEX UX_ArizaKayitlari_ArizaNo ON dbo.ArizaKayitlari(ArizaNo);
GO

CREATE OR ALTER VIEW dbo.vw_ArizaDetaylari
AS
SELECT a.ArizaID, a.ArizaNo, a.ArizaBasligi, a.ArizaAciklamasi, a.Oncelik, a.Durum,
       a.KayitTarihi, a.ToplamTutar, c.CihazID, c.CihazTuru, c.Marka, c.Model, c.SeriNo,
       m.MusteriID, m.Ad + N' ' + m.Soyad AS MusteriAdi, m.Telefon, m.Eposta
FROM dbo.ArizaKayitlari a
INNER JOIN dbo.Cihazlar c ON c.CihazID = a.CihazID
INNER JOIN dbo.Musteriler m ON m.MusteriID = c.MusteriID;
GO
