# 25 — Prompts de arte: o catálogo (Fase 4.5)

> **521 imagens**, uma por arma, armadura, item, artefato, terreno, lugar, Reino, povo, dragão e personagem do bestiário. **92 são do livro** (marcadas **livro · gerar primeiro**); as outras são do app do gerador ([[22 O que é gerado]], P-125). Gerado por `arte/prompts/gerar_prompts.py` a partir dos arquivos do livro; o prompt completo (estilo + específico) está em `arte/prompts/catalogo.json`. Não edite à mão: mude o arquivo de origem e rode o script de novo. Personagens das sessões continuam em [[20 Prompts de arte - personagens]].

## Como fazer

1. Para cada linha, gere a imagem com o **prompt completo** do `catalogo.json` (ou: o **estilo** da linha, abaixo, seguido do texto da linha). Canva, *generate-image*, no **formato** da linha (1:1 ou paisagem 3:2).
2. Salve como **`<id>.png`** (o id da linha) em `arte/img/<tipo>/` (ex.: `arte/img/dragao/dragao-lamina.png`).
3. Nas imagens 1:1 o fundo é branco liso: tire o fundo antes de usar (4.6). Cenários (3:2) ficam inteiros.
4. Confira uma a uma: é o objeto ou o ser certo, inteiro, sem texto nem letras inventadas, no traço do estilo e na cor do Valor. Se estiver ruim (cortado, com cenário, com mão, com texto), gere de novo em vez de ajustar.
5. Se o filtro barrar, troque só a palavra que parece violenta (arma, sangue, morte) por uma mais neutra (ferramenta, marca, sono) e gere de novo.
6. Para testar em outra IA: `arte/prompts/descricoes_das_imagens.md` tem a **descrição escrita completa de cada imagem, em português** (assunto + estilo + fundo), pronta para colar.

## Estilos

| Código | Para | Texto (vai antes de cada prompt) |
|---|---|---|
| **P** | Personagem e criatura (1:1) | Bold graphic fantasy RPG illustration in a thick expressive ink style: heavy black outlines, flat vibrant colors with hard block shadows, dry-brush paper texture, stylized exaggerated proportions, comic-book cel shading, not realistic, not 3D, not a painting. Full body character, dynamic pose, strong facial expression. Plain solid white background, no scenery, no text, no logo. Subject (in Portuguese): |
| **D** | Dragão (1:1) | Bold graphic fantasy RPG illustration in a thick expressive ink style: heavy black outlines, flat vibrant colors with hard block shadows, dry-brush paper texture, stylized exaggerated proportions, comic-book cel shading, not realistic, not 3D, not a painting. An imposing, noble full body dragon seen slightly from below, head raised, chest proud, huge wingspan, majestic even when threatening. It is an ancient desire that became flesh and scales, complete, alive and full of soul: made of order and change at once, part of its body whole, closed and symmetrical, part cracked open with light leaking through and shapes shifting. Slender elegant body with a long neck and long tail. Soft, smooth, organic scales: small overlapping scales that flow with the curves of the body like the skin of a snake or a lizard, drawn with few simple strokes and soft gradients of flat color; NOT stone, NOT rock, NOT armor plates, NOT rigid or artificial; only a few spines. A limited palette of two or three hues plus one glowing accent color. The flames are NOT realistic fire: they are ghostly, translucent, stylized flame shapes drawn as clean flat graphic ribbons and smoke-like curls with simple edges, flowing in long elegant curves that trace and frame the silhouette of the body like a spectral mantle. Their color, shape and behavior follow the dragon's Value, as described in the subject. They appear only in a few places: the wing tips, the tip of the tail, the crest along the spine and the eyes, where the body seems to dissolve into flame; plus a few thin glowing veins along the scales. Deep expressive eyes that carry emotional weight, love, fear or longing, looking at the viewer as if mortals were passing seasons. Plain light gray background, no scenery, no text, no logo, no signature. Subject (in Portuguese): |
| **I** | Item (1:1) | Bold graphic fantasy RPG illustration in a thick expressive ink style: heavy black outlines, flat vibrant colors with hard block shadows, dry-brush paper texture, stylized exaggerated proportions, comic-book cel shading, not realistic, not 3D, not a painting. A single isolated object, whole and centered, three-quarter view, signs of use. Plain solid white background, no hand, no character, no text, no logo. Subject (in Portuguese): |
| **C** | Cenário (paisagem 3:2) | Fantasy RPG environment illustration in a bold graphic ink style: heavy black outlines, flat vibrant colors with hard block shadows, dry-brush paper texture, wide establishing view, dramatic light, chapter-opening mood, no characters in the foreground, no text, no logo. Subject (in Portuguese): |

## Lista

### Armas (16)

- **Adaga** · Toque · d8+1 · Arremessável (Perto) · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `adaga`  
  > adaga curta de lâmina larga, cabo de couro gasto.
- **Espada curta** · Toque · d8+2 · Precisa · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `espada-curta`  
  > espada curta reta e equilibrada, guarda simples.
- **Espada longa** · Toque · d10+3 · — · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `espada-longa`  
  > espada longa de duas mãos, lâmina comprida e guarda em cruz.
- **Machado** · Toque · d10+2 · Cruel (dano Grave: +1 PV) · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `machado`  
  > machado de uma mão com lâmina curva e cabo de madeira escura.
- **Martelo de guerra** · Toque · d12+3 · Pesada (−1 Esquiva), Atordoante · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `martelo-de-guerra`  
  > martelo de guerra enorme de duas mãos, cabeça maciça de metal amassado.
- **Lança** · Perto · d10+2 · Alcance · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `lanca`  
  > lança longa com ponta de folha e fitas amarradas perto da ponta.
- **Chicote** · Perto · d6+2 · Desarmar, Puxar · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `chicote`  
  > chicote de couro trançado enrolado, ponta com nó de metal.
- **Cajado** · Toque · d8+1 · Serve de foco arcano · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `cajado`  
  > cajado de madeira nodosa, alto como uma pessoa, com uma pedra presa no topo.
- **Soqueira** · Toque · d6+2 · Não pode ser desarmada · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `soqueira`  
  > soqueira de metal com quatro anéis grossos e rebites.
- **Funda** · Médio · d6+1 · Barata (munição de graça) · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `funda`  
  > funda de couro com tiras compridas e uma pedra lisa no bolsão.
- **Arco curto** · Longe · d8+2 · — · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `arco-curto`  
  > arco curto de madeira clara com corda tensa.
- **Arco longo** · Longe · d10+3 · Pesada · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `arco-longo`  
  > arco longo, alto, de madeira escura laminada.
- **Besta** · Longe · d10+2 · Recarga · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `besta`  
  > besta de madeira e ferro com manivela de recarga.
- **Pistola** · Médio · d10+2 · Barulhenta, Recarga · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `pistola`  
  > pistola tecnomaga de latão e madeira, cano curto com runas, engrenagens à mostra.
- **Rifle** · Longe · d12+3 · Barulhenta, Recarga · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `rifle`  
  > rifle tecnomago comprido de latão e madeira, cano com runas e mira de lente.
- **Espada de Bronze Arco** · metal arcano comum · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `espada-de-bronze-arco`  
  > espada longa de Bronze Arco, metal dourado-escuro, com runas gravadas ao longo da lâmina acendendo em luz vermelho-ferrugem.

### Armaduras (3)

- **Armadura Leve** · Proteção · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `armadura-leve`  
  > armadura leve de couro e gibão acolchoado, casaco reforçado, exposta num manequim de madeira simples.
- **Armadura Média** · Proteção · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `armadura-media`  
  > armadura média de cota de malha e escamas de metal, exposta num manequim de madeira simples.
- **Armadura Pesada** · Proteção · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `armadura-pesada`  
  > armadura pesada de placas de metal com rebites e amassados, exposta num manequim de madeira simples.

### Itens mágicos (Relíquias) (26)

- **Luva do Estrangulador** · Palavra (Dominum) · M1 · estilo **I** · 1:1  
  id `luva-do-estrangulador`  
  > Luva do Estrangulador: objeto mágico (uma palavra de Valor gravada, brilhando de leve, brilho de magia roxo-sangue).
- **Espelho de Bolso** · Palavra (Alvorum) · M1 · estilo **I** · 1:1  
  id `espelho-de-bolso`  
  > Espelho de Bolso: objeto mágico (uma palavra de Valor gravada, brilhando de leve, brilho de magia dourada e preta).
- **Colar de Dente de Lobo** · Palavra (Primitivum) · M1 · estilo **I** · 1:1  
  id `colar-de-dente-de-lobo`  
  > Colar de Dente de Lobo: objeto mágico (uma palavra de Valor gravada, brilhando de leve, brilho de magia verde-musgo e âmbar).
- **Pena de Mentira** · Palavra (Cognitium) · M2 · estilo **I** · 1:1  
  id `pena-de-mentira`  
  > Pena de Mentira: objeto mágico (uma palavra de Valor gravada, brilhando de leve, brilho de magia índigo).
- **Anel de Areia Parada** · Palavra (Eternyr) · M3 · estilo **I** · 1:1  
  id `anel-de-areia-parada`  
  > Anel de Areia Parada: objeto mágico (uma palavra de Valor gravada, brilhando de leve, brilho de magia azul-noite com riscos brancos).
- **Botas de Salto do Gato** · Ofício · M1 · estilo **I** · 1:1  
  id `botas-de-salto-do-gato`  
  > Botas de Salto do Gato: objeto mágico (útil e bem-feito, de artesão).
- **Luvas de Veludo Preto** · Ofício · M1 · estilo **I** · 1:1  
  id `luvas-de-veludo-preto`  
  > Luvas de Veludo Preto: objeto mágico (útil e bem-feito, de artesão).
- **Broche do Anfitrião** · Ofício · M2 · estilo **I** · 1:1  
  id `broche-do-anfitriao`  
  > Broche do Anfitrião: objeto mágico (útil e bem-feito, de artesão).
- **Colete de Pele de Pedra** · Ofício · M2 · estilo **I** · 1:1  
  id `colete-de-pele-de-pedra`  
  > Colete de Pele de Pedra: objeto mágico (útil e bem-feito, de artesão).
- **Olho de Vidro do Vigia** · Ofício · M3 · estilo **I** · 1:1  
  id `olho-de-vidro-do-vigia`  
  > Olho de Vidro do Vigia: objeto mágico (útil e bem-feito, de artesão).
- **Fita do Equilíbrio** · Estabilidade · M1 · estilo **I** · 1:1  
  id `fita-do-equilibrio`  
  > Fita do Equilíbrio: objeto mágico (calmo e equilibrado, brilho sereno).
- **Sino de Contenção** · Estabilidade · M2 · estilo **I** · 1:1  
  id `sino-de-contencao`  
  > Sino de Contenção: objeto mágico (calmo e equilibrado, brilho sereno).
- **Pedra-Par** · Estabilidade · M3 · estilo **I** · 1:1  
  id `pedra-par`  
  > Pedra-Par: objeto mágico (calmo e equilibrado, brilho sereno).
- **Âncora de Sal** · Estabilidade · M4 · estilo **I** · 1:1  
  id `ancora-de-sal`  
  > Âncora de Sal: objeto mágico (calmo e equilibrado, brilho sereno).
- **Bainha Vermelha** · Valor (Conflitüo) · M2 · estilo **I** · 1:1  
  id `bainha-vermelha`  
  > Bainha Vermelha: objeto mágico (brilho de magia vermelho-ferrugem).
- **Ampulheta Rachada** · Valor (Eternyr) · M2 · estilo **I** · 1:1  
  id `ampulheta-rachada`  
  > Ampulheta Rachada: objeto mágico (brilho de magia azul-noite com riscos brancos).
- **Máscara do Palhaço Triste** · Valor (Persona) · M2 · estilo **I** · 1:1  
  id `mascara-do-palhaco-triste`  
  > Máscara do Palhaço Triste: objeto mágico (brilho de magia coral).
- **Lanterna Fechada** · Valor (Alvorum) · M2 · estilo **I** · 1:1  
  id `lanterna-fechada`  
  > Lanterna Fechada: objeto mágico (brilho de magia dourada e preta).
- **Moeda do Barqueiro** · Valor (Morbidyum) · M3 · estilo **I** · 1:1  
  id `moeda-do-barqueiro`  
  > Moeda do Barqueiro: objeto mágico (brilho de magia verde-pálida).
- **Cadinho de Bolso** · Valor (Sublimeyn) · M3 · estilo **I** · 1:1  
  id `cadinho-de-bolso`  
  > Cadinho de Bolso: objeto mágico (brilho de magia prata-mercúrio).
- **Coleira de Prata** · Valor (Dominum) · M3 · estilo **I** · 1:1  
  id `coleira-de-prata`  
  > Coleira de Prata: objeto mágico (brilho de magia roxo-sangue).
- **Máscara de Casca** · Valor (Primitivum) · M3 · estilo **I** · 1:1  
  id `mascara-de-casca`  
  > Máscara de Casca: objeto mágico (brilho de magia verde-musgo e âmbar).
- **Véu de Seda Cinza** · Valor (Cognitium) · M3 · estilo **I** · 1:1  
  id `veu-de-seda-cinza`  
  > Véu de Seda Cinza: objeto mágico (brilho de magia índigo).
- **Luva de Forja** · Grau · M1 · estilo **I** · 1:1  
  id `luva-de-forja`  
  > Luva de Forja: objeto mágico (carregado de poder, brilho forte).
- **Tinteiro de Duas Penas** · Grau · M2 · estilo **I** · 1:1  
  id `tinteiro-de-duas-penas`  
  > Tinteiro de Duas Penas: objeto mágico (carregado de poder, brilho forte).
- **Coroa Pequena** · Grau · M4 · estilo **I** · 1:1  
  id `coroa-pequena`  
  > Coroa Pequena: objeto mágico (carregado de poder, brilho forte).

### Consumíveis (7)

- **Tônico de Sangue** · recupera 1d4 PV · estilo **I** · 1:1  
  id `tonico-de-sangue`  
  > Tônico de Sangue: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Chá de Casca-Amarga** · limpa 1d4 Cansaço · estilo **I** · 1:1  
  id `cha-de-casca-amarga`  
  > Chá de Casca-Amarga: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Pó de Arcano** · +1 Arcano (um por Evento) · estilo **I** · 1:1  
  id `po-de-arcano`  
  > Pó de Arcano: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Óleo de Lâmina** · próximo ataque com arma +1d6 · estilo **I** · 1:1  
  id `oleo-de-lamina`  
  > Óleo de Lâmina: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Bomba de Fumaça** · Vantagem para fugir ou se esconder · estilo **I** · 1:1  
  id `bomba-de-fumaca`  
  > Bomba de Fumaça: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Tinta de Palavra** · escreva uma palavra de qualquer Valor num objeto; vale uma vez · estilo **I** · 1:1  
  id `tinta-de-palavra`  
  > Tinta de Palavra: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.
- **Sal do Equilíbrio** · depois de rolar, troque a paridade de um dado · estilo **I** · 1:1  
  id `sal-do-equilibrio`  
  > Sal do Equilíbrio: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.

### Montarias (3)

- **Cavalo** · leva até Longe fora de combate · estilo **P** · 1:1  
  id `cavalo`  
  > Cavalo de viagem, arreado e pronto, com alforjes; leva até Longe fora de combate.
- **Mula de carga** · carrega muito · estilo **P** · 1:1  
  id `mula-de-carga`  
  > Mula de carga de viagem, arreado e pronto, com alforjes; carrega muito.
- **Barco pequeno** · atravessa água · estilo **I** · 1:1  
  id `barco-pequeno`  
  > Barco pequeno de viagem, arreado e pronto, com alforjes; atravessa água.

### Materiais (10)

- **Aço Riscado** · Conflitüo · estilo **I** · 1:1  
  id `aco-riscado`  
  > Aço Riscado: material raro de fantasia, um pedaço bruto, com brilho de magia vermelho-ferrugem.
- **Cristal de Estado** · Sublimeyn · estilo **I** · 1:1  
  id `cristal-de-estado`  
  > Cristal de Estado: material raro de fantasia, um pedaço bruto, com brilho de magia prata-mercúrio.
- **Osso de Fera Domada** · Dominum · estilo **I** · 1:1  
  id `osso-de-fera-domada`  
  > Osso de Fera Domada: material raro de fantasia, um pedaço bruto, com brilho de magia roxo-sangue.
- **Lágrima Seca** · Persona · estilo **I** · 1:1  
  id `lagrima-seca`  
  > Lágrima Seca: material raro de fantasia, um pedaço bruto, com brilho de magia coral.
- **Pena de Eclipse** · Alvorum · estilo **I** · 1:1  
  id `pena-de-eclipse`  
  > Pena de Eclipse: material raro de fantasia, um pedaço bruto, com brilho de magia dourada e preta.
- **Pó de Túmulo** · Morbidyum · estilo **I** · 1:1  
  id `po-de-tumulo`  
  > Pó de Túmulo: material raro de fantasia, um pedaço bruto, com brilho de magia verde-pálida.
- **Tinta do Esquecimento** · Cognitium · estilo **I** · 1:1  
  id `tinta-do-esquecimento`  
  > Tinta do Esquecimento: material raro de fantasia, um pedaço bruto, com brilho de magia índigo.
- **Areia Parada** · Eternyr · estilo **I** · 1:1  
  id `areia-parada`  
  > Areia Parada: material raro de fantasia, um pedaço bruto, com brilho de magia azul-noite com riscos brancos.
- **Resina Viva** · Primitivum · estilo **I** · 1:1  
  id `resina-viva`  
  > Resina Viva: material raro de fantasia, um pedaço bruto, com brilho de magia verde-musgo e âmbar.
- **Âmbar de Cataclisma** · (qualquer) · estilo **I** · 1:1  
  id `ambar-de-cataclisma`  
  > Âmbar de Cataclisma: material raro de fantasia, um pedaço bruto, âmbar escuro com uma rachadura que brilha em ferrugem.

### Itens mágicos do dia a dia (36)

- **Anel de Comer Pedra** · d66 11 · estilo **I** · 1:1  
  id `anel-de-comer-pedra`  
  > Anel de Comer Pedra: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: você mastiga e digere pedra, metal mole e vidro.
