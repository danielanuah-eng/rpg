# Pedido ao Codex: regenerar a 231 (Wyvern) no estilo v2

Autorizado pelo autor em 02/10/2026 ("regenere no estilo v2"). Pedido de Claude; a geração é do Codex (ver `../../../COLABORACAO-IAS.md`). Claude não gerou nem alterou nenhuma imagem.

## Por quê

A 231 é a única imagem com alfa conferido que não tem `registros/231.json`. O `conferir.mjs` só põe uma imagem em `estiloAtual.numeros` quando existe `registros/NNN.json` com `estiloVersao: 2`; por isso `progresso.json` mostra 240 concluídas / 195 pendentes no topo e 239 / 196 em `estiloAtual`. O PNG existe e passa no alfa, mas não há registro de prompt nem de versão de estilo, então não dá para afirmar que está em v2 (a data do arquivo, 02/10 03:38 UTC, cai entre a 230 e a 232, que são v2).

## O que fazer

1. Gerar de novo a 231 no estilo v2 e salvar em `imagens/wyvern.png` (mesmo nome; o catálogo e `conferir.mjs` dependem dele).
2. Usar como modelo de prompt e de registro `registros/232.json` (mesmo gabarito v2). Assunto do catálogo (`descricoes_das_imagens.md`, linha 1448): "Wyvern, a cria de um dragão: menor, duas patas e asas, sem chamas, olhar de bicho, arreio de couro de montaria."
3. Criar `registros/231.json` com os mesmos campos de `registros/232.json` (`number`, `id: "wyvern"`, `title`, `arquivo`, `estiloVersao: 2`, `prompt`, `fundo`, `ferramenta`, `adaptacoes`, `preferenciaEspada`).
4. Não editar `progresso.json` à mão: rodar `node conferir.mjs` dentro de `arte/`, que recalcula o progresso e a galeria. Resultado esperado: `estiloAtual.numeros` com 240 itens e pendentes 195 em ambos os blocos. Se o alfa não passar, o estado fica "revisar transparência" e a contagem não sobe.

## Formato e destino

- PNG quadrado 1:1 (as vizinhas têm 1254 × 1254), fundo realmente transparente com canal alfa, silhueta inteira na área central, margens transparentes largas, sem corte.
- Destino: `Arcanoclisma/arte/imagens/wyvern.png`.

## Preservação

A versão atual foi copiada, sem alteração, para `versoes-anteriores/wyvern-antes-estilo-v2.png` (SHA-256 igual ao original). Registrar a mudança e o motivo no prompt/registro da nova versão.

## Fora do escopo de Claude

Prompt final, escolha de pose e revisão visual. Este pedido não marca a 231 como v2: ela só entra em `estiloAtual` quando o registro e o PNG novos existirem.
