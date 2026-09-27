# Peças v4 — imagens novas para gerar no Canva

182 imagens (17 sem imagem nenhuma hoje: gere estas primeiro; as outras 165 aparecem por enquanto com a imagem emprestada de uma Peça antiga da casa, do mesmo lugar do corpo).

## Como fazer

1. Para cada linha, gere a imagem com o **prompt** exatamente como está (Canva, *generate-image*, formato quadrado 1:1, estilo concept). Se o filtro barrar, troque só a palavra que parece arma por *tool*, *device*, *emitter* ou *prop* e gere de novo.
2. Salve cada uma como **`<id>.png`** (o id da linha) numa pasta, ex.: `arte3/pecas4/img/`. Fundo branco liso; o script tira o fundo.
3. Rode `python3 arte3/pecas4_injeta.py arte3/pecas4/img` — ele tira o fundo, recorta, reduz e grava `arte3/pecas4_imagens.json`.
4. Rode `python3 gerador/web_rapido.py <pasta com os HTML antigos>` (a ficha nova leva as imagens injetadas).
5. **Confira o encaixe, alinhado e proporcional, Peça por Peça.** Rode `python3 arte3/previa_ficha.py previa.jpg --v4 BRASA` (uma fábrica por vez; cada Peça nova aparece montada num chassi da casa, e mais num chassi de outro porte com `--v4 BRASA ANDORINHA`). Olhe a folha e veja, em cada uma: a Peça está no lugar certo do corpo, no tamanho certo em relação ao chassi (nem miniatura nem gigante), alinhada com o membro (antebraço no eixo do antebraço, perna do joelho ao chão, ombro sentado no ombro), sem cobrir o rosto nem sair do quadro, e com a mesma leitura de peso e de estilo das Peças antigas ao lado.
6. **Ajuste o que não estiver bom** em `arte3/ajustes_pecas.json` — um objeto por Peça, só com o que mudar: `{"Rebitadeira": {"esc": 0.85, "dx": 0.05, "dy": -0.1}}`. `esc` multiplica o tamanho (0.85 = 15% menor), `dx`/`dy` empurram em frações do tamanho da própria Peça (dx 0.1 = um décimo da largura dela para a direita; dy negativo sobe). Para braço e cabeça, `"modo"` troca o jeito de encaixar: braço `antebraco`/`mao`/`escudo`, cabeça `troca`/`visor`/`topo`. Rode o web_rapido e a prévia de novo até todas ficarem certas. Se a imagem em si estiver ruim (torta, cortada, com cenário, com mão ou robô junto), gere de novo em vez de ajustar.
7. Os mesmos ajustes valem para as Peças que reaproveitam imagem antiga: se alguma ficou desproporcional no chassi novo, ajuste também.
8. Commit de `arte3/pecas4_imagens.json` e `arte3/ajustes_pecas.json` (e das PNG em `arte3/pecas4/`) no branch, bundle em `_mesclar`.

## Lista

### BRASA (22)

- **Ouvido de Mecânico** · nível 3 · cabeça · encaixe *topo* · **gerar primeiro**  
  id `peca4_BRASA_OuvidoDeMecanico`  
  > Isolated sci-fi prop design for a giant piloted robot: a head-mounted mechanic's listening rig with a brass stethoscope horn and a long probe rod. Front view of a head-mounted attachment, symmetrical. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Olho de Forneiro** · nível 5 · cabeça · encaixe *visor* · **gerar primeiro**  
  id `peca4_BRASA_OlhoDeForneiro`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy riveted welder's visor helmet with a dark glass slit and heat-blackened edges. Front view of a head-mounted attachment, symmetrical. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Filtro de Chaminé** · nível 5 · cabeça · encaixe *visor* · **gerar primeiro**  
  id `peca4_BRASA_FiltroDeChamine`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy riveted respirator head unit with twin round filter canisters and a soot-stained visor. Front view of a head-mounted attachment, symmetrical. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rádio Peão** · nível 7 · cabeça · encaixe *topo* · **gerar primeiro**  
  id `peca4_BRASA_RadioPeao`  
  > Isolated sci-fi prop design for a giant piloted robot: a boxy riveted radio headset unit with a tall whip antenna and a dented brass loudspeaker horn. Front view of a head-mounted attachment, symmetrical. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rebitadeira** · nível 1 · ferramenta na mão  
  id `peca4_BRASA_Rebitadeira`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy pneumatic rivet driver tool with a long nose, an air hose and a drum of glowing red-hot rivets. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Macaco de Escora** · nível 1 · antebraço · encaixe *antebraco*  
  id `peca4_BRASA_MacacoDeEscora`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm-mounted hydraulic jack with a wide riveted lifting pad and a telescoping steel piston. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Sirene de Fábrica** · nível 1 · ombro  
  id `peca4_BRASA_SireneDeFabrica`  
  > Isolated sci-fi prop design for a giant piloted robot: a big riveted factory siren horn on a shoulder bracket with a hand crank and a pull chain. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Punho de Pistão** · nível 2 · antebraço · encaixe *antebraco*  
  id `peca4_BRASA_PunhoDePistao`  
  > Isolated sci-fi prop design for a giant piloted robot: a massive piston-driven riveted fist on a forearm with a steam cylinder and exhaust ports. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Desentulho** · nível 2 · ferramenta na mão  
  id `peca4_BRASA_Desentulho`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant riveted rubble shovel scoop with a reinforced steel lip and a hydraulic hinge. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lança de Caldeira** · nível 3 · ferramenta na mão  
  id `peca4_BRASA_LancaDeCaldeira`  
  > Isolated sci-fi prop design for a giant piloted robot: a long industrial flame lance tool with a riveted fuel tank, braided hoses and a wide scorched nozzle. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Reaperto** · nível 3 · ferramenta na mão  
  id `peca4_BRASA_Reaperto`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant ratchet wrench tool with a riveted handle and a big hexagonal socket. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Punho em Brasa** · nível 4 · antebraço · encaixe *antebraco*  
  id `peca4_BRASA_PunhoEmBrasa`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy riveted forearm gauntlet with glowing red-hot knuckle plates and small cooling fins. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Regulador de Pressão** · nível 5 · costas  
  id `peca4_BRASA_ReguladorDePressao`  
  > Isolated sci-fi prop design for a giant piloted robot: a riveted boiler backpack with a big brass pressure gauge, relief valves and copper pipes. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Escória Quente** · nível 5 · antebraço · encaixe *antebraco*  
  id `peca4_BRASA_EscoriaQuente`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm-mounted foundry ladle with a pouring spout, a crucible of glowing molten slag and heat shields. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Abafador** · nível 6 · costas  
  id `peca4_BRASA_Abafador`  
  > Isolated sci-fi prop design for a giant piloted robot: a backpack foam tank with a long hose reel, a wide spray nozzle and riveted steel panels. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Maçarico de Corte** · nível 8 · ferramenta na mão  
  id `peca4_BRASA_MacaricoDeCorte`  
  > Isolated sci-fi prop design for a giant piloted robot: a long industrial oxygen cutting lance with a white-hot tip, twin gas bottles and a riveted heat shield. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Guincho de Resgate** · nível 8 · costas  
  id `peca4_BRASA_GuinchoDeResgate`  
  > Isolated sci-fi prop design for a giant piloted robot: a backpack crane winch with a tall folding boom, a steel cable and a big rescue hook. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Caldeira Comunitária** · nível 9 · costas  
  id `peca4_BRASA_CaldeiraComunitaria`  
  > Isolated sci-fi prop design for a giant piloted robot: a big riveted boiler backpack with a glowing firebox door, a kettle spout and a row of hanging tin mugs. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ferrete** · nível 9 · ferramenta na mão  
  id `peca4_BRASA_Ferrete`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant branding iron tool with a long riveted shaft and a glowing foundry-crest head. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Salto de Pistão** · nível 9 · pernas (par)  
  id `peca4_BRASA_SaltoDePistao`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of heavy riveted robot legs with huge hydraulic jump pistons and wide stomping boot soles. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **A Brasa Mais Velha** · nível 10 · peito  
  id `peca4_BRASA_ABrasaMaisVelha`  
  > Isolated sci-fi prop design for a giant piloted robot: an ancient patched chest plate covered in decades of weld seams, rivets, soot and chipped layers of industrial paint. Front view of an armor chest piece, symmetrical, flat on. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Corrida de Aço** · nível 10 · peito  
  id `peca4_BRASA_CorridaDeAco`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest furnace door swung open on heavy hinges, with a riveted pouring lip and molten steel glowing inside. Front view of an armor chest piece, symmetrical, flat on. Colors: rust red, soot black and dull brass, industrial paint over older paint, soot stains; no pink, no magenta, no purple; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### VESPERTINA (23)

