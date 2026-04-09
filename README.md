*[English](README.en.md) | **Portugues***

# RVM.ShopEngine

Motor de e-commerce com catalogo, carrinho por sessao, checkout com controle de estoque e ciclo de pagamento.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-42%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## Sobre

RVM.ShopEngine e um motor de e-commerce completo com catalogo de produtos por categorias (com slug), carrinho baseado em sessao, checkout com validacao e decremento automatico de estoque, e ciclo completo de pagamento (Pending -> Authorized -> Captured -> Failed -> Refunded). Suporta multiplos metodos de pagamento (CreditCard, DebitCard, Pix, BankSlip, Wallet) e mantem snapshots dos produtos no momento do pedido.

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Banco de dados | PostgreSQL (Npgsql 10.0.1) |
| Logging | Serilog (JSON estruturado) |
| Testes | xUnit 2.9 + Moq 4.20 |
| Containers | Docker Compose |

---

## Arquitetura

```
┌─────────────────────────────────────────────────────┐
│                   API  (ASP.NET Core)               │
│  Controllers ── Dtos ── Services ── Middleware       │
│  Auth (ApiKey) ── Health ── RateLimiter              │
├─────────────────────────────────────────────────────┤
│                  Domain                              │
│  Entities ── Enums ── Interfaces                     │
├─────────────────────────────────────────────────────┤
│               Infrastructure                         │
│  DbContext ── Repositories ── Configurations         │
├─────────────────────────────────────────────────────┤
│               PostgreSQL                             │
└─────────────────────────────────────────────────────┘
```

---

## Estrutura do Projeto

```
RVM.ShopEngine/
├── src/
│   ├── RVM.ShopEngine.API/
│   │   ├── Auth/
│   │   │   ├── ApiKeyAuthHandler.cs
│   │   │   └── ApiKeyAuthOptions.cs
│   │   ├── Controllers/
│   │   │   ├── CartController.cs
│   │   │   ├── CategoriesController.cs
│   │   │   ├── OrdersController.cs
│   │   │   ├── PaymentsController.cs
│   │   │   └── ProductsController.cs
│   │   ├── Dtos/
│   │   │   ├── CartDtos.cs
│   │   │   ├── CategoryDtos.cs
│   │   │   ├── OrderDtos.cs
│   │   │   ├── PaymentDtos.cs
│   │   │   └── ProductDtos.cs
│   │   ├── Health/
│   │   │   └── DatabaseHealthCheck.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Services/
│   │   │   ├── OrderService.cs
│   │   │   └── PaymentService.cs
│   │   └── Program.cs
│   ├── RVM.ShopEngine.Domain/
│   │   ├── Entities/
│   │   │   ├── CartItem.cs
│   │   │   ├── Category.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── Payment.cs
│   │   │   └── Product.cs
│   │   ├── Enums/
│   │   │   ├── OrderStatus.cs
│   │   │   ├── PaymentMethod.cs
│   │   │   └── PaymentStatus.cs
│   │   └── Interfaces/
│   │       ├── ICartItemRepository.cs
│   │       ├── ICategoryRepository.cs
│   │       ├── IOrderRepository.cs
│   │       ├── IPaymentRepository.cs
│   │       └── IProductRepository.cs
│   └── RVM.ShopEngine.Infrastructure/
│       ├── Data/
│       │   ├── Configurations/
│       │   │   ├── CartItemConfiguration.cs
│       │   │   ├── CategoryConfiguration.cs
│       │   │   ├── OrderConfiguration.cs
│       │   │   ├── OrderItemConfiguration.cs
│       │   │   ├── PaymentConfiguration.cs
│       │   │   └── ProductConfiguration.cs
│       │   └── ShopEngineDbContext.cs
│       ├── Repositories/
│       │   ├── CartItemRepository.cs
│       │   ├── CategoryRepository.cs
│       │   ├── OrderRepository.cs
│       │   ├── PaymentRepository.cs
│       │   └── ProductRepository.cs
│       └── DependencyInjection.cs
├── test/
│   └── RVM.ShopEngine.Test/
│       ├── Domain/
│       │   └── EntityTests.cs
│       ├── Infrastructure/
│       │   ├── OrderRepositoryTests.cs
│       │   └── ProductRepositoryTests.cs
│       └── Services/
│           ├── OrderServiceTests.cs
│           └── PaymentServiceTests.cs
├── docker-compose.dev.yml
├── docker-compose.prod.yml
├── global.json
└── RVM.ShopEngine.slnx
```

---

## Como Executar

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/) (ou Docker)

### Banco de dados com Docker

```bash
docker compose -f docker-compose.dev.yml up -d
```

### Executar a API

```bash
cd src/RVM.ShopEngine.API
dotnet run
```

