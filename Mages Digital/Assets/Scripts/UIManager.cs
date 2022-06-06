using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public  static UIManager  instance => _instance;

    [Header("Showing card info")]
    public GameObject showingCardPanel;
    public Image showingCard;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    public void ShowCardInfo(Sprite cardInfo, bool show = true)
    {
        showingCardPanel.SetActive(show);
        if (show)
            showingCard.sprite = cardInfo;
    }
}
