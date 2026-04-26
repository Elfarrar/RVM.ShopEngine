# Testes — RVM.ShopEngine

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.ShopEngine.Test/`
- **Total:** 86 testes
- **Foco:** OrderService (fluxo de pedido, estados), PaymentService (processamento, estorno), logica de carrinho, regras de estoque

```bash
dotnet test test/RVM.ShopEngine.Test/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** catalogo de produtos, adicionar ao carrinho, checkout, acompanhamento de pedido, painel admin

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
SHOPENGINE_BASE_URL=http://localhost:5000
SHOPENGINE_API_KEY=<api-key-dev>
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- `PaymentService` mockado em testes unitarios para simular aprovacao/rejeicao de pagamento