- **Mochila Sem Fundo** · d66 12 · estilo **I** · 1:1  
  id `mochila-sem-fundo`  
  > Mochila Sem Fundo: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: cabe tudo que passar pela boca dela.
- **Sapatos do Silêncio** · d66 13 · estilo **I** · 1:1  
  id `sapatos-do-silencio`  
  > Sapatos do Silêncio: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: invisível enquanto não falar nada.
- **Cajado Falante** · d66 14 · estilo **I** · 1:1  
  id `cajado-falante`  
  > Cajado Falante: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: conversa, sabe história local, tem opinião.
- **Chaleira Sempre Quente** · d66 15 · estilo **I** · 1:1  
  id `chaleira-sempre-quente`  
  > Chaleira Sempre Quente: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: a água ferve na hora, em qualquer lugar.
- **Lanterna de Vaga-lume** · d66 16 · estilo **I** · 1:1  
  id `lanterna-de-vaga-lume`  
  > Lanterna de Vaga-lume: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: acende quando alguém tem medo por perto.
- **Luvas do Oleiro** · d66 21 · estilo **I** · 1:1  
  id `luvas-do-oleiro`  
  > Luvas do Oleiro: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: molda barro, cera e massa como se fossem de nuvem.
- **Cachecol que Abraça** · d66 22 · estilo **I** · 1:1  
  id `cachecol-que-abraca`  
  > Cachecol que Abraça: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: aquece como fogueira.
- **Colher da Fome Certa** · d66 23 · estilo **I** · 1:1  
  id `colher-da-fome-certa`  
  > Colher da Fome Certa: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: qualquer comida vira a sua favorita.
- **Chave de Qualquer Porta de Casa** · d66 24 · estilo **I** · 1:1  
  id `chave-de-qualquer-porta-de-casa`  
  > Chave de Qualquer Porta de Casa: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: abre porta de moradia (não de cofre, prisão, templo).
- **Espelho de Ontem** · d66 25 · estilo **I** · 1:1  
  id `espelho-de-ontem`  
  > Espelho de Ontem: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: mostra o lugar como era ontem, a esta hora.
- **Corda Obediente** · d66 26 · estilo **I** · 1:1  
  id `corda-obediente`  
  > Corda Obediente: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: amarra e desamarra ao comando.
- **Pena que Escreve Sozinha** · d66 31 · estilo **I** · 1:1  
  id `pena-que-escreve-sozinha`  
  > Pena que Escreve Sozinha: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: escreve o que é ditado, em qualquer idioma que você fale.
- **Travesseiro de Sonho Emprestado** · d66 32 · estilo **I** · 1:1  
  id `travesseiro-de-sonho-emprestado`  
  > Travesseiro de Sonho Emprestado: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem dorme nele sonha o sonho de quem dormiu antes.
- **Bússola do Que Falta** · d66 33 · estilo **I** · 1:1  
  id `bussola-do-que-falta`  
  > Bússola do Que Falta: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: aponta para o que você mais precisa agora.
- **Guarda-chuva de Sol** · d66 34 · estilo **I** · 1:1  
  id `guarda-chuva-de-sol`  
  > Guarda-chuva de Sol: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: faz sombra fresca de verdade em qualquer calor.
- **Moeda de Duas Caras** · d66 35 · estilo **I** · 1:1  
  id `moeda-de-duas-caras`  
  > Moeda de Duas Caras: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: ao jogar, cai no lado que você precisa (só para decisões, não para rolagens).
- **Sino de Chamar Bicho** · d66 36 · estilo **I** · 1:1  
  id `sino-de-chamar-bicho`  
  > Sino de Chamar Bicho: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: todo animal pequeno Perto vem até você.
- **Óculos de Ler Letra Apagada** · d66 41 · estilo **I** · 1:1  
  id `oculos-de-ler-letra-apagada`  
  > Óculos de Ler Letra Apagada: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: lê qualquer escrita apagada, queimada ou raspada.
- **Cantil do Rio** · d66 42 · estilo **I** · 1:1  
  id `cantil-do-rio`  
  > Cantil do Rio: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: nunca esvazia de água limpa.
- **Vela de Uma Pergunta** · d66 43 · estilo **I** · 1:1  
  id `vela-de-uma-pergunta`  
  > Vela de Uma Pergunta: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: acesa, um morto Perto responde uma pergunta.
- **Manto de Cor de Parede** · d66 44 · estilo **I** · 1:1  
  id `manto-de-cor-de-parede`  
  > Manto de Cor de Parede: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: fica da cor do que está atrás.
- **Dedal do Alfaiate Cego** · d66 45 · estilo **I** · 1:1  
  id `dedal-do-alfaiate-cego`  
  > Dedal do Alfaiate Cego: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: costura sozinho qualquer roupa rasgada.
- **Pente das Mil Caras** · d66 46 · estilo **I** · 1:1  
  id `pente-das-mil-caras`  
  > Pente das Mil Caras: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: penteia o cabelo e o rosto muda um pouco (não disfarça totalmente).
- **Tinteiro de Mapas** · d66 51 · estilo **I** · 1:1  
  id `tinteiro-de-mapas`  
  > Tinteiro de Mapas: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: a tinta desenha o mapa do lugar onde você está.
- **Cadeado Ciumento** · d66 52 · estilo **I** · 1:1  
  id `cadeado-ciumento`  
  > Cadeado Ciumento: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: nada que ele tranca é aberto por outra pessoa.
- **Sal do Viajante** · d66 53 · estilo **I** · 1:1  
  id `sal-do-viajante`  
  > Sal do Viajante: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: uma pitada e qualquer comida estragada fica boa.
- **Apito de Eco Longe** · d66 54 · estilo **I** · 1:1  
  id `apito-de-eco-longe`  
  > Apito de Eco Longe: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: o som sai de um lugar à sua escolha até Longe.
- **Relógio de Bolso Atrasado** · d66 55 · estilo **I** · 1:1  
  id `relogio-de-bolso-atrasado`  
  > Relógio de Bolso Atrasado: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: sabe sempre quanto falta para o pôr do sol.
- **Bota de Andar em Parede** · d66 56 · estilo **I** · 1:1  
  id `bota-de-andar-em-parede`  
  > Bota de Andar em Parede: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: anda na vertical como no chão.
- **Dado Honesto** · d66 61 · estilo **I** · 1:1  
  id `dado-honesto`  
  > Dado Honesto: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem joga não consegue mentir enquanto o dado gira.
- **Garrafa de Voz** · d66 62 · estilo **I** · 1:1  
  id `garrafa-de-voz`  
  > Garrafa de Voz: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: guarda uma frase dita dentro; toca quando aberta.
- **Chapéu de Chuva Particular** · d66 63 · estilo **I** · 1:1  
  id `chapeu-de-chuva-particular`  
  > Chapéu de Chuva Particular: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: chove só em cima de quem você apontar (Perto).
- **Lenço das Lágrimas** · d66 64 · estilo **I** · 1:1  
  id `lenco-das-lagrimas`  
  > Lenço das Lágrimas: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: enxuga e a pessoa esquece do que chorava por uma hora.
- **Balde que Sobe** · d66 65 · estilo **I** · 1:1  
  id `balde-que-sobe`  
  > Balde que Sobe: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: sobe sozinho de poço, penhasco ou janela.
- **Sementes de Um Dia** · d66 66 · estilo **I** · 1:1  
  id `sementes-de-um-dia`  
  > Sementes de Um Dia: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: plantadas, viram árvore em um dia.

### Maravilhas (36)

- **Porta de Bolso** · d66 11 · estilo **I** · 1:1  
  id `porta-de-bolso`  
  > Porta de Bolso: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: uma porta dobrável que abre para um cômodo que só você conhece.
- **Coroa Sem Rei** · d66 12 · estilo **I** · 1:1  
  id `coroa-sem-rei`  
  > Coroa Sem Rei: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem usa é obedecido em coisas pequenas por quem não o conhece.
- **Capa de Vento** · d66 13 · estilo **I** · 1:1  
  id `capa-de-vento`  
  > Capa de Vento: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: voa enquanto houver vento.
- **Violino do Ontem** · d66 14 · estilo **I** · 1:1  
  id `violino-do-ontem`  
  > Violino do Ontem: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem ouve lembra de algo que tinha esquecido.
- **Máscara do Outro** · d66 15 · estilo **I** · 1:1  
  id `mascara-do-outro`  
  > Máscara do Outro: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem usa tem o rosto e a voz da última pessoa que tocou nela.
- **Barco Dobrável** · d66 16 · estilo **I** · 1:1  
  id `barco-dobravel`  
  > Barco Dobrável: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: vira barco de 4 lugares ao tocar água.
- **Ovo de Wyvern** · d66 21 · estilo **I** · 1:1  
  id `ovo-de-wyvern`  
  > Ovo de Wyvern: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: choca em 30 dias; a wyvern nasce leal a quem estava perto.
- **Olho de Dragão Pálido** · d66 22 · estilo **I** · 1:1  
  id `olho-de-dragao-palido`  
  > Olho de Dragão Pálido: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: vê através de carne, mostrando ossos e doença.
- **Escama do Dragão-Mercúrio** · d66 23 · estilo **I** · 1:1  
  id `escama-do-dragao-mercurio`  
  > Escama do Dragão-Mercúrio: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: muda de estado ao comando: faca, gota, fumaça.
- **Livro Que Lê Você** · d66 24 · estilo **I** · 1:1  
  id `livro-que-le-voce`  
  > Livro Que Lê Você: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: responde perguntas sobre você mesmo com verdade.
- **Ampulheta de Um Minuto** · d66 25 · estilo **I** · 1:1  
  id `ampulheta-de-um-minuto`  
  > Ampulheta de Um Minuto: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: vira, e o último minuto se repete para você (só você lembra).
- **Tapete de Duas Casas** · d66 26 · estilo **I** · 1:1  
  id `tapete-de-duas-casas`  
  > Tapete de Duas Casas: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem pisa nele sai no outro tapete gêmeo, onde estiver.
- **Jaula de Luz** · d66 31 · estilo **I** · 1:1  
  id `jaula-de-luz`  
  > Jaula de Luz: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: prende qualquer coisa do tamanho de um cão enquanto houver luz.
- **Chave da Voragem** · d66 32 · estilo **I** · 1:1  
  id `chave-da-voragem`  
  > Chave da Voragem: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: abre qualquer jaula, coleira ou corrente.
- **Mapa do Reino** · d66 33 · estilo **I** · 1:1  
  id `mapa-do-reino`  
  > Mapa do Reino: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: desenha o caminho até a porta de um Reino.
- **Pena de Idrael** · d66 34 · estilo **I** · 1:1  
  id `pena-de-idrael`  
  > Pena de Idrael: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: acordar alguém de qualquer sono mágico.
- **Semente da Floresta Primeira** · d66 35 · estilo **I** · 1:1  
  id `semente-da-floresta-primeira`  
  > Semente da Floresta Primeira: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: vira um bosque que anda e protege o lugar por uma noite.
- **Espada Que Não Corta** · d66 36 · estilo **I** · 1:1  
  id `espada-que-nao-corta`  
  > Espada Que Não Corta: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: atravessa pessoas sem ferir e corta o que as prende (medo, feitiço, corrente).
- **Cálice do Primeiro Barro** · d66 41 · estilo **I** · 1:1  
  id `calice-do-primeiro-barro`  
  > Cálice do Primeiro Barro: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: o que se põe dentro vira outro material à escolha, do mesmo tamanho.
- **Anel de Tique** · d66 42 · estilo **I** · 1:1  
  id `anel-de-tique`  
  > Anel de Tique: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: o dia de hoje pode ser vivido de novo, uma única vez.
- **Espelho da Cidade dos Rostos** · d66 43 · estilo **I** · 1:1  
  id `espelho-da-cidade-dos-rostos`  
  > Espelho da Cidade dos Rostos: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: mostra o que a pessoa sente, como clima em volta dela.
- **Sino dos Juramentados** · d66 44 · estilo **I** · 1:1  
  id `sino-dos-juramentados`  
  > Sino dos Juramentados: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem ouve não foge da luta.
- **Luneta do Fim** · d66 45 · estilo **I** · 1:1  
  id `luneta-do-fim`  
  > Luneta do Fim: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: mostra onde e como alguém vai morrer, se nada mudar.
- **Cordão de Rhu** · d66 46 · estilo **I** · 1:1  
  id `cordao-de-rhu`  
  > Cordão de Rhu: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: o dono entende e é entendido por lobos.
- **Pedra Parada** · d66 51 · estilo **I** · 1:1  
  id `pedra-parada`  
  > Pedra Parada: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: onde ela é posta, nada muda por uma hora (fogo não espalha, água não sobe, ninguém envelhece).
- **Estilhaço de Disrupto** · d66 52 · estilo **I** · 1:1  
  id `estilhaco-de-disrupto`  
  > Estilhaço de Disrupto: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: onde é jogado, tudo muda de forma por um instante.
- **Trono Dobrável** · d66 53 · estilo **I** · 1:1  
  id `trono-dobravel`  
  > Trono Dobrável: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem senta nele é ouvido por todos na sala.
- **Harpa de Vidro de Liss** · d66 54 · estilo **I** · 1:1  
  id `harpa-de-vidro-de-liss`  
  > Harpa de Vidro de Liss: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: a música endurece o ar em escadas e pontes enquanto toca.
- **Relicário da Terceira Vida** · d66 55 · estilo **I** · 1:1  
  id `relicario-da-terceira-vida`  
  > Relicário da Terceira Vida: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: guarda uma respiração; devolve a quem acabou de morrer.
- **Balança de Serath** · d66 56 · estilo **I** · 1:1  
  id `balanca-de-serath`  
  > Balança de Serath: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem discute sobre ela só consegue dizer o que acha de verdade.
- **Colar de Vyal (falso)** · d66 61 · estilo **I** · 1:1  
  id `colar-de-vyal-falso`  
  > Colar de Vyal (falso): objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: todos acham o dono encantador.
- **Ninho de Estrelas** · d66 62 · estilo **I** · 1:1  
  id `ninho-de-estrelas`  
  > Ninho de Estrelas: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: uma pequena luz que segue o dono como bicho.
- **Cadeira de Espera** · d66 63 · estilo **I** · 1:1  
  id `cadeira-de-espera`  
  > Cadeira de Espera: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: quem senta não sente fome, sede nem tempo.
- **Baralho do Arquivo** · d66 64 · estilo **I** · 1:1  
  id `baralho-do-arquivo`  
  > Baralho do Arquivo: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: cada carta é a memória de alguém; ler é viver ela.
- **Frasco de Maré** · d66 65 · estilo **I** · 1:1  
  id `frasco-de-mare`  
  > Frasco de Maré: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: guarda uma Maré de Valor e solta uma vez (o mestre escolhe qual veio dentro).
- **Coleira de Ouro Vazia** · d66 66 · estilo **I** · 1:1  
  id `coleira-de-ouro-vazia`  
  > Coleira de Ouro Vazia: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: o que for colocado nela vira um familiar leal por um dia.

### Artefatos (9)

- **A Coroa de Vyal** · Persona · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `a-coroa-de-vyal`  
  > A Coroa de Vyal: coroa fina de ouro rosado com uma escama de dragão coral no centro, brilho coral pulsando como um coração.
- **A Primeira Moeda** · palavra Anular · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `a-primeira-moeda`  
  > A Primeira Moeda: artefato lendário, antigo e único. Rumor: o primeiro Ordyn cunhado em Oryuundis; imune a tudo, até a quem o carrega.
- **A Página Arrancada** · palavra Memória · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `a-pagina-arrancada`  
  > A Página Arrancada: artefato lendário, antigo e único. Rumor: a folha que GrandGaleonis arrancou do livro de duelos; quem a lê, lembra de alguém que não conheceu.
- **O Badalo de Vau-Grande** · palavra Voz · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `o-badalo-de-vau-grande`  
  > O Badalo de Vau-Grande: artefato lendário, antigo e único. Rumor: o sino é o que sobrou do marido da sineira; o badalo, dizem, é o que sobrou dele de verdade.
- **Escama do Vyal** · palavra Seduzir · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `escama-do-vyal`  
  > Escama do Vyal: artefato lendário, antigo e único. Rumor: caiu numa briga com o pai; quem a carrega é amado, e nunca sabe se é por ele ou pela escama.
- **Coroa Riscada** · palavra Implacável · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `coroa-riscada`  
  > Coroa Riscada: artefato lendário, antigo e único. Rumor: a tatuagem dos Invictos, gravada em Bronze Arco; pertenceu a duzentos e doze duelos.
- **Lanterna do Véu** · palavra Aurora · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `lanterna-do-veu`  
  > Lanterna do Véu: artefato lendário, antigo e único. Rumor: um pedaço da aurora de OulVeil preso num vidro; se apagar, a aurora apaga também.
- **Âncora do Mesmo Dia** · palavra Ciclo · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `ancora-do-mesmo-dia`  
  > Âncora do Mesmo Dia: artefato lendário, antigo e único. Rumor: segura um instante; Tique quer de volta.
- **Coleira de Kairvahnn** · palavra Obrigar · estilo **I** · 1:1 · **livro · gerar primeiro**  
  id `coleira-de-kairvahnn`  
  > Coleira de Kairvahnn: artefato lendário, antigo e único. Rumor: uma das cem cabeças do Leão ainda está presa nela.

### Terrenos (21)

- **Forja dos Mil Golpes** · 1 · Hostil · Conflitüo · estilo **C** · 3:2  
  id `forja-dos-mil-golpes`  
  > Forja dos Mil Golpes: forja gigantesca de pedra negra, bigornas do tamanho de casas, martelos que batem sozinhos, rios de metal em brasa, fumaça e faíscas vermelhas. Sinais de magia vermelho-ferrugem no ar.
- **Charco Fervente** · 1 · Travessia · Sublimeyn · estilo **C** · 3:2  
  id `charco-fervente`  
  > Charco Fervente: pântano que ferve, poças de lama borbulhando entre lajes de pedra que sobem e afundam, vapor prateado. Sinais de magia prata-mercúrio no ar.