- **Esmalte Pérola** · nível 1 · peito  
  id `peca4_VESPERTINA_EsmaltePerola`  
  > Isolated sci-fi prop design for a giant piloted robot: a glossy pearl-white enamel chest plate with smooth curved panels, a narrow waist and no visible rivets. Front view of an armor chest piece, symmetrical, flat on. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Asa de Planar** · nível 1 · costas  
  id `peca4_VESPERTINA_AsaDePlanar`  
  > Isolated sci-fi prop design for a giant piloted robot: folding glider wings made of thin white ceramic slats on a slim backpack frame, half open like a fan. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Dardo de Punho** · nível 1 · antebraço · encaixe *antebraco*  
  id `peca4_VESPERTINA_DardoDePunho`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim wrist-mounted dart launcher with a row of long thin steel darts in a white ceramic housing. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Jato de Salto** · nível 1 · pernas (par)  
  id `peca4_VESPERTINA_JatoDeSalto`  
  > Isolated sci-fi prop design for a giant piloted robot: compact jump-jet thruster packs strapped to slim digitigrade robot calves, white casing with twin small nozzles. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Indicativo de Chamada** · nível 1 · cabeça · encaixe *topo*  
  id `peca4_VESPERTINA_IndicativoDeChamada`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim white head-mounted transponder unit with a short swept blade antenna and an amber signal lamp. Front view of a head-mounted attachment, symmetrical. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Cintura de Vespa** · nível 2 · peito  
  id `peca4_VESPERTINA_CinturaDeVespa`  
  > Isolated sci-fi prop design for a giant piloted robot: a wasp-waisted white robot torso with a very narrow ceramic waist joint and a swiveling hip ring. Front view of an armor chest piece, symmetrical, flat on. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mão Esquerda** · nível 2 · antebraço · encaixe *mao*  
  id `peca4_VESPERTINA_MaoEsquerda`  
  > Isolated sci-fi prop design for a giant piloted robot: a short parrying blade with a swept guard mounted on a slim white forearm, like a fencing main-gauche. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Malote Expresso** · nível 2 · costas  
  id `peca4_VESPERTINA_MaloteExpresso`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim sealed message canister on a white back mount with an amber courier stripe and quick-release clamps. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Quadro de Liga Leve** · nível 3 · peito  
  id `peca4_VESPERTINA_QuadroDeLigaLeve`  
  > Isolated sci-fi prop design for a giant piloted robot: an exposed lattice frame of pale titanium alloy struts forming a narrow robot ribcage. Front view of an armor chest piece, symmetrical, flat on. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mão de Sacada** · nível 3 · ferramenta na mão  
  id `peca4_VESPERTINA_MaoDeSacada`  
  > Isolated sci-fi prop design for a giant piloted robot: a slender white robot hand with long precise fingers and soft amber grip pads, open palm up. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Espinha de Titânio** · nível 4 · costas  
  id `peca4_VESPERTINA_EspinhaDeTitanio`  
  > Isolated sci-fi prop design for a giant piloted robot: a slender segmented titanium spine running down a white robot back, with pearl-grey vertebra plates. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Pairar** · nível 4 · pernas (par)  
  id `peca4_VESPERTINA_Pairar`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of slim white robot legs with downward-facing hover thrusters glowing amber under the feet. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ajuste Fino** · nível 5 · ferramenta na mão  
  id `peca4_VESPERTINA_AjusteFino`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim precision tool arm with a fine calibration wrench and a tiny amber-lit gauge in white ceramic housing. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Asa Mestra** · nível 8 · costas  
  id `peca4_VESPERTINA_AsaMestra`  
  > Isolated sci-fi prop design for a giant piloted robot: a sleek jet flight backpack with swept white wings and twin turbines, amber light lines along the edges. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Casco de Pluma** · nível 8 · peito  
  id `peca4_VESPERTINA_CascoDePluma`  
  > Isolated sci-fi prop design for a giant piloted robot: an ultralight white robot chest shell with perforated ceramic plates and overlapping feather-like vanes. Front view of an armor chest piece, symmetrical, flat on. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Levar na Asa** · nível 8 · costas  
  id `peca4_VESPERTINA_LevarNaAsa`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim flight backpack with two retractable tow cables ending in padded white clamps. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ler o Piloto** · nível 8 · cabeça · encaixe *visor*  
  id `peca4_VESPERTINA_LerOPiloto`  
  > Isolated sci-fi prop design for a giant piloted robot: a narrow white visor head with a rotating ring of amber analysis lenses and a thin crest. Front view of a head-mounted attachment, symmetrical. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mergulho em Picada** · nível 9 · costas  
  id `peca4_VESPERTINA_MergulhoEmPicada`  
  > Isolated sci-fi prop design for a giant piloted robot: twin long swept dive wings folded back along a white robot back, with a pointed ceramic nose fairing. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ponte Aérea** · nível 9 · costas  
  id `peca4_VESPERTINA_PonteAerea`  
  > Isolated sci-fi prop design for a giant piloted robot: a rescue flight harness on a white robot back with fold-out carrying cradles and amber beacon lights. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **A Vespertina** · nível 10 · peito  
  id `peca4_VESPERTINA_AVespertina`  
  > Isolated sci-fi prop design for a giant piloted robot: a flawless white and pearl-grey chest plate with a narrow wasp waist, a thin amber crest line and an academy medallion. Front view of an armor chest piece, symmetrical, flat on. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **O Céu É Pista** · nível 10 · costas  
  id `peca4_VESPERTINA_OCeuEPista`  
  > Isolated sci-fi prop design for a giant piloted robot: long swept wings of layered white ceramic feathers with folding amber-lit jet pods at the tips. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ninguém Acima** · nível 10 · ombro  
  id `peca4_VESPERTINA_NinguemAcima`  
  > Isolated sci-fi prop design for a giant piloted robot: four tall swept-back white wing fins fanned out on a shoulder mount with amber lights at each tip, raised like a crown. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Corte do Entardecer** · nível 10 · ferramenta na mão  
  id `peca4_VESPERTINA_CorteDoEntardecer`  
  > Isolated sci-fi prop design for a giant piloted robot: a long curved white ceramic blade with an amber-lit edge glowing like a sunset and trailing light. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: pristine white and pearl grey with amber light accents; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### CATEDRAL (21)

