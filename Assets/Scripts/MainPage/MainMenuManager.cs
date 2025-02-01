using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public Button startButton;
    public ErrorManager errorManager;


    private int M, N, K, A, B, C;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClick);
    }

    void OnStartButtonClick()
    {
        try
        {
            M = int.Parse(GameObject.FindWithTag("M").GetComponent<TMP_InputField>().text);
            N = int.Parse(GameObject.FindWithTag("N").GetComponent<TMP_InputField>().text);
            K = int.Parse(GameObject.FindWithTag("K").GetComponent<TMP_InputField>().text);
            A = int.Parse(GameObject.FindWithTag("A").GetComponent<TMP_InputField>().text);
            B = int.Parse(GameObject.FindWithTag("B").GetComponent<TMP_InputField>().text);
            C = int.Parse(GameObject.FindWithTag("C").GetComponent<TMP_InputField>().text);

            Debug.Log("M: " + M + " N: " + N + " K: " + K + " A: " + A + " B: " + B + " C: " + C);
        }
        catch
        {
            errorManager.ShowError("Please enter valid numbers!");
            return;
        }
        if (M < 2 || M > 10) { errorManager.ShowError("M must be between 2 and 10!"); return; }
        if (N < 2 || N > 10) { errorManager.ShowError("N must be between 2 and 10!"); return; }
        if (K < 1 || K > 6) { errorManager.ShowError("K must be between 1 and 6!"); return; }
        if(A < 2 || A > 10) { errorManager.ShowError("A must be between 2 and 10!"); return; }
        if (B < 3 || B > 10) { errorManager.ShowError("B must be between 3 and 10!"); return; }
        if (C < 4 || C > 10) { errorManager.ShowError("C must be between 4 and 10!"); return; }


        if (A >= B) { errorManager.ShowError("A must be smaller than B!"); return; }
        if (B >= C) { errorManager.ShowError("B must be smaller than C!"); return; }


        PlayerPrefs.SetInt("M", M);
        PlayerPrefs.SetInt("N", N);
        PlayerPrefs.SetInt("K", K);
        PlayerPrefs.SetInt("A", A);
        PlayerPrefs.SetInt("B", B);
        PlayerPrefs.SetInt("C", C);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }




}
