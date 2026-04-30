# Testes de Integração - TL.Exemplo API

## 📋 Visão Geral

Os testes de integração testam a API como um todo, simulando fluxos reais de usuários. Diferente dos testes unitários, estes testam múltiplos componentes trabalhando juntos.

## 🗂️ Estrutura dos Testes

```
Integration/
├── IntegrationTestFixture.cs          (Base/Fixture compartilhada)
├── AuthenticationIntegrationTests.cs  (Login e autenticação)
├── ProdutosIntegrationTests.cs        (CRUD de produtos)
├── PaginationIntegrationTests.cs      (Paginação)
├── HealthCheckIntegrationTests.cs     (Health checks)
└── RateLimitingIntegrationTests.cs    (Rate limiting)
```

## 🚀 Como Executar

### Todos os testes de integração:
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Teste específico:
```bash
dotnet test --filter "FullyQualifiedName~AuthenticationIntegrationTests"
```

### Com verbosidade:
```bash
dotnet test --filter "FullyQualifiedName~Integration" -v detailed
```

### Apenas integração (não unitários):
```bash
cd src
dotnet test TL.Exemplo.Tests --filter "FullyQualifiedName~IntegrationTests"
```

## 📝 Detalhes dos Testes

### 1. IntegrationTestFixture

**Propósito:** Base compartilhada para todos os testes de integração.

**Responsabilidades:**
- Criar e gerenciar WebApplicationFactory
- Fazer login e obter tokens JWT
- Fornecedores de métodos para requisições autenticadas
- Limpar contexto entre testes

**Métodos principais:**
```csharp
await _fixture.LoginAsync("admin", "admin123");           // Fazer login
_fixture.SetAuthToken(token);                              // Definir token manualmente
_fixture.ClearAuthToken();                                 // Limpar token
await _fixture.GetAuthenticatedAsync(url);                // GET autenticado
await _fixture.PostAuthenticatedAsync(url, data);         // POST autenticado
await _fixture.PutAuthenticatedAsync(url, data);          // PUT autenticado
await _fixture.DeleteAuthenticatedAsync(url);             // DELETE autenticado
```

### 2. AuthenticationIntegrationTests

**O que testa:**
- ✅ Login com credenciais válidas
- ✅ Login com senha inválida
- ✅ Login com usuário inexistente
- ✅ Validação de entrada (username vazio)
- ✅ Acesso sem token
- ✅ Acesso com token válido
- ✅ Acesso com token inválido

**Total:** 8 testes

### 3. ProdutosIntegrationTests

**O que testa:**
- ✅ Criar produto com dados válidos
- ✅ Criar produto com dados inválidos
- ✅ Listar todos os produtos
- ✅ Obter produto por ID (válido e inválido)
- ✅ Atualizar produto
- ✅ Deletar produto
- ✅ Acesso sem autenticação
- ✅ Fluxo completo (Create → Read → Update → Delete)

**Total:** 8 testes

### 4. PaginationIntegrationTests

**O que testa:**
- ✅ Parâmetros padrão
- ✅ Tamanho de página customizado
- ✅ Limite máximo (100 itens)
- ✅ Número de página inválido
- ✅ Metadados inclusos
- ✅ Filtro apenas ativos
- ✅ Próxima página correta
- ✅ Última página sem próxima
- ✅ Sem autenticação

**Total:** 9 testes

### 5. HealthCheckIntegrationTests

**O que testa:**
- ✅ GET /health retorna 200 OK
- ✅ Health check inclui status
- ✅ GET /health/ready
- ✅ GET /health/live
- ✅ Não requer autenticação
- ✅ Inclui timestamp
- ✅ Formato JSON válido

**Total:** 7 testes

### 6. RateLimitingIntegrationTests

**O que testa:**
- ✅ Requisições dentro do limite
- ✅ Requisições acima do limite são rejeitadas
- ✅ Status 429 quando excedido
- ✅ Mensagem de erro incluída
- ✅ Informação de retry-after
- ✅ Rate limiting é por IP

**Total:** 6 testes

## 📊 Resumo

| Classe | Testes | Cobertura |
|--------|--------|-----------|
| AuthenticationIntegrationTests | 8 | Login, Auth, Tokens |
| ProdutosIntegrationTests | 8 | CRUD completo |
| PaginationIntegrationTests | 9 | Paginação |
| HealthCheckIntegrationTests | 7 | Health checks |
| RateLimitingIntegrationTests | 6 | Rate limiting |
| **Total** | **38** | **Completo** |

## 🔄 Fluxo de Testes

### Estrutura Padrão de Teste:

```csharp
[Fact]
public async Task ShouldDoSomething()
{
    // Arrange - Preparar dados/estado
    await _fixture.LoginAsync();
    
    // Act - Executar ação
    var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
    
    // Assert - Verificar resultado
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## ⚠️ Notas Importantes

### 1. **WebApplicationFactory**
- Cria uma instância real da aplicação para teste
- Inclui todos os middlewares e serviços
- Limpo automaticamente após cada teste

### 2. **Autenticação**
- Usa credenciais reais do repositório em memória
- Admin: `admin` / `admin123`
- User: `user` / `user123`

### 3. **Dados**
- **NÃO** usa banco de dados persistente (recomendável para isolar testes)
- Testes atuais compartilham estado do banco
- Para isolamento completo, considerar usar In-Memory database ou TestContainers

### 4. **Rate Limiting**
- Limite é por IP
- Reseta automaticamente após 1 minuto
- Alguns testes podem falhar se executados em sequência rápida

## 🔧 Troubleshooting

### Testes falhando com "Connection refused"
- **Causa:** SQL Server não está rodando
- **Solução:** Verificar appsettings.json e iniciar SQL Server

### Rate limiting causando falhas
- **Causa:** Limite foi atingido em teste anterior
- **Solução:** Aguardar 1 minuto ou reiniciar a aplicação

### Token expirado
- **Causa:** Teste rodou muito tempo
- **Solução:** JWT tem expiração de 1 hora, deve ser suficiente

### WebApplicationFactory não encontrada
- **Causa:** Program.cs não está acessível
- **Solução:** Verificar referência do projeto de testes

## 🚀 Próximas Melhorias

1. **Isolar banco de dados por teste**
   - Usar TestContainers ou banco em memória
   - Cada teste teria seu próprio schema limpo

2. **Adicionar testes de erro**
   - Testes de validação mais robustos
   - Testes de edge cases

3. **Adicionar testes de performance**
   - Verificar tempo de resposta
   - Verificar consumo de memória

4. **Testes de concorrência**
   - Múltiplas requisições simultâneas
   - Verificar race conditions

5. **Testes de segurança**
   - CORS
   - CSRF
   - SQL Injection

## 📚 Referências

- [xUnit Documentation](https://xunit.net/docs/getting-started/netfx)
- [FluentAssertions](https://fluentassertions.com/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

**Total de Testes de Integração:** 38  
**Status:** ✅ Completo  
**Versão:** 1.0.0