- **Floresta que Anda** · 1 · Travessia · Primitivum · estilo **C** · 3:2  
  id `floresta-que-anda`  
  > Floresta que Anda: floresta densa em que as árvores têm raízes como pernas e mudam de lugar, trilhas que se fecham, luz âmbar entre as folhas. Sinais de magia verde-musgo e âmbar no ar.
- **Feira das Máscaras** · 1 · Social · Persona · estilo **C** · 3:2  
  id `feira-das-mascaras`  
  > Feira das Máscaras: feira noturna lotada, barracas coloridas, todo mundo de máscara, lanternas coral, música no ar. Sinais de magia coral no ar.
- **Arena de Kairvahnn** · 2 · Hostil · Dominum · estilo **C** · 3:2  
  id `arena-de-kairvahnn`  
  > Arena de Kairvahnn: arena circular de pedra com arquibancadas cheias, correntes pendendo, portões de jaula, poeira e luz roxa. Sinais de magia roxo-sangue no ar.
- **Farol Sem Noite** · 2 · Exploração · Alvorum · estilo **C** · 3:2  
  id `farol-sem-noite`  
  > Farol Sem Noite: farol altíssimo num penhasco jogando luz dourada em todas as direções, sem nenhuma sombra no chão, céu escuro em volta. Sinais de magia dourada e preta no ar.
- **Cemitério que Respira** · 2 · Hostil · Morbidyum · estilo **C** · 3:2  
  id `cemiterio-que-respira`  
  > Cemitério que Respira: cemitério antigo sob névoa verde-pálida, a terra das covas subindo e descendo devagar como um peito respirando. Sinais de magia verde-pálida no ar.
- **Biblioteca que Esquece** · 2 · Exploração · Cognitium · estilo **C** · 3:2  
  id `biblioteca-que-esquece`  
  > Biblioteca que Esquece: biblioteca labiríntica de estantes infinitas, livros abertos com páginas em branco, letras índigo escapando e se apagando no ar. Sinais de magia índigo no ar.
- **Estação Parada** · 3 · Travessia · Eternyr · estilo **C** · 3:2  
  id `estacao-parada`  
  > Estação Parada: estação de trem tecnomaga com um relógio gigante parado, vapor congelado no ar, trem imóvel de latão, poeira suspensa. Sinais de magia azul-noite com riscos brancos no ar.
- **Cidade-Muralha** · 1 · Social/Hostil · Conflitüo · estilo **C** · 3:2  
  id `cidade-muralha`  
  > Cidade-Muralha: cidade cercada por muralhas altas e grossas, ruas estreitas cheias de gente armada, estandartes vermelho-ferrugem. Sinais de magia vermelho-ferrugem no ar.
- **Porto de Vidro** · 2 · Social · Sublimeyn · estilo **C** · 3:2  
  id `porto-de-vidro`  
  > Porto de Vidro: cidade portuária toda de vidro, casas e cais transparentes brilhando ao sol, barcos atracados, reflexos prateados. Sinais de magia prata-mercúrio no ar.
- **Mercado de Feras** · 1 · Social · Dominum · estilo **C** · 3:2  
  id `mercado-de-feras`  
  > Mercado de Feras: mercado barulhento de jaulas e cercados, feras exóticas, domadores com chicotes, lama e palha, luz roxa nas coleiras. Sinais de magia roxo-sangue no ar.
- **Vila do Mesmo Dia** · 2 · Exploração · Eternyr · estilo **C** · 3:2  
  id `vila-do-mesmo-dia`  
  > Vila do Mesmo Dia: vila pequena ao pôr do sol, sombras repetidas como ecos, relógio de torre, céu azul-noite com riscos brancos. Sinais de magia azul-noite com riscos brancos no ar.
- **Cidade Insone** · 2 · Social · Alvorum · estilo **C** · 3:2  
  id `cidade-insone`  
  > Cidade Insone: cidade noturna toda acesa, janelas iluminadas, gente acordada nas ruas, sonhos dourados e pretos vazando das janelas como fumaça. Sinais de magia dourada e preta no ar.
- **Aldeia da Curandeira** · 1 · Social · Morbidyum · estilo **C** · 3:2  
  id `aldeia-da-curandeira`  
  > Aldeia da Curandeira: aldeia pequena de ervas penduradas nas portas, horta de plantas medicinais, cabana da curandeira com brilho verde-pálido. Sinais de magia verde-pálida no ar.
- **Rua dos Espelhos** · 2 · Exploração · Cognitium · estilo **C** · 3:2  
  id `rua-dos-espelhos`  
  > Rua dos Espelhos: rua estreita cheia de espelhos nas paredes, cada reflexo mostrando uma cena diferente, luz índigo. Sinais de magia índigo no ar.
- **Vila da Colheita** · 1 · Social · Primitivum · estilo **C** · 3:2  
  id `vila-da-colheita`  
  > Vila da Colheita: vila rural em época de colheita, campos dourados, carroças de feno, bandeirolas, luz âmbar de fim de tarde. Sinais de magia verde-musgo e âmbar no ar.
- **Estrada do Crepúsculo** · 2 · Travessia · Alvorum · estilo **C** · 3:2  
  id `estrada-do-crepusculo`  
  > Estrada do Crepúsculo: estrada longa que corta a paisagem ao meio: um lado em pleno dia, o outro em noite estrelada, a linha do crepúsculo cruzando a estrada. Sinais de magia dourada e preta no ar.
- **Mina Funda** · 1 · Travessia · Sublimeyn · estilo **C** · 3:2  
  id `mina-funda`  
  > Mina Funda: galeria de mina profunda, vigas de madeira, trilhos, minério prateado brilhando nas paredes, pouca luz de lanterna. Sinais de magia prata-mercúrio no ar.
- **Pátio de Duelos** · 1 · Social · qualquer · estilo **C** · 3:2  
  id `patio-de-duelos`  
  > Pátio de Duelos: pátio de pedra de uma escola de magia, círculos de duelo desenhados no chão, arquibancadas de alunos, marcas de feitiço.
- **Torre dos Protetores** · 3 · Hostil · (nenhum) · estilo **C** · 3:2  
  id `torre-dos-protetores`  
  > Torre dos Protetores: torre alta e austera de pedra cinza coberta de selos e lacres, uma cúpula apagada em volta onde nenhuma magia brilha.

### Lugares (22)

- **Permal** · tudo passa, mas nem sempre · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `permal`  
  > Permal: Um grande vale nevado de montanhas, inóspito. Poucos se aventuram por lá, mas há muitas riquezas. Clima: tudo passa, mas nem sempre.
- **TinThrohr** · completude e troca · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `tinthrohr`  
  > TinThrohr: Uma floresta enorme e mágica, de muitos seres e povos. Nela tudo se troca: aqui se dá, aqui se devolve, e os povos que vivem ali protegem esse equilíbrio. Clima: completude e troca.
- **O Postulário** · saber sem arrogância; o conhecimento dado de bom grado · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-postulario`  
  > O Postulário: Uma grande biblioteca de conhecimentos que vaga entre os planos, indefinidamente. Dizem que um Antigo Bibliotecário vive lá dentro, e que ela é guardada por autômatos diferentes, poderosos, que cuidam para que nenhum mal aconteça ao lugar. Clima: saber sem arrogância; o conhecimento dado de bom grado.
- **AlvoMarfim** · esperança · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `alvomarfim`  
  > AlvoMarfim: Grandes terras, majestosas, com diferentes reinos e possibilidades. Os humanos costumam morar em AlvoMarfim, e ali todos estão sujeitos a regras humanas. Clima: esperança.
- **Profulgar** · resiliência; e também luxúria, ambição, crueldade, deturpação · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `profulgar`  
  > Profulgar: Terra de ninguém. Bandidos, ladrões, pobres, criminosos: para todos tem espaço. Clima: resiliência; e também luxúria, ambição, crueldade, deturpação.
- **Nostrum** · ordem, restrição · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `nostrum`  
  > Nostrum: O grande tribunal mágico, onde os crimes são julgados sem parar. Alguns têm mais prioridade que outros; todos são julgados proporcionalmente. Clima: ordem, restrição.
- **Fhim** · paciência, fé · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `fhim`  
  > Fhim: O vale da noite: um grande deserto de pedra, ameno, silencioso. Se houver outra criatura ali, muito provavelmente ela está se afastando. Clima: paciência, fé.
- **OulVeil** · acolhimento e saudade: a vila cria quem o mar deixa sozinho · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `oulveil`  
  > OulVeil: Cidade simples, de pesca, sob um grande véu de aurora, dia e noite (um resquício da Cisão, dizem). A praia é parte da cidade, mas a vida acontece em cima do mar: casas, estalagens e bares de madeira sobre palafitas, ligados por pontes, cordas e passarelas, barcos amarrados como se fossem ruas, lanternas balançando, muitas estalagens para quem chega pelo mar. Clima: acolhimento e saudade: a vila cria quem o mar deixa sozinho.
- **Vau-Grande** · vigília: alguém sempre tem que ficar acordado pelos outros · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `vau-grande`  
  > Vau-Grande: Cidade de rio. O Sino da Maré avisa as cheias há duzentos anos. Clima: vigília: alguém sempre tem que ficar acordado pelos outros.
- **Confluência e GrandGaleonis** · pertencer (e o medo de não ser bom o bastante para pertencer) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `confluencia-e-grandgaleonis`  
  > Confluência e GrandGaleonis: Planície vasta de criaturas mágicas pacíficas, onde Arcano e Cataclisma vivem em homeostase. Nela, a escola-cidade GrandGaleonis. Clima: pertencer (e o medo de não ser bom o bastante para pertencer).
- **A Cidade-Muralha** · proteção (e o medo do que vem de fora) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-cidade-muralha`  
  > A Cidade-Muralha: Todo mundo anda armado, e nenhuma guerra de fora entra. Clima: proteção (e o medo do que vem de fora).
- **O Porto de Vidro** · transformação (e a vaidade de nunca parecer frágil) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-porto-de-vidro`  
  > O Porto de Vidro: Oficinas de Sublimeyn, tudo reaproveitado e refundido. O corpo da rainha muda com o humor, e o porto muda com ela. Clima: transformação (e a vaidade de nunca parecer frágil).
- **A Cidade Insone** · vigília e exaustão · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-cidade-insone`  
  > A Cidade Insone: Ninguém dorme; todo sonho vaza para a rua. Clima: vigília e exaustão.
- **A Vila do Mesmo Dia** · solidão de quem lembra · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-vila-do-mesmo-dia`  
  > A Vila do Mesmo Dia: O dia recomeça; quase ninguém lembra. Clima: solidão de quem lembra.
- **A boca da Voragem** · lealdade (e a posse que às vezes se disfarça dela) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-boca-da-voragem`  
  > A boca da Voragem: Onde Garmhal, o Domador guarda a entrada do Reino de Dominum. Clima: lealdade (e a posse que às vezes se disfarça dela).
- **Sarn-das-Escadas** · ambição (e o que se pisa para subir) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `sarn-das-escadas`  
  > Sarn-das-Escadas: Cidade vertical cavada num paredão sobre o mar. Quem mora mais alto manda. Clima: ambição (e o que se pisa para subir).
- **Brejo-Alto** · apego (o que não se deixa ir) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `brejo-alto`  
  > Brejo-Alto: Cidade de palafitas num pântano que respira. As casas se movem com a maré de lama. Clima: apego (o que não se deixa ir).
- **Querela** · impermanência (e o medo de que a festa acabe) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `querela`  
  > Querela: Cidade-feira que só existe três dias por ano, no cruzamento de quatro estradas. Este ano, o quarto dia amanheceu, e ninguém consegue ir embora. Clima: impermanência (e o medo de que a festa acabe).
- **Cinzarel** · recomeço (e o cansaço de recomeçar) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `cinzarel`  
  > Cinzarel: Cidade que queima e é reconstruída a cada geração, sempre no mesmo lugar, sempre com a mesma planta. Ninguém sabe se o fogo é acidente, promessa ou castigo. Clima: recomeço (e o cansaço de recomeçar).
- **O Mar Quieto de Ulen** · escuta (o que ficou por dizer) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-mar-quieto-de-ulen`  
  > O Mar Quieto de Ulen: Um mar sem ondas, liso como vidro, onde a voz viaja longe demais: conversas de séculos atrás ainda ecoam sobre a água, e quem rema ouve o que não foi dito a tempo. Clima: escuta (o que ficou por dizer).
- **As Pontes de Sethra** · confiança (e o medo da queda) · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `as-pontes-de-sethra`  
  > As Pontes de Sethra: Cidade de pontes suspensas entre ilhas de pedra que flutuam. Ninguém mora no chão; ninguém sabe o que há embaixo das nuvens. Clima: confiança (e o medo da queda).
- **Orrun, o Planalto dos Espelhos** · arrependimento e escolha · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `orrun-o-planalto-dos-espelhos`  
  > Orrun, o Planalto dos Espelhos: Planalto de lagos parados como espelhos. Em cada um, quem olha vê quem poderia ter sido se tivesse escolhido diferente. Clima: arrependimento e escolha.

### Reinos (9)

- **O Campo Sem Trégua** · Conflitüo · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-campo-sem-tregua`  
  > O Campo Sem Trégua, o plano de Conflitüo: uma batalha sem lados que nunca acaba; todo encontro vira duelo. Quem vive lá: os Juramentados: soldados que morreram lutando e esqueceram por quê; ferreiros de guerra. Cor dominante: vermelho-ferrugem.
- **O Mar de Estados** · Sublimeyn · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-mar-de-estados`  
  > O Mar de Estados, o plano de Sublimeyn: o mar é pedra de manhã, água à tarde e vapor à noite; ilhas de vidro. Quem vive lá: os Moldados (gente de matéria viva), alquimistas náufragos. Cor dominante: prata-mercúrio.
- **A Voragem** · Dominum · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-voragem`  
  > A Voragem, o plano de Dominum: abismo em espiral de carne e caverna; tudo lá tem dono. Quem vive lá: Kairvahnn, as feras acorrentadas, os Mansos (gente nascida lá, que serve). Cor dominante: roxo-sangue.
- **A Cidade dos Rostos** · Persona · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-cidade-dos-rostos`  
  > A Cidade dos Rostos, o plano de Persona: uma cidade que imita todas as cidades humanas; o que você sente aparece como clima em volta de você. Quem vive lá: os Sentidos: emoções que andam em forma de gente. Cor dominante: coral.
- **O Céu de Duas Pálpebras** · Alvorum · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-ceu-de-duas-palpebras`  
  > O Céu de Duas Pálpebras, o plano de Alvorum: metade da terra é dia sem fim, metade é noite sem fim; o crepúsculo é uma estrada. Quem vive lá: sonhadores, insones, os Vigias. Cor dominante: dourada e preta.
- **A Antessala** · Morbidyum · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-antessala`  
  > A Antessala, o plano de Morbidyum: o lugar entre os dois lados da Porta; cidades de mortos. Quem vive lá: Velhena, fantasmas em fila, os que voltaram. Cor dominante: verde-pálida.
- **O Arquivo Sem Dono** · Cognitium · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-arquivo-sem-dono`  
  > O Arquivo Sem Dono, o plano de Cognitium: biblioteca-labirinto de todas as memórias esquecidas; ilusão lá é matéria. Quem vive lá: os Esquecidos; no centro, o Trono Vazio. Cor dominante: índigo.
- **O Entretempo** · Eternyr · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `o-entretempo`  
  > O Entretempo, o plano de Eternyr: ruínas de todas as épocas ao mesmo tempo; salas que são anos. Quem vive lá: os Repetidos, que vivem o mesmo instante. Cor dominante: azul-noite com riscos brancos.
- **A Floresta Primeira** · Primitivum · estilo **C** · 3:2 · **livro · gerar primeiro**  
  id `a-floresta-primeira`  
  > A Floresta Primeira, o plano de Primitivum: a mata de onde veio tudo que é vivo. Quem vive lá: Tahwë, espíritos-animais, os bichos que vieram antes dos bichos. Cor dominante: verde-musgo e âmbar.

### Povos (23)

- **Anões** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-anoes`  
  > Uma pessoa do povo Anões, roupas de viagem simples: baixos, largos, de pedra e forja; memória longa, rancor mais longo.
- **Elfos** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-elfos`  
  > Uma pessoa do povo Elfos, roupas de viagem simples: longevos, de gestos lentos; parecem sempre ouvir uma música que os outros não ouvem.
- **Halflings** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-halflings`  
  > Uma pessoa do povo Halflings, roupas de viagem simples: pequenos, de casa cheia e mesa farta; a coragem aparece na hora certa.
- **Golias** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-golias`  
  > Uma pessoa do povo Golias, roupas de viagem simples: gigantes de montanha, pele marcada como rocha; medem tudo pela escalada.
- **Tieflings** · aparência · às vezes magia · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-tieflings`  
  > Uma pessoa do povo Tieflings, roupas de viagem simples: chifres, cauda, olhos de brasa; carregam no corpo um pacto antigo de alguém da família.
- **Verdejantes** · aparência · magia (Primitivum) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-verdejantes`  
  > Uma pessoa do povo Verdejantes, roupas de viagem simples: seres do verde: casca, folha, musgo, flor; vivem com as estações.
- **Orcs** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-orcs`  
  > Uma pessoa do povo Orcs, roupas de viagem simples: fortes, de presas, de clã; honra dita alto.
- **Goblins** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-goblins`  
  > Uma pessoa do povo Goblins, roupas de viagem simples: pequenos, rápidos, engenhosos; vivem nas frestas das cidades dos outros.
- **Gnomos** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-gnomos`  
  > Uma pessoa do povo Gnomos, roupas de viagem simples: curiosos, inventores, riem de tudo; toda casa gnoma tem uma máquina que ninguém sabe para que serve.
- **Trolls** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-trolls`  
  > Uma pessoa do povo Trolls, roupas de viagem simples: enormes, de pele grossa que regenera; lentos para decidir, impossíveis de esquecer.
- **Fadas** · aparência · magia · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-fadas`  
  > Uma pessoa do povo Fadas, roupas de viagem simples: minúsculas, feéricas, de regras próprias; toda promessa feita a uma fada vale.
- **Leprechauns** · aparência · magia (sorte) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-leprechauns`  
  > Uma pessoa do povo Leprechauns, roupas de viagem simples: pequenos, apostadores, colecionadores de ouro e de dívidas.
