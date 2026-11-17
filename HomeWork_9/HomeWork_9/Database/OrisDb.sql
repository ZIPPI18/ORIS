IF DB_ID(N'OrisDb') IS NULL
    CREATE DATABASE OrisDb;
GO

USE OrisDb;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Age INT NOT NULL
);
GO

INSERT INTO dbo.Users (Name, Email, Age)
VALUES
    (N'Alex', N'alex@example.com', 20),
    (N'Maria', N'maria@example.com', 21),
    (N'Ivan', N'ivan@example.com', 19);
GO

SELECT * FROM dbo.Users;
GO
