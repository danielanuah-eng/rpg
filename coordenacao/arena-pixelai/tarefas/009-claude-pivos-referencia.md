# 009 — Aplicar pivôs registrados e conferir transições no Unity

**De:** codex, conforme pedido técnico do autor
**Para:** claude
**Status:** pendente de integração
**Responsável:** claude

Auditoria concluída dos 284 clipes presentes no snapshot de 12 pastas. `auditorias/sincronia-2026-10-10/ancoras-para-claude.json` registra a primeira pose de cada clipe na referência estática da mesma direção. Alfa e RGBA visível são idênticos em 284/284 casos, descontando apenas as margens transparentes. O pivô derivado conserva a origem da referência e vale para **todos os frames do clipe**.

127 clipes têm diferença em relação ao pivô importado no snapshot, porque a origem atual foi inferida do pixel opaco mais baixo de toda a animação. Exemplos: Ciantia idle NE/walk SW 4 px; Dragão walk NE 13 px e NW 16 px; Berserk antigo idle SW 5 px. Nenhum sprite foi redimensionado. O JSON inclui `difference_from_imported_pivot_px` e `pivot_for_all_frames` para aplicação direta no importador e .meta, preservando GUIDs.

1. Aplicar o pivô por clipe ao recortar as tiras; manter PPU 128 e escala vinculada à referência. Não recalcular por partículas, arma, último death, volume total do canvas ou posição de cada passo.
2. Reimportar os nove reparos finais (Damian pistol SE agora **168 px**, não 172). A nova forma possuída terá manifesto próprio de pivôs para 40 clipes.
3. Testar static ↔ idle ↔ walk e mudança de direção nas oito orientações. Há 48 GIFs comparativos de assets em `auditorias/sincronia-2026-10-10/transicoes/`; não equivalem a execução no Unity.
4. As animações atuais só têm quatro diagonais. Mapear explicitamente fallback/espelho das cardinais sem trocar arma/mão, ou solicitar as cardinais faltantes; não anunciar oito direções completas para os personagens antigos. A nova forma terá oito reais.
5. Confirmar duração real por ação/direção, recuperação e momento de impacto. Não interromper hit pela espera fixa de 0,45 s nem encurtar a recuperação para iniciar outro evento. Preservar o avanço suave por distância que Claude já implementou.

Limite: a origem estática é preservada, mas centrar fisicamente entre as botas ainda exige avaliar a referência, sobretudo para unidades voadoras, capas e sombras. Variação do bounding box por arma/efeito e passos alternados não é mudança de volume do corpo nem motivo para fixar o centro de massa.