- **Elementais** · magia (Sublimeyn) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-elementais`  
  > Uma pessoa do povo Elementais, roupas de viagem simples: corpos de fogo, água, pedra, ar ou mistura; o elemento é o humor deles.
- **Draconatos** · aparência · magia (o Valor do dragão de origem) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-draconatos`  
  > Uma pessoa do povo Draconatos, roupas de viagem simples: gente com sangue de dragão: escamas, sopro, orgulho.
- **Luphos** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-luphos`  
  > Uma pessoa do povo Luphos, roupas de viagem simples: humanoides com aparência de lobo, cão, raposa, coiote; de matilha e faro.
- **Almum** · magia · o que você habita · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-almum`  
  > Uma pessoa do povo Almum, roupas de viagem simples: seres espirituais que se prendem ao plano terreno num corpo, numa arma ou num animal, e ali ficam.
- **Autômatos** · aparência · às vezes magia (Dispositivo) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-automatos`  
  > Uma pessoa do povo Autômatos, roupas de viagem simples: máquinas que acordaram; umas lembram quem as fez, outras não.
- **Vikings** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-vikings`  
  > Uma pessoa do povo Vikings, roupas de viagem simples: humanos maiores e mais fortes, que lutam em bandos, saqueiam e aguentam condições extremas.
- **Corves** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-corves`  
  > Uma pessoa do povo Corves, roupas de viagem simples: vivem em cavernas nas montanhas geladas; pele cinza, pelo marrom, olhos espalhados pelo rosto e uma boca de muitos dentes; pacíficos e acolhedores, apesar do tamanho e das feições. Para eles, dificuldade se passa em grupo.
- **Tritões e sereias** · aparência · às vezes magia (Persona) · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-tritoes-e-sereias`  
  > Uma pessoa do povo Tritões e sereias, roupas de viagem simples: gente do mar, de guelras e escamas; cantam de um jeito que faz barco se perder ou achar o caminho.
- **Aviares** · aparência · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-aviares`  
  > Uma pessoa do povo Aviares, roupas de viagem simples: gente-pássaro, de penas e ossos leves; vivem nas escarpas e nos ventos.
- **Fidraus** · aparência · magia · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-fidraus`  
  > Uma pessoa do povo Fidraus, roupas de viagem simples: feitos de energia, música e sentimentos; a aparência muda de acordo com a cultura para quem se apresentam, e se vestem como ela, mas reluzem com uma energia sólida, bruxuleante, resplandecente.
- **Innhomus** · fora de Contacto · estilo **P** · 1:1 · **livro · gerar primeiro**  
  id `povo-innhomus`  
  > Um Innhomus: divindade esquecida e cataclísmica, ser disforme e abstrato feito de formas geométricas, letras soltas, sinais e símbolos flutuando juntos, quase indescritível.

### Dragões (10)

- **Dragão-Lâmina** · Conflitüo · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-lamina`  
  > Dragão-Lâmina, dragão de Conflitüo: vermelho-ferrugem, espinhos como lâminas. Its flames: ghostly rust-red flames shaped like blade edges and flying shards, pulsing like a heartbeat in battle. O sopro: estilhaços de metal em brasa.
- **Dragão-Mercúrio** · Sublimeyn · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-mercurio`  
  > Dragão-Mercúrio, dragão de Sublimeyn: prata que escorre e endurece. Its flames: ghostly silver-blue flames that keep changing state as they rise: vapor, then droplets, then little crystal shards. O sopro: muda o estado do chão.
- **Dragão-Coleira** · Dominum · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-coleira`  
  > Dragão-Coleira, dragão de Dominum: roxo-sangue, escamas que parecem correntes. Its flames: ghostly violet flames shaped like chain links and collar rings, held tight to the body, burning brightest in the eyes. O sopro: quem respira obedece uma ordem.
- **Dragão-Coral** · Persona · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-coral`  
  > Dragão-Coral, dragão de Persona: coral, com um rosto quase humano. Its flames: ghostly coral-pink flames soft as breath that shift hue with its mood, sometimes curling into faint face-like shapes. O sopro: uma emoção.
- **Dragão-Eclipse** · Alvorum · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-eclipse`  
  > Dragão-Eclipse, dragão de Alvorum: dourado de um lado, negro do outro. Its flames: ghostly flames split in two: radiant white flames on the spines and black shadow-flames in the eyes, like day and night. O sopro: cegueira ou pesadelo.
- **Dragão-Pálido** · Morbidyum · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-palido`  
  > Dragão-Pálido, dragão de Morbidyum: verde-pálido, ossos aparecendo. Its flames: ghostly pale-green flames, cold and trembling, drifting upward like will-o-the-wisps and small departing souls. O sopro: apodrece os inimigos, ou fecha feridas de quem ele quiser.
- **Dragão-Véu** · Cognitium · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-veu`  
  > Dragão-Véu, dragão de Cognitium: índigo, meio transparente. Its flames: ghostly almost colorless veil-like flames, seen only at the edge of sight, blurring into indigo mist and thin labyrinth lines. O sopro: esquecimento.
- **Dragão-Ampulheta** · Eternyr · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-ampulheta`  
  > Dragão-Ampulheta, dragão de Eternyr: azul-noite com pontos brancos, como céu. Its flames: ghostly night-blue flames with tiny white star points that move far too slowly, leaving frozen trails like long exposures. O sopro: envelhece, ou para no tempo.
- **Dragão-Musgo** · Primitivum · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `dragao-musgo`  
  > Dragão-Musgo, dragão de Primitivum: verde-musgo e âmbar, galhos nos chifres. Its flames: ghostly amber flames like fireflies and floating seeds, drifting around moss and branch-like horns. O sopro: raízes que prendem, ou um instante como bicho.
- **Wyvern** · cria de dragão · estilo **D** · 1:1 · **livro · gerar primeiro**  
  id `wyvern`  
  > Wyvern, a cria de um dragão: menor, duas patas e asas, sem chamas, olhar de bicho, arreio de couro de montaria.

### Bestiário · Gente comum (83)

- **Guarda da Ponte** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `guarda-da-ponte`  
  > Guarda da Ponte: guarda; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento; um de um bando; no gesto de estocar.
- **Sargento de Ronda** · M1 · Líder · Conflitüo · estilo **P** · 1:1  
  id `sargento-de-ronda`  
  > Sargento de Ronda: soldado, veterano; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; postura de quem comanda; no gesto de cortar; magia vermelho-ferrugem.
- **Bandido de Estrada** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `bandido-de-estrada`  
  > Bandido de Estrada: ladrão; pessoa comum do mundo, roupas de trabalho e de rua; jeito covarde; um de um bando; no gesto de esfaquear.
- **Chefe de Quadrilha** · M1 · Padrão · Cognitium · estilo **P** · 1:1  
  id `chefe-de-quadrilha`  
  > Chefe de Quadrilha: bandido, mentiroso; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto, covarde; no gesto de atirar; magia índigo.
- **Nobre Ofendido** · M1 · Social · Persona · estilo **P** · 1:1  
  id `nobre-ofendido`  
  > Nobre Ofendido: nobre; pessoa comum do mundo, roupas de trabalho e de rua; jeito belo; pose de conversa; no gesto de humilhar; magia coral.
- **Mercador de Sal** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `mercador-de-sal`  
  > Mercador de Sal: mercador; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de pechinchar; magia prata-mercúrio.
- **Mercenário de Serath** · M2 · Padrão · Conflitüo · estilo **P** · 1:1  
  id `mercenario-de-serath`  
  > Mercenário de Serath: soldado, contratado; pessoa comum do mundo, roupas de trabalho e de rua; jeito blindado; no gesto de cortar; magia vermelho-ferrugem.
- **Carrasco da Cidade-Muralha** · M2 · Bruto · Conflitüo · estilo **P** · 1:1  
  id `carrasco-da-cidade-muralha`  
  > Carrasco da Cidade-Muralha: carrasco; pessoa comum do mundo, roupas de trabalho e de rua; jeito enorme, silencioso; grande e pesado; no gesto de decepar; magia vermelho-ferrugem.
- **Pescadora de OulVeil** · M1 · Social · — · estilo **P** · 1:1  
  id `pescadora-de-oulveil`  
  > Pescadora de OulVeil: pescadora, vizinha; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de remar.
- **Estalajadeiro de Vau-Grande** · M1 · Social · — · estilo **P** · 1:1  
  id `estalajadeiro-de-vau-grande`  
  > Estalajadeiro de Vau-Grande: estalajadeiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento; pose de conversa; no gesto de servir.
- **Criança de Rua** · M1 · Social · — · estilo **P** · 1:1  
  id `crianca-de-rua`  
  > Criança de Rua: criança, ladra; pessoa comum do mundo, roupas de trabalho e de rua; jeito minúsculo, veloz; pose de conversa; no gesto de surrupiar.
- **Capitão Otávio, da Guilda do Rio** · M1 · Líder · — · estilo **P** · 1:1  
  id `capitao-otavio-da-guilda-do-rio`  
  > Capitão Otávio, da Guilda do Rio: capitão, guilda; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro, leal; postura de quem comanda; no gesto de cortar.
- **Dona Iolanda, a Sineira** · M1 · Suporte · Sublimeyn · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `iolanda-fundir`  
  > Senhora idosa, rabugenta, avental de couro, mangas arregaçadas, segurando com a mão nua um pedaço de metal que derrete em prata-mercúrio e escorre para dentro de uma tabela da página, preenchendo as células.
- **Lino, o Sineiro de Plantão** · M1 · Social · — · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `lino-sineiro`  
  > Menino de 12 anos, magro, fita vermelha no pulso, agarrado a uma corda de sino, gritando; a corda sai do quadro e amarra o título da seção.
- **Barqueiro do Rio** · M1 · Social · — · estilo **P** · 1:1  
  id `barqueiro-do-rio`  
  > Barqueiro do Rio: barqueiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de remar.
- **Contrabandista de Porto** · M1 · Padrão · — · estilo **P** · 1:1  
  id `contrabandista-de-porto`  
  > Contrabandista de Porto: contrabandista; pessoa comum do mundo, roupas de trabalho e de rua; jeito mentiroso; no gesto de esfaquear.
- **Guarda-Costas Contratado** · M1 · Padrão · — · estilo **P** · 1:1  
  id `guarda-costas-contratado`  
  > Guarda-Costas Contratado: guarda-costas; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro, leal (enquanto pago); no gesto de golpear.
- **Caçador de Recompensas** · M2 · Atirador · — · estilo **P** · 1:1  
  id `cacador-de-recompensas`  
  > Caçador de Recompensas: caçador; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente, silencioso; no gesto de atirar.
- **Parteira da Aldeia** · M1 · Suporte · — · estilo **P** · 1:1  
  id `parteira-da-aldeia`  
  > Parteira da Aldeia: parteira; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de quem ajuda os outros; no gesto de estancar.
- **Ferreira de Vau-Grande** · M1 · Social · — · estilo **P** · 1:1  
  id `ferreira-de-vau-grande`  
  > Ferreira de Vau-Grande: ferreira; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; pose de conversa; no gesto de martelar.
- **Taverneira que Vende Segredos** · M1 · Social · Cognitium · estilo **P** · 1:1  
  id `taverneira-que-vende-segredos`  
  > Taverneira que Vende Segredos: taverneira, informante; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto, mentiroso; pose de conversa; no gesto de servir; magia índigo.
- **Arruaceiro de Taverna** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `arruaceiro-de-taverna`  
  > Arruaceiro de Taverna: bêbado; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento; um de um bando; no gesto de socar.
- **Vigia de Muralha** · M1 · Atirador · — · estilo **P** · 1:1  
  id `vigia-de-muralha`  
  > Vigia de Muralha: vigia; pessoa comum do mundo, roupas de trabalho e de rua; jeito silencioso; no gesto de atirar.
- **Cobrador de Impostos** · M1 · Social · — · estilo **P** · 1:1  
  id `cobrador-de-impostos`  
  > Cobrador de Impostos: cobrador; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; pose de conversa; no gesto de cobrar.
- **Herdeira em Fuga** · M1 · Social · Persona · estilo **P** · 1:1  
  id `herdeira-em-fuga`  
  > Herdeira em Fuga: herdeira, fugitiva; pessoa comum do mundo, roupas de trabalho e de rua; jeito belo; pose de conversa; no gesto de seduzir; magia coral.
- **Mendigo Que Viu Demais** · M1 · Social · — · estilo **P** · 1:1  
  id `mendigo-que-viu-demais`  
  > Mendigo Que Viu Demais: mendigo, testemunha; pessoa comum do mundo, roupas de trabalho e de rua; jeito estranho (diz a verdade, mas só em rima); pose de conversa; no gesto de pedir.
- **Pregador de Esquina** · M1 · Social · — · estilo **P** · 1:1  
  id `pregador-de-esquina`  
  > Pregador de Esquina: pregador; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento; pose de conversa; no gesto de pregar.
- **Milícia de Aldeia** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `milicia-de-aldeia`  
  > Milícia de Aldeia: camponês, milícia; pessoa comum do mundo, roupas de trabalho e de rua; jeito numeroso, covarde; um de um bando; no gesto de forcado.
- **Chefe da Aldeia** · M1 · Social · — · estilo **P** · 1:1  
  id `chefe-da-aldeia`  
  > Chefe da Aldeia: anciã, chefe; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de decidir.
- **Caravaneiro** · M1 · Social · — · estilo **P** · 1:1  
  id `caravaneiro`  
  > Caravaneiro: mercador, viajante; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de negociar.
- **Salteador a Cavalo** · M1 · Ágil · — · estilo **P** · 1:1  
  id `salteador-a-cavalo`  
  > Salteador a Cavalo: salteador, cavaleiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito veloz; leve e veloz; no gesto de cortar.
- **Guarda da Cidade-Muralha** · M2 · Padrão · Conflitüo · estilo **P** · 1:1  
  id `guarda-da-cidade-muralha`  
  > Guarda da Cidade-Muralha: soldado; pessoa comum do mundo, roupas de trabalho e de rua; jeito blindado; no gesto de estocar; magia vermelho-ferrugem.
- **Recruta Assustado** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `recruta-assustado`  
  > Recruta Assustado: recruta; pessoa comum do mundo, roupas de trabalho e de rua; jeito covarde; um de um bando; no gesto de estocar.
- **Duelista de Honra** · M2 · Ágil · — · estilo **P** · 1:1  
  id `duelista-de-honra`  
  > Duelista de Honra: duelista, nobre; pessoa comum do mundo, roupas de trabalho e de rua; jeito belo, veloz; leve e veloz; no gesto de estocar.
- **Tecelã de Redes** · M1 · Social · — · estilo **P** · 1:1  
  id `tecela-de-redes`  
  > Tecelã de Redes: tecelã; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de tecer.
- **Menestrel Viajante** · M1 · Social · Persona · estilo **P** · 1:1  
  id `menestrel-viajante`  
  > Menestrel Viajante: menestrel; pessoa comum do mundo, roupas de trabalho e de rua; jeito belo, barulhento; pose de conversa; no gesto de cantar; magia coral.
- **Curandeira de Ervas** · M1 · Suporte · — · estilo **P** · 1:1  
  id `curandeira-de-ervas`  
  > Curandeira de Ervas: herborista; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de quem ajuda os outros; no gesto de emplastro.
- **Coveiro de Vau-Grande** · M1 · Social · Morbidyum · estilo **P** · 1:1  
  id `coveiro-de-vau-grande`  
  > Coveiro de Vau-Grande: coveiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito pálido, silencioso; pose de conversa; no gesto de cavar; magia verde-pálida.
- **Escriba do Porto** · M1 · Social · — · estilo **P** · 1:1  
  id `escriba-do-porto`  
  > Escriba do Porto: escriba; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de anotar.
- **Pirata de Rio** · M1 · Padrão · — · estilo **P** · 1:1  
  id `pirata-de-rio`  
  > Pirata de Rio: pirata; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento; no gesto de cortar.
- **Capitã de Navio Mercante** · M2 · Líder · — · estilo **P** · 1:1  
  id `capita-de-navio-mercante`  
  > Capitã de Navio Mercante: capitã, marinheira; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro, leal; postura de quem comanda; no gesto de cortar.
- **Marinheiro** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `marinheiro`  
  > Marinheiro: marinheiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito numeroso; um de um bando; no gesto de gancho.
- **Monge da Estrada** · M2 · Padrão · — · estilo **P** · 1:1  
  id `monge-da-estrada`  
  > Monge da Estrada: monge, andarilho; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente, duro; no gesto de golpear.
- **Assassina Paga** · M2 · Ágil · — · estilo **P** · 1:1  
  id `assassina-paga`  
  > Assassina Paga: assassina; pessoa comum do mundo, roupas de trabalho e de rua; jeito silencioso; leve e veloz; no gesto de apunhalar.
- **Espião da Coroa** · M2 · Social · Cognitium · estilo **P** · 1:1  
  id `espiao-da-coroa`  
  > Espião da Coroa: espião; pessoa comum do mundo, roupas de trabalho e de rua; jeito mentiroso, astuto; pose de conversa; no gesto de mentir; magia índigo.
- **Juíza da Cidade** · M2 · Social · — · estilo **P** · 1:1  
  id `juiza-da-cidade`  
  > Juíza da Cidade: juíza; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente, duro; pose de conversa; no gesto de sentenciar.
- **Carcereiro** · M1 · Padrão · — · estilo **P** · 1:1  
  id `carcereiro`  
  > Carcereiro: carcereiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito pesado; no gesto de porretear.
- **Prisioneiro Esquecido** · M1 · Social · — · estilo **P** · 1:1  
  id `prisioneiro-esquecido`  
  > Prisioneiro Esquecido: prisioneiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito frágil; pose de conversa; no gesto de contar.
- **Mãe Procurando o Filho** · M1 · Social · — · estilo **P** · 1:1  
  id `mae-procurando-o-filho`  
  > Mãe Procurando o Filho: mãe; pessoa comum do mundo, roupas de trabalho e de rua; jeito leal; pose de conversa; no gesto de implorar.
