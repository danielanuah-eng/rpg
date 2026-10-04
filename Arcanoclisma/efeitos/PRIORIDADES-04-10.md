# Prioridades visuais do autor — 04/10/2026

Depois da etapa atual de revisão das artes do jogo, produzir primeiro os dois assets abaixo, antes de retomar a expansão. Não iniciar outros agentes durante a produção.

## D20 da abertura da partida

- Dado de vinte faces triangulares; todos os lados e números de 1 a 20 legíveis e consistentes.
- Cores do Dragão Transplanar da primeira carta: azul profundo, azul-lilás/violeta e energia azul suave. Números em dourado claro e brilhante, com contraste; brilho localizado.
- Preferir um modelo 3D reutilizável com mapa de faces e materiais, em vez de vinte imagens com geometria inconsistente. Entregar todas as faces, vistas e animação de rolagem, com resultado configurável.
- A rolagem deve ter preparação, giro, contatos com a superfície, desaceleração, acomodação e leitura do número. Respeitar movimento reduzido.
- Claude determina o resultado e a regra de início da partida. A animação só apresenta o resultado confirmado, não escolhe jogador inicial nem usa aleatoriedade visual como regra.
- Referência: `Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/dragao-da-travessia-arte-v1.png`.

## Entrada gradual da arena

- Apresentar a arena por partes: piso/fundo, zonas dos dois jogadores, conjuradores, baralhos/mãos e controles discretos.
- Transições suaves com sobreposição de tempos, sem aparecer tudo de uma vez e sem prolongar o bloqueio dos comandos.
- Não revelar informação privada do rival; as cartas dele continuam mostrando o verso.
- Entrar apenas ao abrir uma partida/arena; não repetir a introdução em cada redesenho do estado.
- Perfil inicial: 1600 ms com intervalos de 180 ms entre grupos; ajuste de polimento após prévia. Redução de movimento mostra o estado final diretamente.

## Ritmo durante o jogo

O autor pediu movimentos automáticos mais lentos e fluidos: compra para a mão, reposicionamento, carta virando/suspendendo, entrada no campo, ataque/bloqueio e colisão. O perfil reutilizável está em `online/public/js/ritmo-visual-v13.js`; o contrato de ligação está em `coordenacao/site-online/RITMO-E-ATRIBUTOS-04-10.md`.
