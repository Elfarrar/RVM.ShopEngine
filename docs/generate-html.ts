/**
 * RVM.ShopEngine — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone
 * com descritivos de cada funcionalidade.
 *
 * Uso:
 *   npx tsx docs/generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'dashboard',
    title: '1. Dashboard',
    description:
      'Painel central do RVM.ShopEngine com metricas de vendas, pedidos recentes ' +
      'e indicadores de desempenho da loja. Visao executiva em tempo real.',
    screenshot: '01-dashboard',
    features: [
      'Total de vendas do periodo (dia/semana/mes)',
      'Numero de pedidos: novos, em andamento e concluidos',
      'Produtos mais vendidos',
      'Grafico de receita por periodo',
      'Alertas de estoque baixo',
      'Ultimos pedidos com status',
    ],
    tips: [
      'Use os filtros de periodo para comparar desempenho entre diferentes intervalos.',
      'Clique nos cards de metricas para navegar diretamente ao modulo correspondente.',
    ],
  },
  {
    id: 'produtos',
    title: '2. Produtos',
    description:
      'Gerencie o catalogo completo de produtos da loja. Cadastre itens com nome, ' +
      'descricao, preco, fotos, categorias e controle de estoque.',
    screenshot: '02-produtos',
    features: [
      'Listagem de produtos com foto, preco e estoque atual',
      'Cadastrar novo produto com nome, descricao, SKU e preco',
      'Upload de multiplas fotos por produto',
      'Organizacao por categorias e subcategorias',
      'Controle de estoque com alertas de quantidade minima',
      'Ativar/desativar produto sem excluir',
      'Busca e filtros por categoria, preco ou disponibilidade',
    ],
    tips: [
      'Use o SKU para facilitar o controle de estoque e identificacao nos pedidos.',
      'Produtos inativos nao aparecem na loja para os clientes.',
    ],
  },
  {
    id: 'pedidos',
    title: '3. Pedidos',
    description:
      'Acompanhe e gerencie todos os pedidos da loja. Visualize os itens comprados, ' +
      'dados do cliente, endereco de entrega e status de cada pedido.',
    screenshot: '03-pedidos',
    features: [
      'Listagem de pedidos com status (pendente, confirmado, enviado, entregue, cancelado)',
      'Detalhes do pedido: itens, quantidades e valores',
      'Dados do cliente e endereco de entrega',
      'Atualizar status do pedido com notificacao automatica',
      'Historico completo de alteracoes',
      'Filtros por status, data, cliente ou valor',
      'Exportar lista de pedidos (CSV)',
    ],
    tips: [
      'Ao atualizar o status para "Enviado", inclua o codigo de rastreamento.',
      'Pedidos novos aparecem no topo com destaque em azul.',
    ],
  },
  {
    id: 'pagamentos',
    title: '4. Pagamentos',
    description:
      'Gerencie as transacoes financeiras da loja. Acompanhe cobranças Pix, Boleto ' +
      'e Cartao de Credito, com status de pagamento e historico de transacoes.',
    screenshot: '04-pagamentos',
    features: [
      'Listagem de transacoes com status (pendente, pago, cancelado, estornado)',
      'Detalhes por metodo de pagamento: Pix, Boleto, Cartao',
      'Data de vencimento e data de pagamento',
      'Valor bruto, taxas e valor liquido',
      'Acoes: confirmar pagamento manual, estornar, cancelar',
      'Relatorio financeiro por periodo',
      'Integracao com gateway de pagamento',
    ],
    tips: [
      'Pagamentos Pix sao confirmados automaticamente via webhook.',
      'Para estornos de cartao, o prazo de processamento e de 5 a 10 dias uteis.',
    ],
  },
];

// ---------------------------------------------------------------------------
// Gerar HTML
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.ShopEngine - Manual do Usuario</title>
  <style>
    :root {
      --primary: #8b5cf6;
      --surface: #ffffff;
      --bg: #f4f6fa;
      --text: #1e293b;
      --text-muted: #64748b;
      --border: #e2e8f0;
      --sidebar-bg: #1e1b4b;
      --accent: #8b5cf6;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header {
      background: var(--sidebar-bg);
      color: white;
      padding: 3rem 1.5rem;
      text-align: center;
    }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #c4b5fd; font-size: 1rem; }
    header .version { color: #a78bfa; font-size: 0.85rem; margin-top: 0.5rem; }
    nav {
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      padding: 1rem 1.5rem;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a {
      display: inline-block;
      padding: 0.35rem 0.75rem;
      border-radius: 0.5rem;
      font-size: 0.85rem;
      color: var(--text);
      text-decoration: none;
      background: var(--bg);
      transition: background 0.2s;
    }
    nav a:hover { background: var(--primary); color: white; }
    section {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 1rem;
      padding: 2rem;
      margin-bottom: 2rem;
    }
    section h2 {
      font-size: 1.5rem;
      color: var(--primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--border);
    }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips {
      background: var(--bg);
      border-radius: 0.75rem;
      padding: 1rem 1.25rem;
      margin-bottom: 1.25rem;
    }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #f5f3ff; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots {
      display: flex;
      gap: 1.5rem;
      margin-top: 1rem;
      align-items: flex-start;
    }
    .screenshot-group {
      position: relative;
      flex: 1;
      border: 1px solid var(--border);
      border-radius: 0.75rem;
      overflow: hidden;
    }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      background: var(--sidebar-bg);
      color: white;
      font-size: 0.7rem;
      padding: 0.2rem 0.5rem;
      border-radius: 0.35rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .no-screenshot {
      background: var(--bg);
      padding: 2rem;
      border-radius: 0.75rem;
      text-align: center;
      color: var(--text-muted);
    }
    footer {
      text-align: center;
      padding: 2rem 1rem;
      color: var(--text-muted);
      font-size: 0.85rem;
    }
    @media (max-width: 768px) {
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 100%; flex: 1; }
      section { padding: 1.25rem; }
    }
    @media print {
      nav { display: none; }
      section { break-inside: avoid; page-break-inside: avoid; }
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 250px; }
    }
  </style>
</head>
<body>
  <header>
    <h1>RVM.ShopEngine - Manual do Usuario</h1>
    <p>Plataforma de E-commerce — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.ShopEngine</strong> e uma plataforma de e-commerce completa para gestao
        de lojas online. Controle produtos, pedidos e pagamentos em um unico painel administrativo.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Dashboard executivo</strong> — metricas de vendas e desempenho em tempo real</li>
          <li><strong>Catalogo de produtos</strong> — gestao completa com categorias, fotos e estoque</li>
          <li><strong>Gestao de pedidos</strong> — ciclo completo do pedido ao post-venda</li>
          <li><strong>Pagamentos</strong> — Pix, Boleto e Cartao de Credito integrados</li>
          <li><strong>Relatorios</strong> — analise financeira e de vendas por periodo</li>
          <li><strong>Multi-tenant</strong> — suporte a multiplas lojas na mesma plataforma</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Plataforma de E-commerce</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

// ---------------------------------------------------------------------------
// Gerar Markdown
// ---------------------------------------------------------------------------
function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.ShopEngine - Manual do Usuario

> Plataforma de E-commerce — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.ShopEngine** e uma plataforma de e-commerce completa para gestao
de lojas online. Controle produtos, pedidos e pagamentos em um unico painel administrativo.

**Recursos principais:**
- **Dashboard executivo** — metricas de vendas e desempenho em tempo real
- **Catalogo de produtos** — gestao completa com categorias, fotos e estoque
- **Gestao de pedidos** — ciclo completo do pedido ao post-venda
- **Pagamentos** — Pix, Boleto e Cartao de Credito integrados
- **Relatorios** — analise financeira e de vendas por periodo
- **Multi-tenant** — suporte a multiplas lojas na mesma plataforma

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(
      path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`),
    );

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) {
      md += `- ${f}\n`;
    }
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) {
        md += `> - ${t}\n`;
      }
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n`;
      md += `|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Backend** | ASP.NET Core + Blazor Server |
| **Banco de dados** | PostgreSQL 16 + EF Core |
| **Pagamentos** | Pix, Boleto, Cartao de Credito via gateway |
| **Estoque** | Controle automatico com alertas de minimo |
| **Deploy** | Docker Compose + Nginx |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------
const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
