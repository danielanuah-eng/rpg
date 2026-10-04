// Tempos de apresentação. Não modifica regras, custos, prioridades ou turnos.
export const RITMO_VISUAL = Object.freeze({
  fator: 1.5,
  entrarEmCampo: 930,
  ataqueBloqueio: 1320,
  suspender: 540,
  reposicionarMao: 360,
  comprarParaMao: 780,
  voltarParaMao: 540,
  arrasteParaCampo: 680,
  desaparecer: 400,
  entradaPilha: 750,
  entradaArena: 1600,
  intervaloEntradaArena: 180,
});
export function duracaoVisual(duracao, minimo = 900, maximo = 1800) {
  const numero = Number(duracao);
  return Math.min(maximo, Math.max(minimo, (Number.isFinite(numero) && numero > 0 ? numero : 900) * RITMO_VISUAL.fator));
}