- **Rei do Esgoto** · M1 · Líder · — · estilo **P** · 1:1  
  id `rei-do-esgoto`  
  > Rei do Esgoto: chefe, gente-de-esgoto; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto, covarde; postura de quem comanda; no gesto de faca.
- **Gente-de-Esgoto** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `gente-de-esgoto`  
  > Gente-de-Esgoto: ladrão, gangue; pessoa comum do mundo, roupas de trabalho e de rua; jeito numeroso, imundo; um de um bando; no gesto de faca.
- **Veterana de Guerra** · M2 · Padrão · — · estilo **P** · 1:1  
  id `veterana-de-guerra`  
  > Veterana de Guerra: veterana; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; no gesto de cortar.
- **Gladiador da Arena** · M2 · Bruto · — · estilo **P** · 1:1  
  id `gladiador-da-arena`  
  > Gladiador da Arena: gladiador; pessoa comum do mundo, roupas de trabalho e de rua; jeito enorme, barulhento; grande e pesado; no gesto de esmagar.
- **Dono da Arena** · M2 · Social · — · estilo **P** · 1:1  
  id `dono-da-arena`  
  > Dono da Arena: empresário; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto, mentiroso; pose de conversa; no gesto de apostar.
- **Cavaleiro Errante** · M2 · Padrão · — · estilo **P** · 1:1  
  id `cavaleiro-errante`  
  > Cavaleiro Errante: cavaleiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito blindado, leal; no gesto de lança.
- **Escudeiro** · M1 · Suporte · — · estilo **P** · 1:1  
  id `escudeiro`  
  > Escudeiro: escudeiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito leal, covarde; pose de quem ajuda os outros; no gesto de estocar.
- **Caçadora da Mata** · M1 · Atirador · — · estilo **P** · 1:1  
  id `cacadora-da-mata`  
  > Caçadora da Mata: caçadora; pessoa comum do mundo, roupas de trabalho e de rua; jeito silencioso; no gesto de atirar.
- **Lenhador** · M1 · Padrão · — · estilo **P** · 1:1  
  id `lenhador`  
  > Lenhador: lenhador; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; no gesto de machado.
- **Mineira** · M1 · Social · — · estilo **P** · 1:1  
  id `mineira`  
  > Mineira: mineira; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro; pose de conversa; no gesto de picareta.
- **Contadora de Histórias** · M1 · Social · — · estilo **P** · 1:1  
  id `contadora-de-historias`  
  > Contadora de Histórias: velha, memória; pessoa comum do mundo, roupas de trabalho e de rua; jeito antigo; pose de conversa; no gesto de contar.
- **Noivo Abandonado** · M1 · Social · — · estilo **P** · 1:1  
  id `noivo-abandonado`  
  > Noivo Abandonado: noivo; pessoa comum do mundo, roupas de trabalho e de rua; jeito belo, furioso; pose de conversa; no gesto de chorar.
- **Sacerdotisa de Zahun** · M2 · Suporte · Persona · estilo **P** · 1:1  
  id `sacerdotisa-de-zahun`  
  > Sacerdotisa de Zahun: sacerdotisa; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de quem ajuda os outros; no gesto de abençoar; magia coral.
- **Inquisidor Anti-Magia** · M2 · Líder · — · estilo **P** · 1:1  
  id `inquisidor-anti-magia`  
  > Inquisidor Anti-Magia: inquisidor, fanático; pessoa comum do mundo, roupas de trabalho e de rua; jeito sagrado, duro; postura de quem comanda; no gesto de cortar.
- **Engenheira de Pontes** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `engenheira-de-pontes`  
  > Engenheira de Pontes: engenheira; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de medir; magia prata-mercúrio.
- **Cocheiro** · M1 · Social · — · estilo **P** · 1:1  
  id `cocheiro`  
  > Cocheiro: cocheiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito veloz; pose de conversa; no gesto de chicotear.
- **Ladrão de Túmulos** · M1 · Padrão · — · estilo **P** · 1:1  
  id `ladrao-de-tumulos`  
  > Ladrão de Túmulos: ladrão; pessoa comum do mundo, roupas de trabalho e de rua; jeito covarde, imundo; no gesto de pá.
- **Trapaceiro de Feira** · M1 · Social · — · estilo **P** · 1:1  
  id `trapaceiro-de-feira`  
  > Trapaceiro de Feira: trapaceiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito mentiroso, veloz; pose de conversa; no gesto de enganar.
- **Leiloeiro** · M1 · Social · — · estilo **P** · 1:1  
  id `leiloeiro`  
  > Leiloeiro: leiloeiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito barulhento, astuto; pose de conversa; no gesto de anunciar.
- **Pastora de Cabras** · M1 · Social · — · estilo **P** · 1:1  
  id `pastora-de-cabras`  
  > Pastora de Cabras: pastora; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de assobiar.
- **Criado de Mansão** · M1 · Social · — · estilo **P** · 1:1  
  id `criado-de-mansao`  
  > Criado de Mansão: criado; pessoa comum do mundo, roupas de trabalho e de rua; jeito silencioso; pose de conversa; no gesto de servir.
- **Capitã Mercenária** · M2 · Líder · — · estilo **P** · 1:1  
  id `capita-mercenaria`  
  > Capitã Mercenária: capitã, mercenária; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; postura de quem comanda; no gesto de cortar.
- **Rei Mendigo da Cidade Baixa** · M2 · Social · — · estilo **P** · 1:1  
  id `rei-mendigo-da-cidade-baixa`  
  > Rei Mendigo da Cidade Baixa: rei, mendigo; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de mandar.
- **Estudante da Escola de Magia** · M1 · Social · (sorteado) · estilo **P** · 1:1  
  id `estudante-da-escola-de-magia`  
  > Estudante da Escola de Magia: aprendiz, estudante; pessoa comum do mundo, roupas de trabalho e de rua; jeito frágil; pose de conversa; no gesto de estudar.
- **Professora da Escola de Magia** · M2 · Suporte · Cognitium · estilo **P** · 1:1  
  id `professora-da-escola-de-magia`  
  > Professora da Escola de Magia: professora; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente, astuto; pose de quem ajuda os outros; no gesto de corrigir; magia índigo.
- **Tatuadora de Marcas** · M1 · Social · Persona · estilo **P** · 1:1  
  id `tatuadora-de-marcas`  
  > Tatuadora de Marcas: tatuadora; pessoa comum do mundo, roupas de trabalho e de rua; jeito silencioso; pose de conversa; no gesto de marcar; magia coral.
- **Cartógrafo Perdido** · M1 · Social · — · estilo **P** · 1:1  
  id `cartografo-perdido`  
  > Cartógrafo Perdido: cartógrafo; pessoa comum do mundo, roupas de trabalho e de rua; jeito estranho (nunca erra um mapa, sempre erra o caminho); pose de conversa; no gesto de medir.
- **Boticária do Porto de Vidro** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `boticaria-do-porto-de-vidro`  
  > Boticária do Porto de Vidro: boticária; pessoa comum do mundo, roupas de trabalho e de rua; jeito astuto; pose de conversa; no gesto de vender; magia prata-mercúrio.
- **Velho Relojoeiro da Vila do Mesmo Dia** · M1 · Social · Eternyr · estilo **P** · 1:1  
  id `velho-relojoeiro-da-vila-do-mesmo-dia`  
  > Velho Relojoeiro da Vila do Mesmo Dia: relojoeiro; pessoa comum do mundo, roupas de trabalho e de rua; jeito paciente; pose de conversa; no gesto de consertar; magia azul-noite com riscos brancos.
- **Noviça Insone** · M1 · Social · Alvorum · estilo **P** · 1:1  
  id `novica-insone`  
  > Noviça Insone: noviça, insone; pessoa comum do mundo, roupas de trabalho e de rua; jeito pálido; pose de conversa; no gesto de vigiar; magia dourada e preta.
- **Domador de Wyvern-Correio** · M1 · Social · Dominum · estilo **P** · 1:1  
  id `domador-de-wyvern-correio`  
  > Domador de Wyvern-Correio: domador, correio; pessoa comum do mundo, roupas de trabalho e de rua; jeito leal; pose de conversa; no gesto de assobiar; magia roxo-sangue.
- **O Imperador de AlvoMarfim** · M3 · Líder · — (não tem Valor) · estilo **P** · 1:1  
  id `o-imperador-de-alvomarfim`  
  > O Imperador de AlvoMarfim: imperador, humano; pessoa comum do mundo, roupas de trabalho e de rua; jeito leal, duro; postura de quem comanda; no gesto de espada.
- **Mafrisa, do Bar da Mafrisa** · M3 · Suporte · — (fada) · estilo **P** · 1:1  
  id `mafrisa-do-bar-da-mafrisa`  
  > Mafrisa, do Bar da Mafrisa: fada, dona de bar; pessoa comum do mundo, roupas de trabalho e de rua; jeito duro, leal; pose de quem ajuda os outros; no gesto de puxar.
- **Professor Ninguém** · M4 · Chefe · — · Anulador, Espelho · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `professor-ninguem`  
  > Jovem de roupa branca de universidade mágica, luvas brancas, cabelos azuis pintados de azul-escuro, olhar perdido e feliz. Das luvas saem engrenagens e pequenos mecanismos de latão que desenham linhas retas de esquema por cima da página, transformando os títulos em plantas de máquina; um autômato de bolso agarra a margem.

### Bestiário · Comprometidos (48)

- **Aprendiz Descontrolado** · M1 · Conjurador · Sublimeyn · estilo **P** · 1:1  
  id `aprendiz-descontrolado`  
  > Aprendiz Descontrolado: aprendiz; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito frágil, faminto de magia; no gesto de queimar; magia prata-mercúrio.
- **Duelista da Lâmina Vermelha** · M2 · Ágil · Conflitüo · estilo **P** · 1:1  
  id `duelista-da-lamina-vermelha`  
  > Duelista da Lâmina Vermelha: duelista; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito veloz, belo; leve e veloz; no gesto de estocar; magia vermelho-ferrugem.
- **Encantadora de Salão** · M2 · Conjurador · Persona · estilo **P** · 1:1  
  id `encantadora-de-salao`  
  > Encantadora de Salão: cantora, espiã; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito belo, mentiroso; no gesto de cantar; magia coral.
- **Leitor de Sombras** · M2 · Conjurador · Cognitium · estilo **P** · 1:1  
  id `leitor-de-sombras`  
  > Leitor de Sombras: espião, leitor; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito invisível, covarde; no gesto de ler; magia índigo.
- **Sacerdote da Porta** · M2 · Suporte · Morbidyum · estilo **P** · 1:1  
  id `sacerdote-da-porta`  
  > Sacerdote da Porta: sacerdote, coveiro; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito pálido, imundo; pose de quem ajuda os outros; no gesto de tocar; magia verde-pálida.
- **Domadora de Garmhal** · M2 · Líder · Dominum · estilo **P** · 1:1  
  id `domadora-de-garmhal`  
  > Domadora de Garmhal: domadora; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito leal; postura de quem comanda; no gesto de chicotear; magia roxo-sangue.
- **Relojoeiro Fugitivo** · M3 · Conjurador · Eternyr · Resiliente · estilo **P** · 1:1  
  id `relojoeiro-fugitivo`  
  > Relojoeiro Fugitivo: relojoeiro, fugitivo; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente, estranho (quem o mata acorda um dia antes, uma vez); no gesto de atirar; magia azul-noite com riscos brancos.
- **Arquimaga da Torre Inversa** · M4 · Chefe · Sublimeyn + Alvorum · Quimerarcano · estilo **P** · 1:1  
  id `arquimaga-da-torre-inversa`  
  > Arquimaga da Torre Inversa: arquimaga, professora; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito antigo, brilhante; pose imponente de chefe; no gesto de raio; magia prata-mercúrio.
- **Juiz-Feiticeiro** · M3 · Líder · Alvorum · Anulador · estilo **P** · 1:1  
  id `juiz-feiticeiro`  
  > Juiz-Feiticeiro: juiz; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito sagrado, duro; postura de quem comanda; no gesto de condenar; magia dourada e preta.
- **Sargenta do Estandarte** · M2 · Líder · Conflitüo · estilo **P** · 1:1  
  id `sargenta-do-estandarte`  
  > Sargenta do Estandarte: soldada, porta-estandarte; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito leal, barulhento; postura de quem comanda; no gesto de lança; magia vermelho-ferrugem.
- **Quebra-Escudos** · M1 · Bruto · Conflitüo · estilo **P** · 1:1  
  id `quebra-escudos`  
  > Quebra-Escudos: guerreiro, martelo; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito pesado; grande e pesado; no gesto de esmagar; magia vermelho-ferrugem.
- **Arqueira da Última Flecha** · M2 · Atirador · Conflitüo · estilo **P** · 1:1  
  id `arqueira-da-ultima-flecha`  
  > Arqueira da Última Flecha: arqueira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente, silencioso; no gesto de flecha; magia vermelho-ferrugem.
- **Senhor da Trincheira** · M3 · Bruto · Conflitüo · Resiliente · estilo **P** · 1:1  
  id `senhor-da-trincheira`  
  > Senhor da Trincheira: veterano, trincheira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito duro, blindado; grande e pesado; no gesto de esmagar; magia vermelho-ferrugem.
- **Ferreira-Alquimista** · M1 · Suporte · Sublimeyn · estilo **P** · 1:1  
  id `ferreira-alquimista`  
  > Ferreira-Alquimista: ferreira, alquimista; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito ardente; pose de quem ajuda os outros; no gesto de martelar; magia prata-mercúrio.
- **Contrabandista de Mercúrio** · M2 · Ágil · Sublimeyn · estilo **P** · 1:1  
  id `contrabandista-de-mercurio`  
  > Contrabandista de Mercúrio: contrabandista; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito veloz, mentiroso; leve e veloz; no gesto de faca; magia prata-mercúrio.
- **Guardiã do Vapor** · M3 · Conjurador · Sublimeyn · estilo **P** · 1:1  
  id `guardia-do-vapor`  
  > Guardiã do Vapor: maquinista, guardiã; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito ardente, paciente; no gesto de jato; magia prata-mercúrio.
- **Escultor de Sal** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `escultor-de-sal`  
  > Escultor de Sal: artesão; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente; pose de conversa; no gesto de esculpir; magia prata-mercúrio.
- **Mestre de Coleira** · M3 · Líder · Dominum · estilo **P** · 1:1  
  id `mestre-de-coleira`  
  > Mestre de Coleira: domador, mestre; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito duro, astuto; postura de quem comanda; no gesto de chicote; magia roxo-sangue.
- **Cobradora de Dívidas** · M2 · Padrão · Dominum · estilo **P** · 1:1  
  id `cobradora-de-dividas`  
  > Cobradora de Dívidas: cobradora; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente; no gesto de porrete; magia roxo-sangue.
- **Senhor de Escravos da Voragem** · M3 · Chefe · Dominum · Anulador · estilo **P** · 1:1  
  id `senhor-de-escravos-da-voragem`  
  > Senhor de Escravos da Voragem: senhor, tirano; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito duro, covarde; pose imponente de chefe; no gesto de chicote; magia roxo-sangue.
- **Menino Que Manda nos Cães** · M1 · Social · Dominum · estilo **P** · 1:1  
  id `menino-que-manda-nos-caes`  
  > Menino Que Manda nos Cães: menino, matilha; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito minúsculo, leal; pose de conversa; no gesto de assobiar; magia roxo-sangue.
- **Carpideira** · M1 · Social · Persona · estilo **P** · 1:1  
  id `carpideira`  
  > Carpideira: carpideira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente; pose de conversa; no gesto de chorar; magia coral.
- **Lutador de Pele de Pedra** · M2 · Bruto · Persona · estilo **P** · 1:1  
  id `lutador-de-pele-de-pedra`  
  > Lutador de Pele de Pedra: lutador; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito enorme; grande e pesado; no gesto de socar; magia coral.
- **Rainha do Carnaval** · M3 · Líder · Persona · estilo **P** · 1:1  
  id `rainha-do-carnaval`  
  > Rainha do Carnaval: rainha, festa; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito belo, barulhento; postura de quem comanda; no gesto de dançar; magia coral.
- **Cirurgião de Rostos** · M2 · Conjurador · Persona · estilo **P** · 1:1  
  id `cirurgiao-de-rostos`  
  > Cirurgião de Rostos: cirurgião; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito estranho (tem um rosto novo a cada evento); no gesto de bisturi; magia coral.
- **Faroleira de Vau-Grande** · M1 · Suporte · Alvorum · estilo **P** · 1:1  
  id `faroleira-de-vau-grande`  
  > Faroleira de Vau-Grande: faroleira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito brilhante; pose de quem ajuda os outros; no gesto de cintilar; magia dourada e preta.
- **Vidente de Feira** · M1 · Social · Alvorum · estilo **P** · 1:1  
  id `vidente-de-feira`  
  > Vidente de Feira: vidente, charlatã; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito mentiroso; pose de conversa; no gesto de ler; magia dourada e preta.
- **Caçador de Pesadelos** · M3 · Ágil · Alvorum · Resiliente · estilo **P** · 1:1  
  id `cacador-de-pesadelos`  
  > Caçador de Pesadelos: caçador, sonhador; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito pálido, veloz; leve e veloz; no gesto de lâmina; magia dourada e preta.
- **Bispa do Eclipse** · M4 · Conjurador · Alvorum · Resiliente · estilo **P** · 1:1  
  id `bispa-do-eclipse`  
  > Bispa do Eclipse: bispa, sombra; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito antigo, brilhante; no gesto de raio; magia dourada e preta.
- **Curandeira de Beira de Estrada** · M1 · Suporte · Morbidyum · estilo **P** · 1:1  
  id `curandeira-de-beira-de-estrada`  
  > Curandeira de Beira de Estrada: curandeira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito pálido; pose de quem ajuda os outros; no gesto de curar; magia verde-pálida.
- **Embalsamador** · M2 · Conjurador · Morbidyum · estilo **P** · 1:1  
  id `embalsamador`  
  > Embalsamador: embalsamador, artesão; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito silencioso, imundo; no gesto de tocar; magia verde-pálida.
