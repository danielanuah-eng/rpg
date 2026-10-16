# Entrega: expedição local DungeonBDX

Meta funcional concluída: três cenários, equipe de até três entre 31 perfis, combate com as 18 skills, movimentação por PA, adversários, companheiro dracônico, HUD inferior e save contínuo com retomada. Responsável: Codex e agentes internos; Antigravity não utilizado.

Validação: 267/267 testes integrados e 114/114 na reverificação do módulo. Os três smokes do executável final executaram seis comandos cada, criaram escudo e dragão, compararam o estado completo após reabrir o save e verificaram projeção/bloqueio da HUD. Capturas URP reais, não mockups. O teste de retrato é Windows offscreen; não é build Android nem QA físico.

Arte original: 18 FBX (6204 triângulos somados no kit), 18 SFX e três ambientes musicais sintetizados. Fontes Blender/Python e hashes no projeto. Materiais explícitos corrigem shader removido da build. Save ocorre antes de VFX/SFX. A proteção geométrica da HUD funciona antes do primeiro frame desenhado.

Cliente Unity Authentication/MPS compilado/testado estaticamente: login/cadastro, salas privadas e código. Nenhuma chamada de autenticação/sala cloud realizada. Combate multiplayer, convites direcionados, saves cloud, configuração/QA do provedor, arte high-end/rigs/LODs/texturas/trilhas finais e mobile nativo permanecem pendentes. Objetivos especiais e armadilhas desativados como preparação. A imagem de referência continua ausente.

QA do autor: iniciar cada cenário; testar PA/alcance/paredes/fogo amigo; invocar/comandar/recolher; encerrar e retomar mantendo recargas; redimensionar a janela; abrir duas instâncias para conflito do save e RETOMAR DISCO; ouvir os três ambientes; conferir legibilidade/hover/toque e desempenho em dispositivo. Contas/salas precisam de provedor configurado antes do QA cloud; ainda não sincronizam combate.

Build Windows local: 1601757320 bytes. Executar ArenaTaticaUnity/JOGAR-DUNGEONBDX.cmd. Código, assets, scripts e evidências vão ao Git; a saída do build é ignorada/reproduzível.