- **Passo de Cortejo** · nível 1 · pernas (par) · **gerar primeiro**  
  id `peca4_CATEDRAL_PassoDeCortejo`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of heavy armored robot legs with plinth-shaped feet, stepped stone-grey plates and gilded knee guards shaped like gothic arches. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Coluna na Tormenta** · nível 7 · pernas (par) · **gerar primeiro**  
  id `peca4_CATEDRAL_ColunaNaTormenta`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of column-shaped armored robot greaves with anchor spikes that fold down from the heels into the ground, stone-grey and gold. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Raio de Ogiva** · nível 1 · cabeça · encaixe *topo*  
  id `peca4_CATEDRAL_RaioDeOgiva`  
  > Isolated sci-fi prop design for a giant piloted robot: a gothic pointed-arch head crest with a tall narrow stained-glass lens slot, stone-grey plates edged in gold. Front view of a head-mounted attachment, symmetrical. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Holofote de Rosácea** · nível 1 · ombro  
  id `peca4_CATEDRAL_HolofoteDeRosacea`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted floodlight shaped like a round rose window, gothic stone tracery over the glass lens, brass hinges. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Relicário de Carga** · nível 2 · antebraço · encaixe *antebraco*  
  id `peca4_CATEDRAL_RelicarioDeCarga`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm-mounted reliquary: a small gilded gothic casket with glass windows showing a glowing white core, braided cables running to the wrist. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Martelo de Campana** · nível 3 · ferramenta na mão  
  id `peca4_CATEDRAL_MarteloDeCampana`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant bell-hammer: a bronze church bell mounted sideways as the head of a long iron-banded handle with gilded rings. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Braçadeira Capacitora** · nível 3 · antebraço · encaixe *antebraco*  
  id `peca4_CATEDRAL_BracadeiraCapacitora`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy gothic bracer with a row of glass capacitor tubes glowing white along the forearm and gold filigree bands. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mão Estendida** · nível 3 · antebraço · encaixe *antebraco*  
  id `peca4_CATEDRAL_MaoEstendida`  
  > Isolated sci-fi prop design for a giant piloted robot: a massive armored gauntlet with a cable winch at the wrist and a gilded grapple ring, stone-grey plates. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Para-Raios de Torre** · nível 3 · costas  
  id `peca4_CATEDRAL_ParaRaiosDeTorre`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall gothic spire backpack with a copper lightning rod at the top and braided grounding cables hanging down to the heels. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rastro de Funda** · nível 4 · cabeça · encaixe *visor*  
  id `peca4_CATEDRAL_RastroDeFunda`  
  > Isolated sci-fi prop design for a giant piloted robot: a helmet crest of tall bronze tuning-fork tines around a small hanging bell, mounted on a gothic arched visor. Front view of a head-mounted attachment, symmetrical. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Unção de Armas** · nível 5 · ferramenta na mão  
  id `peca4_CATEDRAL_UncaoDeArmas`  
  > Isolated sci-fi prop design for a giant piloted robot: a long gilded aspergillum tool: a slender rod ending in a pierced silver ball that sheds glowing white sparks. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Núcleo Relicário** · nível 6 · peito  
  id `peca4_CATEDRAL_NucleoRelicario`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest-mounted reliquary core: a gilded gothic casket with glass panes and a pulsing white-gold crystal inside, framed by stone-grey plate. Front view of an armor chest piece, symmetrical, flat on. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Contraforte** · nível 6 · peito  
  id `peca4_CATEDRAL_Contraforte`  
  > Isolated sci-fi prop design for a giant piloted robot: a torso armor harness braced by flying-buttress arches of stone-grey plate, with gilded rivets and small pinnacles. Front view of an armor chest piece, symmetrical, flat on. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Asas de Vitral** · nível 6 · costas  
  id `peca4_CATEDRAL_AsasDeVitral`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of tall angel wings made of stained-glass panes set in black iron leading, mounted on a gothic backpack frame. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Arpão de Juízo** · nível 7 · ferramenta na mão  
  id `peca4_CATEDRAL_ArpaoDeJuizo`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant kaiju-hunting harpoon lance with a barbed silver head, a heavy chain coiled along the iron shaft and a small bronze bell at the grip. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Comunhão de Carga** · nível 7 · ombro  
  id `peca4_CATEDRAL_ComunhaoDeCarga`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted rack of three small gilded reliquary capsules with glowing glass cores, linked by braided cables. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Auréola de Vigília** · nível 9 · cabeça · encaixe *topo*  
  id `peca4_CATEDRAL_AureolaDeVigilia`  
  > Isolated sci-fi prop design for a giant piloted robot: a crown of slender gothic spires, each tipped with a small glowing lens, mounted on a helmet ring. Front view of a head-mounted attachment, symmetrical. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Coro de Relicários** · nível 9 · costas  
  id `peca4_CATEDRAL_CoroDeRelicarios`  
  > Isolated sci-fi prop design for a giant piloted robot: a backpack organ of tall gilded pipes made of glass capacitor tubes glowing white, with brass valves. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Anátema** · nível 9 · ombro  
  id `peca4_CATEDRAL_Anatema`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted ward emitter: a stone-grey obelisk with a gilded disc antenna and rings of engraved glyph plates. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Dobre de Finados** · nível 10 · ferramenta na mão  
  id `peca4_CATEDRAL_DobreDeFinados`  
  > Isolated sci-fi prop design for a giant piloted robot: a colossal kaiju-slaying lance whose shaft is stacked bronze bell rings, ending in a long blazing white crystal spike, gothic tracery on the guard. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Serafim de Chapa** · nível 10 · costas  
  id `peca4_CATEDRAL_SerafimDeChapa`  
  > Isolated sci-fi prop design for a giant piloted robot: six narrow wings of layered steel plates edged in gold, fanned out from a gothic backpack like a seraph. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: stone grey and bone white, gold trim, one warm crimson accent; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### ARCO (25)

