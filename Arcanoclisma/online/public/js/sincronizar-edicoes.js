// A sala em espera ainda não fixou sua versão; zero só é uma versão real após o início.
export async function sincronizarEdicoesDaSala(mensagem, edicoes, carregar) {
  if (mensagem.t !== 'estado' || mensagem.edVersao === undefined) return true;
  if (mensagem.status === 'aguardando' && mensagem.edVersao === 0) return true;
  if (mensagem.edVersao === edicoes.id && !edicoes.rascunho) return true;
  if (!await carregar(mensagem.edVersao)) return false;
  return edicoes.id === mensagem.edVersao && !edicoes.rascunho;
}
