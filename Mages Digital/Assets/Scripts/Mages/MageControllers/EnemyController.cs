using Math    = System.Math;
using Convert = System.Convert;
using _Random = System.Random;
using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class EnemyController : MageController
{

    private _Random random = new _Random();

    // TEST
    public override IEnumerator DrawCards(DeckController deck, int amount)
    {
        Debug.Log("ENEMY DRAW FROM " + deck.ToString());
        return base.DrawCards(deck, amount);
    }
    // TEST

    // автоматически создать заклинание
    // поставить мага в готовое состояние
    public void CreateRandomSpell()
    {
        List<SpellCard> spell = new List<SpellCard>();
        List<Order> orders = new List<Order>();
        if (_hand.spellsCount > 0)
        {
            // выбрать случаное количество карт в заклинании от 1 до 3
            int nCardsInSpell = random.Next(1, Math.Min(3, _hand.spellsCount));
            // взять по случайной карте каждого типа, не считая дикую магию
            // взятую карту удаляет из руки
            foreach (Order order in new List<Order>() {Order.SOURCE, Order.QUALITY, Order.DELIVERY})
            {
                List<Card> deck = _hand.GetDeckOfOrderType(order);
                if (deck.Count > 0)
                {
                    // индекс случайной карты
                    int index = random.Next(0, deck.Count);
                    // достать карту из руки
                    SpellCard card = (SpellCard) deck[index];
                    // удалить карту в руке
                    deck.RemoveAt(index);
                    // сохранить выбранную карту и ее порядок
                    spell.Add(card);
                    orders.Add(order);
                }
            }
            // удалить лишнии карты из заклинания
            while (spell.Count > nCardsInSpell)
            {
                int index = random.Next(0, spell.Count);
                spell.RemoveAt(index);
                orders.RemoveAt(index);
            }
            // добавить карту в заклинание и заменить случайные на дикую магию
            for (int i = 0; i < spell.Count; i++)
            {
                SpellCard cardToAdd = spell[i];
                // если в шальной магии есть карты и карта будет заменена на шальную
                if (_hand.wildMagics.Count > 0 && Convert.ToBoolean(random.Next(2)))
                {
                    // взять шальную магию из руки
                    SpellCard wildMagicToUse = (SpellCard) _hand.wildMagics[0];
                    // удалить ее из руки
                    _hand.wildMagics.RemoveAt(0);
                    // добавить заменяемую карту обратно в руку
                    _hand.AddSpellCard( _hand.GetDeckOfOrderType(orders[i]), spell[i] );
                    cardToAdd = wildMagicToUse;
                }
                _hand.spellController.SetCardToOrder(cardToAdd, orders[i]);
            }
        }
        _isReady = true;
    }

}