- **Chapa de Costado** · nível 1 · peito  
  id `peca4_ARCO_ChapaDeCostado`  
  > Isolated sci-fi prop design for a giant piloted robot: a curved ship-side chest plate of overlapping riveted steel strakes in deep sea green, with a small sealed porthole. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lâmpada de Sinais** · nível 1 · ombro  
  id `peca4_ARCO_LampadaDeSinais`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted signal lamp with louvered shutters, a brass rim and a thick sealed glass lens. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Cavername** · nível 2 · peito  
  id `peca4_ARCO_Cavername`  
  > Isolated sci-fi prop design for a giant piloted robot: a rounded submarine chest frame with exposed curved steel ribs, heavy rivets and a pressure porthole. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Torpedo de Ombro** · nível 2 · ombro  
  id `peca4_ARCO_TorpedoDeOmbro`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder rack of four long torpedo-shaped deep-sea probes with small propellers and guide fins. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Esporão de Proa** · nível 2 · antebraço · encaixe *mao*  
  id `peca4_ARCO_EsporaoDeProa`  
  > Isolated sci-fi prop design for a giant piloted robot: a pointed naval ram prow blade on a forearm, with riveted bronze plating and a reinforced keel ridge. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Solda Molhada** · nível 2 · ferramenta na mão  
  id `peca4_ARCO_SoldaMolhada`  
  > Isolated sci-fi prop design for a giant piloted robot: an underwater welding torch tool with a thick insulated cable, a rubber-sealed grip and a glowing electrode tip. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Porão Estanque** · nível 3 · peito  
  id `peca4_ARCO_PoraoEstanque`  
  > Isolated sci-fi prop design for a giant piloted robot: a bulging sealed cargo hold hatch on a chest with a round wheel lock, rubber gaskets and stenciled load marks. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Peixe-Agulha** · nível 3 · ombro  
  id `peca4_ARCO_PeixeAgulha`  
  > Isolated sci-fi prop design for a giant piloted robot: a slim shoulder launcher tube holding one needle-nosed torpedo-shaped probe with a counter-rotating propeller. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Leme de Tempestade** · nível 3 · costas  
  id `peca4_ARCO_LemeDeTempestade`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall dorsal rudder fin on a backpack mount with hydraulic hinges, a storm lamp and a wind vane. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rádio de Capitania** · nível 3 · cabeça · encaixe *topo*  
  id `peca4_ARCO_RadioDeCapitania`  
  > Isolated sci-fi prop design for a giant piloted robot: a sealed glass dome helmet radio set with a tall mast antenna and a rubber-gasketed speaker grille. Front view of a head-mounted attachment, symmetrical. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Periscópio** · nível 4 · cabeça · encaixe *topo*  
  id `peca4_ARCO_Periscopio`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall telescoping periscope mast on a helmet, with a sealed prism head and folding brass handles. Front view of a head-mounted attachment, symmetrical. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Forro de Chumbo** · nível 5 · peito  
  id `peca4_ARCO_ForroDeChumbo`  
  > Isolated sci-fi prop design for a giant piloted robot: a thick lead-lined chest plate with bolted seams, a sealed dosimeter gauge and a faded hazard stripe. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Torpedo Pesado** · nível 5 · costas  
  id `peca4_ARCO_TorpedoPesado`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy submarine tube pod on the back holding two wide torpedo-shaped deep-sea probes, with hatch doors and ballast fins. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Torpedo de Casco** · nível 6 · antebraço · encaixe *antebraco*  
  id `peca4_ARCO_TorpedoDeCasco`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy forearm submarine tube pod with one long torpedo-shaped deep-sea probe, a bronze propeller and guide fins. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Quebra-Mar** · nível 7 · peito  
  id `peca4_ARCO_QuebraMar`  
  > Isolated sci-fi prop design for a giant piloted robot: a massive wedge-shaped breakwater chest armor of layered rounded plates with deep rows of rivets. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Paiol Fundo** · nível 7 · costas  
  id `peca4_ARCO_PaiolFundo`  
  > Isolated sci-fi prop design for a giant piloted robot: a long rounded magazine backpack with sliding hatches showing racked torpedo-shaped deep-sea probes. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Casco de Escolta** · nível 7 · antebraço · encaixe *escudo*  
  id `peca4_ARCO_CascoDeEscolta`  
  > Isolated sci-fi prop design for a giant piloted robot: a large rounded forearm shield shaped like a submarine hull section, with bolted rims and a small porthole. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Escotilhas Fechadas** · nível 8 · peito  
  id `peca4_ARCO_EscotilhasFechadas`  
  > Isolated sci-fi prop design for a giant piloted robot: a rounded chest hull with every hatch dogged shut, round wheel locks, rubber seals and bolted porthole covers. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Estaleiro de Bolso** · nível 8 · antebraço · encaixe *antebraco*  
  id `peca4_ARCO_EstaleiroDeBolso`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm shipyard repair rig with a folding welding arm, a rivet driver and coils of sealing tape. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ouvido de Baleia** · nível 9 · cabeça · encaixe *troca*  
  id `peca4_ARCO_OuvidoDeBaleia`  
  > Isolated sci-fi prop design for a giant piloted robot: a large glass sensor dome helmet ringed with hydrophone pods and a long trailing listening array. Front view of a head-mounted attachment, symmetrical. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Cardume** · nível 9 · ombro  
  id `peca4_ARCO_Cardume`  
  > Isolated sci-fi prop design for a giant piloted robot: a wide shoulder launcher pod honeycombed with small torpedo-shaped deep-sea probes, each with a tiny propeller. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Voz de Farol** · nível 9 · ombro  
  id `peca4_ARCO_VozDeFarol`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted lighthouse lamp with a rotating fresnel lens, a brass cage and a radio mast. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Casco de Mil Braças** · nível 10 · peito  
  id `peca4_ARCO_CascoDeMilBracas`  
  > Isolated sci-fi prop design for a giant piloted robot: a massive spherical bathyscaphe chest hull of thick scarred steel with tiny quartz portholes and deep pressure marks. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Torpedo de Fossa** · nível 10 · costas  
  id `peca4_ARCO_TorpedoDeFossa`  
  > Isolated sci-fi prop design for a giant piloted robot: a single enormous torpedo-shaped deep-sea probe strapped along the back, with a ducted propeller, pressure fins and warning stripes. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **A Arca** · nível 10 · peito  
  id `peca4_ARCO_AArca`  
  > Isolated sci-fi prop design for a giant piloted robot: a wide chest hatch opened like a ship's hold, with fold-down rescue ramps, interior hand rails and rows of seats. Front view of an armor chest piece, symmetrical, flat on. Colors: deep sea green and tide grey, wet glossy metal; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### GRALHA (26)

