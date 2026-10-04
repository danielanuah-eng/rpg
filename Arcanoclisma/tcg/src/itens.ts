// Itens do Conjurador (pedido do autor no envio de 03/10): Canal (arma), Armadura e Extra (item consumível de suporte).
// Ficam num side deck fora do baralho; cada jogador escolhe antes da partida um de cada espaço, e não troca durante a partida.
// O Conjurador ataca com o Ataque do Canal; quando bloqueia, a Armadura evita dano até a sua Defesa e se desgasta até quebrar;
// o Extra é usado uma vez (ativação) e vai para o cemitério.
// Estes são itens básicos, sem Valor e sem efeitos, para testar a mecânica; raridade e efeitos vêm depois.
import type { CartaDef, CategoriaItem } from './tipos.ts';

function item(id: string, nome: string, categoriaItem: CategoriaItem, campos: Pick<CartaDef, 'ataque' | 'defesa' | 'vidaDesgaste'>, texto: string): CartaDef {
  return {
    id, nome, tipo: 'item', categoriaItem, ...(categoriaItem === 'canal' ? { tipoCanal: 'arma' as const } : {}), subtipos: [categoriaItem === 'armadura' ? 'Proteção' : 'Equipamento'], valores: [],
    custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 0, ...campos, cenario: '', texto,
  };
}

export const itensBasicos: CartaDef[] = [
  item('item-canal-cajado', 'Cajado de aprendiz', 'canal', { ataque: 1 }, 'Canal. Arma: Ataque 1.'),
  item('item-canal-lamina', 'Lâmina rúnica', 'canal', { ataque: 2 }, 'Canal. Arma: Ataque 2.'),
  item('item-canal-lanca', 'Lança do eclipse', 'canal', { ataque: 3 }, 'Canal. Arma: Ataque 3.'),
  item('item-armadura-gibao', 'Gibão de couro', 'armadura', { defesa: 1, vidaDesgaste: 4 }, 'Armadura. Defesa 1, desgaste 4.'),
  item('item-armadura-couraca', 'Couraça de placas', 'armadura', { defesa: 2, vidaDesgaste: 6 }, 'Armadura. Defesa 2, desgaste 6.'),
  { ...item('item-extra-pocao', 'Poção de cura', 'extra', {}, 'Extra. Consumível: cure 3 do seu Conjurador.'), ativacao: { custo: 0, suspende: false, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'conjuradorProprio' }] } },
  { ...item('item-extra-elixir', 'Elixir de foco', 'extra', {}, 'Extra. Consumível: ganhe 2 de Arcano.'), ativacao: { custo: 0, suspende: false, efeitos: [{ tipo: 'arcano', valor: 2 }] } },
];
