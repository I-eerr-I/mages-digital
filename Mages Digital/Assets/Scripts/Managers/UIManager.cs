using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    private static UIManager _instance;
    public  static UIManager  instance => _instance;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
    public Button showingBonusDropButton;
    public Button showingBonusCancleButton;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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


    public void ShowBonusInfo(List<CardController> threeBonusCards, 
        bool show = true, 
        bool withDropButton = false,
        string dropButtonText = "Сбросить",
        bool withCancleButton = false, 
        MageController choosingMage = null)
    {
        showingBonusPanel.SetActive(show);
        if (show)
        {
            SetupShowingBonusImage(threeBonusCards, showingBonusLeftImage,   2);
            SetupShowingBonusImage(threeBonusCards, showingBonusCenterImage, 0);
            SetupShowingBonusImage(threeBonusCards, showingBonusRightImage,  1);

            if (withDropButton)
            {
                showingBonusDropButton.gameObject.SetActive(true);
                showingBonusDropButton.gameObject.GetComponentInChildren<TMP_Text>().text = dropButtonText;
                showingBonusDropButton.onClick.AddListener(() => 
                {
                    if (threeBonusCards[0] != null)
                    {
                        choosingMage.chosenCard = threeBonusCards[0];
                        GameManager.instance.StopChoosing();
                    }
                });
            }
            if (withCancleButton)
            {
                showingBonusCancleButton.gameObject.SetActive(true);
                showingBonusCancleButton.onClick.AddListener(() => 
                {
                    GameManager.instance.StopChoosing();
                });
            }
        }
        else
        {
            showingBonusCancleButton.gameObject.SetActive(false);
            showingBonusDropButton.gameObject.SetActive(false);
        }
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
    void SetupShowingBonusImage(List<CardController> bonusCards, Image bonusImage, int index)
    {
        bool isNotNull     = bonusCards[index] != null;
        bonusImage.enabled = isNotNull;
        if (isNotNull)
            bonusImage.sprite = bonusCards[index].card.front;
    }


}
