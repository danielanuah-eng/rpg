import fs from 'node:fs/promises';
import { constants } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import crypto from 'node:crypto';
const require = createRequire(import.meta.url);
const sharp = require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '../../../..');
// Preservar a decisão do autor caso o registro inicial seja executado novamente.
try {
 const aprovado=JSON.parse(await fs.readFile(path.join(here,'manifesto.json'),'utf8'));
 if(aprovado.aprovacao?.varianteSelecionada==='v2-liso') {
  console.log('V2 já aprovada; registro inicial preservado.');
  process.exit(0);
 }
} catch(erro) {if(erro.code!=='ENOENT')throw erro;}
const fontes = [
  {
    "cartaId": "ciantia-philgard-campo",
    "variante": "v1",
    "arquivo": "ciantia-philgard-campo-adolescente-v1.png",
    "origem": "C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-9d6908da-9420-4d56-8f54-f10d48bc4999.png",
    "referencias": [
      "Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/licao-do-jardim-arte-v2.png"
    ]
  },
  {
    "cartaId": "philgard-luz-da-esperanca",
    "variante": "v1",
    "arquivo": "philgard-luz-da-esperanca-adulto-v1.png",
    "origem": "C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-627ed0a3-b5cc-4723-8612-fbb04a524d1e.png",
    "referencias": [
      "Arcanoclisma/cartas/modelos-visuais/2026-10-03-philgard/ciantia-philgard-campo-adolescente-v1.png"
    ]
  },
  {
    "cartaId": "ciantia-philgard-campo",
    "variante": "v2-liso",
    "arquivo": "ciantia-philgard-campo-adolescente-v2-liso.png",
    "origem": "C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-848fca55-7bb1-417f-b10c-6f3a4c90d94e.png",
    "referencias": [
      "Arcanoclisma/cartas/modelos-visuais/2026-10-03-philgard/ciantia-philgard-campo-adolescente-v1.png"
    ]
  },
  {
    "cartaId": "philgard-luz-da-esperanca",
    "variante": "v2-liso",
    "arquivo": "philgard-luz-da-esperanca-adulto-v2-liso.png",
    "origem": "C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-95fec73f-a1a4-4637-8c4f-76ff4c847b29.png",
    "referencias": [
      "Arcanoclisma/cartas/modelos-visuais/2026-10-03-philgard/philgard-luz-da-esperanca-adulto-v1.png",
      "C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-848fca55-7bb1-417f-b10c-6f3a4c90d94e.png"
    ]
  }
];
const prompts = {
  "ferramenta": "image_gen.imagegen integrada",
  "modo": "built-in",
  "transparent_background": false,
  "ciantia-philgard-campo-adolescente-v1": "Create a NEW full vertical 63:88 Arcanoclisma TCG card scene using the reference only for Ciantia's identity, clothing, small golden energy dragon's design, and illustrated art direction. Scene 1 of a linked pair: Ciantia in an open meadow, seen fully FROM BEHIND, playfully interacting with her dragon now ADOLESCENT. Her face is not turned toward the viewer. Recognizable long blonde half-braided hair flowing in the breeze, black and dark plum robes/cape with restrained aged bronze/gold details, dark boots, relaxed playful standing posture, one hand offering a waving stalk of grass like a toy. Philgard, the golden dragon from the reference, is no longer a tiny baby: about pony/dog-sized adolescent, elegant lively body, recognizable warm amber-golden magical substance with a substantial readable dragon silhouette, slightly translucent details and gently luminous veins rather than a blinding neon outline. FOUR anatomically coherent legs and TWO wings, short swept-back horns, expressive intelligent eyes, long graceful tail, curious playful head tilt, one front paw raised and the others supporting a believable light bounding pose. No excessive spines, no fire breath or explosion. Keep the gold light soft and local. Clear youthful proportions and playful interaction. This is a quiet affectionate everyday moment, not an epic event. Environment: a grounded spacious green meadow with clover, restrained wildflowers, an S-shaped narrow stream, a distinctive forked willow on the RIGHT leaning left, a low oval flat grey stone in the foreground and distant rounded hills with one recognizable double-peaked silhouette. A small closed dark plum/brown book with pale page edges, modest aged-gold corner fittings and a simple round ornament lies on the flat stone near Ciantia; no readable lettering. These are continuity landmarks for the future adult scene. Gentle diffuse mid/late AFTERNOON daylight, blue-green greens and a muted sky with soft clouds, natural contact shadows and coherent size relationships. No city, no floating arches or portal; background never plain/white/blank. Characters large and legible with foreground/midground/background depth and room for card overlays, but no text/numbers/frame/logos inside the art. Clean expressive lines, rich restrained flat/block colours, fine graphic textures, polished Arcanoclisma fantasy illustration matching the reference, not generic anime/HDR or photorealism. Rarity and rules are pending Claude's definition: this is a conceptual card-art delivery, so no rarity-specific foil baked into the artwork. Fully painted opaque background. Preserve the approved reference; this is a separate image.",
  "philgard-luz-da-esperanca-adulto-v1": "Use case: illustration-story. Create the second NEW vertical 63:88 Arcanoclisma TCG card illustration in a linked pair. The reference is the first scene and establishes Philgard's identity, Ciantia's book and the actual meadow; do not copy its pose. Philgard, the golden magical dragon, has now grown into a large, strong ADULT and has returned alone to this same place. NO human figures. He rests peacefully in the meadow, sleepy, head lowered with his chin over the closed book below his face. The book is clearly visible and intact: the same dark plum/brown cover, pale page edges, aged-gold corners, circular cover ornament; not letters or a new design. Keep the same landmarks at coherent scales: the leaning willow on the right, curved narrow stream, flat grey stone under/near the book and the recognizable double-peaked distant mountains. Use a new low three-quarter frontal perspective near his face and forepaws, with his large body lying diagonally into the scene and two folded wings. FOUR anatomically coherent legs, TWO wings: both forelegs clearly distinct and the rear legs visibly folded along the body, no missing arms. The dragon remains recognizably Philgard: swept-back horns, elegant long neck, readable gold/amber energy scales and veins, graceful tail, restrained spines, expressive eyes now half-closed and drowsy, relaxed mouth. Adult proportions, heavier chest, broad paws and mature wings; no fur and no explosive fire. His energy glows SOFTLY and locally through textured warm gold and amber, never a solid overexposed neon silhouette. Quiet affectionate nostalgia and comfort, not grief: no graves, ghosts, funerary imagery or implication that Ciantia died. Change time of day to calm early EVENING/blue hour: muted cool green grass, dark teal willow leaves, soft blue-grey sky and distant mountains, natural contact shadows; a small warm glow near his face and the book. Do not bathe the entire setting in radiant gold. Keep the environment fully painted, detailed but subordinate to the dragon, atmospheric depth with coherent foreground/midground/background proportions. Polished clean expressive illustrated lines, rich restrained colour blocks and fine graphic textures matching the reference and Arcanoclisma's established art direction; no generic anime/HDR, no photorealism, no white/blank background, no portal, no spectacle. Large readable character, sensible space for card overlays. No card frame, text, numbers, logos or watermark inside the bitmap. Rarity, card type and mechanics remain pending Claude's definition; no rarity-specific foil baked in. Opaque complete scene. Save as a separate image, preserve the reference.",
  "ciantia-philgard-campo-adolescente-v2-liso": "Use case: style-transfer limited to the environment. EDIT the provided Ciantia-and-adolescent-Philgard card art to create a V2 comparison. Preserve the image composition, back-facing Ciantia's exact identity, blonde half-braided hair, dark/plum clothing, playful raised hand, the adolescent golden energy dragon's pose, anatomy and identity, book on the foreground flat stone, afternoon palette and lighting, stream, right-side leaning willow and double-peaked mountains. The author's requested change is ONLY a smoother, calmer landscape treatment: remove the excessively rough, intensely expressive surface rendering. Make rocks and tree bark softly illustrated with clean broad shapes and gentle tonal gradients, not gnarly etched cracks or aggressive brush marks. Simplify the grass and distant foliage into coherent smooth colour masses; substantially reduce the dense high-contrast micro-speckles, tiny outlined flowers and competing texture on the ground. Retain selected flowers and readable depth so it is still a beautiful lived-in meadow, not a blank background. More restrained edge contrast and quieter background contours, subtle material detail, soft diffuse shading; no grain, impasto or gritty noise. Retain the polished Arcanoclisma illustrated linework and carefully drawn character details, with a smooth background that supports the subjects. Do not blur the whole image, make it pixelated, erase the setting, change the character poses or redesign the dragon. Keep his gold magic local; do not increase glow or sky brightness. Same portrait aspect ratio and complete opaque scene. No text, card frame, numbers or watermark. Save a separate variant; do not replace V1.",
  "philgard-luz-da-esperanca-adulto-v2-liso": "Use case: style-transfer limited to the environment. Image 1 is the edit target: adult Philgard sleeping at the meadow. Image 2 is ONLY a reference for the smoother, calmer landscape rendering chosen for the comparison pair. Create V2 of image 1. Preserve its exact composition and all central subjects: the large ADULT golden energy dragon alone, lowered sleepy face, two distinct forepaws, adult anatomy, folded wings, graceful tail, book below the snout, the book's dark plum cover and gold round ornament, low stone, stream curve, right-side leaning willow, double-peaked mountains and quiet cool EVENING lighting. Do not copy the adolescent scene's pose, daylight, character or brightness. Change only the ENVIRONMENT's excessively rough and intensely expressive surface rendering: smooth broad illustrated shapes for rocks and bark, gentle tonal gradients, no gnarly gritty cracks, dense etched marks or aggressive brush texture. Quiet and simplify grass, tree foliage and distant terrain into coherent softly shaded colour masses; reduce the very dense high-contrast speckles, micro-outlines and busy tiny flowers. Retain some carefully placed flowers, flowing stream reflections, real depth and contact shadows, so the meadow remains a complete beautiful setting. Background should support the dragon, not compete with it. Preserve the dragon's intricate identity and characteristic golden/amber energy scale pattern; do not redesign him or blur him. Keep energy glow local, do not increase the golden luminance or wash the landscape in gold. Polished Arcanoclisma fantasy illustration with clean expressive character lines, quieter scene contours and smooth subtle material details; no grain, impasto, harsh painterly noise or whole-image blur. Maintain the source's portrait ratio. Complete opaque scene, no empty or white background. No Ciantia, other characters, graves, text, card frame or watermark. Separate comparison variant, preserve V1."
};
const modelos = {
  "versao": 1,
  "data": "2026-10-03",
  "estado": "aguardando_avaliacao",
  "origemDoPedido": "Autor nesta conversa",
  "nomeDoDragao": "Philgard, a Luz da Esperança",
  "instrucoesAutor": [
    "Ciantia em campo aberto, de costas, brincando com esse dragão adolescente.",
    "O mesmo dragão sozinho, voltando ao lugar, já crescido, descansando com o livro dela abaixo do rosto sonolento.",
    "Comparar versões das duas últimas cartas com cenários mais lisos, menos rugosos e menos extremamente expressivos."
  ],
  "localId": "campo-philgard",
  "continuidade": {
    "marcos": [
      "salgueiro inclinado à direita",
      "riacho sinuoso",
      "pedra plana do livro",
      "montanhas de dois picos"
    ],
    "objeto": "livro fechado de Ciantia, marrom/ameixa com cantos dourados e ornamento circular",
    "cronologia": "adolescência com Ciantia; retorno adulto sozinho",
    "naoInferir": "morte de Ciantia"
  },
  "cartas": [
    {
      "id": "ciantia-philgard-campo",
      "nomeProvisorio": "Brincadeira no Campo",
      "nomeDefinidoPeloAutor": false,
      "personagens": [
        "Ciantia",
        "Philgard adolescente"
      ],
      "momentoDoDia": "tarde",
      "cena": "Ciantia de costas ergue um ramo para brincar com Philgard adolescente; o livro repousa na pedra junto ao riacho.",
      "escala": "dragão adolescente menor que Ciantia; riacho, árvore e montanhas dão referências de profundidade",
      "tipo": null,
      "raridade": null,
      "arteAlternativa": null,
      "valores": null,
      "custo": null,
      "grau": null,
      "arcanoGerado": null,
      "ataque": null,
      "vida": null,
      "textoDeRegras": null,
      "versaoSelecionada": null
    },
    {
      "id": "philgard-luz-da-esperanca",
      "nome": "Philgard, a Luz da Esperança",
      "nomeDefinidoPeloAutor": true,
      "personagens": [
        "Philgard adulto"
      ],
      "momentoDoDia": "entardecer",
      "cena": "Philgard adulto repousa sozinho no mesmo campo, com o livro de Ciantia abaixo do rosto sonolento.",
      "escala": "corpo adulto grande, patas dianteiras e livro em primeiro plano; local reconhecível ao fundo",
      "tipo": null,
      "raridade": null,
      "arteAlternativa": null,
      "valores": null,
      "custo": null,
      "grau": null,
      "arcanoGerado": null,
      "ataque": null,
      "vida": null,
      "textoDeRegras": null,
      "versaoSelecionada": null
    }
  ],
  "pendencias": [
    "Autor escolher V1 ou V2 independentemente para cada carta",
    "Claude definir identidade mecânica, raridade e demais campos quando precisar das cartas"
  ],
  "enriquecimentosVisuais": {
    "autor": "Codex a partir do pedido do autor",
    "escolhas": [
      "marcos de continuidade no campo",
      "livro visível nas duas cenas",
      "tarde e entardecer distinguem a passagem do tempo"
    ]
  },
  "originalLicaoDoJardim": "Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/licao-do-jardim-arte-v2.png",
  "originalAprovadoPreservado": true,
  "integracaoClaudeConfirmada": false,
  "pushAutorizado": false
};
const rel = arquivo => path.relative(repo, arquivo).split(path.sep).join('/');
const hash = dados => crypto.createHash('sha256').update(dados).digest('hex');
const salvar = async (arquivo, dados) => {
 await fs.mkdir(path.dirname(arquivo), {recursive:true});
 await fs.writeFile(arquivo, JSON.stringify(dados,null,2)+'\n','utf8');
};
const imagens = [];
for (const fonte of fontes) {
 const destino = path.join(here, fonte.arquivo);
 const dadosOrigem = await fs.readFile(fonte.origem);
 try { await fs.copyFile(fonte.origem,destino,constants.COPYFILE_EXCL); }
 catch (erro) {
  if (erro.code !== 'EEXIST') throw erro;
  if (hash(await fs.readFile(destino)) !== hash(dadosOrigem)) throw new Error('Destino com versão diferente: '+fonte.arquivo);
 }
 const metadados = await sharp(destino).metadata();
 const {data,info} = await sharp(destino).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let min=255,max=0,transparentes=0,semitransparentes=0;
 for(let i=3;i<data.length;i+=4) {const a=data[i];min=Math.min(min,a);max=Math.max(max,a);if(a===0)transparentes++;else if(a<255)semitransparentes++;}
 const cantos=[0,info.width-1,(info.height-1)*info.width,info.height*info.width-1].map(i=>data[i*4+3]);
 imagens.push({...fonte,caminho:rel(destino),dimensoes:[metadados.width,metadados.height],sha256:hash(dadosOrigem),hasAlpha:metadados.hasAlpha,alfa:{min,max,pixelsTransparentes:transparentes,pixelsSemitransparentes:semitransparentes,cantos},fundo:'Cenário completo opaco intencional',estado:'aguardando_avaliacao'});
}
for (const carta of modelos.cartas) {
 carta.variantes=imagens.filter(i=>i.cartaId===carta.id).map(i=>({variante:i.variante,arquivo:i.caminho,tratamentoCenario:i.variante==='v1'?'original com textura mais intensa':'V2 com planos mais lisos, menos textura e contraste de detalhes',estado:'aguardando_avaliacao'}));
}
await salvar(path.join(here,'prompts.json'),prompts);
await salvar(path.join(here,'modelos.json'),modelos);
await salvar(path.join(here,'manifesto.json'),{
 versao:1,data:'2026-10-03',estado:'aguardando_avaliacao',meta:{cartas:2,variantesPorCarta:2,imagensGeradas:4},
 ferramenta:'image_gen.imagegen integrada',modo:'built-in',imagens,prompts:'prompts.json',
 composicao:'Prévia HTML editável com overlay genérico v5 existente, proporção 63 × 88 mm',
 templateCanvaNovo:null,template:'Arcanoclisma/arena/templates/overlay-generico-v5.svg',
 impressao:{proporcaoMm:[63,88],exportacaoFinalComSangria:false},originalAprovadoPreservado:true,
 dadosMecanicos:'Não definidos; traços na prévia não significam zero',integracaoClaudeConfirmada:false,pushAutorizado:false
});
await salvar(path.join(here,'elementos-derivaveis.json'),{
 estado:'ideias_visuais_sem_producao_autorizada',elementos:[
 {id:'philgard',tipo:'personagem',descricao:'Dragão dourado de energia, adolescente e adulto, identificado pelo autor como Philgard, a Luz da Esperança'},
 {id:'livro-ciantia',tipo:'objeto',descricao:'Livro fechado marrom/ameixa com ornamento circular e cantos dourados, presente nas duas cenas'},
 {id:'campo-philgard',tipo:'lugar',descricao:'Campo com riacho sinuoso, salgueiro à direita, pedra plana e montanhas de dois picos'}
 ],observacao:'Não são cartas adicionais encomendadas; Claude define futuras necessidades.'
});
await salvar(path.join(repo,'Arcanoclisma/cartas/entregas/philgard-2026-10-03.visual.json'),{
 tarefaId:'philgard-2026-10-03',versao:1,pedidoVersao:null,origemDoPedido:'Autor nesta conversa',
 estado:'entregue_aguardando_avaliacao',cartas:modelos.cartas.map(c=>({id:c.id,nome:c.nome||c.nomeProvisorio,variantes:c.variantes})),
 arquivos:imagens.map(i=>({tipo:'arte_de_carta',caminho:i.caminho,sha256:i.sha256,dimensoes:i.dimensoes,variante:i.variante})),
 previa:rel(path.join(here,'index.html')),descricoes:rel(path.join(here,'modelos.json')),prompts:rel(path.join(here,'prompts.json')),manifesto:rel(path.join(here,'manifesto.json')),
 paraClaude:[
 'Lição do Jardim diurna v2 permanece vigente; o estudo noturno não foi selecionado.',
 'Philgard é o dragão dourado desta dupla. Não confundir com Dragão Transplanar azul-violeta ou reutilizar suas regras.',
 'Aguardar escolha do autor para a variante de cada carta. Tipo, custo, Grau, Arcano gerado, Valores, habilidades e raridade pendentes.',
 'Raridade altera apenas estética. Informar raridade e arteAlternativa no briefing futuro; lendária/alternativa segue acabamento v9.',
 'O mesmo local liga as duas cenas por pedido do autor; outras cartas não devem repetir este cenário sem motivo.'
 ],
 integracaoClaudeConfirmada:false,publicacao:{estado:'local',pushAutorizado:false},arquivamentoAuxiliar:'adiado conforme preferência do autor'
});
const pastaAnterior=path.resolve(here,'../2026-10-03-v1');
const arquivoNoturno=path.join(pastaAnterior,'licao-do-jardim-estudo-noturno.visual.json');
const noturno=JSON.parse(await fs.readFile(arquivoNoturno,'utf8'));
noturno.estado='estudo_nao_selecionado';
noturno.decisaoAutor={data:'2026-10-03',texto:'Essa carta em específico não precisava mudar.',acao:'Preservar Lição do Jardim diurna v2 aprovada; guardar estudo sem substituir.'};
await salvar(arquivoNoturno,noturno);
const arquivoManifesto=path.join(pastaAnterior,'manifesto.json');
const anterior=JSON.parse(await fs.readFile(arquivoManifesto,'utf8'));
for(const estudo of anterior.estudosVisuais||[]) if(estudo.registro==='licao-do-jardim-estudo-noturno.visual.json') {estudo.estado='estudo_nao_selecionado';estudo.decisaoAutor=noturno.decisaoAutor;}
await salvar(arquivoManifesto,anterior);
const arquivoAuditoria=path.join(repo,'Arcanoclisma/cartas/entregas/revisao-cenarios-2026-10-03.visual.json');
const auditoria=JSON.parse(await fs.readFile(arquivoAuditoria,'utf8'));
if(auditoria.estudoNoturno) {auditoria.estudoNoturno.estado='estudo_nao_selecionado';auditoria.estudoNoturno.decisaoAutor=noturno.decisaoAutor;}
await salvar(arquivoAuditoria,auditoria);
const leiaAnterior=path.join(pastaAnterior,'README.md');
const textoAnterior=await fs.readFile(leiaAnterior,'utf8');
await fs.writeFile(leiaAnterior,textoAnterior.replace('Estudo ainda aguardando avaliação; novo push não autorizado.','O autor indicou que a mudança não era necessária; estudo não selecionado, preservado sem substituir a versão diurna aprovada. Novo push não autorizado.'),'utf8');
console.log(JSON.stringify({estado:'aguardando_avaliacao',imagens:imagens.map(i=>({arquivo:i.arquivo,dimensoes:i.dimensoes,alfa:i.alfa})),entrega:'Arcanoclisma/cartas/entregas/philgard-2026-10-03.visual.json'}));

