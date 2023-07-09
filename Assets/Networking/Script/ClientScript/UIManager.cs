using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Fade
{
    In,
    Out,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    public GameObject TitleCanvas;
    public GameObject CharacterCanvas;
    public GameObject OtherPlayerText;

    public Color FadeColor;
    public Image FadeImage;
    public float FadeSpeed = 0.5f;
    public Fade FadeState;

    public Text Character1Name;
    public Text Character2Name;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        FadeState = Fade.Out;
    }

    private void Update()
    {
        switch (FadeState)
        {
            case Fade.In:
                if(FadeColor.a <= 1)
                FadeColor.a += FadeSpeed * Time.deltaTime;
                break;

            case Fade.Out:
                if(FadeColor.a >= 0)
                    FadeColor.a -= FadeSpeed * Time.deltaTime;
                break;
        }

        FadeImage.color = FadeColor;
    }

    public void ConnectToServer()
    {
        Client.Instance.ConnectToServer();
        FadeState = Fade.In;

        StartCoroutine(WaitOtherPlayer());
    }

    public void IsGotoCharacter(bool Check)
    {
        if(Check == true)
        {
            OtherPlayerText.SetActive(false);

            TitleCanvas.SetActive(false);
            CharacterCanvas.SetActive(true);

            FadeState = Fade.Out;
        }
    }

    public void CharacterSelect(int Type)
    {
        ClientSend.CharacterSelect(Type);
    }

    public void SetCharacterName(int Type, string Name)
    {
        switch(Type)
        {
            case 1:
                Character1Name.text = Name;
                break;

            case 2:
                Character2Name.text = Name;
                break;
        }
    }

    public void StartShowScene()
    {
        FadeState = Fade.In;

        StartCoroutine(ChangeStartShowScene());
    }

    private IEnumerator WaitOtherPlayer()
    {
        yield return new WaitForSeconds(2.3f);

        OtherPlayerText.SetActive(true);
        ClientSend.waitOtherPlayer();
        yield break;
    }

    private IEnumerator ChangeStartShowScene()
    {
        yield return new WaitForSeconds(2.3f);

        SceneManager.LoadScene("StartShowScene");
    }
}
