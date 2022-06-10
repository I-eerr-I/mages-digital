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

    [Header("Информация о бонусных картах")]
    public GameObject showingBonusPanel;
    public Image showingBonusLeftImage;
    public Image showingBonusCenterImage;
    public Image showingBonusRightImage;

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

    public void ShowBonusInfo(List<CardController> threeBonusCards, bool show = true)
    {
        showingBonusPanel.SetActive(show);
        if (show)
        {
            showingBonusLeftImage.enabled   = threeBonusCards[0] != null;
            showingBonusLeftImage.sprite    = threeBonusCards[0].card.front;

            showingBonusCenterImage.enabled = threeBonusCards[1] != null;
            showingBonusCenterImage.sprite  = threeBonusCards[1].card.front;

            showingBonusRightImage.enabled  = threeBonusCards[2] != null;
            showingBonusRightImage.sprite   = threeBonusCards[2].card.front;
        }
    }
}