- **Ceifadora da Peste** · M3 · Conjurador · Morbidyum · Resiliente · estilo **P** · 1:1  
  id `ceifadora-da-peste`  
  > Ceifadora da Peste: ceifadora, doença; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito imundo, paciente; no gesto de foice; magia verde-pálida.
- **Médium do Luto** · M1 · Social · Morbidyum · estilo **P** · 1:1  
  id `medium-do-luto`  
  > Médium do Luto: médium; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito pálido; pose de conversa; no gesto de falar; magia verde-pálida.
- **Professor de Lógica** · M1 · Social · Cognitium · estilo **P** · 1:1  
  id `professor-de-logica`  
  > Professor de Lógica: professor; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito astuto; pose de conversa; no gesto de discutir; magia índigo.
- **Ladra de Memórias** · M2 · Ágil · Cognitium · estilo **P** · 1:1  
  id `ladra-de-memorias`  
  > Ladra de Memórias: ladra; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito invisível, veloz; leve e veloz; no gesto de faca; magia índigo.
- **Doutrinador** · M3 · Líder · Cognitium · Anulador · estilo **P** · 1:1  
  id `doutrinador`  
  > Doutrinador: pregador, mente; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito belo, mentiroso; postura de quem comanda; no gesto de falar; magia índigo.
- **Ilusionista do Labirinto** · M2 · Conjurador · Cognitium · estilo **P** · 1:1  
  id `ilusionista-do-labirinto`  
  > Ilusionista do Labirinto: ilusionista; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito mentiroso, estranho (a sombra dele aponta para o lado errado); no gesto de faca; magia índigo.
- **Mensageira do Portal** · M2 · Ágil · Eternyr · estilo **P** · 1:1  
  id `mensageira-do-portal`  
  > Mensageira do Portal: mensageira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito veloz; leve e veloz; no gesto de faca; magia azul-noite com riscos brancos.
- **Guardião do Ciclo** · M3 · Padrão · Eternyr · Resiliente · estilo **P** · 1:1  
  id `guardiao-do-ciclo`  
  > Guardião do Ciclo: guardião, relógio; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente, duro; no gesto de bastão; magia azul-noite com riscos brancos.
- **Viajante do Entretempo** · M3 · Social · Eternyr · estilo **P** · 1:1  
  id `viajante-do-entretempo`  
  > Viajante do Entretempo: viajante; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito antigo, estranho (fala do amanhã no passado); pose de conversa; no gesto de contar; magia azul-noite com riscos brancos.
- **Engenheira de Distâncias** · M2 · Suporte · Eternyr · estilo **P** · 1:1  
  id `engenheira-de-distancias`  
  > Engenheira de Distâncias: engenheira; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito astuto; pose de quem ajuda os outros; no gesto de medir; magia azul-noite com riscos brancos.
- **Druida de Tahwë** · M2 · Suporte · Primitivum · estilo **P** · 1:1  
  id `druida-de-tahwe`  
  > Druida de Tahwë: druida; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito paciente; pose de quem ajuda os outros; no gesto de cajado; magia verde-musgo e âmbar.
- **Metamorfo Caçador** · M2 · Ágil · Primitivum · estilo **P** · 1:1  
  id `metamorfo-cacador`  
  > Metamorfo Caçador: caçador, lobo; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito veloz, faminto; leve e veloz; no gesto de garra; magia verde-musgo e âmbar.
- **Invocadora de Espíritos** · M3 · Conjurador · Primitivum · estilo **P** · 1:1  
  id `invocadora-de-espiritos`  
  > Invocadora de Espíritos: invocadora, xamã; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito antigo; no gesto de cajado; magia verde-musgo e âmbar.
- **Menina-Raposa** · M1 · Ágil · Primitivum · estilo **P** · 1:1  
  id `menina-raposa`  
  > Menina-Raposa: menina, raposa; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito minúsculo, covarde; leve e veloz; no gesto de morder; magia verde-musgo e âmbar.
- **Professora Ciantia** · M4 · Chefe · todos os Valores · Quimerarcano, Resiliente · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `ciantia-dragoes`  
  > Mulher jovem, loira com mechas brancas platinadas, olhos verdes, roupa preta e jaqueta com bordas de veludo, sorriso confiante e controlado. Um dragãozinho de chamas fantasmagóricas translúcidas enrolado no braço dela; atrás dela, a cabeça de um dragão enorme emoldura a página, e as chamas de nove cores (uma por Valor) escorrem pela borda por cima do texto.
- **DisScar** · M3 · Chefe · Persona, Conflitüo, Cognitium · Quimerarcano · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `disscar-rasgo`  
  > Roqueiro punk de pele branca, meio jovem, roupas de couro, cabelo curto em degradê com moicano, escuro com partes pintadas de roxo vibrante, piercings nas orelhas e na boca, sorriso desafiador de canto a canto. Segura uma guitarra com lâminas; ondas de som coral com faíscas roxas saem dela e empurram as colunas de texto para as bordas, e três clones dele saem das ondas, cada um numa pose diferente.
- **O Antigo Bibliotecário** · M4 · Social · Cognitium · Inquebrável · estilo **P** · 1:1  
  id `o-antigo-bibliotecario`  
  > O Antigo Bibliotecário: bibliotecário, homem que pensou; um Comprometido (usuário de magia), a magia visível em volta das mãos; jeito antigo, estranho (sempre parece confuso, e é); pose de conversa; no gesto de achar; magia índigo.

### Bestiário · Povos infundidos (15)

- **Menina de Pele de Mercúrio** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `menina-de-pele-de-mercurio`  
  > Menina de Pele de Mercúrio: menina, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito estranho (a pele reflete quem olha); pose de conversa; no gesto de escorrer; magia prata-mercúrio.
- **Estivador de Pedra** · M1 · Bruto · Sublimeyn · estilo **P** · 1:1  
  id `estivador-de-pedra`  
  > Estivador de Pedra: estivador, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito pesado, duro; grande e pesado; no gesto de esmagar; magia prata-mercúrio.
- **Marcada da Corrente** · M2 · Padrão · Dominum · estilo **P** · 1:1  
  id `marcada-da-corrente`  
  > Marcada da Corrente: guerreira, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito leal; no gesto de corrente; magia roxo-sangue.
- **Rapaz do Rosto Mutável** · M1 · Social · Persona · estilo **P** · 1:1  
  id `rapaz-do-rosto-mutavel`  
  > Rapaz do Rosto Mutável: rapaz, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito belo; pose de conversa; no gesto de mudar; magia coral.
- **Cantora-Coral** · M2 · Suporte · Persona · estilo **P** · 1:1  
  id `cantora-coral`  
  > Cantora-Coral: cantora, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito belo, brilhante; pose de quem ajuda os outros; no gesto de cantar; magia coral.
- **Olhos-de-Farol** · M1 · Atirador · Alvorum · estilo **P** · 1:1  
  id `olhos-de-farol`  
  > Olhos-de-Farol: vigia, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito brilhante; no gesto de pedra; magia dourada e preta.
- **Sonâmbula das Duas Pálpebras** · M1 · Social · Alvorum · estilo **P** · 1:1  
  id `sonambula-das-duas-palpebras`  
  > Sonâmbula das Duas Pálpebras: sonâmbula, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito estranho; pose de conversa; no gesto de andar; magia dourada e preta.
- **Velho de Ossos Claros** · M1 · Social · Morbidyum · estilo **P** · 1:1  
  id `velho-de-ossos-claros`  
  > Velho de Ossos Claros: ancião, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito pálido, antigo; pose de conversa; no gesto de contar; magia verde-pálida.
- **Moça Que Não Sangra** · M2 · Padrão · Morbidyum · estilo **P** · 1:1  
  id `moca-que-nao-sangra`  
  > Moça Que Não Sangra: guerreira, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito duro, pálido; no gesto de espada; magia verde-pálida.
- **Esquecido de Nascença** · M1 · Social · Cognitium · estilo **P** · 1:1  
  id `esquecido-de-nascenca`  
  > Esquecido de Nascença: rapaz, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito invisível; pose de conversa; no gesto de passar; magia índigo.
- **Contadora de Mentes** · M2 · Social · Cognitium · estilo **P** · 1:1  
  id `contadora-de-mentes`  
  > Contadora de Mentes: contadora, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito astuto; pose de conversa; no gesto de calcular; magia índigo.
- **Menino de Dois Instantes** · M1 · Social · Eternyr · estilo **P** · 1:1  
  id `menino-de-dois-instantes`  
  > Menino de Dois Instantes: menino, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito veloz, estranho; pose de conversa; no gesto de estar; magia azul-noite com riscos brancos.
- **Anciã do Tempo Lento** · M2 · Suporte · Eternyr · estilo **P** · 1:1  
  id `ancia-do-tempo-lento`  
  > Anciã do Tempo Lento: anciã, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito antigo, paciente; pose de quem ajuda os outros; no gesto de falar; magia azul-noite com riscos brancos.
- **Caçador de Chifres** · M2 · Ágil · Primitivum · estilo **P** · 1:1  
  id `cacador-de-chifres`  
  > Caçador de Chifres: caçador, infundido; de um povo infundido, o Valor visível no próprio corpo; jeito veloz, faminto; leve e veloz; no gesto de chifrar; magia verde-musgo e âmbar.
- **Mulher-Musgo** · M1 · Suporte · Primitivum · estilo **P** · 1:1  
  id `mulher-musgo`  
  > Mulher-Musgo: jardineira, infundida; de um povo infundido, o Valor visível no próprio corpo; jeito paciente, regenerante; pose de quem ajuda os outros; no gesto de enraizar; magia verde-musgo e âmbar.

### Bestiário · Feras e monstros (45)

- **Rato-de-Esgoto Gigante** · M1 · Lacaio · Primitivum · estilo **P** · 1:1  
  id `rato-de-esgoto-gigante`  
  > Rato-de-Esgoto Gigante: rato; criatura selvagem; jeito numeroso, imundo; um de um bando; no gesto de roer; magia verde-musgo e âmbar.
- **Javali de Ferro** · M1 · Bruto · Primitivum · estilo **P** · 1:1  
  id `javali-de-ferro`  
  > Javali de Ferro: javali; criatura selvagem; jeito blindado, furioso; grande e pesado; no gesto de chifrar; magia verde-musgo e âmbar.
- **Lobo-de-Cinza** · M1 · Ágil · Morbidyum · estilo **P** · 1:1  
  id `lobo-de-cinza`  
  > Lobo-de-Cinza: lobo, luto; criatura selvagem; jeito pálido, faminto; leve e veloz; no gesto de morder; magia verde-pálida.
- **Aranha-Tecelã** · M1 · Atirador · Dominum · estilo **P** · 1:1  
  id `aranha-tecela`  
  > Aranha-Tecelã: aranha; criatura selvagem; jeito venenoso, silencioso; no gesto de picar; magia roxo-sangue.
- **Corvo-Presságio** · M1 · Suporte · Morbidyum · estilo **P** · 1:1  
  id `corvo-pressagio`  
  > Corvo-Presságio: corvo, aviso; criatura selvagem; jeito voador, barulhento; pose de quem ajuda os outros; no gesto de bicar; magia verde-pálida.
- **Cão de Coleira Dupla** · M2 · Padrão · Dominum · estilo **P** · 1:1  
  id `cao-de-coleira-dupla`  
  > Cão de Coleira Dupla: cão, servo; criatura selvagem; jeito leal, duro; no gesto de morder; magia roxo-sangue.
- **Serpente-Espelho** · M2 · Ágil · Alvorum · estilo **P** · 1:1  
  id `serpente-espelho`  
  > Serpente-Espelho: serpente; criatura selvagem; jeito brilhante, venenoso; leve e veloz; no gesto de picar; magia dourada e preta.
- **Urso-Raiz** · M2 · Bruto · Primitivum · estilo **P** · 1:1  
  id `urso-raiz`  
  > Urso-Raiz: urso, árvore; criatura selvagem; jeito enorme, regenerante; grande e pesado; no gesto de esmagar; magia verde-musgo e âmbar.
- **Leão de Cem Dentes** · M3 · Chefe · Dominum · Inquebrável · estilo **P** · 1:1  
  id `leao-de-cem-dentes`  
  > Leão de Cem Dentes: leão, filhote de kairvahnn; criatura selvagem; jeito enorme, faminto; pose imponente de chefe; no gesto de despedaçar; magia roxo-sangue.
- **Grifo-de-Vidro** · M3 · Ágil · Sublimeyn · estilo **P** · 1:1  
  id `grifo-de-vidro`  
  > Grifo-de-Vidro: grifo; criatura selvagem; jeito voador, frágil; leve e veloz; no gesto de rasgar; magia prata-mercúrio.
- **Hidra de Barro** · M4 · Chefe · Sublimeyn + Primitivum · estilo **P** · 1:1  
  id `hidra-de-barro`  
  > Hidra de Barro: hidra; criatura selvagem; jeito regenerante, enorme; pose imponente de chefe; no gesto de morder; magia prata-mercúrio.
- **Cavalo de Guerra** · M1 · Padrão · — · estilo **P** · 1:1  
  id `cavalo-de-guerra`  
  > Cavalo de Guerra: cavalo, montaria; criatura selvagem; jeito leal; no gesto de coice.
- **Mula Teimosa** · M1 · Social · — · estilo **P** · 1:1  
  id `mula-teimosa`  
  > Mula Teimosa: mula; criatura selvagem; jeito pesado; pose de conversa; no gesto de empacar.
- **Gato de Navio** · M1 · Social · — · estilo **P** · 1:1  
  id `gato-de-navio`  
  > Gato de Navio: gato; criatura selvagem; jeito minúsculo, silencioso; pose de conversa; no gesto de arranhar.
- **Cão Pastor** · M1 · Suporte · — · estilo **P** · 1:1  
  id `cao-pastor`  
  > Cão Pastor: cão; criatura selvagem; jeito leal; pose de quem ajuda os outros; no gesto de morder.
- **Bode das Encostas** · M1 · Ágil · Primitivum · estilo **P** · 1:1  
  id `bode-das-encostas`  
  > Bode das Encostas: bode; criatura selvagem; jeito veloz; leve e veloz; no gesto de chifrar; magia verde-musgo e âmbar.
- **Enxame de Vespas** · M1 · Lacaio · — · estilo **P** · 1:1  
  id `enxame-de-vespas`  
  > Enxame de Vespas: vespas; criatura selvagem; jeito numeroso, venenoso; um de um bando; no gesto de picar.
- **Crocodilo de Lama** · M2 · Bruto · — · estilo **P** · 1:1  
  id `crocodilo-de-lama`  
  > Crocodilo de Lama: crocodilo; criatura selvagem; jeito aquático, paciente; grande e pesado; no gesto de morder.
- **Morcego-Vampiro Gigante** · M1 · Ágil · Morbidyum · estilo **P** · 1:1  
  id `morcego-vampiro-gigante`  
  > Morcego-Vampiro Gigante: morcego; criatura selvagem; jeito voador, cego; leve e veloz; no gesto de morder; magia verde-pálida.
- **Tatu de Couraça** · M1 · Padrão · — · estilo **P** · 1:1  
  id `tatu-de-couraca`  
  > Tatu de Couraça: tatu; criatura selvagem; jeito blindado, covarde; no gesto de garras.
- **Coruja-Sábia** · M1 · Suporte · Alvorum · estilo **P** · 1:1  
  id `coruja-sabia`  
  > Coruja-Sábia: coruja, vigia; criatura selvagem; jeito voador, silencioso; pose de quem ajuda os outros; no gesto de bicar; magia dourada e preta.
- **Onça-das-Sombras** · M2 · Ágil · Alvorum · estilo **P** · 1:1  
  id `onca-das-sombras`  
  > Onça-das-Sombras: onça; criatura selvagem; jeito silencioso, invisível (no escuro); leve e veloz; no gesto de garra; magia dourada e preta.
- **Touro de Kairvahnn** · M2 · Bruto · Dominum · estilo **P** · 1:1  
  id `touro-de-kairvahnn`  
  > Touro de Kairvahnn: touro, fera; criatura selvagem; jeito enorme, furioso; grande e pesado; no gesto de chifrar; magia roxo-sangue.
- **Matilha da Voragem** · M2 · Lacaio · Dominum · estilo **P** · 1:1  
  id `matilha-da-voragem`  
  > Matilha da Voragem: cães, matilha; criatura selvagem; jeito numeroso, faminto; um de um bando; no gesto de morder; magia roxo-sangue.
- **Quimera de Três Fomes** · M3 · Bruto · Dominum + Primitivum · estilo **P** · 1:1  
  id `quimera-de-tres-fomes`  
  > Quimera de Três Fomes: quimera; criatura selvagem; jeito enorme, faminto; grande e pesado; no gesto de morder; magia roxo-sangue.
- **Mantícora** · M3 · Atirador · Dominum · estilo **P** · 1:1  
  id `manticora`  
  > Mantícora: mantícora; criatura selvagem; jeito voador, espinhoso; no gesto de espinhos; magia roxo-sangue.
- **Basilisco de Estrada** · M3 · Padrão · Sublimeyn · estilo **P** · 1:1  
  id `basilisco-de-estrada`  
  > Basilisco de Estrada: lagarto; criatura selvagem; jeito pesado, paciente; no gesto de morder; magia prata-mercúrio.
- **Verme da Areia** · M3 · Bruto · Primitivum · estilo **P** · 1:1  
  id `verme-da-areia`  
  > Verme da Areia: verme; criatura selvagem; jeito escavador, enorme; grande e pesado; no gesto de engolir; magia verde-musgo e âmbar.
- **Lesma-Lâmpada** · M1 · Social · Alvorum · estilo **P** · 1:1  
  id `lesma-lampada`  
  > Lesma-Lâmpada: lesma; criatura selvagem; jeito brilhante, pesado; pose de conversa; no gesto de brilhar; magia dourada e preta.
- **Garça-de-Neblina** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `garca-de-neblina`  
  > Garça-de-Neblina: garça; criatura selvagem; jeito voador, etéreo; pose de conversa; no gesto de bicar; magia prata-mercúrio.
- **Polvo do Porto de Vidro** · M2 · Ágil · Sublimeyn · estilo **P** · 1:1  
  id `polvo-do-porto-de-vidro`  
  > Polvo do Porto de Vidro: polvo; criatura selvagem; jeito aquático, astuto; leve e veloz; no gesto de tentáculo; magia prata-mercúrio.
