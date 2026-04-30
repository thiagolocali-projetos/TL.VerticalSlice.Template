# Refresh Token Implementation Guide

## Visão Geral

O refresh token permite que usuários obtenham novos access tokens sem fazer login novamente. O access token expira em 1 hora, enquanto o refresh token expira em 7 dias.

## Fluxo de Autenticação

```
1. Login
   POST /api/v1/auth/login
   {
     "username": "admin",
     "password": "senha123"
   }
   
   Response:
   {
     "accessToken": "eyJhbGciOiJIUzI1NiIs...",
     "refreshToken": "4j2k3j4l23j4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l",
     "tokenType": "Bearer",
     "expiresIn": 3600
   }

2. Usar Access Token para requisições
   GET /api/v1/produtos
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

3. Quando Access Token expirar, usar Refresh Token
   POST /api/v1/auth/refresh
   {
     "refreshToken": "4j2k3j4l23j4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l"
   }
   
   Response:
   {
     "accessToken": "eyJhbGciOiJIUzI1NiIs...",
     "refreshToken": "8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c",
     "tokenType": "Bearer",
     "expiresIn": 3600
   }

4. Logout (revoga todos os refresh tokens)
   POST /api/v1/auth/logout
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## Endpoints

### POST /api/v1/auth/login
Autentica usuário e retorna access + refresh tokens.

**Request:**
```json
{
  "username": "admin",
  "password": "senha123"
}
```

**Response (200 OK):**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "4j2k3j4l23j4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l",
    "tokenType": "Bearer",
    "expiresIn": 3600
  },
  "message": "Success",
  "isSuccess": true
}
```

### POST /api/v1/auth/refresh
Obtém novo access token usando refresh token.

**Request:**
```json
{
  "refreshToken": "4j2k3j4l23j4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l4k2j3l"
}
```

**Response (200 OK):**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c",
    "tokenType": "Bearer",
    "expiresIn": 3600
  },
  "message": "Success",
  "isSuccess": true
}
```

### POST /api/v1/auth/logout
Revoga todos os refresh tokens do usuário.

**Request:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Response (200 OK):**
```json
{
  "data": "Logout realizado com sucesso",
  "message": "Success",
  "isSuccess": true
}
```

## Segurança

### Access Token
- **Duração:** 60 minutos
- **Armazenamento:** Em memória (JavaScript) ou header (requisições HTTP)
- **Revogação:** Imediata ao logout

### Refresh Token
- **Duração:** 7 dias
- **Armazenamento:** Banco de dados (seguro)
- **Revogação:** Ao fazer logout ou quando revogado manualmente
- **Validação:** Verifica se existe no BD, se não foi revogado e se não expirou

## Exemplo com Curl

```bash
# 1. Login
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"senha123"}'

# Copia o refreshToken da response

# 2. Usar access token para requisições
curl -X GET http://localhost:5001/api/v1/produtos \
  -H "Authorization: Bearer <ACCESS_TOKEN>"

# 3. Refresh token (quando access token expirar)
curl -X POST http://localhost:5001/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<REFRESH_TOKEN>"}'

# 4. Logout
curl -X POST http://localhost:5001/api/v1/auth/logout \
  -H "Authorization: Bearer <ACCESS_TOKEN>"
```

## Exemplo com JavaScript

```javascript
let accessToken = null;
let refreshToken = null;

// 1. Login
async function login(username, password) {
  const response = await fetch('http://localhost:5001/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });
  
  const { data } = await response.json();
  accessToken = data.accessToken;
  refreshToken = data.refreshToken;
  
  // Agendar refresh do token antes de expirar
  scheduleTokenRefresh();
}

// 2. Requisição autenticada
async function getProducts() {
  const response = await fetch('http://localhost:5001/api/v1/produtos', {
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });
  return response.json();
}

// 3. Refresh token automaticamente
async function refreshAccessToken() {
  const response = await fetch('http://localhost:5001/api/v1/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  const { data } = await response.json();
  accessToken = data.accessToken;
  refreshToken = data.refreshToken;
  
  scheduleTokenRefresh();
}

// 4. Agendar refresh 5 minutos antes de expirar
function scheduleTokenRefresh() {
  const expiresIn = 3600; // 1 hora em segundos
  const refreshIn = (expiresIn - 300) * 1000; // 55 minutos em ms
  
  setTimeout(refreshAccessToken, refreshIn);
}

// 5. Logout
async function logout() {
  await fetch('http://localhost:5001/api/v1/auth/logout', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });
  
  accessToken = null;
  refreshToken = null;
}

// Uso
await login('admin', 'senha123');
await getProducts();
```

## Database

### Tabela RefreshTokens

```sql
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(MAX) NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2 NULL
);
```

## Tratamento de Erros

### Refresh Token Inválido
```json
{
  "errors": ["Refresh token invalido ou expirado."],
  "message": "Validation failed",
  "isSuccess": false
}
```

### Refresh Token Expirado
```json
{
  "errors": ["Refresh token invalido ou expirado."],
  "message": "Validation failed",
  "isSuccess": false
}
```

### Usuario não encontrado
```json
{
  "errors": ["Usuario nao encontrado."],
  "message": "Validation failed",
  "isSuccess": false
}
```

## Limpeza de Tokens Expirados

Para limpar tokens expirados do banco de dados (agendar em background job):

```csharp
await _refreshTokenRepository.DeleteExpiredAsync();
```

Ou via SQL:

```sql
DELETE FROM RefreshTokens WHERE ExpiresAt < GETUTCDATE();
```

## Fluxo Recomendado para Frontend

1. Armazene accessToken em memória (não localStorage)
2. Armazene refreshToken em HTTPOnly cookie ou sessionStorage
3. Configure interceptor HTTP para fazer refresh automaticamente
4. Lide com erros 401 fazendo refresh e tentando novamente
5. Se refresh falhar (401), redirecione para login

## Segurança em Produção

- [ ] Usar HTTPS sempre
- [ ] Armazenar refreshToken em HTTPOnly, Secure, SameSite cookies
- [ ] Implementar CSRF protection
- [ ] Rate limit nos endpoints de auth
- [ ] Rotação automática de refresh tokens
- [ ] Limpeza periódica de tokens expirados
- [ ] Auditoria de access/refresh
- [ ] Implementar revogação de tokens por dispositivo/sessão
