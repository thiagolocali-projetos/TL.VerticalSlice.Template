# TL.Exemplo - Vertical Slice API

API RESTful desenvolvida em .NET 8 com arquitetura Vertical Slice, CQRS e padrões modernos.

## 🚀 Início Rápido

### Pré-requisitos
- .NET 8 SDK
- SQL Server (LocalDB ou servidor)
- Redis (para cache)

### Configuração

1. **Configurar a string de conexão** em `src/TL.Exemplo.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=TLExemplo;Trusted_Connection=true;",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-super-segura-com-mais-de-32-caracteres!!!",
    "Issuer": "TL.Exemplo.API",
    "Audience": "TL.Exemplo.Users"
  }
}
```

2. **Executar a aplicação**
```bash
cd src
dotnet run --project TL.Exemplo.API
```

A API estará disponível em `https://localhost:5001`

## ✨ Fase 1 e Fase 2 Implementadas

### ✅ FASE 1: Urgente
- ✅ Remover código de debug
- ✅ Autenticação JWT com login
- ✅ Proteção de endpoints com [Authorize]
- ✅ Testes unitários (xUnit)

### ✅ FASE 2: Importante
- ✅ Paginação (GET /api/v1/produtos/paged)
- ✅ Health checks (/health, /health/ready, /health/live)
- ✅ Rate limiting (100 req/min por IP)

## 🔐 Autenticação

### Login
```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

### Usuários de Teste
- **Admin**: `admin` / `admin123`
- **User**: `user` / `user123`

### Usar o Token
```bash
curl -H "Authorization: Bearer <seu-token>" \
  https://localhost:5001/api/v1/produtos
```

## 📄 Endpoints Principais

### Autenticação
- `POST /api/v1/auth/login` - Login

### Produtos
- `GET /api/v1/produtos` - Listar todos
- `GET /api/v1/produtos/paged?pageNumber=1&pageSize=20` - Com paginação
- `GET /api/v1/produtos/{id}` - Obter por ID
- `POST /api/v1/produtos` - Criar (requer auth)
- `PUT /api/v1/produtos/{id}` - Atualizar (requer auth)
- `DELETE /api/v1/produtos/{id}` - Deletar (requer auth)

### Health Checks
- `GET /health` - Status completo
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

## 📊 Paginação

```bash
GET /api/v1/produtos/paged?pageNumber=1&pageSize=20&apenasAtivos=true
```

Resposta com: `items`, `pageNumber`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`

## 🏥 Health Checks

```bash
curl https://localhost:5001/health
```

Verifica: SQL Server, Redis, Aplicação

## 🛡️ Rate Limiting

100 requisições por minuto por IP

Status: `429 Too Many Requests`

## 🧪 Testes

```bash
dotnet test
```

Testes incluem:
- CreateProdutoCommandTests
- GetAllProdutosQueryTests
- LoginCommandTests

## 📝 Estrutura

```
src/
├── TL.Exemplo.API/              (Presentation)
│   ├── Features/
│   │   ├── Authentication/      (Login JWT)
│   │   └── Produtos/            (CRUD)
│   ├── Middleware/              (Exception, RateLimiting)
│   └── Extensions/              (Config, HealthChecks)
├── TL.Exemplo.Application/      (CQRS)
│   └── Features/
│       └── Produtos/            (Commands, Queries)
├── TL.Exemplo.Infrastructure/   (BD, Cache, Auth)
├── TL.Exemplo.Domain/           (Entities)
└── TL.Exemplo.Tests/            (xUnit, Moq)
```

## 🔍 Swagger

Acesse: https://localhost:5001

Documenta todos os endpoints e permite testes interativos.

## 📞 Próxima Fase

FASE 3 (Roadmap):
- [ ] Unit of Work Pattern
- [ ] Database Migrations
- [ ] Options Pattern para config
- [ ] Testes de integração

---

**Versão:** 1.0.0  
**Status:** Fase 1 e 2 ✅ Implementadas
