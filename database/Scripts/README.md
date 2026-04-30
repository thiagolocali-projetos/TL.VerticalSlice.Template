# Scripts de Database

Scripts SQL para setup inicial da base de dados TLExemplo.

## Como Usar

### Opção 1: Executar via PowerShell (Recomendado)

```powershell
cd Database\Scripts

# Executar todos os scripts em ordem
.\run-setup.ps1
```

### Opção 2: Executar via SQL Server Management Studio (SSMS)

1. Abrir SSMS e conectar ao SQL Server (localhost,1433)
2. Credenciais: `sa` / `Estudos@2026`
3. Executar scripts na seguinte ordem:
   - `01_CreateDatabase.sql` - Cria banco de dados e tabela Produtos
   - `02_SeedProducts.sql` - Popula dados iniciais
   - `03_CreateHangfireTables.sql` - Info sobre tabelas do Hangfire

### Opção 3: Executar via Docker

```bash
docker exec sqlserver-estudos /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Estudos@2026" -i /scripts/01_CreateDatabase.sql
```

## Estrutura de Tabelas

### Tabela: Produtos

```sql
CREATE TABLE Produtos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(200) NOT NULL,
    Descricao NVARCHAR(MAX) NOT NULL,
    Preco DECIMAL(18, 2) NOT NULL CHECK (Preco >= 0),
    QuantidadeEstoque INT NOT NULL CHECK (QuantidadeEstoque >= 0),
    Ativo BIT NOT NULL DEFAULT 1,
    CriadoEm DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AtualizadoEm DATETIME2 NULL
);

-- Índices
CREATE INDEX IDX_Produtos_Ativo ON Produtos(Ativo);
CREATE INDEX IDX_Produtos_Nome ON Produtos(Nome);
```

## Dados de Exemplo

O script `02_SeedProducts.sql` insere 12 produtos de exemplo:
- Notebook Dell XPS 13
- Mouse Logitech MX Master 3
- Teclado Mecânico Corsair K95
- Monitor LG UltraWide 34"
- E mais 8 produtos...

## Hangfire

As tabelas do Hangfire são criadas **automaticamente** quando a aplicação inicia pela primeira vez.

Script `03_CreateHangfireTables.sql` é fornecido apenas como referência para restauração manual se necessário.

## Resetar Database (Cuidado!)

Para resetar completamente o banco de dados:

```powershell
# Via PowerShell
$connString = "Server=localhost,1433;User Id=sa;Password=Estudos@2026;TrustServerCertificate=true;"
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = $connString
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "DROP DATABASE TLExemplo"
$cmd.ExecuteNonQuery()
$conn.Close()

# Então execute 01_CreateDatabase.sql novamente
```

## Connection String

```
Server=localhost,1433;Database=TLExemplo;User Id=sa;Password=Estudos@2026;TrustServerCertificate=true;
```

## Troubleshooting

### Erro: "Cannot open database 'TLExemplo'"
- Execute `01_CreateDatabase.sql` primeiro
- Verifique se SQL Server está rodando: `docker ps | grep sqlserver`

### Erro: "Login failed for user 'sa'"
- Verifique credenciais em `appsettings.json`
- Reinicie o container: `docker restart sqlserver-estudos`
- Aguarde 30 segundos para SQL Server inicializar completamente

### Erro: "Address already in use (port 1433)"
- Outro SQL Server já está rodando
- Execute `netstat -an | findstr 1433` para verificar
