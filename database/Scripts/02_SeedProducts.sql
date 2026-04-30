-- Script para popular dados iniciais de produtos
-- Executar apos 01_CreateDatabase.sql

USE TLExemplo;

GO

-- Limpar dados existentes (opcional - comentar se nao quiser)
-- DELETE FROM Produtos;

-- Verificar se ja existem dados
IF (SELECT COUNT(*) FROM Produtos) = 0
BEGIN
    PRINT 'Inserindo dados de exemplo...';

    INSERT INTO Produtos (Nome, Descricao, Preco, QuantidadeEstoque, Ativo, CriadoEm)
    VALUES
    ('Notebook Dell XPS 13', 'Notebook ultraportatil com processador Intel i7, 16GB RAM, 512GB SSD.', 4299.99, 15, 1, GETUTCDATE()),
    ('Mouse Logitech MX Master 3', 'Mouse sem fio com rastreamento precisao 4K, 8 botoes personalizaveis.', 329.99, 45, 1, GETUTCDATE()),
    ('Teclado Mecanico Corsair K95 Platinum', 'Teclado mecanico RGB com switches Cherry MX, aluminio anodizado.', 899.99, 12, 1, GETUTCDATE()),
    ('Monitor LG UltraWide 34 polegadas', 'Monitor ultrawide curvado 3440x1440, 144Hz, DisplayPort, HDMI.', 1999.99, 8, 1, GETUTCDATE()),
    ('Webcam Logitech C920', 'Webcam Full HD 1080p, foco automatico, microfone integrado.', 249.99, 32, 1, GETUTCDATE()),
    ('Headset SteelSeries Arctis 7', 'Headset wireless 2.4GHz, som surround 7.1, microfone ClearCast.', 649.99, 20, 1, GETUTCDATE()),
    ('SSD NVMe Samsung 970 EVO Plus 1TB', 'SSD NVMe 1TB, leitura ate 4.2GB/s, escrita ate 3.5GB/s.', 299.99, 50, 1, GETUTCDATE()),
    ('Memoria RAM Corsair Vengeance RGB 32GB', 'Kit 2x16GB DDR4 3600MHz, latencia CAS 18, iluminacao RGB.', 599.99, 25, 1, GETUTCDATE()),
    ('Fonte Modular Corsair RM850x 850W', 'Fonte modular 850W, certificado Gold, ventilador silencioso.', 499.99, 18, 1, GETUTCDATE()),
    ('Case NZXT H510 Flow', 'Case mid-tower com painel temperado, fluxo de ar otimizado.', 329.99, 22, 1, GETUTCDATE()),
    ('Monitor BenQ EW2480', 'Monitor IPS 24 polegadas, 1920x1080, sem flickering.', 399.99, 30, 1, GETUTCDATE()),
    ('Mousepad SteelSeries QcK', 'Mousepad de tecido 320x270mm, superficie otimizada para jogos.', 79.99, 60, 1, GETUTCDATE());

    PRINT 'Dados inseridos com sucesso! Total: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' produtos';
END
ELSE
BEGIN
    DECLARE @totalProdutos INT = (SELECT COUNT(*) FROM Produtos);
    PRINT 'Tabela Produtos ja contem dados. Total: ' + CAST(@totalProdutos AS NVARCHAR(10)) + ' produtos';
END

GO

-- Exibir dados inseridos
SELECT
    Id,
    Nome,
    Preco,
    QuantidadeEstoque,
    Ativo,
    CriadoEm
FROM Produtos
ORDER BY CriadoEm DESC;
