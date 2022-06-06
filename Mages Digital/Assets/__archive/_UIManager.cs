// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class UIManager : MonoBehaviour
// {
//     // private static UIManager _instance;
//     // public static UIManager instance => _instance;

//     // private List<Animator> mageChoiceButtonAnimators = new List<Animator>();
    
//     // private Text infoText;

//     // [Header("Mage Choice Menu")]
//     // public GameObject mageChoiceMenu;

//     // [Header("Info Panel")]
//     // public GameObject info;

//     // [Header("Player UI")]
//     // public GameObject playerUI;



//     // void Awake()
//     // {
//     //     if (_instance == null) _instance = this;
//     //     else Destroy(gameObject);
//     // }

//     // void Start()
//     // {
//     //     infoText = info.GetComponent<Text>();
//     // }

//     // // создает кнопки для выбора мага
//     // public IEnumerator CreateMageChoiceMenuButtons()
//     // {
//     //     GameObject mageChoiceContainer = mageChoiceMenu.transform.GetChild(0).gameObject;
//     //     GameObject mageChoiceButton    = mageChoiceContainer.transform.GetChild(0).gameObject;
        
//     //     foreach (Mage mage in GameManager.instance.mages)
//     //     {
//     //         GameObject newMageChoiceButton = Instantiate(mageChoiceButton, mageChoiceContainer.transform);
//     //         newMageChoiceButton.SetActive(true);
//     //         Button button = newMageChoiceButton.GetComponent<Button>();
//     //         mageChoiceButtonAnimators.Add(button.GetComponent<Animator>());
//     //         Text   text   = newMageChoiceButton.GetComponentInChildren<Text>();
//     //         text.text     = mage.mageName;
//     //         button.onClick.AddListener(() => OnMageChoiceButtonClick(mage));
//     //     }
//     //     foreach(Animator animator in mageChoiceButtonAnimators)
//     //     {
//     //         animator.enabled = true;
//     //         yield return new WaitForSeconds(0.5f);
//     //     }
//     // }

//     // public void OnMageChoiceButtonClick(Mage mage)
//     // {
//     //     GameManager.instance.SetPlayerMage(mage);
//     //     GameManager.instance.TransitionToState(new RoundStartState());
//     // }

//     // public IEnumerator FadeInMagesChoiceMenu()
//     // {
//     //     mageChoiceMenu.SetActive(true);
//     //     yield return new WaitForSeconds(1.0f);
//     // }


//     // public IEnumerator FadeOutMagesChoiceMenu()
//     // {
//     //     yield return FadeOutButtons();
//     //     mageChoiceMenu.GetComponent<Animator>().SetBool("IsFadeOut", true);
//     //     yield return new WaitForSeconds(1.0f);
//     //     mageChoiceMenu.SetActive(false);
//     // }

//     // public IEnumerator FadeOutButtons()
//     // {
//     //     for (int i = mageChoiceButtonAnimators.Count - 1; i >= 0; i--)
//     //     {
//     //         mageChoiceButtonAnimators[i].SetBool("IsFadeOut", true);
//     //         yield return new WaitForSeconds(0.5f);
//     //     }
//     // }

//     // public IEnumerator FadeInAndOutInfoText(string text)
//     // {
//     //     info.SetActive(true);
//     //     infoText.text = text;
//     //     yield return new WaitForSeconds(3.0f);
//     //     info.SetActive(false);
//     // }

// }
