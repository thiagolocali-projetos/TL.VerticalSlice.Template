-- Script para criar tabela RefreshTokens
-- Executar apos 01_CreateDatabase.sql

USE TLVerticalSliceTemplate;

GO

-- Criar tabela RefreshTokens se nao existir
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(MAX) NOT NULL,
        Token NVARCHAR(MAX) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsRevoked BIT NOT NULL DEFAULT 0,
        RevokedAt DATETIME2 NULL,

        CONSTRAINT CK_ExpiresAt CHECK (ExpiresAt > CreatedAt)
    );

    CREATE INDEX IDX_RefreshTokens_UserId ON RefreshTokens(UserId(100));
    CREATE INDEX IDX_RefreshTokens_Token ON RefreshTokens(Token(100));
    CREATE INDEX IDX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);

    PRINT 'Tabela RefreshTokens criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela RefreshTokens ja existe.';
END

GO

