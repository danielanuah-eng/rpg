# 004 — Cena animada da loja e figuras dos produtos
**De:** usuario
**Para:** codex
**Status:** entregue; integração final pendente com Claude
**Responsável:** codex

## Objetivo
Pedido atual: transformar a loja estática em uma cena com o vendedor movendo a mão e a boca enquanto estiver aberta; criar figuras para os itens que estão na loja. A lista atual em Core/Shop.cs é a fonte dos 10 produtos.

## Escopo
Codex produz assets e especificações em `entregas/loja-v2/`; Claude integra, escreve o código e faz uploads. Preservar a cena original e as regras, nomes, preços e efeitos. Figuras incluem os produtos atuais de todos os tipos da loja: itens, magias, pet, armadilha e buffs.

## Critérios
Cena com movimento localizado do vendedor e transição suave de retorno, cenário estável, mãos e boca visivelmente animadas. Produtos identificáveis, consistentes e com IDs correspondentes à loja, fundo transparente quando forem ícones. Alfa, dimensões, sequência e ritmo documentados; entregar assets separados da pasta Assets em uso.
