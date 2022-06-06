// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class _Archive : MonoBehaviour
// {
//     /*
//     // автоматически создать заклинание
//     // поставить мага в готовое состояние
//     // ADD можно добавить добавление шальной магии при неполном заклинании
//     public void CreateRandomSpell()
//     {
//         List<SpellCard> newSpell = new List<SpellCard>();
//         List<Order> orders = new List<Order>();
//         if (_hand.spellsCount > 0)
//         {
//             // выбрать случаное количество карт в заклинании от 1 до 3
//             int nCardsInSpell = _random.Next(1, Math.Min(3, hand.spellsCount));
//             // взять по случайной карте каждого типа, не считая дикую магию
//             // взятую карту удаляет из руки
//             foreach (Order order in new List<Order>() {Order.SOURCE, Order.QUALITY, Order.DELIVERY})
//             {
//                 List<Card> deck = _hand.GetDeckOfOrderType(order);
//                 if (deck.Count > 0)
//                 {
//                     // индекс случайной карты
//                     int index = _random.Next(0, deck.Count);
//                     // достать карту из руки
//                     SpellCard card = (SpellCard) deck[index];
//                     // удалить карту в руке
//                     deck.RemoveAt(index);
//                     // сохранить выбранную карту и ее порядок
//                     newSpell.Add(card);
//                     orders.Add(order);
//                 }
//             }
//             // удалить лишнии карты из заклинания
//             while (newSpell.Count > nCardsInSpell)
//             {
//                 int index = _random.Next(0, newSpell.Count);
//                 hand.AddSpellCard(hand.GetDeckOfOrderType(orders[index]), newSpell[index]);
//                 newSpell.RemoveAt(index);
//                 orders.RemoveAt(index);
//             }
//             // добавить карту в заклинание и заменить случайные на дикую магию
//             for (int i = 0; i < newSpell.Count; i++)
//             {
//                 SpellCard cardToAdd = newSpell[i];
//                 // если в шальной магии есть карты и карта будет заменена на шальную
//                 if (hand.wildMagics.Count > 0 && Convert.ToBoolean(_random.Next(2)))
//                 {
//                     // взять шальную магию из руки
//                     SpellCard wildMagicToUse = (SpellCard) hand.wildMagics[0];
//                     // удалить ее из руки
//                     hand.wildMagics.RemoveAt(0);
//                     // добавить заменяемую карту обратно в руку
//                     hand.AddSpellCard( hand.GetDeckOfOrderType(orders[i]), newSpell[i] );
//                     cardToAdd = wildMagicToUse;
//                 }
//                 spell.SetCardToOrder(cardToAdd, orders[i]);
//             }
//         }
//         spell.PrepareSpellCards();
//         _isReady = true;
//     }
//     */
// }
