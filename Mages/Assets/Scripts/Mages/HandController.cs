using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private MageController _mageController;
    public MageController mageController
    {
        get => _mageController;
    }

    private HandSpellController _spellController;
    public  HandSpellController spellController
    {
        get => _spellController;
    }

    private List<CardController> sources;
    private List<CardController> qualities;
    private List<CardController> deliveries;
    private List<CardController> wildMagics;
    private List<CardController> treasures;
    private List<CardController> deads;
     
}
