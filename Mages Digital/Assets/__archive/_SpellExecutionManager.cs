// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SpellExecutionManager : MonoBehaviour
// {
//     // private static SpellExecutionManager _instance;
//     // public  static SpellExecutionManager instance => _instance;


//     // public List<MageController> aliveMages = new List<MageController>();

//     // public List<HandSpellController> spellsOrder = new List<HandSpellController>();

//     // public List<HandSpellController> oneCardSpells   = new List<HandSpellController>();
//     // public List<HandSpellController> twoCardSpells   = new List<HandSpellController>();
//     // public List<HandSpellController> threeCardSpells = new List<HandSpellController>();

//     // // список тех кто уже походил

//     // public MageController current;
     

//     // // самый живучий враг

//     // // самый хилый враг

//     // void Awake()
//     // {
//     //     if (_instance == null) _instance = this;
//     //     else Destroy(gameObject);
//     // }

//     // public IEnumerator SetupSpellExecution()
//     // {
//     //     // очистить очередь магов
//     //     spellsOrder.Clear();
//     //     // получить список живых магов
//     //     aliveMages = GameManager.instance.mageControllers.FindAll((mage) => !mage.isDead);
//     //     // добавить в очередь магов, у которых готов спелл
//     //     foreach (MageController mage in aliveMages)
//     //     {
//     //         if (mage.spell.IsExecutable()) spellsOrder.Add(mage.spell);
//     //     }
//     //     // найти заклинания состоящие из 1, 2 и 3 карт
//     //     // карты сортируются по инициативе
//     //     AddSortedNCardSpells(oneCardSpells,   1);
//     //     AddSortedNCardSpells(twoCardSpells,   2);
//     //     AddSortedNCardSpells(threeCardSpells, 3);

//     //     // объединить списки найденных карт по порядку от 1-карточных до 3-карточных
//     //     spellsOrder.Clear();
//     //     spellsOrder.AddRange(oneCardSpells);
//     //     spellsOrder.AddRange(twoCardSpells);
//     //     spellsOrder.AddRange(threeCardSpells);

//     //     yield break;
//     // }

//     // public IEnumerator OnExecutionStart()
//     // {
//     //     // установить в current данного врага
//     //     // найти самого хилого врага
//     //     // найти самого живочего
//     //     yield break;
//     // }

//     // // public MageController FindСамогоХилого(....);
//     // // FindСамогоЖивочего(...);

//     // void AddSortedNCardSpells(List<HandSpellController> nCardSpells, int n)
//     // {
//     //     List<HandSpellController> spells = spellsOrder.FindAll( (spell) => spell.GetSpellCount() == n );
//     //     spells.Sort((spell1, spell2) => spell1.GetSpellInitiative().CompareTo(spell2.GetSpellInitiative()) );
//     //     nCardSpells.AddRange(spells);
//     // }

// }