A API sobe por padrao em `http://localhost:5000`.

### Executar os testes

```bash
dotnet test
```

### Variaveis de ambiente

| Variavel | Descricao |
|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexao PostgreSQL |
| `ApiKeys__Keys__0__Key` | Chave de autenticacao da API |
| `App__PathBase` | Path base (ex: `/shopengine`) |

---

## Endpoints da API

### Produtos

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/api/products` | Buscar produtos (query, categoryId, activeOnly, offset, limit) | Nao |
| `GET` | `/api/products/{id}` | Buscar produto por ID | Nao |
| `POST` | `/api/products` | Criar produto | Sim |
| `PUT` | `/api/products/{id}` | Atualizar produto | Sim |
| `DELETE` | `/api/products/{id}` | Remover produto | Sim |

### Categorias

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/api/categories` | Listar categorias | Nao |
| `GET` | `/api/categories/{id}` | Buscar categoria por ID | Nao |
| `POST` | `/api/categories` | Criar categoria | Sim |
| `PUT` | `/api/categories/{id}` | Atualizar categoria | Sim |
| `DELETE` | `/api/categories/{id}` | Remover categoria | Sim |

### Carrinho

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/api/cart/{sessionId}` | Ver carrinho da sessao | Sim |
| `POST` | `/api/cart/{sessionId}` | Adicionar item ao carrinho | Sim |
| `PUT` | `/api/cart/{sessionId}/items/{productId}` | Atualizar quantidade do item | Sim |
| `DELETE` | `/api/cart/items/{itemId}` | Remover item do carrinho | Sim |
| `DELETE` | `/api/cart/{sessionId}` | Limpar carrinho | Sim |

### Pedidos

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/api/orders` | Listar pedidos (email, status, offset, limit) | Sim |
| `GET` | `/api/orders/{id}` | Buscar pedido por ID | Sim |
| `POST` | `/api/orders` | Criar pedido a partir do carrinho | Sim |
| `PUT` | `/api/orders/{id}/status` | Atualizar status do pedido | Sim |

### Pagamentos

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/api/payments/{id}` | Buscar pagamento por ID | Sim |
| `POST` | `/api/payments` | Criar pagamento para pedido | Sim |
| `POST` | `/api/payments/{id}/capture` | Capturar pagamento | Sim |
| `POST` | `/api/payments/{id}/refund` | Estornar pagamento | Sim |

### Health Check

| Metodo | Rota | Descricao | Auth |
|---|---|---|---|
| `GET` | `/health` | Verificar saude da aplicacao | Nao |

---

## Testes

42 testes automatizados cobrindo dominio, infraestrutura e servicos.

```
RVM.ShopEngine.Test/
├── Domain/
│   └── EntityTests.cs              18 testes (defaults, enums, calculos)
├── Infrastructure/
│   ├── OrderRepositoryTests.cs      6 testes (CRUD, busca, filtros)
│   └── ProductRepositoryTests.cs    9 testes (CRUD, busca, paginacao)
└── Services/
    ├── OrderServiceTests.cs         4 testes (criacao, validacao, estoque)
    └── PaymentServiceTests.cs       5 testes (criacao, captura, estorno)
```

```bash
dotnet test --verbosity normal
```

---

## Funcionalidades

- **Catalogo** — Produtos com categorias, slugs, SKU, preco comparativo, imagem e status ativo/inativo
- **Carrinho por sessao** — Itens vinculados a SessionId, com merge automatico ao adicionar produto repetido
- **Checkout com validacao** — Verifica estoque disponivel e decrementa automaticamente ao criar pedido
- **OrderNumber automatico** — Formato `ORD-yyyyMMddHHmmss-RAND` gerado no momento do checkout
- **Snapshot de produto** — ProductName, ProductSku e UnitPrice gravados no OrderItem no momento do pedido
- **Ciclo de vida do pedido** — Pending -> Confirmed -> Processing -> Shipped -> Delivered -> Cancelled -> Refunded
- **Ciclo de pagamento** — Pending -> Authorized -> Captured -> Failed -> Refunded
- **5 metodos de pagamento** — CreditCard, DebitCard, Pix, BankSlip, Wallet
- **Busca com filtros** — Produtos por query/categoria, pedidos por email/status, com paginacao offset/limit
- **Autenticacao por ApiKey** — Scheme customizado com suporte a multiplas chaves
- **Rate Limiting** — 60 requisicoes/minuto por IP
- **Correlation ID** — Header `X-Correlation-ID` propagado em todas as requisicoes
- **Health Check** — Endpoint `/health` com verificacao de conectividade do banco
- **Logging estruturado** — Serilog com formato JSON compacto

---

<p align="center">
  <strong>RVM Tech</strong> &mdash; Ecossistema de microsservicos
</p>
