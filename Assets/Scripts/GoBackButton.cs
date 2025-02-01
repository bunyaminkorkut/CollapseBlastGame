using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoBackButton : MonoBehaviour
{
    public Button goBackButton;

    void Start()
    {
        goBackButton.onClick.AddListener(OnGoBackButtonClick);
    }

    void OnGoBackButtonClick()
    {
        SceneManager.LoadScene(0);
    }
}
