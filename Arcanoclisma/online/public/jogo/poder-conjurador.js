// Gerado por online/construir.ts a partir de tcg/src/poder-conjurador.ts. Não edite: edite o .ts e reconstrua.
// Editor de Conjuradores (pedido do autor, 04/10): o poder do Conjurador abre no construtor de efeitos das cartas.
// O construtor trabalha com uma lista de habilidades (condição + custo + efeitos) e campos de passiva; o poder guarda a
// ativação e o gatilho antigos em campos próprios. Estas duas funções convertem nos dois sentidos sem perder nada:
// poder → construtor → poder devolve o mesmo JSON (mesmas chaves, mesma ordem). Sem dependências: vira
// online/public/jogo/poder-conjurador.js no build (construir.ts) e serve ao Editor do site e aos testes.
                                                                                        

/** Passivas numéricas do poder (campos do construtor, na ordem em que aparecem). */
export const PASSIVAS_CONJURADOR = ['primeiroDanoDeAliadoMais', 'primeiraCartaMaisBarata', 'primeiraRepentinaMaisBarata', 'aliadosProtegidos']         ;
/** Condições do gatilho antigo (`poder.gatilho`): sempre uma vez por turno. */
export const QUANDO_GATILHO_CONJURADOR = ['aliadoEntra', 'aliadoMorre', 'aliadoDanoConjurador']         ;

/** Habilidade como o construtor a vê: as marcas `_de`/`_extra`/`_ordem` guardam de onde veio, para voltar igual. */
                                                                                                                                      
                                    
                                      
                                    
                                   
                                       
                             
                                    
                                                                            
                                   
                                            
                    
 

const clonar =    (v   )    => (v === undefined ? v : JSON.parse(JSON.stringify(v)));
const CHAVES_ATIVACAO = ['repentina', 'custo', 'suspende', 'efeitos', 'descartar', 'danoNoConjurador', 'requisito'];
const CHAVES_GATILHO = ['quando', 'efeitos', 'soNoSeuTurno'];
const CHAVES_PODER = ['ativacao', 'gatilho', ...PASSIVAS_CONJURADOR, 'permiteJogarDe', 'habilidades'];

/** Reordena as chaves de `o` pela ordem guardada (as novas vão para o fim, na ordem em que foram criadas). */
function ordenar                  (o   , ordem                      )    {
  if (!ordem) return o;
  const r                          = {};
  for (const k of ordem) if (k in o) r[k] = (o                           )[k];
  for (const [k, v] of Object.entries(o)) if (!(k in r)) r[k] = v;
  return r     ;
}
const semIndefinidos =                   (o   )    => Object.fromEntries(Object.entries(o).filter(([, v]) => v !== undefined))     ;

