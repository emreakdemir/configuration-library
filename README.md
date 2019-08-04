# CodeSide Configuration Library

# CodeSide.ConfigurationApi

- api/configuration endpoint üzerinden Configuration kayıtları GET, POST, PUT ve DELETE methodları ile düzenlenebilir, listelenebilir.

- appsettings.json içerisinde MySql ve Redis connection string değerleri set edilmelidir.

# CodeSide.ConfigurationLibrary

- ConfigurationReader class'ı Application, RefreshTime ve ConnectionString bilgileri ile initialize edildiğinde kullanılabilir. Configuration kayıtlarını Redis üzerinde bekletir, belirlenen aralıkta yenileyerek istenilen Type için kullanılmasını sağlar.

# codeside.ui

- Vue.JS ile basit CRUD ekranlarını kapsar. Configuration kayıtlarının listelenmesi, filtrelenmesi, eklenmesi, güncellenmesi ve silinmesi işlemlerini CodeSide.ConfigurationApi üzerinden yapılmasına olanak sağlar.

- Codeside.ConfigurationApi url'i test için https://localhost:5001 olacak şekilde ayarlanmalıdır.

# MySQL Table

-- auto-generated definition
CREATE TABLE Configuration
(
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationName VARCHAR(100)     NOT NULL,
    Name            VARCHAR(100)     NOT NULL,
    Type            VARCHAR(50)      NOT NULL,
    Value           VARCHAR(500)     NOT NULL,
    IsActive        BIT DEFAULT b'1' NULL
);
