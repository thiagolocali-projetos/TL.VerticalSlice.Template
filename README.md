# TL.Exemplo-VerticalSlice

API REST de exemplo implementando a arquitetura **Vertical Slice** com .NET 8, MediatR, FluentValidation e Dapper.

---

## 🏗️ Arquitetura

### Vertical Slice
Cada funcionalidade (feature) é organizada verticalmente, contendo tudo o que precisa para funcionar: Command/Query, Validator e Handler no mesmo local. Isso evita o excesso de camadas horizontais e facilita a manutenção e o crescimento do projeto.

```
Feature: CreateProduto
├── CreateProdutoCommand.cs   ← Command + Validator + Handler (tudo junto)
```

### Estrutura de Projetos

```
TL.Exemplo-VerticalSlice/
├── src/
│   ├── TL.Exemplo.API/                    ← Entrada HTTP, Controllers, Middleware
│   │   ├── Features/Produtos/
│   │   │   └── ProdutosController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   └── Program.cs
│   │
│   ├── TL.Exemplo.Application/            ← Regras de negócio, CQRS, Validações
│   │   ├── Common/
│   │   │   ├── Behaviors/ValidationBehavior.cs
│   │   │   ├── Exceptions/Exceptions.cs
│   │   │   └── Models/ApiResponse.cs / ProdutoDto.cs
│   │   └── Features/Produtos/
│   │       ├── Commands/
│   │       │   ├── CreateProduto/CreateProdutoCommand.cs
│   │       │   ├── UpdateProduto/UpdateProdutoCommand.cs
│   │       │   └── DeleteProduto/DeleteProdutoCommand.cs
│   │       └── Queries/
│   │           ├── GetAllProdutos/GetAllProdutosQuery.cs
│   │           └── GetProdutoById/GetProdutoByIdQuery.cs
│   │
│   ├── TL.Exemplo.Domain/                 ← Entidades do domínio (sem dependências)
│   │   └── Entities/Produto.cs
│   │
│   └── TL.Exemplo.Infrastructure/         ← Dapper, SQL Server, Repositórios
│       ├── Data/DbConnectionFactory.cs
│       └── Repositories/ProdutoRepository.cs
│
└── database/
    └── create-database.sql                ← Script de criação do banco
```

---

## 🚀 Como executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local, Docker ou Azure)

### 1. Configurar o banco de dados

Execute o script SQL no seu SQL Server:
```bash
# Via sqlcmd
sqlcmd -S localhost -U sa -P SuaSenhaAqui@123 -i database/create-database.sql
```
Ou abra o arquivo `database/create-database.sql` no SSMS e execute.

### 2. Configurar a connection string

Edite `src/TL.Exemplo.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=SEU_SERVIDOR;Database=TLExemploDB;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
  }
}
```

### 3. Restaurar pacotes e executar

```bash
cd src/TL.Exemplo.API
dotnet restore
dotnet run
```

### 4. Acessar o Swagger

A aplicação abre o Swagger na raiz:
```
https://localhost:7000/
http://localhost:5000/
```

---

## 📋 Endpoints disponíveis

| Método | Rota                       | Descrição                        |
|--------|----------------------------|----------------------------------|
| GET    | `/api/v1/produtos`         | Lista todos os produtos          |
| GET    | `/api/v1/produtos?apenasAtivos=true` | Lista apenas produtos ativos |
| GET    | `/api/v1/produtos/{id}`    | Busca produto por Id             |
| POST   | `/api/v1/produtos`         | Cria um novo produto             |
| PUT    | `/api/v1/produtos/{id}`    | Atualiza um produto              |
| DELETE | `/api/v1/produtos/{id}`    | Remove um produto                |

---

## 📦 Padrão de resposta (ApiResponse\<T\>)

Todas as respostas seguem o mesmo contrato:

```json
{
  "sucesso": true,
  "dados": { ... },
  "mensagem": "Recurso criado com sucesso.",
  "erros": []
}
```

Em caso de erro:
```json
{
  "sucesso": false,
  "dados": null,
  "mensagem": "Erros de validação encontrados.",
  "erros": [
    "O nome é obrigatório.",
    "O preço deve ser maior que zero."
  ]
}
```

---

## ✅ Tratamento de Exceções

| Exceção              | HTTP Status | Quando ocorre                        |
|----------------------|-------------|--------------------------------------|
| `NotFoundException`  | 404         | Recurso não encontrado               |
| `ValidationException`| 400         | Falha na validação FluentValidation  |
| `BusinessException`  | 400         | Regra de negócio violada             |
| Qualquer outra       | 500         | Erro interno inesperado              |

---

## 🛠️ Tecnologias utilizadas

| Pacote                                | Versão  | Função                             |
|---------------------------------------|---------|------------------------------------|
| MediatR                               | 12.3.0  | CQRS / Mediator pattern            |
| FluentValidation                      | 11.9.2  | Validações declarativas            |
| FluentValidation.DependencyInjection  | 11.9.2  | Registro automático de validators  |
| Dapper                                | 2.1.35  | Micro-ORM para SQL Server          |
| Microsoft.Data.SqlClient              | 5.2.2   | Driver SQL Server                  |
| Swashbuckle.AspNetCore                | 6.9.0   | Swagger / OpenAPI                  |

---

## 💡 Exemplo de requisição POST

```json
POST /api/v1/produtos
Content-Type: application/json

{
  "nome": "Câmera Sony ZV-E10",
  "descricao": "Câmera mirrorless compacta ideal para criadores de conteúdo, 24.2MP.",
  "preco": 3299.90,
  "quantidadeEstoque": 15
}
```

---

## 📝 Exemplo de uso com Docker (SQL Server)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SuaSenhaAqui@123" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```
