using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialScriptManager : MonoBehaviour
{

    public GameObject[] popUps;
    private static bool startedTutorial = false;
    private static bool didTutorial = false;
    private int popUpIndex = 0;
    private void Start()
    {
        if (didTutorial || startedTutorial)
        {
            didTutorial = true;
            Destroy(gameObject);
        }
        startedTutorial = true;
    }

    // Update is called once per frame
    void Update()
    {   
        for(int i = 0; i < popUps.Length; i++){
            if(i == popUpIndex){
                popUps[i].SetActive(true);
            }
            else {
                popUps[i].SetActive(false);
            }
        }

        if(Keyboard.current.rightArrowKey.wasPressedThisFrame){
            popUpIndex++;
        }

        if(Keyboard.current.leftArrowKey.wasPressedThisFrame){
            if(popUpIndex > 0){
                popUpIndex--;
            }
        }

        if(popUpIndex >= popUps.Length){
            didTutorial = true;
            Destroy(gameObject);
        }
    }
}
