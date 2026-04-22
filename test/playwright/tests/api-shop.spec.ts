import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.SHOPENGINE_BASE_URL ?? 'https://shopengine.lab.rvmtech.com.br';

test.describe('ShopEngine API', () => {
  test.skip(
    process.env.SHOPENGINE_RUN_SMOKE !== '1',
    'Defina SHOPENGINE_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/products retorna lista de produtos ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/products`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/categories retorna categorias ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/categories`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/cart retorna carrinho ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/cart`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/orders retorna pedidos ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/orders`);
    expect([200, 401]).toContain(response.status());
  });

  test('POST /api/cart sem corpo retorna 400 ou 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/cart`);
    expect([400, 401]).toContain(response.status());
  });
});
