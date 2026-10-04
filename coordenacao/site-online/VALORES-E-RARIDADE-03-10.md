# Direção atual do autor: Valores, itens e raridade

## Divisão de trabalho

Codex: ilustrações, templates, esferas, animações e assets. Entrega visual v12 em `Arcanoclisma/online/public/js/identidade-valor-v12.js`, CSS correspondente e `public/kit/templates/valores-v12/`. Claude: catálogo, regras, editor, triggers, custos, keywords e integração. Não editar os mesmos arquivos simultaneamente. Os ajustes visuais de `public/js/carta.js` desta entrega pertencem ao Codex; editor, motor e dados permanecem com Claude.

## Raridade

- Sem raridade automática no app. A escolha pertence ao autor no editor.
- O bloqueio atual é de aplicação: **não apagar** texturas, filtros, animações ou padrões de brilho existentes.
- Ao definir uma raridade, reutilizar o acabamento correspondente. Raridade continua exclusivamente estética.
- Não herdar raridade de um briefing antigo ou de um tipo de carta, nem marcar todo Conjurador como lendário.
- Artes existentes e seus arquivos originais continuam preservados. Não refazer uma arte só para remover reflexos já pintados.

## Templates e Valores

- Toda carta não equipada tem Valor; não existe carta neutra. Somente equipamentos podem ser genéricos, com template cinza. Equipamentos exclusivos têm o(s) Valor(es) definido(s) pelo catálogo.
- Moldura discreta na cor do Valor, sem cobrir a ilustração. Esfera do custo com número legível e setores/símbolos para todos os Valores da carta. Conjurador mantém esfera centralizada sob o nome.
- O desenho suporta um a nove Valores; a interface não inventa nem atribui Valores.
- Diretriz de composição do catálogo: normalmente um Valor; dois aproximadamente a cada dez cartas; três a cada cinquenta; quatro a cada 250; cinco a cada 1.250; seis a cada 6.250; sete a cada 31.250; oito a cada 156.250; nove a cada 781.250. Claude determina as cartas e exceções; não sortear Valores em runtime.
- Cartas de um personagem usam apenas os Valores dele, salvo exceção explícita de regra.
- Mostrar `Item — Canal`, `Item — Armadura` ou `Item — Extra`, preservando subtipos. Temporárias mostram Findante ou Repentina conforme a velocidade registrada.

## Solicitações para Claude

1. Canal admite Armas, Foco Arcano e Transcritores. Foco Arcano altera poderes/efeitos de feitiços; Transcritor trabalha baralho, mão, compra, descarte e redução de custos.
2. Itens podem atender early, mid ou late game; ativações combinam suspensão do personagem e outro custo, como descarte, Arcano ou dano. Extras complementam Competências: invocações temáticas, luvas de força e similares. Implementar custos e efeitos reais no motor, não apenas texto visual.
3. Quando o autor pedir verificar atualizações do site após editar texto/valores, confrontar texto, modelo de efeitos e triggers. Atualizar mecanicamente quando necessário; não presumir que mudar uma descrição já implementou seu efeito.
4. Disponibilizar a aba de keywords (Etéreo, Eficiente etc.), com as definições reais. Não inventar equivalências.
5. Enviar pedidos de novas cartas mais variados: mortos-vivos, feitiços temporários, mais Repentinas, Místicas, estruturas e ambientes domésticos/casas. Distribuir feitiços entre Valores distintos. Codex cria ilustrações novas com ambientação e identidade consistente depois de receber os IDs, descrição e dados de Claude; não reaproveitar um recorte como carta pronta.

## Publicação e expansão

O autor reafirmou aprovação de todos os pushes das imagens. Publicar as imagens prontas e seus registros com o estado real de revisão; autorização de push não equivale à aprovação final de cada arte nova. A fila atual tem 151 artes; a entrega parcial presente tem 82 prontas. Não apresentar a fila inteira como concluída.

**A Mestra dos Dragões**, 90 cartas, continua bloqueada para inclusão em jogo e alteração de cartas até liberação específica do autor. Push das artes não libera a expansão.

## Uso visual

`desenharCarta` chama `aplicarIdentidadeValor(carta, modelo)` depois de montar a frente. O módulo só lê o modelo visível; cartas ocultas mantêm o verso. Não altera recursos, custos ou estado da partida.

As 75 molduras SVG preservam a geometria aprovada de 63 × 88 mm. `arena/scripts/gerar-templates-valor-v12.mjs` regenera os arquivos; eles ficam versionados no destino público e não dependem de uma IA para funcionar. O módulo carrega seu CSS uma vez. O construtor atual mantém estes arquivos públicos; se a rotina de build passar a limpar o kit, chamar o gerador depois da limpeza.

Entrega de código e assets; sem conferência no navegador nesta etapa.
