using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ErrorManager : MonoBehaviour
{
    public GameObject errorBox;  
    public Text errorMessage;       

    private void Start()
    {
        errorBox.SetActive(false); 
    }

    public void ShowError(string message)
    {
        errorMessage.text = message;  
        errorBox.SetActive(true); 
        StartCoroutine(HideErrorAfterDelay(3f)); 
    }

    private IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorBox.SetActive(false); 
    }
}
