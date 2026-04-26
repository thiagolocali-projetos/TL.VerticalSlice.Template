-- ============================================================
-- Script de criação do banco de dados e tabela Produtos
-- TL.Exemplo-VerticalSlice
-- ============================================================

-- 1. Criar o banco de dados (execute conectado ao master)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TLExemploDB')
BEGIN
    CREATE DATABASE TLExemploDB;
    PRINT 'Banco de dados TLExemploDB criado com sucesso.';
END
GO

USE TLExemploDB;
GO

-- 2. Criar a tabela Produtos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Produtos')
BEGIN
    CREATE TABLE Produtos (
        Id                INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Nome              NVARCHAR(150) NOT NULL,
        Descricao         NVARCHAR(500) NOT NULL,
        Preco             DECIMAL(10,2) NOT NULL,
        QuantidadeEstoque INT           NOT NULL DEFAULT 0,
        Ativo             BIT           NOT NULL DEFAULT 1,
        CriadoEm         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        AtualizadoEm     DATETIME2     NULL
    );

    PRINT 'Tabela Produtos criada com sucesso.';
END
GO

-- 3. Inserir dados de exemplo
IF NOT EXISTS (SELECT 1 FROM Produtos)
BEGIN
    INSERT INTO Produtos (Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm)
    VALUES
        ('Notebook Dell XPS 15', 'Notebook premium com tela OLED 15.6", Intel Core i9, 32GB RAM, 1TB SSD.', 12999.99, 10, 1, GETUTCDATE()),
        ('Mouse Logitech MX Master 3', 'Mouse sem fio ergonômico com scroll MagSpeed e conexão Bluetooth.', 549.90, 50, 1, GETUTCDATE()),
        ('Teclado Mecânico Keychron K2', 'Teclado mecânico compacto 75%, switches Red, compatível com Windows e Mac.', 699.00, 30, 1, GETUTCDATE()),
        ('Monitor LG UltraWide 34"', 'Monitor ultrawide 34 polegadas, resolução 3440x1440, 144Hz, IPS.', 3299.00, 8, 1, GETUTCDATE()),
        ('Headset Sony WH-1000XM5', 'Fone over-ear com cancelamento de ruído líder do setor, 30h de bateria.', 1899.90, 20, 0, GETUTCDATE());

    PRINT 'Dados de exemplo inseridos com sucesso.';
END
GO

-- 4. Verificar dados
SELECT * FROM Produtos;
GO
