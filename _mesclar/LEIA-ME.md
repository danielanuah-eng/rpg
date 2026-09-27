# _mesclar — como várias conversas mexem no Peso do Céu sem uma apagar a outra

A fonte da verdade é `Rpg\O Peso do Ceu - git.bundle` (branch `main`).
Cada conversa trabalha num **branch próprio** e, assim que as mudanças de
fonte estiverem prontas, deixa aqui um bundle só com elas:

    Rpg\_mesclar\<data> <assunto>.bundle     (git bundle create X main..<branch>)

Assim, mesmo que a conversa pare no meio, o trabalho dela não se perde.

## Quem termina por último mescla tudo

1. Clona o `git.bundle` atual da pasta Rpg (não um antigo da própria sessão).
2. Para cada bundle desta pasta: `git fetch "<arquivo>" <branch>:<branch>`
   e `git merge <branch>` no `main`. Conflito se resolve lendo os dois
   lados — nenhuma regra de um pode sumir por causa do outro.
3. Recompila tudo (livro, baralho, escudo, fichas, os dois HTML, site,
   vault), roda `python3 calc/conferir.py`.
4. Copia as versões antigas para `Rpg\_lixo\<data>\`, grava as novas,
   grava o `git.bundle` novo e **move os bundles mesclados daqui para
   `Rpg\_lixo\<data>\mesclados\`**.
5. Atualiza `claude/peso-do-ceu-estado.md` no Projeto.
6. Avisa o Anuah para rodar `PUBLICAR E INSTALAR.cmd` (sobe o site e
   instala o vault).

Se esta pasta estiver vazia (só este LEIA-ME), não há nada pendente: o
`git.bundle` da pasta Rpg já tem tudo.

## Pendentes

Nenhum. O `git.bundle` da pasta Rpg já tem tudo (versão 3.8.0, 26/09: Peças v4
só no app, agora com as 182 imagens novas na ficha).

**Imagens das Peças v4: feito.** As 182 foram geradas no Canva, baixadas em
resolução cheia, injetadas (`arte3/pecas4_imagens.json`) e conferidas Peça
por Peça no colosso da casa e na ANDORINHA; encaixe em
`arte3/ajustes_imagens.json` → `arte3/ajustes_pecas.json`. O corte de alguns
chassis foi consertado na arte da ficha (`arte3/capa_chassi.py`, chamado
pelo `web_rapido.py`): capa da ARCADA fora do braço e das pernas, saiote
de GANCHO/SOMBRA fora das canelas, cabeça de RONDA/TORRE sem o mastro,
bolsões de fundo branco em REMENDO/CARROÇA/SOMBRA/GANCHO/ARCADA. Os PNG
em resolução cheia ficam fora do git (nos zips `pdc_pecas4_lote*.zip` da
Área de Trabalho).

**Atrasados de propósito (o Anuah pediu só o app nesta rodada):** livro,
baralho, escudo, fichas em PDF e vault ainda descrevem as Peças antigas
(160, com Tensa) e as classes antigas. A próxima rodada que mexer no papel
precisa gerar tudo isso de `dados/pecas4` e das classes novas.

## Já mesclados (podem ser apagados daqui; cópia em `Rpg\_lixo\2026-09-24\mesclados\`)

| bundle | branch | o que mudou | mesclado em |
|---|---|---|---|
| `2026-09-24 movimento.bundle` | `movimento` | Movimento como no Daggerheart: sem Deslocamento, ninguém age primeiro; Passada dos chassis; Peças, classes, licença da Vespertina (Passo de Academia), adversários e chefes reescritos | 24/09, `main` 3.4.0 |
| `2026-09-24 armas.bundle` | `armas` | Armas de colosso (tiro, laser, katana, lâmina, espadão, 4 tiers) com imagem 2D, créditos, Glasgow RP-1 no nível 1 e licença de partida no nível 2, sistemas do casco (Detector + 2 por casa, graus 5 e 8), cartão de armas e skills na ficha (no lugar da Aparência), Gerador com quantidade de inimigos, habilidades e armas com custo em Medo | 24/09 noite, `main` 3.5.0 (já com o movimento) |
| (sem bundle — mesclado direto) | `prototipos` | 24 protótipos de alta tecnologia (3 por licença, Escalão 3 e 4, Vespertina em rosa), só no Gerador, com arte concept; textos já na regra de movimento nova | 24/09 noite, `main` 3.6.0 |
| (sem bundle — mesclado direto) | `pecas4` | Peças v4 (640: 8 por nível × 10 × 8 fábricas; Esperança/Estresse, Gralha um ou outro, Ossatura PV; sem Tensa, sem Medo, sem mapa), Invasão estilo Lancer, drones de 1 Estrutura, classes com subclasse de mundo e de cabine, armas avançadas de piloto, exoarmaduras e preços, 16 ambientes; ficha com filtros por categoria, descontos, Absorver (Ossatura), foto do piloto e loja; Gerador com Lugares e habilidades sorteadas só de colosso; catálogo de 182 prompts de imagem | 25/09, `main` 3.7.0 |
| (sem bundle — mesclado direto) | `imagens4` | As 182 imagens novas das Peças v4 (Canva) na ficha, conferidas Peça por Peça no colosso; ajustes de encaixe (tamanho, deslocamento, modo) também nas Peças com imagem antiga; capa/saiote/mastro/bolsões consertados na arte dos chassis (`arte3/capa_chassi.py`); o pulso some junto com o antebraço trocado | 26/09, `main` 3.8.0 |
