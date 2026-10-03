# Arena e templates — Arcanoclisma

Abra `index.html` para a prévia local: campo ampliado nos dois eixos, duas perspectivas frontais, conjuradores centrais, modelos editáveis de cartas e itens.

- `raster/`: arena e verso com alfa medido; revisões preservadas.
- `templates/`: oito molduras SVG, quatro overlays transparentes e PDF com três itens.
- `vetores/`: ícones, contadores, baralhos e guias separados.
- `cartas.css`: suspensão ao atacar/bloquear e brilho progressivo por raridade.
- `arena.js`: composição visual e API `ArcanoclismaCardVisual`.
- `layout.json`: palco 1920 × 1440, posições conceituais editáveis.
- `modelos-cartas.json`: exemplos visuais; Canal, Armadura e Extra provisório.
- `mapa-canva.json`: IDs e estado dos rascunhos aguardando aprovação.
- `assets.json`: dimensões, hashes e alfa medido.
- `entrega-claude.md`: integração dos campos e movimentos.

Cartas em proporção 63:88 (63 × 88 mm), overlays 1260 × 1760. Rasters na resolução nativa registrada; SVGs escaláveis. Valores e raridade não encobrem os números. Exemplos não definem balanceamento.

Revisões do Canva aguardam aprovação após a prévia. Claude integra motor, rede e permissões. Publicação deste pacote no Git autorizada pelo autor em 03/10/2026.

## Pedido atual para Claude

As artes aprovadas agora são propostas de **Dragão Transplanar** e **Lição do Jardim**. Ler `../cartas/propostas/ciantia-dragoes-2026-10-03.autor.md` e seu JSON para definir números, redação e integração. A arena inclui ataque e bloqueio animados do conjurador; consultar `entrega-claude.md`.

## Revisão v4

Suspender é deitar 90°: ataque e bloqueio mantêm a carta deitada até preparar. Botão Levantar cartas demonstra o retorno à vertical. Indicadores/nome com acabamento moderado, símbolos de Valor e Grau mais presentes. Temporárias identificam Findante/Repentina; seletor de velocidade disponível no inspetor. Lendária tem iridescência própria, cosmética. Estado de cartas e aprovação dos rascunhos dependem, respectivamente, do motor e do autor. Arte nova por carta obrigatória para personagens existentes.

## Película lendária v6

Textura holográfica dourada/arco-íris sobre a face, com microtrama e reflexo animado. Asset vetorial: vetores/foil-lendaria-v6.svg; exportação semitransparente: templates/foil-lendaria-v6.png (1260 × 1760). A configuração está em acabamento-v6.json. Overlays Canva de atributos continuam v5.