function ativacaoParaHabilidade(a          )                       {
  const custo = semIndefinidos({ suspender: a.suspende || undefined, arcano: a.custo || undefined, vida: a.danoNoConjurador, descartar: a.descartar, grau: a.requisito?.grau });
  const h                       = { quando: 'ativada', custo, efeitos: clonar(a.efeitos ?? []) };
  if (a.repentina !== undefined) h.repentina = a.repentina;
  if (a.requisito?.arcano !== undefined) h.requisito = { arcano: a.requisito.arcano };
  const extra = Object.fromEntries(Object.entries(a).filter(([k]) => !CHAVES_ATIVACAO.includes(k)));
  if (Object.keys(extra).length) h._extra = clonar(extra);
  // custo 0 e suspende false voltam sozinhos (são obrigatórios na ativação); a ordem das chaves fica guardada
  h._ordem = Object.keys(a);
  h._de = 'ativacao';
  return h;
}
function habilidadeParaAtivacao(h                      )           {
  const c = h.custo ?? {};
  const req = semIndefinidos({ grau: c.grau || undefined, arcano: h.requisito?.arcano });
  const extra = h._extra ?? {};
  const a = semIndefinidos({
    repentina: h.repentina, custo: c.arcano ?? 0, suspende: !!c.suspender, efeitos: h.efeitos ?? [],
    descartar: c.descartar, danoNoConjurador: c.vida, requisito: Object.keys(req).length ? req : undefined, ...extra,
  })            ;
  return ordenar(a, h._ordem);
}
function gatilhoParaHabilidade(g                                         )                       {
  const h                       = { quando: g.quando, efeitos: clonar(g.efeitos ?? []), umaVezPorTurno: true };
  if (g.soNoSeuTurno !== undefined) h.soNoSeuTurno = g.soNoSeuTurno;
  const extra = Object.fromEntries(Object.entries(g).filter(([k]) => !CHAVES_GATILHO.includes(k)));
  if (Object.keys(extra).length) h._extra = clonar(extra);
  h._ordem = Object.keys(g);
  h._de = 'gatilho';
  return h;
}
function habilidadeParaGatilho(h                      )                                          {
  const g = semIndefinidos({ quando: h.quando, efeitos: h.efeitos ?? [], soNoSeuTurno: h.soNoSeuTurno, ...(h._extra ?? {}) });
  return ordenar(g, h._ordem)                                           ;
}
/** Uma habilidade da lista que volta para `ativacao` (ainda é ativada) ou para `gatilho` (ainda é gatilho de Aliado, uma vez por turno). */
const voltaComoAtivacao = (h                      ) => h._de === 'ativacao' && h.quando === 'ativada';
const voltaComoGatilho = (h                      ) => h._de === 'gatilho' && (QUANDO_GATILHO_CONJURADOR                     ).includes(h.quando) && !!h.umaVezPorTurno;
/** Tira as marcas do construtor (habilidade que vai para `habilidades`). */
function limpa(h                      )             {
  const { _de, _extra, _ordem, ...r } = h;
  void _de; void _extra; void _ordem;
  if (r.quando !== 'ativada') { delete r.custo; delete r.repentina; delete r.requisito; }
  return r;
}

/** Poder do Conjurador → o que o construtor de efeitos edita (cópia; o poder original não muda). */
export function poderParaConstrutor(p                                    )                    {
  const v                    = { habilidades: [] };
  if (!p) return v;
  if (p.ativacao) v.habilidades.push(ativacaoParaHabilidade(p.ativacao));
  if (p.gatilho) v.habilidades.push(gatilhoParaHabilidade(p.gatilho));
  for (const h of p.habilidades ?? []) v.habilidades.push(clonar(h));
  for (const k of PASSIVAS_CONJURADOR) if (p[k] !== undefined) v[k] = p[k];
  if (p.permiteJogarDe !== undefined) v.permiteJogarDe = clonar(p.permiteJogarDe);
  const resto = Object.fromEntries(Object.entries(p).filter(([k]) => !CHAVES_PODER.includes(k)));
  if (Object.keys(resto).length) v._resto = clonar(resto);
  v._ordem = Object.keys(p);
  return v;
}

/** O que o construtor editou → poder do Conjurador (a primeira ativação/gatilho antigos voltam aos campos deles). */
export function construtorParaPoder(v                   )                  {
  const p                                            = {};
  const habs               = [];
  for (const h of v.habilidades ?? []) {
    if (!p.ativacao && voltaComoAtivacao(h)) p.ativacao = habilidadeParaAtivacao(h);
    else if (!p.gatilho && voltaComoGatilho(h)) p.gatilho = habilidadeParaGatilho(h);
    else habs.push(clonar(limpa(h)));
  }
  for (const k of PASSIVAS_CONJURADOR) if (v[k] !== undefined && v[k] !== null) p[k] = v[k];
  if (v.permiteJogarDe !== undefined) p.permiteJogarDe = clonar(v.permiteJogarDe);
  if (habs.length) p.habilidades = habs;
  Object.assign(p, clonar(v._resto ?? {}));
  return ordenar(p, v._ordem);
}
