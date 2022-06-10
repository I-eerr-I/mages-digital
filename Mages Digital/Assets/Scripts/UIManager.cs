using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public  static UIManager  instance => _instance;

    [Header("Информация о карте")]
    public GameObject showingCardPanel;
    public Image showingCard;

    [Header("Информация о маге")]
    public GameObject showingMagePanel;
    public ShowMagePanelController showingMagePanelController;
    public Image showingMage;
    public TMP_Text showingMageHealthText;
    public TMP_Text showingMageMedalsText;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    public void ShowCardInfo(CardController card, bool show = true)
    {
        showingCardPanel.SetActive(show);
        if (show)
            showingCard.sprite = card.card.front;
    }

    public void ShowMageInfo(MageController mage, bool show = true)
    {
        showingMagePanel.SetActive(show);
        if (show)
        {
            showingMage.sprite = mage.mage.front;
            showingMageHealthText.text = $"Здоровье: {mage.health}";
            showingMageMedalsText.text = $"Побед: {mage.medals}";
        }
    }
}
