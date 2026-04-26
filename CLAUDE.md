# RVM.ShopEngine

## Visao Geral
Plataforma de e-commerce com catalogo de produtos, gerenciamento de carrinho, checkout e ciclo completo de pagamento. Interface Blazor Server para administracao e endpoints REST para integracoes. `OrderService` coordena o fluxo de pedido; `PaymentService` gerencia status de pagamento.

Projeto portfolio demonstrando arquitetura de e-commerce: catalogo, estoque, ciclo de pedido, integracao com pagamento e painel administrativo.

## Stack
- .NET 10, ASP.NET Core, Blazor Server
- Entity Framework Core + PostgreSQL (produtos, pedidos, pagamentos)
- Autenticacao via API Key
- Rate limiting: 60 req/min global
- Serilog + Seq, RVM.Common.Security
- xUnit 86 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.ShopEngine.API/
    Auth/                     # ApiKeyAuthHandler
    Components/               # Blazor pages (produtos, pedidos, carrinho, checkout)
    Controllers/              # REST: catalogo, carrinho, pedidos, pagamentos
    Dtos/                     # Request/Response DTOs
    Health/                   # DatabaseHealthCheck
    Middleware/               # CorrelationIdMiddleware
    Services/
      OrderService            # Ciclo de pedido (criar, atualizar status, cancelar)
      PaymentService          # Processamento e status de pagamento
  RVM.ShopEngine.Domain/
    Entities/                 # Product, Order, OrderItem, Cart, Payment
    Enums/                    # OrderStatus, PaymentStatus, PaymentMethod
    Interfaces/               # IOrderRepository, IProductRepository...
  RVM.ShopEngine.Infrastructure/
    Data/                     # ShopEngineDbContext
    Repositories/             # Implementacoes dos repositories
test/
  RVM.ShopEngine.Test/        # xUnit (86 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- `OrderService` e `PaymentService` sao scoped — um contexto de DB por operacao
- Enums de status em `Domain/Enums/` — nunca strings hardcoded para status de pedido/pagamento
- DTOs separados das entidades de dominio (pasta `Dtos/` na API)
- `EnsureCreated` em dev, migration EF Core em producao
- PathBase configuravel via `App:PathBase` para deploy atras de reverse proxy

## Como Rodar
### Dev
```bash
docker compose -f docker-compose.dev.yml up -d
cd src/RVM.ShopEngine.API
dotnet run
```

### Testes
```bash
dotnet test test/RVM.ShopEngine.Test/
```

## Decisoes Arquiteturais
- **OrderService + PaymentService separados**: ciclo de pedido e ciclo de pagamento evoluem independentemente — pedido pode ter multiplas tentativas de pagamento; pagamento pode ser estornado sem cancelar o pedido
- **Enums para status**: evita strings magicas e permite exhaustive switch — mudanca de status e tipo-segura em compile time
- **DTOs na camada API**: dominio nao expoe entidades diretamente — evita over-posting e permite versionar a API sem alterar o dominio
- **Blazor admin + REST**: admin usa Blazor para interatividade; integrações externas (marketplace, app mobile) usam REST com API Key
