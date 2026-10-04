# Entrega visual v19 — iconografia e conferência de magias

Pedido atendido: `Arcanoclisma/online/pedidos-visuais/iconografia.claude.json`, versão 2. Produção e conferência concluídas em 04/10/2026.

## 32 ícones de mecânicas

Os 32 SVGs estão em `Arcanoclisma/online/public/kit/icones/`, com originais iguais em `Arcanoclisma/arena/icones/`. O manifesto segue exatamente o contrato `{ "id": { "arquivo": "/kit/icones/id.svg", "okGpt": true } }`.

Cada desenho tem viewBox 64 × 64, fundo transparente, contorno firme e cor `currentColor`, sem letras nem números desenhados. Os números continuam no texto editável. Uso único usa frasco riscado e uma marca pictográfica para conciliar a descrição com a exigência de não embutir algarismos.

Incluído também o pedido de Protessência acrescentado pelo Claude durante a conferência final: carta envolta em aura, mira riscada e trajetória desviada. O catálogo e o manifesto final contêm 32 ícones.

- Gerador reutilizável: `Arcanoclisma/arena/scripts/gerar-iconografia-v19.mjs`.
- Entrega por ID e hash: `Arcanoclisma/online/entregas-visuais/iconografia-v19.codex.json`.
- Catálogo: `/kit/icones/catalogo.html`, com exemplos em 16, 20 e 48 px, sobre fundos claro e escuro.

## Apresentação no site

O módulo de integração do Claude, `public/js/icones.js`, já consulta o manifesto. O adaptador visual `public/js/iconografia-v19.js`, com `public/css/iconografia-v19.css`, é carregado pelo módulo visual existente `public/js/mesa-legibilidade-v15.js`.

O adaptador usa máscara CSS para herdar a cor do texto: um SVG externo em `img` não herda essa cor sozinho. Também mantém o símbolo junto da palavra ou quantidade seguinte, evitando ícones soltos no fim da linha. Preserva o conteúdo do texto, os valores e as regras.

Conferência local em 1366 × 768: 201 cartas, 249 símbolos e 249 grupos de símbolo/texto, sem corte de regras ou de nomes. Carta longa aberta: Arquivista Sem Rosto. Conferida também a aba Palavras-chave com símbolos claros sobre fundo escuro.

**Próxima integração do Claude:** aplicar os mesmos ícones nos balões do tutorial, conforme o próprio pedido. Não foi alterado o módulo funcional de tutorial durante esta entrega. Os arquivos estão prontos para a publicação do site; esta conferência não atesta uma implantação pública.

Na sessão local já aberta, Protessência ainda apareceu como texto na aba Palavras-chave. O SVG novo e seu registro no manifesto passaram pelo contrato e pela medição de alfa; o Claude deve confirmar o carregamento desse último símbolo ao publicar/atualizar a sessão. A medição das 201 cartas acima corresponde à sessão com os 31 símbolos iniciais; a adição final foi conferida no catálogo vetorial.

## Três situações de magia conferidas

| Situação | Execução | Resultado observado |
| --- | --- | --- |
| Anulação na pilha | Tutorial oficial `/#/tutorial/pilha`: Olho-que-Fecha contra Brasa de Forja, 1366 × 768 | Anulação resolvida pelo motor; selo inteiro dentro da tela; magia anulada sem efeito de dano. |
| Magia nos dois campos | Charco Fervente, cenário local com motor e mesa reais, 1280 × 720 | Ondas delimitadas por cada campo; dano aplicado às duas unidades pelo motor. |
| Ressurgimento | Cemitério que Respira, cenário local com motor e mesa reais, 1280 × 720 | Trajetória parte do cemitério e chega à unidade que volta ao campo; motor confirma a entrada. |

A geometria da carta na pilha já havia sido corrigida pelo Claude. Ainda faltava reservar espaço para o diâmetro do selo. O runtime visual `public/kit/efeitos/magias-v17.js` agora aplica essa margem somente a efeitos destinados ao alvo da pilha. Não altera o alvo lógico nem outras geometrias de magia.

Não sobraram elementos transitórios após anulação e ressurgimento. Não foram encontrados avisos/erros no console nessas execuções.

Os cenários locais podem ser repetidos em `/qa/magias-v19.html`. A página prepara duas situações isoladas, sem persistir partidas; jogadas, alvos, pagamentos e resoluções passam pela interface e pelo motor existentes. Não são testes de partida multijogador. As três cenas não equivalem à revisão visual individual das 132 magias.

## Verificação reproduzível

Na raiz do repositório:

```powershell
node Arcanoclisma/arena/scripts/verificar-iconografia-v19.mjs
node --test Arcanoclisma/efeitos/verificar-magias-v17.test.mjs
```

Resultado: 32 ícones aprovados no contrato, cópias, hashes distintos e alfa medido em 16, 20 e 64 px; 6 testes de contrato dos efeitos aprovados. A medição rasteriza os SVGs apenas para conferir transparência; os assets entregues permanecem vetoriais.

Evidências e medições reais: `coordenacao/site-online/evidencias-v19/`. Incluem catálogo, carta aberta, Palavras-chave, as três animações e JSONs de alfa, textos e geometria.
