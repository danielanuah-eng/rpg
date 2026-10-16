# DungeonBDX — assets originais v0.1

Base funcional de arte e áudio produzida pelo Codex após a redistribuição das tarefas do Antigravity. São modelos 3D originais em FBX, com materiais por cor, fonte Blender editável e síntese de áudio reproduzível. Não foram copiados tiles, personagens, imagens ou músicas do Arcadia Quest.

O manifesto `manifest.json` contém os caminhos `Resources`, IDs, medidas geométricas, contagem real de triângulos, hashes e limites da entrega. A escala é 1 metro por unidade; os FBX exportam com Y para cima e o pivô no chão. Os pisos ocupam 1 x 1 metro. Colisores devem ser atribuídos pelo consumidor conforme o manifesto. Aberturas da porta permanecem livres.

Modelos em `Resources/DungeonBDX/Models/`: três pisos temáticos, parede, arco de porta, coluna, estante, braseiro, formação de basalto, ovo de dragão, cristal de objetivo, caixa, entulho, armadilha de espinhos; Lyra Faísca, Orion Prisma, Vaelis do Vínculo e um pequeno dragão companheiro. As personagens têm silhuetas distintas e modelos estáticos; movimento por transformação no runtime não equivale a animação esquelética. Para URP, usar um material compatível preservando as cores dos materiais `DBDX_*` importados do FBX.

Cada cenário possui um loop ambiente estéreo original de 16 segundos em PCM 16-bit / 22050 Hz; as 18 habilidades possuem SFX mono próprios, com os mesmos IDs `dbdx.sfx.*` do catálogo. O volume de origem tem folga para a mixagem. São protótipos de síntese, não a trilha sonora final solicitada. Os loops têm frequência e envelopes periódicos para repetição contínua; a reprodução usa `AudioSource.loop`.

Reprodução da fonte, a partir de `ArenaTaticaUnity`:

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python Tools/GenerateDungeonAssets/generate_assets.py
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python Tools/GenerateDungeonAssets/render_preview.py
```

O render `Preview/original-kit.png` mostra os objetos efetivamente produzidos, não uma proposta visual. A fonte de geração e o `.blend` ficam em `Tools/GenerateDungeonAssets/`, fora de `Assets`, para a Unity importar apenas os FBX destinados ao jogo.

Pendências: rigor/qualidade final high-end, texturas autorais detalhadas, LODs, rigs e clips esqueléticos, adaptação visual das 28 identidades herdadas da Arena, 31 templates de cartas, composição musical final, conferência da imagem de referência ausente e medição de desempenho em aparelhos reais. Não há números de GPU inventados nem aprovação visual do autor.