- **Tubarão-Lâmina** · M2 · Bruto · Conflitüo · estilo **P** · 1:1  
  id `tubarao-lamina`  
  > Tubarão-Lâmina: tubarão; criatura selvagem; jeito aquático, faminto; grande e pesado; no gesto de morder; magia vermelho-ferrugem.
- **Serpente-Marinha** · M3 · Chefe · Primitivum · estilo **P** · 1:1  
  id `serpente-marinha`  
  > Serpente-Marinha: serpente, mar; criatura selvagem; jeito aquático, enorme; pose imponente de chefe; no gesto de morder; magia verde-musgo e âmbar.
- **Caranguejo-Ermitão Gigante** · M2 · Padrão · Sublimeyn · estilo **P** · 1:1  
  id `caranguejo-ermitao-gigante`  
  > Caranguejo-Ermitão Gigante: caranguejo; criatura selvagem; jeito blindado, covarde; no gesto de pinça; magia prata-mercúrio.
- **Sapo-Profeta** · M1 · Social · Morbidyum · estilo **P** · 1:1  
  id `sapo-profeta`  
  > Sapo-Profeta: sapo; criatura selvagem; jeito estranho; pose de conversa; no gesto de coaxar; magia verde-pálida.
- **Ursa-Mãe** · M2 · Bruto · Primitivum · estilo **P** · 1:1  
  id `ursa-mae`  
  > Ursa-Mãe: ursa, mãe; criatura selvagem; jeito enorme, furioso; grande e pesado; no gesto de garras; magia verde-musgo e âmbar.
- **Lebre-Relâmpago** · M1 · Social · Eternyr · estilo **P** · 1:1  
  id `lebre-relampago`  
  > Lebre-Relâmpago: lebre; criatura selvagem; jeito veloz, minúsculo; pose de conversa; no gesto de sumir; magia azul-noite com riscos brancos.
- **Cervo Branco** · M2 · Social · Alvorum · estilo **P** · 1:1  
  id `cervo-branco`  
  > Cervo Branco: cervo, presságio; criatura selvagem; jeito brilhante, etéreo; pose de conversa; no gesto de aparecer; magia dourada e preta.
- **Gorila de Pedra** · M2 · Bruto · Sublimeyn · estilo **P** · 1:1  
  id `gorila-de-pedra`  
  > Gorila de Pedra: gorila; criatura selvagem; jeito duro, pesado; grande e pesado; no gesto de socar; magia prata-mercúrio.
- **Abutre da Ferida** · M1 · Lacaio · Morbidyum · estilo **P** · 1:1  
  id `abutre-da-ferida`  
  > Abutre da Ferida: abutres; criatura selvagem; jeito voador, numeroso; um de um bando; no gesto de bicar; magia verde-pálida.
- **Escorpião-Rei** · M3 · Padrão · Dominum · estilo **P** · 1:1  
  id `escorpiao-rei`  
  > Escorpião-Rei: escorpião; criatura selvagem; jeito blindado, venenoso; no gesto de pinça; magia roxo-sangue.
- **Besouro-Carruagem** · M1 · Social · — · estilo **P** · 1:1  
  id `besouro-carruagem`  
  > Besouro-Carruagem: besouro, montaria; criatura selvagem; jeito blindado, pesado; pose de conversa; no gesto de chifrar.
- **Ninho de Harpias** · M2 · Lacaio · Conflitüo · estilo **P** · 1:1  
  id `ninho-de-harpias`  
  > Ninho de Harpias: harpias; criatura selvagem; jeito voador, barulhento; um de um bando; no gesto de garras; magia vermelho-ferrugem.
- **Troll de Ponte** · M3 · Bruto · Primitivum · estilo **P** · 1:1  
  id `troll-de-ponte`  
  > Troll de Ponte: troll; criatura selvagem; jeito regenerante, enorme; grande e pesado; no gesto de porrete; magia verde-musgo e âmbar.
- **Mãe-da-Lama** · M4 · Chefe · Primitivum + Morbidyum · Resiliente · estilo **P** · 1:1  
  id `mae-da-lama`  
  > Mãe-da-Lama: pântano, mãe; criatura selvagem; jeito enorme, regenerante; pose imponente de chefe; no gesto de afogar; magia verde-pálida.

### Bestiário · Disruptos (nascidos do Cataclisma) (20)

- **Sobra** · M1 · Lacaio · (sorteado) · Instável · estilo **P** · 1:1  
  id `sobra`  
  > Sobra: sobra; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito estranho (tem a forma de uma magia que alguém errou); um de um bando; no gesto de arranhar.
- **O Eco que Come o Som** · M1 · Padrão · Alvorum · Instável · prompt do [[20 Prompts de arte - personagens]] · estilo **P** · 1:1  
  id `eco-sobras`  
  > Massa de fios cinza-perolados cobrindo um sino, rosto sem olhos feito só de bocas abertas, fios comendo as letras de um parágrafo e deixando buracos brancos no texto. Duas bolas de fio pequenas rolando pela margem.
- **Rasgo Faminto** · M2 · Padrão · (sorteado) · Instável · estilo **P** · 1:1  
  id `rasgo-faminto`  
  > Rasgo Faminto: rasgo, boca; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito faminto de magia; no gesto de morder.
- **Eco de Magia Morta** · M2 · Conjurador · (sorteado) · Instável · estilo **P** · 1:1  
  id `eco-de-magia-morta`  
  > Eco de Magia Morta: eco; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito etéreo, estranho; no gesto de repetir.
- **Nó de Ímpares** · M3 · Bruto · Quimerarcano · Instável · estilo **P** · 1:1  
  id `no-de-impares`  
  > Nó de Ímpares: nó; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito enorme, faminto de magia; grande e pesado; no gesto de esmagar.
- **Disrupto Gêmeo** · M3 · Ágil (sempre 2) · (sorteado) · Instável · estilo **P** · 1:1  
  id `disrupto-gemeo`  
  > Disrupto Gêmeo: metade; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito leal (se um cai, o outro age na hora); no gesto de cortar.
- **A Coisa Que Sobrou** · M4 · Chefe · três Valores · Quimerarcano, Resiliente, Instável · estilo **P** · 1:1  
  id `a-coisa-que-sobrou`  
  > A Coisa Que Sobrou: sobra, tempestade; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito estranho, enorme; pose imponente de chefe; no gesto de rasgar.
- **Chuva de Faíscas** · M1 · Lacaio · Conflitüo · Instável · estilo **P** · 1:1  
  id `chuva-de-faiscas`  
  > Chuva de Faíscas: faíscas; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito numeroso, ardente; um de um bando; no gesto de queimar; magia vermelho-ferrugem.
- **Poça que Pensa** · M1 · Social · Sublimeyn · Instável · estilo **P** · 1:1  
  id `poca-que-pensa`  
  > Poça que Pensa: poça, disrupto; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito estranho, minúsculo; pose de conversa; no gesto de imitar; magia prata-mercúrio.
- **Carcaça Andante** · M2 · Bruto · Morbidyum · Instável · estilo **P** · 1:1  
  id `carcaca-andante`  
  > Carcaça Andante: cadáver, sobra; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito imundo, duro; grande e pesado; no gesto de esmagar; magia verde-pálida.
- **Coleira Sem Dono** · M2 · Padrão · Dominum · Instável · estilo **P** · 1:1  
  id `coleira-sem-dono`  
  > Coleira Sem Dono: coleira, corrente; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito faminto, estranho (procura um pescoço); no gesto de chicotear; magia roxo-sangue.
- **Rosto Sem Gente** · M2 · Conjurador · Persona · Instável · estilo **P** · 1:1  
  id `rosto-sem-gente`  
  > Rosto Sem Gente: rosto, máscara; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito belo e grotesco, alternando; no gesto de gritar; magia coral.
- **Sombra Que Ficou** · M2 · Ágil · Alvorum · Instável · estilo **P** · 1:1  
  id `sombra-que-ficou`  
  > Sombra Que Ficou: sombra; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito invisível, silencioso; leve e veloz; no gesto de arranhar; magia dourada e preta.
- **Relógio Quebrado** · M2 · Conjurador · Eternyr · Instável · estilo **P** · 1:1  
  id `relogio-quebrado`  
  > Relógio Quebrado: relógio, disrupto; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito estranho, barulhento; no gesto de ponteiro; magia azul-noite com riscos brancos.
- **Pensamento Solto** · M1 · Conjurador · Cognitium · Instável · estilo **P** · 1:1  
  id `pensamento-solto`  
  > Pensamento Solto: ideia, disrupto; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito invisível; no gesto de sussurrar; magia índigo.
- **Raiz Errada** · M2 · Padrão · Primitivum · Instável · estilo **P** · 1:1  
  id `raiz-errada`  
  > Raiz Errada: raiz, disrupto; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito regenerante, espinhoso; no gesto de enroscar; magia verde-musgo e âmbar.
- **Enxame de Palavras** · M3 · Lacaio · (sorteado) · Instável · estilo **P** · 1:1  
  id `enxame-de-palavras`  
  > Enxame de Palavras: palavras; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito numeroso, faminto de magia; um de um bando; no gesto de cortar.
- **Colosso de Ferida** · M4 · Bruto · (sorteado) + Conflitüo · Instável, Resiliente · estilo **P** · 1:1  
  id `colosso-de-ferida`  
  > Colosso de Ferida: colosso, ferida; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito enorme, pesado; grande e pesado; no gesto de esmagar; magia vermelho-ferrugem.
- **Menino de Cataclisma** · M3 · Social · (sorteado) · Instável · estilo **P** · 1:1  
  id `menino-de-cataclisma`  
  > Menino de Cataclisma: menino, disrupto; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito minúsculo, faminto de magia; pose de conversa; no gesto de chorar.
- **Maré de Sobras** · M3 · Chefe · (sorteado) · Instável, Quimerarcano · estilo **P** · 1:1  
  id `mare-de-sobras`  
  > Maré de Sobras: maré, sobras; criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas; jeito numeroso, estranho; pose imponente de chefe; no gesto de afogar.

### Bestiário · Protetores do Arcano (15)

- **Sentinela de Selo** · M2 · Padrão · — · Anulador · estilo **P** · 1:1  
  id `sentinela-de-selo`  
  > Sentinela de Selo: guardião, selo; guardião que anula a magia, armadura com selos e lacres; jeito duro, paciente; no gesto de golpear.
- **Caçador de Ímpares** · M3 · Ágil · — · Anulador · estilo **P** · 1:1  
  id `cacador-de-impares`  
  > Caçador de Ímpares: caçador; guardião que anula a magia, armadura com selos e lacres; jeito silencioso, veloz; leve e veloz; no gesto de atirar.
- **Juiz do Selo** · M4 · Chefe · — · Anulador, Inquebrável · estilo **P** · 1:1  
  id `juiz-do-selo`  
  > Juiz do Selo: juiz, estátua; guardião que anula a magia, armadura com selos e lacres; jeito sagrado, pesado; pose imponente de chefe; no gesto de golpear.
- **Arquivista Sem Rosto** · M3 · Suporte · Cognitium · Resiliente · estilo **P** · 1:1  
  id `arquivista-sem-rosto`  
  > Arquivista Sem Rosto: arquivista; guardião que anula a magia, armadura com selos e lacres; jeito astuto, mentiroso; pose de quem ajuda os outros; no gesto de anotar; magia índigo.
- **Noviça da Ordem** · M1 · Suporte · — · Anulador · estilo **P** · 1:1  
  id `novica-da-ordem`  
  > Noviça da Ordem: noviça, protetora; guardião que anula a magia, armadura com selos e lacres; jeito paciente; pose de quem ajuda os outros; no gesto de bastão.
- **Inspetor de Escola** · M2 · Social · — · Anulador · estilo **P** · 1:1  
  id `inspetor-de-escola`  
  > Inspetor de Escola: inspetor, protetor; guardião que anula a magia, armadura com selos e lacres; jeito duro, paciente; pose de conversa; no gesto de anotar.
- **Carcereira de Palavras** · M3 · Padrão · — · Anulador · estilo **P** · 1:1  
  id `carcereira-de-palavras`  
  > Carcereira de Palavras: carcereira, protetora; guardião que anula a magia, armadura com selos e lacres; jeito duro; no gesto de corrente.
- **Mestre da Torre do Silêncio** · M4 · Líder · — · Anulador, Inquebrável · estilo **P** · 1:1  
  id `mestre-da-torre-do-silencio`  
  > Mestre da Torre do Silêncio: mestre, torre; guardião que anula a magia, armadura com selos e lacres; jeito antigo, sagrado; postura de quem comanda; no gesto de bastão.
- **Olho-que-Fecha** · M1 · Social · — · Anulador · estilo **P** · 1:1  
  id `olho-que-fecha`  
  > Olho-que-Fecha (espécie): olho, protetor; guardião que anula a magia, armadura com selos e lacres; jeito minúsculo, voador; pose de conversa; no gesto de piscar.
- **Cão-Selo** · M2 · Padrão · — · Anulador · estilo **P** · 1:1  
  id `cao-selo`  
  > Cão-Selo (espécie): cão, selo; guardião que anula a magia, armadura com selos e lacres; jeito leal, silencioso; no gesto de morder.
- **Muralha Viva** · M3 · Bruto · — · Anulador, Inquebrável · estilo **P** · 1:1  
  id `muralha-viva`  
  > Muralha Viva (espécie): muralha, protetor; guardião que anula a magia, armadura com selos e lacres; jeito enorme, pesado; grande e pesado; no gesto de esmagar.
- **Enxame de Selos** · M2 · Lacaio · — · Anulador · estilo **P** · 1:1  
  id `enxame-de-selos`  
  > Enxame de Selos (espécie): selos; guardião que anula a magia, armadura com selos e lacres; jeito numeroso, voador; um de um bando; no gesto de grudar.
- **Anjo do Fechamento** · M4 · Chefe · — · Anulador, Inquebrável, Resiliente · estilo **P** · 1:1  
  id `anjo-do-fechamento`  
  > Anjo do Fechamento (espécie): anjo, selo; guardião que anula a magia, armadura com selos e lacres; jeito sagrado, brilhante; pose imponente de chefe; no gesto de espada.
- **Desertora da Ordem** · M2 · Ágil · Alvorum · — · estilo **P** · 1:1  
  id `desertora-da-ordem`  
  > Desertora da Ordem: desertora, protetora; guardião que anula a magia, armadura com selos e lacres; jeito veloz, covarde; leve e veloz; no gesto de espada; magia dourada e preta.
- **Guardiã da Fronteira** · M2 · Atirador · — · Anulador · estilo **P** · 1:1  
  id `guardia-da-fronteira`  
  > Guardiã da Fronteira: guardiã, protetora; guardião que anula a magia, armadura com selos e lacres; jeito paciente; no gesto de besta.

### Bestiário · Seres de energia (14)

- **Fogo-Fátuo** · M1 · Ágil · Alvorum · estilo **P** · 1:1  
  id `fogo-fatuo`  
  > Fogo-Fátuo: luz, engano; ser feito de energia luminosa e sólida, bruxuleante; jeito etéreo, brilhante; leve e veloz; no gesto de queimar; magia dourada e preta.
- **Vento Com Rancor** · M2 · Atirador · Sublimeyn · estilo **P** · 1:1  
  id `vento-com-rancor`  
  > Vento Com Rancor: vento; ser feito de energia luminosa e sólida, bruxuleante; jeito etéreo, invisível; no gesto de cortar; magia prata-mercúrio.
- **Chama que Lembra** · M2 · Conjurador · Sublimeyn + Cognitium · estilo **P** · 1:1  
  id `chama-que-lembra`  
  > Chama que Lembra: chama, memória; ser feito de energia luminosa e sólida, bruxuleante; jeito ardente, etéreo; no gesto de queimar; magia prata-mercúrio.
- **Hora Solta** · M3 · Ágil · Eternyr · Resiliente · estilo **P** · 1:1  
  id `hora-solta`  
  > Hora Solta: instante; ser feito de energia luminosa e sólida, bruxuleante; jeito etéreo, estranho (não pode ser atacada duas vezes seguidas pelo mesmo jogador); leve e veloz; no gesto de tocar; magia azul-noite com riscos brancos.
- **Faísca de Tempestade** · M1 · Lacaio · Conflitüo · estilo **P** · 1:1  
  id `faisca-de-tempestade`  
  > Faísca de Tempestade: raio; ser feito de energia luminosa e sólida, bruxuleante; jeito numeroso, brilhante; um de um bando; no gesto de choque; magia vermelho-ferrugem.
- **Nuvem Triste** · M1 · Social · Persona · estilo **P** · 1:1  
  id `nuvem-triste`  
  > Nuvem Triste: nuvem, tristeza; ser feito de energia luminosa e sólida, bruxuleante; jeito etéreo, voador; pose de conversa; no gesto de chover; magia coral.
- **Aurora Que Desceu** · M3 · Suporte · Alvorum · estilo **P** · 1:1  
  id `aurora-que-desceu`  
  > Aurora Que Desceu: aurora; ser feito de energia luminosa e sólida, bruxuleante; jeito brilhante, etéreo, estranho (tem as cores do véu de oulveil); pose de quem ajuda os outros; no gesto de iluminar; magia dourada e preta.
- **Brasa de Forja** · M1 · Padrão · Sublimeyn · estilo **P** · 1:1  
  id `brasa-de-forja`  
  > Brasa de Forja: brasa; ser feito de energia luminosa e sólida, bruxuleante; jeito ardente; no gesto de queimar; magia prata-mercúrio.
- **Vento Mensageiro** · M1 · Social · Eternyr · estilo **P** · 1:1  
  id `vento-mensageiro`  
  > Vento Mensageiro: vento; ser feito de energia luminosa e sólida, bruxuleante; jeito voador, invisível; pose de conversa; no gesto de levar; magia azul-noite com riscos brancos.
- **Luz de Velório** · M2 · Conjurador · Morbidyum · estilo **P** · 1:1  
  id `luz-de-velorio`  
  > Luz de Velório: luz, luto; ser feito de energia luminosa e sólida, bruxuleante; jeito etéreo, pálido; no gesto de queimar; magia verde-pálida.
