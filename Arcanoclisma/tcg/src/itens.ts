// Itens do Conjurador (pedido do autor no envio de 03/10): Canal (arma), Armadura e Extra (item consumível de suporte).
// Ficam num side deck fora do baralho; cada jogador escolhe antes da partida um de cada espaço, e não troca durante a partida.
// O Conjurador ataca com o Ataque do Canal; quando bloqueia, a Armadura evita dano até a sua Defesa e se desgasta até quebrar;
// o Extra é usado uma vez (ativação) e vai para o cemitério.
// Estes são os itens básicos (genéricos, sem Valor e sem efeitos). Padrão das armas básicas (pedido do autor, 04/10):
// Ataque 1 de graça e Grau 0; Ataque 2 com Grau 1 e custo 1; Ataque 3 com Grau 3 e custo 2. O custo (Arcano) e o Grau da
// carta do item são o que se paga e o Grau mínimo para atacar com a arma (ou para usar/proteger, nos outros itens).
import type { CartaDef, CategoriaItem } from './tipos.ts';

function item(id: string, nome: string, categoriaItem: CategoriaItem, campos: Partial<Pick<CartaDef, 'ataque' | 'defesa' | 'vidaDesgaste' | 'custo' | 'grau'>>, texto: string): CartaDef {
  return {
    id, nome, tipo: 'item', categoriaItem, ...(categoriaItem === 'canal' ? { tipoCanal: 'arma' as const } : {}), subtipos: [categoriaItem === 'armadura' ? 'Proteção' : 'Equipamento'], valores: [],
    custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 0, ...campos, cenario: '', texto,
  };
}

export const itensBasicos: CartaDef[] = [
  item('item-canal-cajado', 'Cajado de aprendiz', 'canal', { ataque: 1 }, 'Canal. Arma: Ataque 1. Atacar é de graça.'),
  item('item-canal-lamina', 'Lâmina rúnica', 'canal', { ataque: 2, custo: 1, grau: 1 }, 'Canal. Arma: Ataque 2. Para atacar: Grau 1 e pague 1.'),
  item('item-canal-lanca', 'Lança do eclipse', 'canal', { ataque: 3, custo: 2, grau: 3 }, 'Canal. Arma: Ataque 3. Para atacar: Grau 3 e pague 2.'),
  item('item-armadura-gibao', 'Gibão de couro', 'armadura', { defesa: 1, vidaDesgaste: 4 }, 'Armadura. Defesa 1, desgaste 4.'),
  item('item-armadura-couraca', 'Couraça de placas', 'armadura', { defesa: 2, vidaDesgaste: 6, grau: 2 }, 'Armadura. Defesa 2, desgaste 6. Protege a partir do Grau 2.'),
  { ...item('item-extra-pocao', 'Poção de cura', 'extra', {}, 'Extra. Consumível: cure 3 do seu Conjurador.'), ativacao: { custo: 0, suspende: false, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'conjuradorProprio' }] } },
  { ...item('item-extra-elixir', 'Elixir de foco', 'extra', { grau: 1 }, 'Extra. Consumível (Grau 1): ganhe 2 de Arcano.'), ativacao: { custo: 0, suspende: false, efeitos: [{ tipo: 'arcano', valor: 2 }] } },
];
