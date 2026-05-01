-- Script para criar o banco de dados TLVerticalSliceTemplate
-- Executar com credenciais SA do SQL Server

-- Criar banco se nÃ£o existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TLVerticalSliceTemplate')
BEGIN
    CREATE DATABASE TLVerticalSliceTemplate;
    PRINT 'Banco de dados TLVerticalSliceTemplate criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Banco de dados TLVerticalSliceTemplate jÃ¡ existe.';
END

GO

-- Usar o banco criado
USE TLVerticalSliceTemplate;

GO

-- Criar tabela Samples
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Samples')
BEGIN
    CREATE TABLE Samples (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(200) NOT NULL,
        Descricao NVARCHAR(MAX) NOT NULL,
        Preco DECIMAL(18, 2) NOT NULL,
        QuantidadeEstoque INT NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        CriadoEm DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AtualizadoEm DATETIME2 NULL,

        -- Ãndices para melhor performance
        CONSTRAINT CK_Preco CHECK (Preco >= 0),
        CONSTRAINT CK_QuantidadeEstoque CHECK (QuantidadeEstoque >= 0)
    );

    CREATE INDEX IDX_Samples_Ativo ON Samples(Ativo);
    CREATE INDEX IDX_Samples_Nome ON Samples(Nome);

    PRINT 'Tabela Samples criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Samples jÃ¡ existe.';
END

GO