- **Redemoinho de Areia** · M2 · Bruto · Sublimeyn · estilo **P** · 1:1  
  id `redemoinho-de-areia`  
  > Redemoinho de Areia: redemoinho; ser feito de energia luminosa e sólida, bruxuleante; jeito enorme, etéreo; grande e pesado; no gesto de arrastar; magia prata-mercúrio.
- **Canto Sem Cantor** · M2 · Conjurador · Persona · estilo **P** · 1:1  
  id `canto-sem-cantor`  
  > Canto Sem Cantor: música; ser feito de energia luminosa e sólida, bruxuleante; jeito invisível, belo; no gesto de tocar; magia coral.
- **Fidrau Cantor** · M2 · Suporte · Persona · estilo **P** · 1:1  
  id `fidrau-cantor`  
  > Fidrau Cantor: fidrau, música; ser feito de energia luminosa e sólida, bruxuleante; jeito brilhante, belo; pose de quem ajuda os outros; no gesto de cantar; magia coral.
- **Tempestade Viva** · M4 · Chefe · Sublimeyn + Conflitüo · Resiliente · estilo **P** · 1:1  
  id `tempestade-viva`  
  > Tempestade Viva: tempestade; ser feito de energia luminosa e sólida, bruxuleante; jeito enorme, voador; pose imponente de chefe; no gesto de raio; magia vermelho-ferrugem.

### Bestiário · Seres dos planos dos Valores (25)

- **Juramentado** · M3 · Padrão · Conflitüo · estilo **P** · 1:1  
  id `juramentado`  
  > Juramentado (Campo Sem Trégua): soldado, morto; ser de outro plano, disforme e estranho; jeito duro; no gesto de cortar; magia vermelho-ferrugem.
- **Ferreiro de Guerra** · M3 · Suporte · Conflitüo · estilo **P** · 1:1  
  id `ferreiro-de-guerra`  
  > Ferreiro de Guerra (Campo Sem Trégua): ferreiro, juramentado; ser de outro plano, disforme e estranho; jeito ardente, paciente; pose de quem ajuda os outros; no gesto de martelo; magia vermelho-ferrugem.
- **Estandarte Sem Exército** · M2 · Social · Conflitüo · estilo **P** · 1:1  
  id `estandarte-sem-exercito`  
  > Estandarte Sem Exército (Campo Sem Trégua): estandarte; ser de outro plano, disforme e estranho; jeito estranho; pose de conversa; no gesto de tremular; magia vermelho-ferrugem.
- **Moldado** · M3 · Ágil · Sublimeyn · estilo **P** · 1:1  
  id `moldado`  
  > Moldado (Mar de Estados): moldado; ser de outro plano, disforme e estranho; jeito estranho; leve e veloz; no gesto de envolver; magia prata-mercúrio.
- **Alquimista Náufrago** · M2 · Social · Sublimeyn · estilo **P** · 1:1  
  id `alquimista-naufrago`  
  > Alquimista Náufrago (Mar de Estados): alquimista, náufrago; ser de outro plano, disforme e estranho; jeito estranho (metade do corpo é vidro); pose de conversa; no gesto de trocar; magia prata-mercúrio.
- **Ilha de Vidro Viva** · M4 · Bruto · Sublimeyn · Resiliente · estilo **P** · 1:1  
  id `ilha-de-vidro-viva`  
  > Ilha de Vidro Viva (Mar de Estados): ilha; ser de outro plano, disforme e estranho; jeito enorme, frágil; grande e pesado; no gesto de estilhaçar; magia prata-mercúrio.
- **Manso** · M2 · Social · Dominum · estilo **P** · 1:1  
  id `manso`  
  > Manso (Voragem): servo, nascido-na-voragem; ser de outro plano, disforme e estranho; jeito leal; pose de conversa; no gesto de guiar; magia roxo-sangue.
- **Fera Acorrentada** · M3 · Bruto · Dominum · estilo **P** · 1:1  
  id `fera-acorrentada`  
  > Fera Acorrentada (Voragem): fera, corrente; ser de outro plano, disforme e estranho; jeito enorme, furioso; grande e pesado; no gesto de despedaçar; magia roxo-sangue.
- **Sentido** · M3 · Conjurador · Persona · estilo **P** · 1:1  
  id `sentido`  
  > Sentido (Cidade dos Rostos): emoção (uma só: raiva, saudade, inveja...); ser de outro plano, disforme e estranho; jeito belo ou grotesco; no gesto de contagiar; magia coral.
- **O Seu Rosto** · M3 · Social · Persona · estilo **P** · 1:1  
  id `o-seu-rosto`  
  > O Seu Rosto (Cidade dos Rostos): duplo; ser de outro plano, disforme e estranho; jeito estranho (tem o rosto de um dos jogadores); pose de conversa; no gesto de imitar; magia coral.
- **Vigia** · M3 · Atirador · Alvorum · Anulador · estilo **P** · 1:1  
  id `vigia`  
  > Vigia (Céu de Duas Pálpebras): vigia; ser de outro plano, disforme e estranho; jeito cego (vê por dentro); no gesto de flecha; magia dourada e preta.
- **Sonhador Perdido** · M2 · Social · Alvorum · estilo **P** · 1:1  
  id `sonhador-perdido`  
  > Sonhador Perdido (Céu de Duas Pálpebras): sonhador; ser de outro plano, disforme e estranho; jeito etéreo; pose de conversa; no gesto de contar; magia dourada e preta.
- **Fantasma em Fila** · M1 · Lacaio · Morbidyum · estilo **P** · 1:1  
  id `fantasma-em-fila`  
  > Fantasma em Fila (Antessala): fantasmas; ser de outro plano, disforme e estranho; jeito numeroso, etéreo; um de um bando; no gesto de tocar; magia verde-pálida.
- **Porteira da Antessala** · M4 · Social · Morbidyum · Inquebrável · estilo **P** · 1:1  
  id `porteira-da-antessala`  
  > Porteira da Antessala (Antessala): porteira, velha e parteira; ser de outro plano, disforme e estranho; jeito antigo, paciente; pose de conversa; no gesto de perguntar; magia verde-pálida.
- **O Que Voltou** · M3 · Padrão · Morbidyum · estilo **P** · 1:1  
  id `o-que-voltou`  
  > O Que Voltou (Antessala): morto, retornado; ser de outro plano, disforme e estranho; jeito pálido, duro; no gesto de espada; magia verde-pálida.
- **Esquecido** · M2 · Lacaio · Cognitium · estilo **P** · 1:1  
  id `esquecido`  
  > Esquecido (Arquivo Sem Dono): esquecidos; ser de outro plano, disforme e estranho; jeito numeroso, invisível; um de um bando; no gesto de apagar; magia índigo.
- **Bibliotecária do Arquivo** · M3 · Suporte · Cognitium · estilo **P** · 1:1  
  id `bibliotecaria-do-arquivo`  
  > Bibliotecária do Arquivo (Arquivo Sem Dono): bibliotecária; ser de outro plano, disforme e estranho; jeito astuto, paciente; pose de quem ajuda os outros; no gesto de achar; magia índigo.
- **Sombra do Trono Vazio** · M4 · Chefe · Cognitium · Inquebrável, Resiliente · estilo **P** · 1:1  
  id `sombra-do-trono-vazio`  
  > Sombra do Trono Vazio (Arquivo Sem Dono): trono, ausência; ser de outro plano, disforme e estranho; jeito invisível, antigo; pose imponente de chefe; no gesto de esquecer; magia índigo.
- **Repetido** · M3 · Lacaio · Eternyr · estilo **P** · 1:1  
  id `repetido`  
  > Repetido (Entretempo): repetido; ser de outro plano, disforme e estranho; jeito numeroso, paciente; um de um bando; no gesto de repetir; magia azul-noite com riscos brancos.
- **Sala Que É Um Ano** · M3 · Social · Eternyr · estilo **P** · 1:1  
  id `sala-que-e-um-ano`  
  > Sala Que É Um Ano (Entretempo): sala, ano; ser de outro plano, disforme e estranho; jeito estranho; pose de conversa; no gesto de envelhecer; magia azul-noite com riscos brancos.
- **Espírito-Animal** · M2 · Padrão · Primitivum · estilo **P** · 1:1  
  id `espirito-animal`  
  > Espírito-Animal (Floresta Primeira): espírito, animal; ser de outro plano, disforme e estranho; jeito etéreo, leal; no gesto de garra; magia verde-musgo e âmbar.
- **Bicho-Antes-dos-Bichos** · M4 · Chefe · Primitivum · Deidade · estilo **P** · 1:1  
  id `bicho-antes-dos-bichos`  
  > Bicho-Antes-dos-Bichos (Floresta Primeira): fera, origem; ser de outro plano, disforme e estranho; jeito enorme, antigo; pose imponente de chefe; no gesto de morder; magia verde-musgo e âmbar.
- **Autômato do Postulário** · M4 · Bruto · — · Anulador, Inquebrável · estilo **P** · 1:1  
  id `automato-do-postulario`  
  > Autômato do Postulário (O Postulário): autômato, guardião; ser de outro plano, disforme e estranho; jeito enorme, paciente; grande e pesado; no gesto de esmagar.
- **O Ministrum** · M4 · Líder · Dominum + Cognitium · Inquebrável · estilo **P** · 1:1  
  id `o-ministrum`  
  > O Ministrum (Nostrum): juiz, lógica; ser de outro plano, disforme e estranho; jeito antigo, duro; postura de quem comanda; no gesto de sentenciar; magia roxo-sangue.
- **A Dama da Noite de Fhim** · M4 · Social · Alvorum · Deidade · estilo **P** · 1:1  
  id `a-dama-da-noite-de-fhim`  
  > A Dama da Noite de Fhim (Fhim): divindade, noite; ser de outro plano, disforme e estranho; jeito belo, etéreo; pose de conversa; no gesto de observar; magia dourada e preta.

### Bestiário · Semideuses (9)

- **Vyal, o Primeiro Filho** · M4 · Chefe · Persona · Deidade · estilo **P** · 1:1  
  id `vyal-o-primeiro-filho`  
  > Vyal, o Primeiro Filho: semideus, filho de zahun; semideus, presença enorme e solene, aura divina; jeito belo, antigo; pose imponente de chefe; no gesto de tocar; magia coral.
- **Serath Duas-Mãos** · M4 · Chefe · Conflitüo · Deidade · estilo **P** · 1:1  
  id `serath-duas-maos`  
  > Serath Duas-Mãos: semideusa, general; semideus, presença enorme e solene, aura divina; jeito blindado, antigo; pose imponente de chefe; no gesto de cortar; magia vermelho-ferrugem.
- **Liss de Vidro** · M4 · Chefe · Sublimeyn · Deidade · estilo **P** · 1:1  
  id `liss-de-vidro`  
  > Liss de Vidro: semideusa, rainha; semideus, presença enorme e solene, aura divina; jeito belo, estranho; pose imponente de chefe; no gesto de toque; magia prata-mercúrio.
- **Garmhal, o Domador** · M4 · Chefe · Dominum + Primitivum · Deidade, Quimerarcano · estilo **P** · 1:1  
  id `garmhal-o-domador`  
  > Garmhal, o Domador: semideus, cavaleiro; semideus, presença enorme e solene, aura divina; jeito enorme (com a montaria), leal; pose imponente de chefe; no gesto de chicotear; magia roxo-sangue.
- **Hael, o Insone** · M4 · Chefe · Alvorum · Deidade · estilo **P** · 1:1  
  id `hael-o-insone`  
  > Hael, o Insone: semideus, insone; semideus, presença enorme e solene, aura divina; jeito pálido, brilhante; pose imponente de chefe; no gesto de olhar; magia dourada e preta.
- **Morwen Terceira-Vida** · M4 · Suporte · Morbidyum · Deidade · estilo **P** · 1:1  
  id `morwen-terceira-vida`  
  > Morwen Terceira-Vida: semideusa, curandeira; semideus, presença enorme e solene, aura divina; jeito paciente, antigo; pose de quem ajuda os outros; no gesto de curar; magia verde-pálida.
- **O Sem-Nome** · M4 · Social · Cognitium · Deidade · estilo **P** · 1:1  
  id `o-sem-nome`  
  > O Sem-Nome: semideus, esquecido; semideus, presença enorme e solene, aura divina; jeito invisível, antigo; pose de conversa; no gesto de existir; magia índigo.
- **Tique** · M4 · Social · Eternyr · Deidade · estilo **P** · 1:1  
  id `tique`  
  > Tique: criança, semideus; semideus, presença enorme e solene, aura divina; jeito minúsculo, antigo; pose de conversa; no gesto de brincar; magia azul-noite com riscos brancos.
- **Rhu, a Matilha** · M4 · Chefe · Primitivum · Deidade · estilo **P** · 1:1  
  id `rhu-a-matilha`  
  > Rhu, a Matilha: semideus, matilha; semideus, presença enorme e solene, aura divina; jeito numeroso, leal; pose imponente de chefe; no gesto de morder; magia verde-musgo e âmbar.

### Bestiário · Dragões e wyverns (10)

- **Dragonete** · M1 · Ágil · Valor do dragão · estilo **D** · 1:1  
  id `dragonete`  
  > Dragonete: um filhote pequeno, curioso e selvagem; o desejo nele é puro impulso: fome, brilho, a mãe. Cor, forma e chamas do Valor do dragão (desenhe no Valor escolhido; padrão: Conflitüo, vermelho-ferrugem).
- **Dragão Jovem** · M2 · Bruto · Valor do dragão · estilo **D** · 1:1  
  id `dragao-jovem`  
  > Dragão Jovem: um dragão jovem, fera territorial e faminta, ainda sem sabedoria, desejo em impulso. Cor, forma e chamas do Valor do dragão (desenhe no Valor escolhido; padrão: Conflitüo, vermelho-ferrugem).
- **Dragão Adulto** · M3 · Chefe · Valor do dragão · Resiliente · estilo **D** · 1:1  
  id `dragao-adulto`  
  > Dragão Adulto: um dragão adulto, senhor do seu Domínio, orgulhoso e consciente do próprio desejo. Cor, forma e chamas do Valor do dragão (desenhe no Valor escolhido; padrão: Conflitüo, vermelho-ferrugem).
- **Dragão Ancião** · M4 · Chefe · Valor do dragão + um segundo · Deidade · estilo **D** · 1:1  
  id `dragao-anciao`  
  > Dragão Ancião: um dragão ancião, memória do mundo, sábio e paciente, cicatrizes antigas, Eras inteiras no olhar. Cor, forma e chamas do Valor do dragão (desenhe no Valor escolhido; padrão: Conflitüo, vermelho-ferrugem).
- **Aurelhaz, Dragoa-Coral** · M3 · Chefe · Persona · Resiliente · estilo **D** · 1:1  
  id `aurelhaz-dragoa-coral`  
  > Aurelhaz, Dragoa-Coral: dragoa, colecionadora; dragão ou wyvern; jeito belo, voador; pose imponente de chefe; no gesto de morder; magia coral.
- **Velho Ossário, Dragão-Pálido** · M4 · Chefe · Morbidyum + Eternyr · Deidade · estilo **D** · 1:1  
  id `velho-ossario-dragao-palido`  
  > Velho Ossário, Dragão-Pálido: dragão, ancião; dragão ou wyvern; jeito antigo, pálido; pose imponente de chefe; no gesto de morder; magia verde-pálida.
- **Wyvern Selvagem** · M1 · Ágil · (Valor fraco, opcional) · estilo **P** · 1:1  
  id `wyvern-selvagem`  
  > Wyvern Selvagem: wyvern; dragão ou wyvern; jeito voador, covarde; leve e veloz; no gesto de garras.
- **Wyvern de Guerra** · M2 · Padrão · — · estilo **P** · 1:1  
  id `wyvern-de-guerra`  
  > Wyvern de Guerra (montaria de cavaleiros): wyvern, montaria; dragão ou wyvern; jeito voador, leal; no gesto de garras.
- **Wyvern-Correio** · M1 · Social · — · estilo **P** · 1:1  
  id `wyvern-correio`  
  > Wyvern-Correio: wyvern, mensageira; dragão ou wyvern; jeito voador, leal; pose de conversa; no gesto de voar.
- **Wyvern de Carga** · M2 · Bruto · — · estilo **P** · 1:1  
  id `wyvern-de-carga`  
  > Wyvern de Carga: wyvern, montaria; dragão ou wyvern; jeito pesado, voador; grande e pesado; no gesto de garras.

### Bestiário · Era Tecnomaga (6)

- **Autômato de Patrulha** · M2 · Padrão · Sublimeyn · estilo **P** · 1:1  
  id `automato-de-patrulha`  
  > Autômato de Patrulha: autômato, guarda; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito blindado, barulhento; no gesto de porrete; magia prata-mercúrio.
- **Pistoleira de Aluguel** · M2 · Atirador · — · estilo **P** · 1:1  
  id `pistoleira-de-aluguel`  
  > Pistoleira de Aluguel: pistoleira; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito veloz; no gesto de pistola.
- **Hacker de Dispositivos** · M2 · Conjurador · Cognitium · estilo **P** · 1:1  
  id `hacker-de-dispositivos`  
  > Hacker de Dispositivos: hacker, comprometido; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito invisível, astuto; no gesto de invadir; magia índigo.
- **Mecânica de Rua** · M1 · Social · Sublimeyn · estilo **P** · 1:1  
  id `mecanica-de-rua`  
  > Mecânica de Rua: mecânica; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito astuto; pose de conversa; no gesto de consertar; magia prata-mercúrio.
- **Titã de Vapor** · M4 · Chefe · Sublimeyn + Conflitüo · Resiliente · estilo **P** · 1:1  
  id `tita-de-vapor`  
  > Titã de Vapor: titã, máquina; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito enorme, ardente; pose imponente de chefe; no gesto de esmagar; magia vermelho-ferrugem.
- **Executiva da Torre** · M3 · Social · Dominum · estilo **P** · 1:1  
  id `executiva-da-torre`  
  > Executiva da Torre: executiva, dona; estética tecnomaga: magia e máquina, latão, vapor e runas; jeito astuto, mentiroso; pose de conversa; no gesto de comprar; magia roxo-sangue.
