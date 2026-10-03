# Embelezador — animações e efeitos visuais

Skill: `../../.agents/skills/embelezador/SKILL.md`. Atua junto do Designer de Cartas ou separado, conforme a necessidade atual do Claude. Este diretório entrega assets e especificações; a implementação do movimento, interação e renderização fica com Claude em `../tcg/`.

## Regra definida pelo autor

**Todos os aliados usam movimentação genérica, fluida e compartilhada ao atacar. Não possuem efeito específico de ataque.** A carta prepara, avança, apresenta o contato e retorna. Claude define os gatilhos reais e os parâmetros do motor. Movimento por transformações da carta deve ser preferido quando suficiente, evitando gerar quadros redundantes.

O agente pode criar efeitos mágicos, personagens, ambientes, ataques e imagens para outros eventos quando o Claude precisar deles. Tem liberdade estética para enriquecer descrições insuficientes com detalhes e magia, preservando identidade e função. Não criar efeitos individuais de aliados sem uma nova orientação do autor que altere essa regra.

## Coordenação e organização

- Claude escreve `pedidos/<id>.claude.json` com identidade da ação/carta, objetivo atual, referências e requisitos disponíveis. `autor` é Claude; `estado` é solicitado; `necessidadeAtual` é true. Necessidade cancelada interrompe a produção.
- Embelezador escreve `pedidos/<id>.duvidas.json` para definições essenciais e entrega assets em `assets/<id>/vNN/`.
- Embelezador grava `entregas/<id>.visual.json`: ID, versão do pedido, arquivos relativos à raiz do repo, hashes, dimensões, transparência, camadas, pivôs, ordem dos quadros, timings definidos, parâmetros comuns, identidade preservada, escolhas estéticas e ligação aos manifestos de cartas/movimentos.
- Claude escreve `entregas/<id>.integracao.claude.json`, confirmando o uso ou pedindo ajustes. Não chamar uma entrega de integrada sem essa resposta.

Canal atual: arquivos locais e Git autorizado; não há conexão direta confirmada com a conversa externa do Claude. Nenhuma animação ou efeito foi produzido ao definir o agente. Novas produções exigem pedido atual, não apenas a existência desta pasta.
