-- Script para criar o banco de dados TLExemplo
-- Executar com credenciais SA do SQL Server

-- Criar banco se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TLExemplo')
BEGIN
    CREATE DATABASE TLExemplo;
    PRINT 'Banco de dados TLExemplo criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Banco de dados TLExemplo já existe.';
END

GO

-- Usar o banco criado
USE TLExemplo;

GO

-- Criar tabela Produtos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Produtos')
BEGIN
    CREATE TABLE Produtos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(200) NOT NULL,
        Descricao NVARCHAR(MAX) NOT NULL,
        Preco DECIMAL(18, 2) NOT NULL,
        QuantidadeEstoque INT NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        CriadoEm DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AtualizadoEm DATETIME2 NULL,

        -- Índices para melhor performance
        CONSTRAINT CK_Preco CHECK (Preco >= 0),
        CONSTRAINT CK_QuantidadeEstoque CHECK (QuantidadeEstoque >= 0)
    );

    CREATE INDEX IDX_Produtos_Ativo ON Produtos(Ativo);
    CREATE INDEX IDX_Produtos_Nome ON Produtos(Nome);

    PRINT 'Tabela Produtos criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Produtos já existe.';
END

GO
