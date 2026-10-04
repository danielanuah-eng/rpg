// Aba Lore do site (pedido do autor, 04/10): gera public/jogo/lore.json a partir do que já está no repositório.
//  - Valores: tabela "Os 9 Valores" de tcg/docs/TABELA-DE-VALORES.md.
//  - Atlas: arte/descricoes_das_imagens.md (enciclopédia do mundo, por categoria). Só o "Assunto" de cada entrada (o texto do
//    mundo); as instruções de estilo das imagens ficam de fora. Só entram as entradas com arte aprovada (vão para /arte/<id>.webp).
//  - Novidades: as últimas mudanças do jogo, tiradas do histórico do git (as mensagens dos commits).
import { execSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';

type Entrada = { id: string; nome: string; texto: string };
export type Lore = {
  valores: { nome: string; cor: string; forma: string; ideia: string }[];
  categorias: { titulo: string; itens: Entrada[] }[];
  novidades: { data: string; texto: string }[];
};

const limpar = (t: string) => t.replace(/\\+/g, '').replace(/\*\*/g, '').replace(/\s+/g, ' ').trim();

function valores(arquivo: string): Lore['valores'] {
  if (!existsSync(arquivo)) return [];
  const md = readFileSync(arquivo, 'utf8');
  const bloco = md.split('## Os 9 Valores')[1]?.split('\n## ')[0] ?? '';
  return bloco.split('\n').filter((l) => /^\| \*\*/.test(l)).map((l) => {
    const c = l.split('|').map((x) => x.trim());
    return { nome: limpar(c[1]), cor: c[2], forma: c[4], ideia: c[5] };
  });
}

function atlas(arquivo: string, temArte: (id: string) => boolean): Lore['categorias'] {
  if (!existsSync(arquivo)) return [];
  const linhas = readFileSync(arquivo, 'utf8').split('\n');
  const cats: Lore['categorias'] = [];
  let cat: Lore['categorias'][number] | null = null;
  let atual: Partial<Entrada> | null = null;
  const fechar = () => { if (cat && atual?.id && atual.texto && temArte(atual.id)) cat.itens.push(atual as Entrada); atual = null; };
  for (const l of linhas) {
    const h2 = l.match(/^## (.+?)(?: \(\d+\))?$/);
    if (h2) { fechar(); cat = { titulo: h2[1].trim(), itens: [] }; cats.push(cat); continue; }
    const h3 = l.match(/^### \d+ · (.+)$/);
    if (h3) { fechar(); atual = { nome: h3[1].trim() }; continue; }
    if (!atual) continue;
    const id = l.match(/^`([a-z0-9-]+)`/);
    if (id && !atual.id) { atual.id = id[1]; continue; }
    if (l.startsWith('>') && !atual.texto) {
      const t = limpar(l.replace(/^>\s*/, ''));
      const assunto = t.match(/Assunto:\s*(.+?)(?:\s*Estilo:|$)/i)?.[1] ?? t;
      atual.texto = assunto.replace(/\s*\.$/, '') + '.';
    }
  }
  fechar();
  return cats.filter((c) => c.itens.length);
}

function novidades(raiz: string): Lore['novidades'] {
  try {
    const log = execSync('git log -80 --date=short --pretty=format:%ad%x09%s', { cwd: raiz, encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] });
    return log.split('\n').map((l) => l.split('\t')).filter(([, s]) => s && !/^Merge|lote \d+ de|^arte: (entregar|completar|concluir)/i.test(s))
      .map(([data, s]) => ({ data, texto: s.charAt(0).toUpperCase() + s.slice(1) })).slice(0, 40);
  } catch { return []; }
}

export function gerarLore(raizArcano: string, temArte: (id: string) => boolean): Lore {
  return {
    valores: valores(raizArcano + 'tcg/docs/TABELA-DE-VALORES.md'),
    categorias: atlas(raizArcano + 'arte/descricoes_das_imagens.md', temArte),
    novidades: novidades(raizArcano),
  };
}
