CREATE DATABASE HelpdeskDb;
GO

USE HelpdeskDb;
GO

IF OBJECT_ID(N'[dbo].[Categories]', N'U') IS NULL
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END;

IF OBJECT_ID(N'[dbo].[Tickets]', N'U') IS NULL
BEGIN
    CREATE TABLE Tickets (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
        Priority NVARCHAR(50) NOT NULL DEFAULT 'Normal',
        CategoryId INT NOT NULL,
        CreatedById INT NOT NULL,
        AssignedToId INT NULL,
        DeviceSerialNumber NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        ClosedAt DATETIME2 NULL,
        CONSTRAINT FK_Tickets_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
        CONSTRAINT FK_Tickets_CreatedBy FOREIGN KEY (CreatedById) REFERENCES Users(Id),
        CONSTRAINT FK_Tickets_AssignedTo FOREIGN KEY (AssignedToId) REFERENCES Users(Id)
    );
END;

IF OBJECT_ID(N'[dbo].[Comments]', N'U') IS NULL
BEGIN
    CREATE TABLE Comments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        UserId INT NOT NULL,
        Text NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Comments_Ticket FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Comments_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name, IsActive) VALUES
    ('Donanım', 1),
    ('Yazılım', 1),
    ('Ağ ve İnternet', 1),
    ('Kullanıcı Hesabı', 1);
END;
GO