- **Roda de Carreta** · nível 1 · pernas (par) · **gerar primeiro**  
  id `peca4_GRALHA_RodaDeCarreta`  
  > Isolated sci-fi prop design for a giant piloted robot: robot legs ending in big mismatched truck wheels with worn tires and exposed hubs. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ventosa de Vidraceiro** · nível 6 · pernas (par) · **gerar primeiro**  
  id `peca4_GRALHA_VentosaDeVidraceiro`  
  > Isolated sci-fi prop design for a giant piloted robot: robot feet fitted with big rubber suction cups taken from glazier lifting tools. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Pernas de Guindauto** · nível 7 · pernas (par) · **gerar primeiro**  
  id `peca4_GRALHA_PernasDeGuindauto`  
  > Isolated sci-fi prop design for a giant piloted robot: telescopic robot legs made from hydraulic truck crane booms with outrigger feet. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lança-Sucata** · nível 1 · ferramenta na mão  
  id `peca4_GRALHA_LancaSucata`  
  > Isolated sci-fi prop design for a giant piloted robot: a junk launcher made from a scrap pipe, a car coil spring and a hopper full of bolts and hubcaps. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Chapa de Porta** · nível 1 · peito  
  id `peca4_GRALHA_ChapaDePorta`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest plate made of three mismatched car doors welded together, one still with its handle and window crank. Front view of an armor chest piece, symmetrical, flat on. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Antena de Varal** · nível 1 · costas  
  id `peca4_GRALHA_AntenaDeVaral`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall improvised antenna mast made from a clothesline pole, bent wire hangers and an old car radio box. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Arpão de Sucata** · nível 2 · antebraço · encaixe *mao*  
  id `peca4_GRALHA_ArpaoDeSucata`  
  > Isolated sci-fi prop design for a giant piloted robot: a scrap harpoon launcher that fires a sharpened rebar spike on a winch cable. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Olho de Lata** · nível 2 · drone  
  id `peca4_GRALHA_OlhoDeLata`  
  > Isolated sci-fi prop design for a giant piloted robot: a scout drone made from a tin can body, a single camera lens and a rotor from a desk fan. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Chiadeira** · nível 2 · ombro  
  id `peca4_GRALHA_Chiadeira`  
  > Isolated sci-fi prop design for a giant piloted robot: a crude jammer box made from an old car stereo, a tangle of wires and bent antennas. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Para-Choque** · nível 2 · antebraço · encaixe *mao*  
  id `peca4_GRALHA_ParaChoque`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm guard made from a dented truck bumper with a rusty tow hook. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Drone de Solda** · nível 3 · drone  
  id `peca4_GRALHA_DroneDeSolda`  
  > Isolated sci-fi prop design for a giant piloted robot: a repair drone with a small welding arm, a sparking torch tip and a gas bottle slung underneath. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Espantalho** · nível 4 · drone  
  id `peca4_GRALHA_Espantalho`  
  > Isolated sci-fi prop design for a giant piloted robot: a decoy drone built like a scarecrow from pipes, with a flapping tarp and a blinking beacon light. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Garra de Ímã** · nível 5 · antebraço · encaixe *antebraco*  
  id `peca4_GRALHA_GarraDeIma`  
  > Isolated sci-fi prop design for a giant piloted robot: a magnet crane arm with a big round electromagnet plate hanging from a short chain. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Asa de Outdoor** · nível 5 · costas  
  id `peca4_GRALHA_AsaDeOutdoor`  
  > Isolated sci-fi prop design for a giant piloted robot: wings made of torn billboard panels on a folding scaffold frame, with peeling faded paint. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Carro de Som** · nível 5 · ombro  
  id `peca4_GRALHA_CarroDeSom`  
  > Isolated sci-fi prop design for a giant piloted robot: a cluster of mismatched horn loudspeakers strapped to a pole with tape and wire. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Porta Voadora** · nível 6 · drone  
  id `peca4_GRALHA_PortaVoadora`  
  > Isolated sci-fi prop design for a giant piloted robot: a shield drone carrying a dented car door as a plate, with four small rotors at the corners. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Motor na Corrente** · nível 6 · ferramenta na mão  
  id `peca4_GRALHA_MotorNaCorrente`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy engine block swinging on a thick chain from a steel handle. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Morteiro de Sucata** · nível 7 · ombro  
  id `peca4_GRALHA_MorteiroDeSucata`  
  > Isolated sci-fi prop design for a giant piloted robot: a squat launcher tube made from an old water boiler, with a hopper of scrap and a fridge door as a blast plate. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Drone Pirata** · nível 7 · drone  
  id `peca4_GRALHA_DronePirata`  
  > Isolated sci-fi prop design for a giant piloted robot: a pirate radio drone made from a battered box with a jumble of antennas, a small speaker and ceiling fan rotors. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Para-Raios Roubado** · nível 7 · ombro  
  id `peca4_GRALHA_ParaRaiosRoubado`  
  > Isolated sci-fi prop design for a giant piloted robot: an old church lightning rod on a tall mast with a braided copper cable running down. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Motosserra de Desmanche** · nível 8 · ferramenta na mão  
  id `peca4_GRALHA_MotosserraDeDesmanche`  
  > Isolated sci-fi prop design for a giant piloted robot: a huge chop-shop chainsaw built from a truck engine and a long bent saw bar. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Urubus** · nível 8 · drone  
  id `peca4_GRALHA_Urubus`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of scavenger drones shaped like vultures, with hooked grabber claws and scrap-metal wings. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rotor de Helicóptero** · nível 8 · costas  
  id `peca4_GRALHA_RotorDeHelicoptero`  
  > Isolated sci-fi prop design for a giant piloted robot: a salvaged helicopter rotor mounted on a backpack mast with a patched gearbox. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Bate-Estaca Remendado** · nível 9 · antebraço · encaixe *antebraco*  
  id `peca4_GRALHA_BateEstacaRemendado`  
  > Isolated sci-fi prop design for a giant piloted robot: a pneumatic pile driver arm with a massive steel ram and a rusty piston housing. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Amortecedor de Vagão** · nível 9 · peito  
  id `peca4_GRALHA_AmortecedorDeVagao`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest plate with two railway buffer pads mounted on big coil springs. Front view of an armor chest piece, symmetrical, flat on. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Revoada** · nível 10 · drone  
  id `peca4_GRALHA_Revoada`  
  > Isolated sci-fi prop design for a giant piloted robot: a swarm of small mismatched scrap drones built from fan blades, headlights and tin cans. Three-quarter view of a small hovering drone. Colors: rusty brown, faded orange and dull olive mismatched panels, the paint never matches; no pink, no magenta, no neon colors; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### SELO (21)

