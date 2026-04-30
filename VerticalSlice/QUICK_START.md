# Quick Start - Teste Rápido da API

## ⚡ 5 Minutos para Começar

### 1️⃣ Abrir Terminal

```bash
cd src
```

### 2️⃣ Executar a API

```bash
dotnet run --project TL.Exemplo.API
```

Espere até ver:
```
🚀 Iniciando TL.Exemplo API...
```

### 3️⃣ Abrir Swagger

Navegue para: **https://localhost:5001**

Verá a documentação interativa de todos os endpoints.

---

## 🔐 Teste de Autenticação

### Fazer Login

**No Swagger:**
1. Clique em `POST /api/v1/auth/login`
2. Clique em "Try it out"
3. Cole:
```json
{
  "username": "admin",
  "password": "admin123"
}
```
4. Click "Execute"
5. Copie o `accessToken` da resposta

**Ou com curl:**
```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -k
```

---

## 📦 Teste CRUD de Produtos

### Autorizar no Swagger

1. Clique no botão verde "Authorize" no topo
2. Cole: `Bearer <seu-token>`
3. Clique "Authorize"
4. Feche a janela

### Criar Produto

1. `POST /api/v1/produtos` → "Try it out"
2. Request body:
```json
{
  "nome": "Notebook Dell",
  "descricao": "Core i7 16GB",
  "preco": 3500.00,
  "quantidadeEstoque": 5
}
```
3. "Execute"

### Listar Produtos (sem paginação)

1. `GET /api/v1/produtos` → "Execute"

### Listar com Paginação

1. `GET /api/v1/produtos/paged` → "Try it out"
2. Parâmetros:
   - pageNumber: 1
   - pageSize: 20
   - apenasAtivos: true
3. "Execute"

**Resposta:**
```json
{
  "sucesso": true,
  "dados": {
    "items": [...]
    "totalCount": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

### Obter Produto por ID

1. `GET /api/v1/produtos/{id}` → "Try it out"
2. id: 1
3. "Execute"

### Atualizar Produto

1. `PUT /api/v1/produtos/{id}` → "Try it out"
2. id: 1
3. Request body:
```json
{
  "id": 1,
  "nome": "Notebook Dell XPS",
  "descricao": "Core i7 16GB Atualizado",
  "preco": 3800.00,
  "quantidadeEstoque": 3
}
```
4. "Execute"

### Deletar Produto

1. `DELETE /api/v1/produtos/{id}` → "Try it out"
2. id: 1
3. "Execute"

---

## 🏥 Teste Health Checks

```bash
# Health completo
curl https://localhost:5001/health -k

# Readiness (K8s)
curl https://localhost:5001/health/ready -k

# Liveness (K8s)
curl https://localhost:5001/health/live -k
```

---

## 🛡️ Teste Rate Limiting

Fazer 101 requisições em 60 segundos:

```bash
for i in {1..105}; do
  curl -H "Authorization: Bearer <TOKEN>" \
    https://localhost:5001/api/v1/produtos -k
  echo "Requisição $i"
done
```

Na requisição 101+, receberá:
```json
{
  "error": "Rate limit exceeded",
  "message": "Você excedeu o limite de 100 requisições por minuto",
  "retryAfter": 60
}
```

---

## 🧪 Executar Testes

```bash
dotnet test
```

Saída esperada:
```
Passed: 6 tests
Failed: 0 tests
```

---

## 📋 Usuários de Teste

| Username | Password | Roles |
|----------|----------|-------|
| admin | admin123 | Admin, User |
| user | user123 | User |

---

## 🔍 Verificar se Tudo Funciona

### ✅ Checklist

- [ ] API iniciou sem erros
- [ ] Swagger abre em https://localhost:5001
- [ ] Login retorna token
- [ ] Criar produto funciona
- [ ] Listar produtos funciona
- [ ] Paginação funciona
- [ ] Health check retorna Healthy
- [ ] Testes passam (dotnet test)

---

## ⚠️ Troubleshooting

### Erro: "Connection string not found"
- Edite `appsettings.json`
- Adicione sua string de conexão SQL Server

### Erro: "Unable to connect to Redis"
- Redis é opcional para teste
- Ou inicie Redis: `redis-server`

### Erro: "Certificate verification failed"
Use `-k` flag com curl para HTTPS self-signed:
```bash
curl -k https://localhost:5001/health
```

### Erro: "Port 5001 already in use"
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5001
kill -9 <PID>
```

---

## 🚀 Próximos Passos

1. Explore os endpoints no Swagger
2. Execute os testes
3. Leia `README.md` para documentação completa
4. Leia `IMPLEMENTACAO_RESUMO.md` para detalhes técnicos

---

**Divirta-se! 🎉**
