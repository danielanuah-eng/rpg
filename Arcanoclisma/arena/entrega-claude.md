# Entrega visual — Arcanoclisma TCG

Revisão de 2026-10-03. Kit visual local; Claude integra regras e rede.

## Campo e camadas

Palco lógico 1920 × 1440, ampliado horizontal e verticalmente. Piso com alfa real: `raster/arena-jogador-1-ampla-larga-v2.png` e `raster/arena-jogador-2-ampla-larga-v2.png`. Estudos anteriores preservados. `layout.json` contém coordenadas; adaptar à viewport e quantidade real, sem limitar campo ou mão aos exemplos exibidos.

Conjuradores no centro, encarando-se, à frente das respectivas fileiras de aliados. Botão de itens junto deles; no jogo físico, itens abaixo do conjurador. A prévia mostra modelos públicos. Inventário real e permissão de visualização dependem do motor.

## Cartas e itens

63 × 88 mm; overlays 1260 × 1760 e molduras SVG escaláveis. Arte central full art, detalhes finos em margens e cantos. Nome central serifado itálico; tipo, subtipos e regras espaçados. Raridade no centro da borda inferior.

- Geração: esfera única com cor simples e número.
- Custo: esfera própria, número livre; anel colorido por um ou vários Valores.
- Grau: losango, ocultar quando não aplicável.
- Ataque/vida: hexágono/escudo acima do texto.
- Armadura: defesa e vida de desgaste, símbolos próprios.

Valores são classificação extra e podem ser múltiplos. Manter listas `valores` e `subtipos`; mostrar cores no custo, sem concatenar os Valores ao texto do tipo. Preservar subtipos importantes como Dragão.

Itens propostos: `tipo: 'item'`, `categoriaItem: 'canal' | 'armadura' | 'extra'`, `defesa`, `vidaDesgaste`. Extra é nome provisório. Implementar suporte no motor antes de usá-los em partidas. Dados de demonstração não são balanceamento; vida do jogador vem do conjurador real e dano atual.

## Suspensão, ataque e bloqueio — revisão v4

Suspender significa deitar a carta inteira em 90 graus. `cartas.css` gira o wrapper completo; a carta pode elevar-se brevemente durante avanço/contato, mas pousa e permanece deitada. Ataque e bloqueio compartilham essa regra visual, preservando a orientação do observador e o brilho interno.

```js
await ArcanoclismaCardVisual.animateMotion(wrapperDaCarta, {
  action: 'attack', // ou 'block'
  opponent: false // true para avanço do rival
});
```

880 ms, cancela animação anterior no mesmo wrapper; Promise com limpeza de classes, prazo de segurança e movimento reduzido. Ataque e bloqueio terminam com `is-suspended`, rotação de 90°. Para preparar novamente: `ArcanoclismaCardVisual.setSuspended(wrapper, false)`, com retorno suave à vertical em 360 ms. Sincronizar com `Inst.suspensa`; só o motor permite levantar a carta. Carta já deitada não anima novo ataque/bloqueio. Os botões da prévia escolhem uma carta pronta e permitem levantar todas. Sem execução da animação no navegador nesta revisão.

Brilho progressivo: incomum com borda suave, rara com reflexo mais forte, lendária com acabamento iridescente azul/rosa/dourado na borda e na área de arte, preservando a caixa de regras. Apenas frentes autorizadas; versos ocultos idênticos e sem identidade/raridade privada no DOM. Raridade cosmética, sem alterar mecânicas.

## Canva e integração

`mapa-canva.json` contém links estáveis e IDs dos modelos genérico e dos três itens. Revisões em rascunho aguardando aprovação explícita após a prévia, exigida pela ferramenta. Base anterior salva; não presumir rascunho publicado.

Duplicar página, definir cena como fundo e manter moldura transparente sobre ela. Preencher nome, recursos, textos e estatísticas reais; remover campos não aplicáveis. Não esticar a arte. Consultar dataset atual antes de autofill em lote.

Assets separados e fontes locais editáveis. Sem mudanças no motor/rede. Alfa medido localmente. Não houve push desta entrega; aprovação das artes e autorização de push são decisões distintas.

## Tipos e arte por carta

