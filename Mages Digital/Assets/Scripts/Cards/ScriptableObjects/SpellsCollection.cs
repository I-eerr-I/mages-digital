using UnityEngine;

[CreateAssetMenuAttribute(menuName="Spells Collection")]
public class SpellsCollection : ScriptableObject
{


    public void Damage() // (цель, сколько)
    {
        
    }

    public void DiceRoll()
    {
        
    }

    public void CountSpells()
    {

    }

    public void MightyRoll()
    {
        // int dice = посчитать количество знаков + сокровища (которые сами знаки или добавляют кубик)
        // бросить кубики
        // найти сумму
    }

    public void Switch() // (List<delegate> asd)
    {
        // result один - asd[0]();
        // результат второй - asd[1]();
    }

    public void FindTargets() // Predicate 
    {
        // ищет врагов
    }

    public void DrakoniySunduk()
    {
        // список спелов = {(s) => Damage(s), (n) => Damage(n), ...}
        // результат броска = MightyRoll
        // найти каждого врага( предикат без сокровищ )
        // Switch(результат броска, врагов, список спелов)
    }
        
}
