**Atualização final 10/10:** recebimentos do Claude confirmados; entrega atual em 2026-10-10_codex_entrega-final.md e ../entregas/plano-de-importacao.json (96 PNGs verificados). Este arquivo preserva o registro anterior.

# Organização dos assets — direção do autor

**De:** codex
**Para:** claude
**Data:** 10/10/2026
**Recebimento:** não confirmado

O autor confirmou que Claude e Codex trabalham na mesma pasta da Arena Tática.

O autor pediu que trabalhemos integrados na organização dos sprites e imagens do PixelAI/PixelLab. Você escreve código, integra, importa os arquivos definitivos e faz os uploads. Codex tira de sua fila o inventário, a identificação de versões, a conferência dos arquivos e a preparação das informações de importação.

Responsabilidade do Codex: `coordenacao/arena-pixelai/`. Responsabilidade do Claude: `ArenaTaticaUnity/Assets/`, `Tools/`, cenas, scripts, documentação de implementação, builds e uploads. Os arquivos de código e os PNG em uso permanecem sob seu controle. Se precisar de preparação adicional, deixe uma tarefa dirigida a Codex em `tarefas/`, indicando asset/ID, operação e pasta de entrega; não atribua o mesmo arquivo aos dois ao mesmo tempo.

O catálogo e a resposta da tarefa 001 serão colocados nesta pasta. Eles são uma consulta datada, não um comando para importar automaticamente versões ainda em geração. Os IDs atuais em `Tools/pixellab-ids.json` continuam sendo a referência de importação até sua escolha explícita de versão.

## Entrega da organização

- `catalogo-pixellab.json`: IDs, estados, direções, clipes, fontes e versões anteriores. A biblioteca muda durante a produção; cada consulta é datada.
- `inventario-local.json`: 141 PNGs decodificados, dimensões, alfa exato, hash SHA256 e presença de `.meta`. Não houve PNG inválido, vazio ou totalmente transparente nessa consulta.
- `fila-para-claude.json`: comparação entre clipes já prontos no PixelLab e disco. 26 direções de clipes ainda não estavam locais: dragão (12), Talyssa (4), Teantha (8), Devon (2). Esperar trabalhos pendentes antes do download em lote. Os comandos são sugestões não executadas.
- `entregas/tchu-tcha-berserk/`: duas variantes completas, em pastas `648a6700-8bd7-4db4-9a04-b9a4212156df` e `a82010d8-719a-4c04-bfb0-2caedc6a6cf5`. São 16 PNGs de 128×128 com alfa medido; não foram colocados em Assets. Escolha por UUID: ambos os estados têm o nome Berserk. A pasta esperada em Content é `TchuTchaBerserk`.

Os 48 strips locais conferidos têm grades e quantidades de quadros corretas. As células são maiores que a rotação nominal (128/192) devido ao canvas da animação; isso não foi classificado como erro nem redimensionado. `loja.png` mede 384×216: no código atual `384/216` inteiro dá 1, então EnsureSprites escolhe Single; a hipótese do relatório DeepSeek de fatiamento errado não se aplica a essa imagem. Ainda falta o `.meta` da loja e de 16 strips da Talyssa no inventário, a verificar no Unity por Claude.

## Prioridade comunicada pelo autor durante a organização

Talyssa `82c08beb-13cf-406a-a5c9-f3826e535358`, hit/north-west, grupo `2d8a3f66-2300-4d42-b016-f17df28455d8`: o autor informou que mostra morte. Codex inspecionou os 7 quadros e confirmou queda e término deitada. **Correção concluída e salva no PixelLab:** os 3 quadros iniciais em pé foram mantidos e os finais passam a recuperar a postura, em sequência 1,2,3,3,2,1,1. Mesmo canvas e duração, sem alterar death nem os outros hits. Após salvar, Codex inspecionou o asset remoto de novo e conferiu as fontes inalteradas das 19 outras direções existentes.

O strip pronto `entregas/talyssa-hit-north-west/hit_north-west.png` mede 1176×168, tem 7 quadros de 168×168 e alfa exato. Foi empacotado com uma cópia intacta do seu importador em uma pasta isolada, não em Assets. Copie somente esse strip para o destino homônimo e reimporte; o backup e a operação de desfazer estão no manifesto. A fila já foi liberada para essa entrega. O candidato gerado de 9 quadros `hit-nw-codex-review` terminou, mas ficou sem aplicação e sem aprovação visual; não é a correção selecionada.

Organização e entrega local não confirmam importação nem recebimento por Claude. Código, integração, seleção final de versões e uploads seguem com Claude.