Consultar `tipos-documentados.json`: Temporária mostra Findante ou Repentina. Repentina deve aparecer em outros tipos quando aplicável; Findante é o nome mantido pelo autor. Extra multivalor é distinto do slot provisório de item. Não escrever Permanente por padrão.

Consultar `diretrizes-arte-por-carta.json`: personagem existente requer cena inédita em cada carta que o cita, com nova pose/ação/ambiente e identidade preservada. A arte do conjurador nunca é reaproveitada em outra carta. Amostras antigas desta prévia são referências de layout, não artes finais de novas cartas.

### Giro com perspectiva do campo

A suspensão usa a matriz `perspective(900px) rotateX(var(--table-tilt)) rotateZ(90deg)` no wrapper completo. O giro ocorre no plano do tabuleiro antes da projeção da câmera. Removida a inclinação duplicada da carta interna. Ataque/bloqueio usam `translate3d` para avanço e altura, mantendo a mesma projeção; preparar transiciona a matriz para giro zero. Rival conserva orientação interna de 180°. Ajustar `--table-tilt` à câmera final.

## Acabamento v5 — 2026-10-03

Lendária: arco-íris com dourado mais visível. Rara: prata e roxo. Incomum: iridescência azul/rosa/dourado da lendária anterior, refinada. CSS parametrizado por raridade, sem alterar regras. Reflexo fica abaixo dos campos de números/texto e limitado à área de arte. Movimento reduzido remove animação, preserva borda estática. Versos ocultos idênticos.

Ícones e ornamentos v5 têm filigranas, bordas em camadas e pequenos cristais; centro numérico livre. Overlays SVG/PNG v5 para o Canva; revisão anterior preservada. Detalhes/valores em acabamento-v5.json. A animação CSS não foi executada no navegador nesta revisão.

## Lendária v6 — textura sobre a face

O renderer público adiciona .foil-texture dentro de .front somente para data-rarity="lendaria". CSS usa vetores/foil-lendaria-v6.svg em soft-light, camada 3, cobrindo toda a face. Nome/regras/estatísticas ficam na camada 4 e recursos na 5. Reflexo dourado/arco-íris varia em 6 segundos; prefers-reduced-motion conserva uma película estática. Halo externo reduzido a 3px, borda com opacidade 0,4. Versos ocultos não recebem a camada.

Asset reutilizável: templates/foil-lendaria-v6.png, 1260 × 1760, alfa mínimo 0/máximo 139, quatro cantos com alfa 0. Origem procedural SVG exportada com Sharp, sem modificar as artes. Gerar novamente com node scripts/criar-foil-lendaria-v6.mjs. A animação não foi executada no navegador nesta revisão. Rascunhos Canva de atributos continuam v5.

## Reflexo diagonal lendário v7

Montar .foil-reflection como irmã de .foil-texture dentro da frente pública lendária. Ambas ficam na camada 3; textos/estatísticas na 4 e recursos na 5. A textura SVG v6 permanece, com opacidade 0,96. O reflexo screen tem uma faixa larga arco-íris/dourada (pico 0,66) e uma linha de luz dourada (pico 0,28), atravessando a face do canto superior esquerdo ao inferior direito a cada 5,2s. A camada independente preserva o brilho do screen sem atenuá-lo pelo soft-light da textura. Halo/borda seguem discretos. Movimento reduzido deixa reflexo estático. CSS: legendary-diagonal-v7; configuração: acabamento-v7.json. Refinamento da animação salvo, sem execução no navegador nesta revisão.

## Conjurador v8 — Valores e tipo

A esfera fica centralizada logo abaixo do nome, fora da caixa de texto: centro 315,116 e diâmetro 56 no viewBox 630×880. É pequena e discreta, dividida verticalmente. Cada metade contém cor e símbolo de um Valor, sem número. Usar ConjuradorDef.valores; aceita IDs em minúsculas ou nomes legíveis. ConjuradorDef.comprometimento alimenta a linha de tipo; opções verificadas: Invocador, Brigão, Parrudão da quebrada, Habilidoso, Preparado. descricaoComprometimento é uma descrição visual opcional, separada da habilidade/texto de regras. O Comprometimento não informado permanece como campo a preencher. A prévia de Ciantia usa Persona/Sublimeyn como exemplos de layout, sem afirmar identidade canônica. Para dados reais, mapear habilidade → texto e danoBasico → ataque.