- **Perseguição** · nível 3 · pernas (par) · **gerar primeiro**  
  id `peca4_SELO_Perseguicao`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of sturdy off-white robot legs with retractable pursuit wheels at the heels and reflective yellow ankle bands. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Suspensão de Patrulha** · nível 5 · pernas (par) · **gerar primeiro**  
  id `peca4_SELO_SuspensaoDePatrulha`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of robot legs with long-travel coil-over shock absorbers, reinforced knee joints and reflective yellow bands. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Escudo Antimotim** · nível 1 · antebraço · encaixe *escudo*  
  id `peca4_SELO_EscudoAntimotim`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall rectangular riot shield plate with a clear armored viewing window, a red-and-blue light bar along the top edge and yellow reflective stripes. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Gaiola de Cabine** · nível 2 · peito  
  id `peca4_SELO_GaiolaDeCabine`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest cockpit hatch wrapped in a thick tubular steel roll cage, with yellow grab handles and a stenciled serial number. Front view of an armor chest piece, symmetrical, flat on. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Viseira de Identidade** · nível 2 · cabeça · encaixe *visor*  
  id `peca4_SELO_ViseiraDeIdentidade`  
  > Isolated sci-fi prop design for a giant piloted robot: a head-mounted identity scanner visor with a wide amber lens, a small fingerprint reader housing and yellow reflective stripes. Front view of a head-mounted attachment, symmetrical. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Dardo Rastreador** · nível 2 · ombro  
  id `peca4_SELO_DardoRastreador`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted tracer dart launcher pod with a row of small blinking dart tips in a yellow-and-black striped housing. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Faixa Refletiva** · nível 3 · peito  
  id `peca4_SELO_FaixaRefletiva`  
  > Isolated sci-fi prop design for a giant piloted robot: a smooth off-white chest plate crossed by wide mirror-bright reflective yellow and silver stripes. Front view of an armor chest piece, symmetrical, flat on. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Munição de Borracha** · nível 3 · ombro  
  id `peca4_SELO_MunicaoDeBorracha`  
  > Isolated sci-fi prop design for a giant piloted robot: a boxy shoulder launcher pod loaded with big black rubber slugs behind a yellow-and-black striped muzzle guard. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Quebra-Mato** · nível 4 · peito  
  id `peca4_SELO_QuebraMato`  
  > Isolated sci-fi prop design for a giant piloted robot: a massive chest-mounted bull bar push guard of welded steel tubes with yellow reflective tips. Front view of an armor chest piece, symmetrical, flat on. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Chave de Algema** · nível 4 · ferramenta na mão  
  id `peca4_SELO_ChaveDeAlgema`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant hydraulic cuff key and bolt-cutter tool with long yellow handles and a toothed steel jaw. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Espuma de Contenção** · nível 4 · antebraço · encaixe *antebraco*  
  id `peca4_SELO_EspumaDeContencao`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm-mounted restraint foam sprayer with a wide flat nozzle, twin yellow foam tanks and hazard stripes. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Esfera de Leitura** · nível 5 · cabeça · encaixe *topo*  
  id `peca4_SELO_EsferaDeLeitura`  
  > Isolated sci-fi prop design for a giant piloted robot: a rotating spherical sensor dome studded with round lenses and short antennas, off-white with yellow stripes. Front view of a head-mounted attachment, symmetrical. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lança-Rede** · nível 5 · ombro  
  id `peca4_SELO_LancaRede`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted net launcher pod with a folded steel-cable net in a wide square mouth and yellow-and-black hazard stripes. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Jato de Dispersão** · nível 6 · ferramenta na mão  
  id `peca4_SELO_JatoDeDispersao`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy high-pressure riot water nozzle with a long steel lance, an armored hose and a big yellow control valve. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Drone Megafone** · nível 6 · drone  
  id `peca4_SELO_DroneMegafone`  
  > Isolated sci-fi prop design for a giant piloted robot: a round drone with a big horn loudspeaker slung underneath, a single camera eye and a red-and-blue light ring. Three-quarter view of a small hovering drone. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Drone de Captura** · nível 7 · drone  
  id `peca4_SELO_DroneDeCaptura`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy six-rotor capture drone with a folded steel net under its belly and a yellow-and-black striped frame. Three-quarter view of a small hovering drone. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Chegada de Resgate** · nível 8 · costas  
  id `peca4_SELO_ChegadaDeResgate`  
  > Isolated sci-fi prop design for a giant piloted robot: a compact backpack thruster pack with twin ducted fans, a red-and-blue light bar and a coiled rescue cable. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Casco de Carro-Forte** · nível 9 · peito  
  id `peca4_SELO_CascoDeCarroForte`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy armored-truck chest plate with vault-door rivets, a thick hatch wheel and a big stenciled serial number. Front view of an armor chest piece, symmetrical, flat on. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Escolta de Drones** · nível 9 · drone  
  id `peca4_SELO_EscoltaDeDrones`  
  > Isolated sci-fi prop design for a giant piloted robot: a flat shield drone carrying a transparent riot plate, with four ducted rotors and a red-and-blue light bar. Three-quarter view of a small hovering drone. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Número Um** · nível 10 · peito  
  id `peca4_SELO_NumeroUm`  
  > Isolated sci-fi prop design for a giant piloted robot: a battered off-white chest plate with a huge stenciled number one, faded reflective stripes and decades of dents and weld repairs. Front view of an armor chest piece, symmetrical, flat on. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Satélite Próprio** · nível 10 · costas  
  id `peca4_SELO_SateliteProprio`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall folding backpack satellite uplink dish with a gold-foil receiver horn, stabilizer gyros and yellow hazard stripes. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: dirty white and signal yellow with black hazard stripes; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### CARVAO (23)

