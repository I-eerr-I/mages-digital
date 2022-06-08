using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_scripts : MonoBehaviour
{

    public TestMageController Mage1;
    public TestMageController Mage2;
    public TestMageController Mage3;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.X))
        {
            foreach(TestCardController card in Mage1.spell)
            {
                if(card == null)
                {
                    continue;
                }
                // StartCoroutine(card.card.spell);
                card.ExecuteSpell();
            }
            
        }
        
    }
}