Arquivos: conjurador-v8.json, templates/conjurador-v8.svg e overlay-conjurador-v8.svg/png (1260×1760, centro transparente); vetores/esfera-conjurador-v8.svg e valor-*-simbolo-v8.svg. Os campos textuais continuam editáveis. Editor local permite escolher o par, Comprometimento e descrição. O brilho v7 aprovado permanece. Canva não foi alterado nesta revisão.

## Raridades v9 — diretriz para Claude

Aplicar acabamento-v9.css após cartas.css. Inserir filtro SVG inline arcano-highlight-v9 e camadas do renderer: realce de pixels luminosos para lendárias/alternativas, abaixo da moldura e dos textos. Texturas em SVG/PNG e configuração em acabamento-v9.json. Carta oculta retorna antes de montar acabamento ou dados de variante. Alternativa usa cosmetico.arteAlternativa e conserva a raridade original; profundidade fica dentro da carta. Prévia oferece caixa Arte alternativa. Incomum herda rara v5; rara herda lendária v7 em prata/roxo; lendária recebe arco-íris/dourado mais fortes.

Pedidos/prompts devem indicar raridade e arteAlternativa. Raridade não altera números, efeitos, regras ou balanceamento. Artes lendárias/alternativas têm iluminação, brilho e textura na própria ilustração, segundo DIRETRIZ-ACABAMENTO-E-RARIDADE.md. Entrega por arquivos locais; leitura/integração pelo Claude não confirmadas. Animação/filtro não executados no navegador nesta revisão.

## Dois modelos narrativos e suspensão do conjurador

Modelos em ../cartas/modelos-visuais/2026-10-03-v1/modelos.json; artes, prompts, medidas/alfa e hash em manifesto.json. Exemplos visíveis em #modelos-cenarios, usando API create e acabamento v9. Não são definições mecânicas: números — e texto a preencher pelo Claude. Arte do dragão aprovada; Ciantia vigente v2, iluminação difusa/roupa preta-roxa. Ideias de cartas futuras em elementos-derivaveis.json. Suspender conjurador é opção de estudo visual em modelos-visuais-ui.js, com wrapper do conjurador próprio, via setSuspended; motor autoriza ações reais. Campos/estado são sincronizados com câmera e botão Levantar cartas. Animação não executada em navegador nesta revisão.

## Pedido atual do autor — cartas de Ciantia e movimentos do conjurador

Esta seção substitui o estado de simples estudos descrito acima: **as duas artes foram aprovadas** e agora são propostas para existirem no jogo. Ler **Arcanoclisma/cartas/propostas/ciantia-dragoes-2026-10-03.autor.md** e o JSON de mesmo nome para implementar a mecânica solicitada.

- **Dragão Transplanar**: Aliado lendário, Dragão, Extra exclusivo de Ciantia desenvolvida, presença de late game; jogar Dragão da mão sem pagar seus custos, exigindo Grau. Claude escolhe proteção, custos, estatísticas, ativação e como conciliar Aliado/Extra.
- **Lição do Jardim**: Temporária Findante, feitiço; Dragões podem se suspender para gerar 1 Arcano em velocidade Repentina. Até fim do turno contribuem para o número de Grau. Confirmar duração da concessão da primeira habilidade e demais parâmetros.
- O motor não foi alterado. Registrar decisões e integração no arquivo de resposta indicado na proposta. Não assumir custo zero nos campos —. Raridade só estética.
- A prévia tem botões **Atacar com conjurador** e **Bloquear com conjurador** chamando `animateMotion` no wrapper do próprio conjurador. Mesma suspensão de 90° no plano do tabuleiro, carta inteira, com ataque/bloqueio e retorno animados. Novas chamadas durante movimento retornam `reason: 'em_movimento'`; carta suspensa retorna `reason: 'suspensa'`. Preparar pode interromper/resetar pelo `setSuspended`. Conservar a perspectiva ao integrar; o motor define permissões e destino real.
- Publicação deste pacote autorizada pelo autor em 03/10/2026. Não há confirmação de recebimento pelo Claude. Animação não executada no navegador nesta revisão.