- **Pé de Pano** · nível 2 · pernas (par) · **gerar primeiro**  
  id `peca4_CARVAO_PeDePano`  
  > Isolated sci-fi prop design for a giant piloted robot: wide padded feet wrapped in layers of rags, rope and rubber dampening pads. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Sebo nas Canelas** · nível 2 · pernas (par) · **gerar primeiro**  
  id `peca4_CARVAO_SeboNasCanelas`  
  > Isolated sci-fi prop design for a giant piloted robot: knee and ankle joints wrapped in greasy rubber sleeves with small oil reservoirs. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mola de Contrabando** · nível 6 · pernas (par) · **gerar primeiro**  
  id `peca4_CARVAO_MolaDeContrabando`  
  > Isolated sci-fi prop design for a giant piloted robot: long-stroke leg springs and shock absorbers with ground-off serial plates. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ponta de Prova** · nível 1 · antebraço · encaixe *mao*  
  id `peca4_CARVAO_PontaDeProva`  
  > Isolated sci-fi prop design for a giant piloted robot: a matte black forearm cyberdeck with a worn keypad, coiled cables and needle-tipped data spikes. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Olho de Receptador** · nível 1 · cabeça · encaixe *visor*  
  id `peca4_CARVAO_OlhoDeReceptador`  
  > Isolated sci-fi prop design for a giant piloted robot: a jeweler's loupe visor with stacked magnifying lenses on a hinged arm. Front view of a head-mounted attachment, symmetrical. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Casco Oco** · nível 1 · peito  
  id `peca4_CARVAO_CascoOco`  
  > Isolated sci-fi prop design for a giant piloted robot: a hinged false chest panel with fake rivets opening onto a hidden cargo hold with cargo straps. Front view of an armor chest piece, symmetrical, flat on. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Pico de Tensão** · nível 2 · antebraço · encaixe *mao*  
  id `peca4_CARVAO_PicoDeTensao`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm capacitor pack with copper coils and a pair of clamp-on data leads. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Unha de Gato** · nível 2 · ferramenta na mão  
  id `peca4_CARVAO_UnhaDeGato`  
  > Isolated sci-fi prop design for a giant piloted robot: matte black climbing claws with hooked steel talons fixed over the knuckles. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Orelha no Rádio** · nível 2 · cabeça · encaixe *troca*  
  id `peca4_CARVAO_OrelhaNoRadio`  
  > Isolated sci-fi prop design for a giant piloted robot: a scratched headset with a coil of cable taps and alligator clips hanging from it. Front view of a head-mounted attachment, symmetrical. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Coroa de Escuta** · nível 3 · cabeça · encaixe *topo*  
  id `peca4_CARVAO_CoroaDeEscuta`  
  > Isolated sci-fi prop design for a giant piloted robot: a head-mounted signal-intrusion antenna crown with bent whip aerials and taped cables. Front view of a head-mounted attachment, symmetrical. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lâmina Calada** · nível 3 · antebraço · encaixe *mao*  
  id `peca4_CARVAO_LaminaCalada`  
  > Isolated sci-fi prop design for a giant piloted robot: a thin matte black folding forearm blade wrapped in strips of canvas. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Tinta de Outra Casa** · nível 3 · ferramenta na mão  
  id `peca4_CARVAO_TintaDeOutraCasa`  
  > Isolated sci-fi prop design for a giant piloted robot: a paint sprayer rig with a stack of stencil plates showing different emblems. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Viga Serrada** · nível 4 · peito  
  id `peca4_CARVAO_VigaSerrada`  
  > Isolated sci-fi prop design for a giant piloted robot: a patched chest frame reinforced with a mismatched steel girder bolted across it. Front view of an armor chest piece, symmetrical, flat on. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Voz Emprestada** · nível 4 · costas  
  id `peca4_CARVAO_VozEmprestada`  
  > Isolated sci-fi prop design for a giant piloted robot: a scratched backpack radio set with swapped faceplates and a tangle of whip antennas. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Vulto de Luz** · nível 5 · ombro  
  id `peca4_CARVAO_VultoDeLuz`  
  > Isolated sci-fi prop design for a giant piloted robot: a shoulder-mounted hologram projector with a ring of lenses casting a flickering silhouette. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mosca Morta** · nível 5 · drone  
  id `peca4_CARVAO_MoscaMorta`  
  > Isolated sci-fi prop design for a giant piloted robot: a palm-sized matte black insect-like drone with folded rotor wings and a single dim lens. Three-quarter view of a small hovering drone. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Capuz de Lona** · nível 7 · cabeça · encaixe *troca*  
  id `peca4_CARVAO_CapuzDeLona`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy waxed canvas hood draped over a cockpit head with a narrow viewing slit and frayed edges. Front view of a head-mounted attachment, symmetrical. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mão no Volante** · nível 7 · antebraço · encaixe *antebraco*  
  id `peca4_CARVAO_MaoNoVolante`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm-mounted remote control rig with a folding joystick, a small screen and a long antenna. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Fura-Bloqueio** · nível 8 · peito  
  id `peca4_CARVAO_FuraBloqueio`  
  > Isolated sci-fi prop design for a giant piloted robot: a thick patchwork chest armor of mismatched plates from different factories, welded and riveted together. Front view of an armor chest piece, symmetrical, flat on. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Revoada Roubada** · nível 8 · costas  
  id `peca4_CARVAO_RevoadaRoubada`  
  > Isolated sci-fi prop design for a giant piloted robot: a tall folding signal mast backpack with a cluster of small dish antennas and blinking relays. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mesa de Controle** · nível 9 · peito  
  id `peca4_CARVAO_MesaDeControle`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest-mounted fold-out console with rows of toggle switches, patch cables and a cracked screen. Front view of an armor chest piece, symmetrical, flat on. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Blecaute** · nível 10 · costas  
  id `peca4_CARVAO_Blecaute`  
  > Isolated sci-fi prop design for a giant piloted robot: a heavy black backpack transmitter with a folded umbrella dish and rows of glowing vacuum tubes. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Parafuso Que Ninguém Checa** · nível 10 · ferramenta na mão  
  id `peca4_CARVAO_ParafusoQueNinguemCheca`  
  > Isolated sci-fi prop design for a giant piloted robot: a long thin precision drill-lance with a tiny glowing tip and a cable spool on the grip. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: matte black and charcoal with faint rust showing through; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

