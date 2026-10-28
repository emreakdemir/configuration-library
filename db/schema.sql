-- HotConfig admin API: MySQL 8 schema. Run once against an empty database.
CREATE TABLE IF NOT EXISTS Configuration
(
    Id              INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ApplicationName VARCHAR(100) NOT NULL,
    Name            VARCHAR(100) NOT NULL,
    Type            VARCHAR(50)  NOT NULL,
    Value           VARCHAR(500) NOT NULL,
    IsActive        BIT(1)       NOT NULL DEFAULT b'1',
    -- Redis keeps one field per (application, lower-case name), so the pair must be unique.
    -- The default collation is case-insensitive, which matches the lower-casing.
    CONSTRAINT UX_Configuration_Application_Name UNIQUE (ApplicationName, Name)
) CHARACTER SET utf8mb4;
