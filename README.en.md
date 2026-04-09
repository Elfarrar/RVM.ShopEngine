***English** | [Portugues](README.md)*

# RVM.ShopEngine

E-commerce engine with product catalog, session-based cart, checkout with stock control and payment lifecycle.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-42%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## About

RVM.ShopEngine is a full-featured e-commerce engine with a product catalog organized by categories (with slugs), session-based shopping cart, checkout with stock validation and automatic decrement, and a complete payment lifecycle (Pending -> Authorized -> Captured -> Failed -> Refunded). It supports multiple payment methods (CreditCard, DebitCard, Pix, BankSlip, Wallet) and keeps product snapshots at the time of order placement.

---

## Technologies

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Npgsql 10.0.1) |
| Logging | Serilog (structured JSON) |
| Testing | xUnit 2.9 + Moq 4.20 |
| Containers | Docker Compose |

---

## Architecture

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

## Project Structure

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

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)

### Database with Docker

```bash
docker compose -f docker-compose.dev.yml up -d
```

### Run the API

```bash
cd src/RVM.ShopEngine.API
dotnet run
```

The API starts by default at `http://localhost:5000`.

### Run the tests

```bash
dotnet test
```

### Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ApiKeys__Keys__0__Key` | API authentication key |
| `App__PathBase` | Path base (e.g., `/shopengine`) |

---

## API Endpoints

### Products

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/api/products` | Search products (query, categoryId, activeOnly, offset, limit) | No |
| `GET` | `/api/products/{id}` | Get product by ID | No |
| `POST` | `/api/products` | Create product | Yes |
| `PUT` | `/api/products/{id}` | Update product | Yes |
| `DELETE` | `/api/products/{id}` | Delete product | Yes |

### Categories

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/api/categories` | List categories | No |
| `GET` | `/api/categories/{id}` | Get category by ID | No |
| `POST` | `/api/categories` | Create category | Yes |
| `PUT` | `/api/categories/{id}` | Update category | Yes |
| `DELETE` | `/api/categories/{id}` | Delete category | Yes |

### Cart

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/api/cart/{sessionId}` | Get session cart | Yes |
| `POST` | `/api/cart/{sessionId}` | Add item to cart | Yes |
| `PUT` | `/api/cart/{sessionId}/items/{productId}` | Update item quantity | Yes |
| `DELETE` | `/api/cart/items/{itemId}` | Remove item from cart | Yes |
| `DELETE` | `/api/cart/{sessionId}` | Clear cart | Yes |

### Orders

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/api/orders` | List orders (email, status, offset, limit) | Yes |
| `GET` | `/api/orders/{id}` | Get order by ID | Yes |
| `POST` | `/api/orders` | Create order from cart | Yes |
| `PUT` | `/api/orders/{id}/status` | Update order status | Yes |

### Payments

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/api/payments/{id}` | Get payment by ID | Yes |
| `POST` | `/api/payments` | Create payment for order | Yes |
| `POST` | `/api/payments/{id}/capture` | Capture payment | Yes |
| `POST` | `/api/payments/{id}/refund` | Refund payment | Yes |

### Health Check

| Method | Route | Description | Auth |
|---|---|---|---|
| `GET` | `/health` | Application health check | No |

---

## Tests

42 automated tests covering domain, infrastructure and services.

```
RVM.ShopEngine.Test/
├── Domain/
│   └── EntityTests.cs              18 tests (defaults, enums, calculations)
├── Infrastructure/
│   ├── OrderRepositoryTests.cs      6 tests (CRUD, search, filters)
│   └── ProductRepositoryTests.cs    9 tests (CRUD, search, pagination)
└── Services/
    ├── OrderServiceTests.cs         4 tests (creation, validation, stock)
    └── PaymentServiceTests.cs       5 tests (creation, capture, refund)
```

```bash
dotnet test --verbosity normal
```

---

## Features

- **Product catalog** — Products with categories, slugs, SKU, compare-at price, image and active/inactive status
- **Session-based cart** — Items linked to SessionId, with automatic merge when adding duplicate products
- **Checkout with validation** — Checks available stock and decrements automatically when creating an order
- **Automatic OrderNumber** — Format `ORD-yyyyMMddHHmmss-RAND` generated at checkout time
- **Product snapshot** — ProductName, ProductSku and UnitPrice stored in OrderItem at order time
- **Order lifecycle** — Pending -> Confirmed -> Processing -> Shipped -> Delivered -> Cancelled -> Refunded
- **Payment lifecycle** — Pending -> Authorized -> Captured -> Failed -> Refunded
- **5 payment methods** — CreditCard, DebitCard, Pix, BankSlip, Wallet
- **Search with filters** — Products by query/category, orders by email/status, with offset/limit pagination
- **ApiKey authentication** — Custom scheme supporting multiple keys
- **Rate Limiting** — 60 requests/minute per IP
- **Correlation ID** — `X-Correlation-ID` header propagated across all requests
- **Health Check** — `/health` endpoint with database connectivity verification
- **Structured logging** — Serilog with compact JSON format

---

<p align="center">
  <strong>RVM Tech</strong> &mdash; Microservices ecosystem
</p>
