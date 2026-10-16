# 04. Modelos, tiles e áudio

> Direção posterior do autor: não utiliza Antigravity; Codex redistribui e continua a base funcional. Dependências abaixo referem-se ao briefing original. Estado atual e pendências: PIPELINE.md e entregas/04-06-local/.

- **De:** autor / UltraBridge, consolidado por codex
- **Para:** codex / agente de assets / Blender
- **Status:** base-funcional-entregue; acabamento-final-pendente
- **Projeto / pasta de trabalho:** C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg
- **Entrega:** arquivos locais, sem commit/push do executor
- **Dependência:** 03 revisada por Codex; contratos de skills e métricas 02 disponíveis

Gate 03 aprovado: resposta `respostas/03-codex-skills.resposta.md`, contrato `contratos/skills.json` v1.0.0 e `contratos/balanceamento.json` v1.0.0. O autor atribuiu 02–03 ao Codex. Entradas autossuficientes em `contratos/assets-pedido.json`: 3 novos heróis + 1 companheiro, 28 perfis de origem para adaptação, 18 bindings de animação/SFX/ícones e 3 cenários planejados com BGM dedicado. Cenários ainda não foram mapeados e nenhum asset final foi produzido por esse pedido.

Criar com Blender modelos/props/dragões e animações, tiles de masmorra muito detalhados, VFX/SFX por skill e trilha dedicada por cenário. Estética HQ sombria inspirada em AQ, recursos próprios ou licenciados para o projeto. Não redistribuir tiles extraídos de AQ. Recursos Unity Store devem ter URL, licença, titular e comprovante; não efetuar compras sem autorização específica de gasto.

Propriedade exclusiva: `coordenacao/dungeonbdx/entregas/04-assets/` (fontes, exports, previews e manifesto.json), `coordenacao/dungeonbdx/respostas/04-antigravity-assets.resposta.md`. Não editar scripts do Codex/Claude nem baixar packs em arquivos compartilhados do Unity.

Manifesto: IDs de cenário/herói/skill, arquivos, hash, licença/procedência, escala, pivô, colisão, métricas dos tiles e encaixes, sockets, animações/duração/loop, materiais/LOD, orçamento PC/mobile, áudio/duração/loop/loudness. Entregar fontes Blender e export glTF/FBX; texturas com tamanhos e alfa medido. Definir orçamento de luzes e texturas por plataforma para 05. Paleta final aguarda referência visual ausente.

Vincular por animationId/soundId/iconId e ID do herói/skin/hash da imagem correspondente. Ranger preserva arma/identidade visual de origem (por exemplo arco/esporos), mesmo quando compartilha uma regra de disparo do pool. O motor já resolveu o efeito ao emitir SkillUseResult; eventos visuais não devem aplicar dano novamente. Não reutilizar o PNG frontal corrigido retirado de Ciantia nem conteúdo da expansão TCG bloqueada.

Pronto: previews verificáveis, import sem erros e relatório de orçamento. Codex revisa e libera 05.
