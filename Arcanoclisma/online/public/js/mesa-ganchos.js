// Ganchos da mesa para os movimentos e efeitos visuais do GPT/Codex. O Claude só liga os acontecimentos do motor
// aos assets: o GPT cria todas as animações, movimentos e imagens (pedido em Arcanoclisma/efeitos/pedidos/mesa-efeitos-v1.claude.json).
//  - Ataque e bloqueio: movimento genérico já entregue (kit/cartas.css: .card-motion + .attack-generic / .block-generic).
//  - Entrar em campo, resolução de cada magia e colisão de cada carta: chamam js/mesa-efeitos-codex.js quando ele
//    existir (entrega do GPT). Sem a entrega, nada é desenhado; imagem só entra com o OK do GPT no manifesto dele.
// A mesa redesenha o tabuleiro a cada mudança; por isso o movimento em curso é reaplicado com o tempo já decorrido.

import { DADOS } from './base.js';
import { RITMO_VISUAL } from './ritmo-visual-v13.js';

// mesmo tempo do CSS .card-motion.attack-generic/.block-generic (mesa-ajuste.css), perfil v13 do GPT
const DURACAO_MOVIMENTO = RITMO_VISUAL.ataqueBloqueio;

export function criarGanchosMesa({ raiz, conteudo, eu }) {
  const movimentos = new Map(); // id da instância -> { acao, t0, rival }
  const vistos = new Set();
  let efeitos = null; // módulo do GPT (opcional)
  if (DADOS.efeitosCodex) import('./mesa-efeitos-codex.js').then((m) => { efeitos = m.criarEfeitosMesa?.(raiz) ?? null; }, () => { efeitos = null; });

  const elDe = (id) => (id ? conteudo.querySelector(`.cm[data-id="${CSS.escape(id)}"]`) : null);
  const reduzido = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
  const chamar = (nome, dados) => { try { efeitos?.[nome]?.(dados); } catch (err) { console.error('efeito', nome, err); } };

  /** Antes de redesenhar: o que estava na mesa e na pilha. */
  function antes(e) {
    if (!e) return null;
    return {
      campo: new Set(e.jogadores.flatMap((z) => z.campo)),
      pilha: new Map(e.pilha.map((p) => [p.id, p])),
      combate: e.combate ? { ...e.combate } : null,
      rects: new Map([...conteudo.querySelectorAll('.cm[data-id]')].map((el) => [el.dataset.id, el.getBoundingClientRect()])),
    };
  }

  /** Depois de redesenhar: dispara movimentos e efeitos dos acontecimentos novos. */
  function depois(e, anterior) {
    if (!e) return;
    const agora = performance.now();
    // ataque e bloqueio declarados (movimento genérico do GPT)
    const c = e.combate;
    if (c) {
      const ka = `a:${e.turno}:${c.atacante}`;
      if (!vistos.has(ka)) { vistos.add(ka); movimentos.set(c.atacante, { acao: 'attack', t0: agora, rival: e.instancias[c.atacante]?.dono !== eu }); }
      if (c.bloqueador) {
        const kb = `b:${e.turno}:${c.bloqueador}`;
        if (!vistos.has(kb)) { vistos.add(kb); movimentos.set(c.bloqueador, { acao: 'block', t0: agora, rival: e.instancias[c.bloqueador]?.dono !== eu }); }
      }
    }
    aplicarMovimentos(e, agora);
    if (!anterior || reduzido()) return;
    // carta que entrou em campo
    for (const z of e.jogadores) for (const id of z.campo) {
      if (anterior.campo.has(id)) continue;
      chamar('entrarEmCampo', { el: elDe(id), id, defId: e.instancias[id]?.defId, dono: e.instancias[id]?.dono, meu: e.instancias[id]?.dono === eu });
    }
    // magia/efeito que saiu da pilha (resolveu ou foi anulado)
    for (const [pid, p] of anterior.pilha) {
      if (e.pilha.some((x) => x.id === pid)) continue;
      const defId = p.tipo === 'carta' ? e.instancias[p.instId]?.defId : null;
      chamar('resolver', { item: p, defId, origemEl: elDe(p.instId ?? p.atacante), alvoEl: elDe(p.alvo), alvoRect: p.alvo ? anterior.rects.get(p.alvo) ?? null : null, meu: p.controlador === eu });
    }
    // colisão: o dano de combate acabou de acontecer (o combate terminou)
    if (anterior.combate && !e.combate && anterior.combate.etapa === 'bloqueado') {
      const k = anterior.combate;
      const alvo = k.bloqueador ?? k.alvo;
      chamar('colidir', {
        atacanteEl: elDe(k.atacante), alvoEl: elDe(alvo), atacanteRect: anterior.rects.get(k.atacante) ?? null, alvoRect: anterior.rects.get(alvo) ?? null,
        atacanteDefId: e.instancias[k.atacante]?.defId, alvoDefId: e.instancias[alvo]?.defId, bloqueio: !!k.bloqueador, meu: e.instancias[k.atacante]?.dono === eu,
      });
    }
  }

  function aplicarMovimentos(e, agora) {
    for (const [id, m] of movimentos) {
      const passou = agora - m.t0;
      const el = elDe(id);
      if (passou >= DURACAO_MOVIMENTO || !el || reduzido()) { movimentos.delete(id); continue; }
      // o movimento do GPT termina com a carta deitada (rotate 90deg); durante ele, sai o "suspensa" da mesa
      el.classList.remove('suspensa');
      el.classList.add('card-motion', m.acao === 'block' ? 'block-generic' : 'attack-generic');
      el.style.setProperty('--motion-advance', m.rival ? '72px' : '-72px');
      el.style.animationDelay = `${-passou}ms`;
      el.addEventListener('animationend', () => {
        el.classList.remove('card-motion', 'attack-generic', 'block-generic');
        el.style.animationDelay = '';
        if (e.instancias[id]?.suspensa) el.classList.add('suspensa');
        movimentos.delete(id);
      }, { once: true });
    }
  }

  return { antes, depois, limpar: () => { movimentos.clear(); try { efeitos?.limpar?.(); } catch { /* nada */ } } };
}
