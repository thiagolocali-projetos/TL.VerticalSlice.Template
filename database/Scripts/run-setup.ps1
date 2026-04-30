param(
    [string]$Server = "localhost,1433",
    [string]$UserId = "sa",
    [string]$Password = "Estudos@2026"
)

Write-Host ""
Write-Host "Setup do Database TL.Exemplo" -ForegroundColor Cyan
Write-Host ""

$masterConnString = "Server=$Server;User Id=$UserId;Password=$Password;TrustServerCertificate=true;Connection Timeout=30;"
$dbConnString = "Server=$Server;Database=TLExemplo;User Id=$UserId;Password=$Password;TrustServerCertificate=true;Connection Timeout=30;"

# Step 1: Create Database
Write-Host "Step 1: Criando Database e Tabelas..." -ForegroundColor Yellow

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $masterConnString
    $conn.Open()

    $createDbScript = Get-Content "01_CreateDatabase.sql" -Raw
    $batches = $createDbScript -split "GO" | Where-Object { $_.Trim().Length -gt 0 }

    foreach ($batch in $batches) {
        if ($batch.Trim().Length -gt 0) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            $cmd.CommandTimeout = 60
            $cmd.ExecuteNonQuery() | Out-Null
        }
    }

    $conn.Close()
    Write-Host "Database criado com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Erro: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Create RefreshTokens Table
Write-Host "Step 2: Criando tabela RefreshTokens..." -ForegroundColor Yellow

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $dbConnString
    $conn.Open()

    $refreshTokenScript = Get-Content "04_CreateRefreshTokenTable.sql" -Raw
    $batches = $refreshTokenScript -split "GO" | Where-Object { $_.Trim().Length -gt 0 }

    foreach ($batch in $batches) {
        if ($batch.Trim().Length -gt 0) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            $cmd.CommandTimeout = 60
            $cmd.ExecuteNonQuery() | Out-Null
        }
    }

    $conn.Close()
    Write-Host "Tabela RefreshTokens criada!" -ForegroundColor Green
}
catch {
    Write-Host "Erro: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Seed Data
Write-Host "Step 3: Populando dados de produtos..." -ForegroundColor Yellow

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $dbConnString
    $conn.Open()

    $seedScript = Get-Content "02_SeedProducts.sql" -Raw
    $batches = $seedScript -split "GO" | Where-Object { $_.Trim().Length -gt 0 }

    foreach ($batch in $batches) {
        if ($batch.Trim().Length -gt 0) {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $batch
            $cmd.CommandTimeout = 60
            $cmd.ExecuteNonQuery() | Out-Null
        }
    }

    $conn.Close()
    Write-Host "Dados populados com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Erro: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setup completo!" -ForegroundColor Green
