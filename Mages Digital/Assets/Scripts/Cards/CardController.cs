using System.Collections;
using System.Collections.Generic;
using CardsToolKit;
using UnityEngine;

public class CardController : MonoBehaviour
{

    [SerializeField] private Card _card;
    [SerializeField] private MageController _owner;

    public Card           card  => _card;
    public MageController owner => _owner;

}
