# Catálogo para a aba Edição de jogo

Pedido direto do autor, registrado nas tarefas 012 e 013. O catálogo abrange entregas antigas e novas da Arena; referências e fontes aparecem com categoria própria. Sua existência em disco não confirma que foram importadas nem que o editor foi implementado. Claude deve responder às tarefas depois de integrar.

Executar `atualizar-catalogo.py` com Python e Pillow quando chegar nova entrega. O gerador lê o plano histórico, as entregas verificadas de Edgar/Baltazar/mapas e descobre arquivos visuais adicionais. O ID de cada arquivo deriva de seu caminho relativo e permanece estável ao atualizar a imagem. `assetId` permite agrupar formatos/variantes. `heroEditableFields` contém movimento, vida, alcance e dano; os valores atuais vêm do Roster do motor, sem sobrescrever ajustes do autor com valores vazios do catálogo.

Na aba, oferecer biblioteca visual de personagens, mapas, props e efeitos, prévias e estado de integração. Referências técnicas não entram automaticamente numa partida. Expor os quatro valores de cada personagem, os parâmetros da aura de Baltazar e seleção de aparência, validar os valores no motor e sincronizar partidas pela autoridade existente. A câmera rotativa é local a cada jogador.

`estado-producao.json` guarda grupos e jobs já iniciados e as próximas etapas. A retomada automática consulta esses IDs antes de gerar, prioriza comandos atuais e trabalha em pequenos lotes. Mapas visualmente entregues ainda precisam passar na validação de jogabilidade de Claude.

Depois da conclusão confirmada da Arena, seguem comparação de todos os assets PixelLab em Blender e preparação do PvEvP de KrosArcano, incluindo revisão de crítico e armadura. Essa revisão não altera a Arena atual.
