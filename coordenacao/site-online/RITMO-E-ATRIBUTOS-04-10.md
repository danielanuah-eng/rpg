# Ritmo e atributos — entrega visual de 04/10/2026

Pedido direto do autor: centralizar ataque/vida e desacelerar animações automáticas. Alterações restritas à apresentação; regras, pagamentos e passagem automática de prioridade permanecem sob responsabilidade do Claude.

## Entregue pelo Codex

- `online/public/css/templates-valor-v12.css`: grupo de atributos centrado em 50% da carta; número centrado dentro do acabamento, ícone pequeno independente. Aplica também defesa/desgaste e carta com um único atributo. CSS carregado pelo renderizador atual.
- `online/public/js/ritmo-visual-v13.js`: constantes compartilhadas de duração; entrada 930 ms, ataque/bloqueio/colisão 1320 ms, suspensão 540 ms, reposicionamento da mão 360 ms, compra para mão 780 ms, retorno 540 ms, arraste para campo 680 ms.
- `online/public/js/mesa-efeitos-codex.js`: entrada, colisão e resolução usam o novo perfil, curva suave e tempos mais longos. Resolução respeita o tempo solicitado com fator 1,5 e limites de 900–1800 ms.
- `online/public/css/cartas-nitidez-v11.css`: giro da carta inteira 540 ms, acomodação da mão 360 ms, chegada à pilha 750 ms. Movimento reduzido preservado.

## Claude: ligações restantes nos seus arquivos em andamento

Não publiquei os arquivos de integração ainda não concluídos do Claude. Aplicar o perfil compartilhado para que os dois sistemas terminem juntos:

1. Em `online/public/js/mesa-ganchos.js`, importar `RITMO_VISUAL` de `./ritmo-visual-v13.js` e substituir `DURACAO_MOVIMENTO = 880` por `RITMO_VISUAL.ataqueBloqueio`. A duração do CSS `.card-motion.attack-generic` e `.card-motion.block-generic` precisa ser a mesma: 1320 ms. Não alongar apenas o CSS deixando a limpeza em 880 ms.
2. Em `online/public/js/mesa.js`, importar o perfil e usar `voltarParaMao` no padrão de `voar`, `arrasteParaCampo` ao jogar e `desaparecer` na saída opaca. A animação de compra do baralho para mão deve usar `comprarParaMao`, verso para o rival e resultado confirmado. Não alterar o fluxo de custo, o timer de prioridade nem os dados privados.
3. Usar o movimento já aplicado à carta inteira, incluindo moldura/texto/estatísticas; não girar só a imagem. Preservar retomada a partir do tempo decorrido após redesenho.
4. Confirmar integração em um retorno do Claude. Esta entrega ainda não foi conferida no navegador; nenhum teste foi solicitado ou executado.

## Próximas prioridades do autor

`Arcanoclisma/efeitos/PRIORIDADES-04-10.md` especifica o D20 nas cores do Dragão Transplanar, todas as vinte faces e rolagem, e a entrada gradual da arena. Produzir depois da etapa atual de artes, antes da expansão. O D20 exibe o resultado do motor; não define regras de início.

## Ciantia

O autor pediu não publicar a correção frontal de Ciantia. Ela havia sido enviada em `dceec35` antes dessa instrução; o manifesto agora marca a retirada de uso. Não usar nem reenviar o PNG. O novo prompt reservado para a expansão mostra Ciantia lateral, olhos fechados, agachada sobre os joelhos brincando com os dois dragões voltados ao jogador, com o livro atrás. Expansão ainda bloqueada no jogo.
