# Embelezador — animações e efeitos visuais

Skill: `../../.agents/skills/embelezador/SKILL.md`. Atua junto do Designer de Cartas ou separado, conforme a necessidade atual do Claude. Este diretório entrega assets e especificações; a implementação do movimento, interação e renderização fica com Claude em `../tcg/`.

## Regra definida pelo autor

Atualização de 04/10/2026: **toda magia nova ou skin nova deve receber animação própria**, baseada na arte efetivamente usada e no efeito atual da carta. Inspecionar a imagem e ler a descrição; preservar elemento, cores e objeto/ação representados. Raio de fogo, por exemplo, deve percorrer a origem até a carta/unidade alvo. Curas, ondas, portais, compras e recursos usam movimentos correspondentes. Não basta escolher uma cor pelo Valor.

Entrega atual: `entregas/magias-v17.visual.json`, 132 cartas com três camadas cada, SVG e PNG com alfa medido; runtime `../online/public/kit/efeitos/magias-v17.js`. Direção individual em `direcao-magias-v17.mjs`; arquivos abertos regeneráveis offline com `construir-magias-v17.mjs`. Prévia em `previas/magias-v17/index.html`. Conferência automática: `node --test Arcanoclisma/efeitos/verificar-magias-v17.test.mjs`, a partir da raiz do repositório.

Para cada nova entrega, registrar ID, imagem/skin e hash, regra vigente, gatilhos aceitos, alvo e fases da animação. Nova skin deve ter uma entrada própria em `skins[skinId]` do manifesto; o integrador passa `skinId`. Mudança de arte/efeito exige revisão desse vínculo. Efeitos só executam após `resultado: 'resolvido'`, respeitando redução de movimento. Não reproduzir dano durante a entrada de uma carta que só causa dano ao ativar sua habilidade.

**Todos os aliados usam movimentação genérica, fluida e compartilhada ao atacar. Não possuem efeito específico de ataque.** A carta prepara, avança, apresenta o contato e retorna. Claude define os gatilhos reais e os parâmetros do motor. Movimento por transformações da carta deve ser preferido quando suficiente, evitando gerar quadros redundantes.

O agente pode criar efeitos mágicos, personagens, ambientes, ataques e imagens para outros eventos quando o Claude precisar deles. Tem liberdade estética para enriquecer descrições insuficientes com detalhes e magia, preservando identidade e função. Não criar efeitos individuais de aliados sem uma nova orientação do autor que altere essa regra.

## Coordenação e organização

- Claude escreve `pedidos/<id>.claude.json` com identidade da ação/carta, objetivo atual, referências e requisitos disponíveis. `autor` é Claude; `estado` é solicitado; `necessidadeAtual` é true. Necessidade cancelada interrompe a produção.
- Embelezador escreve `pedidos/<id>.duvidas.json` para definições essenciais e entrega assets em `assets/<id>/vNN/`.
- Embelezador grava `entregas/<id>.visual.json`: ID, versão do pedido, arquivos relativos à raiz do repo, hashes, dimensões, transparência, camadas, pivôs, ordem dos quadros, timings definidos, parâmetros comuns, identidade preservada, escolhas estéticas e ligação aos manifestos de cartas/movimentos.
- Claude escreve `entregas/<id>.integracao.claude.json`, confirmando o uso ou pedindo ajustes. Não chamar uma entrega de integrada sem essa resposta.

Canal atual: arquivos locais e Git autorizado; não há conexão direta confirmada com a conversa externa do Claude. Nenhuma animação ou efeito foi produzido ao definir o agente. Novas produções exigem pedido atual, não apenas a existência desta pasta.