### OSSATURA (21)

- **Ouvir a Carne** · nível 1 · ferramenta na mão · **gerar primeiro**  
  id `peca4_OSSATURA_OuvirACarne`  
  > Isolated sci-fi prop design for a giant piloted robot: a giant organic hand with soft fleshy pads, fine hair-like cilia and knuckles of bare bone. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Mostrar o Caminho** · nível 8 · ferramenta na mão · **gerar primeiro**  
  id `peca4_OSSATURA_MostrarOCaminho`  
  > Isolated sci-fi prop design for a giant piloted robot: a huge organic hand with long jointed bone fingers and amber pads, one finger pointing. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Costura Viva** · nível 9 · ferramenta na mão · **gerar primeiro**  
  id `peca4_OSSATURA_CosturaViva`  
  > Isolated sci-fi prop design for a giant piloted robot: a delicate organic hand with long bone needle fingers trailing strands of sinew thread. Horizontal side view, the grip end on the RIGHT side of the image, the working end on the left, whole object visible. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Troca de Tendão** · nível 2 · antebraço · encaixe *antebraco*  
  id `peca4_OSSATURA_TrocaDeTendao`  
  > Isolated sci-fi prop design for a giant piloted robot: an organic forearm with exposed tendons rerouted over bone pulleys like steel cables. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Tendão Novo** · nível 2 · pernas (par)  
  id `peca4_OSSATURA_TendaoNovo`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of digitigrade organic robot legs with bone shins, thick exposed tendons and clawed feet. Front view of a matching pair of armored robot feet and lower legs, side by side. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Glândula de Memória** · nível 3 · peito  
  id `peca4_OSSATURA_GlandulaDeMemoria`  
  > Isolated sci-fi prop design for a giant piloted robot: a pulsing translucent absorption gland sac fused to a bone plate. Front view of an armor chest piece, symmetrical, flat on. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Lança de Osso** · nível 3 · antebraço · encaixe *antebraco*  
  id `peca4_OSSATURA_LancaDeOsso`  
  > Isolated sci-fi prop design for a giant piloted robot: a long sharpened bone lance growing out of an organic forearm, wrapped in sinew and amber cartilage. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Couro de Abismo** · nível 4 · peito  
  id `peca4_OSSATURA_CouroDeAbismo`  
  > Isolated sci-fi prop design for a giant piloted robot: a chest covered in thick grey abyssal hide with a salt crust, barnacles and small bone plates. Front view of an armor chest piece, symmetrical, flat on. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Rebrota** · nível 4 · antebraço · encaixe *antebraco*  
  id `peca4_OSSATURA_Rebrota`  
  > Isolated sci-fi prop design for a giant piloted robot: a bud of fresh bone and pink flesh regrowing out of a torn metal socket on a forearm. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Cria Batedora** · nível 4 · drone  
  id `peca4_OSSATURA_CriaBatedora`  
  > Isolated sci-fi prop design for a giant piloted robot: a small crawling organic scout creature with long feelers and one huge amber eye on a stalk. Three-quarter view of a small hovering drone. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Olhos Demais** · nível 5 · cabeça · encaixe *troca*  
  id `peca4_OSSATURA_OlhosDemais`  
  > Isolated sci-fi prop design for a giant piloted robot: a bony head covered in clusters of small amber eyes set in ridged bone sockets. Front view of a head-mounted attachment, symmetrical. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Glândula Ácida** · nível 5 · ombro  
  id `peca4_OSSATURA_GlandulaAcida`  
  > Isolated sci-fi prop design for a giant piloted robot: a cluster of swollen amber acid glands on a bony shoulder mount, with a ribbed spitting duct. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Membrana** · nível 5 · costas  
  id `peca4_OSSATURA_Membrana`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of leathery membrane wings stretched over long bone struts growing from a back plate. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Canto Baixo** · nível 5 · ombro  
  id `peca4_OSSATURA_CantoBaixo`  
  > Isolated sci-fi prop design for a giant piloted robot: a pair of hollow curved bone horns rising from a shoulder mount, with sinew membranes stretched across their mouths. Front three-quarter view of a shoulder-mounted module, mounting bracket at the bottom. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Ferrão de Sono** · nível 7 · antebraço · encaixe *mao*  
  id `peca4_OSSATURA_FerraoDeSono`  
  > Isolated sci-fi prop design for a giant piloted robot: a curved bone stinger growing from an organic forearm, with a swollen venom sac at its base. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Envergadura** · nível 7 · costas  
  id `peca4_OSSATURA_Envergadura`  
  > Isolated sci-fi prop design for a giant piloted robot: a huge pair of translucent bat-like membrane wings with long bone fingers and visible veins. Front view of a backpack module as seen from in front of the robot, symmetrical, parts extending up and out to both sides. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Pulso de Gente** · nível 8 · cabeça · encaixe *troca*  
  id `peca4_OSSATURA_PulsoDeGente`  
  > Isolated sci-fi prop design for a giant piloted robot: a head with thin whisker-like organic feelers and a ring of small heat-sensing pits, like a snake's. Front view of a head-mounted attachment, symmetrical. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Enxame de Dentes** · nível 8 · drone  
  id `peca4_OSSATURA_EnxameDeDentes`  
  > Isolated sci-fi prop design for a giant piloted robot: a swarm of small eyeless organic creatures with bone plates and needle teeth, clinging together in a writhing cluster. Three-quarter view of a small hovering drone. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Outro Batimento** · nível 8 · antebraço · encaixe *antebraco*  
  id `peca4_OSSATURA_OutroBatimento`  
  > Isolated sci-fi prop design for a giant piloted robot: a forearm with exposed pulsing amber veins and a bone-tipped transfusion tendril. Front view as mounted on the outside of a robot forearm, vertical orientation, mounting bracket at the top. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Golfada** · nível 9 · cabeça · encaixe *troca*  
  id `peca4_OSSATURA_Golfada`  
  > Isolated sci-fi prop design for a giant piloted robot: a distended organic jaw with a swollen throat sac and ribbed acid ducts dripping amber fluid. Front view of a head-mounted attachment, symmetrical. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

- **Língua de Funda** · nível 10 · peito  
  id `peca4_OSSATURA_LinguaDeFunda`  
  > Isolated sci-fi prop design for a giant piloted robot: a deep chest cavity lined with ribbed resonating membranes, like the inside of a giant drum made of bone. Front view of an armor chest piece, symmetrical, flat on. Colors: bone white, amber glow and dark red sinew; strictly this palette, no pink, no magenta, no neon green. Mecha concept art, painterly digital illustration with realistic materials and soft rim light. Plain solid white background, no robot, no hand, no text, no logo.

