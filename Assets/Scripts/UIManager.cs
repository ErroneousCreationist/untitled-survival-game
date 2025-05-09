using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    [SerializeField] private GameObject pauseMenu;

    private void Awake()
    {
        instance = this;

        instance.SetLobbyCustomisationSliders();
    }

    public static void TogglePauseMenu(bool open)
    {
        instance.pauseMenu.SetActive(open);
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Player.LocalPlayer && Player.LocalPlayer.ph.isAlive.Value)
        {
            TogglePauseMenu(!GetPauseMenuOpen);
        }
        respawnButton.SetActive(!VivoxManager.LeavingChannel);

        if (regiontitlecountdown > 0)
        {
            regiontitlecountdown -= Time.deltaTime;
            if (regiontitlecountdown <= 0)
            {
                if(currDisplayCoroutine != null) { StopCoroutine(currDisplayCoroutine); RegionTitleText.DOKill(); }
                currDisplayCoroutine = StartCoroutine(DisplayRegionTitle());
            }
        }
    }

    public static bool GetPauseMenuOpen => instance.pauseMenu.activeSelf;

    [SerializeField] private GameObject muteIcon;

    public static void SetMuteIcon(bool mute)
    {
        instance.muteIcon.SetActive(mute);
    }

    [SerializeField] private Image VitalsIndicatorImage;
    [SerializeField] private CanvasGroup VitalsIndicatorCanvasGroup;
    [SerializeField] private Sprite ThumbsDown, ThumbsUp;

    public static void ShowVitalsIndication(bool thumbsup)
    {
        instance.VitalsIndicatorImage.sprite = thumbsup ? instance.ThumbsUp : instance.ThumbsDown;
        instance.VitalsIndicatorCanvasGroup.alpha = 0;
        instance.VitalsIndicatorCanvasGroup.DOKill();
        instance.VitalsIndicatorCanvasGroup.DOFade(1, 0.25f).onComplete += () =>
        {
            instance.VitalsIndicatorCanvasGroup.DOFade(0, 1f);
        };
    }

    [SerializeField] private GameObject gameOverScreen, respawnButton;
    [SerializeField] private CanvasGroup deathText;

    public void Respawn()
    {
        GameManager.RespawnPlayer();
        HideGameOverScreen();
    }

    public static void ShowGameOverScreen()
    {
        instance.gameOverScreen.SetActive(true);
        instance.deathText.alpha = 0;
        instance.deathText.DOKill();
        instance.Invoke(nameof(ShowGameoverText), 1f);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowGameoverText()
    {
        instance.deathText.DOFade(1, 10);
    }

    public static void HideGameOverScreen()
    {
        instance.gameOverScreen.SetActive(false);
        instance.deathText.alpha = 0;
        instance.deathText.DOKill();
    }

    //lobby players ui
    [SerializeField] private Image basePlayerIcon;
    [SerializeField] private Sprite[] playerIcons;
    [SerializeField] private Sprite[] specialPlayerIcons;
    [SerializeField] private Sprite ready, notready;
    public static void UpdatePlayerList(Dictionary<ulong, string> conns, List<ulong> readied)
    {
        foreach (Transform child in instance.basePlayerIcon.transform.parent)
        {
            if (child.gameObject.activeSelf) { Destroy(child.gameObject); }
        }

        foreach (var conn in conns)
        {
            var ob = Instantiate(instance.basePlayerIcon, instance.basePlayerIcon.transform.parent);
            ob.gameObject.SetActive(true);
            var username = conn.Value.Contains('\r') ? conn.Value.Split('\r')[0] : conn.Value;
            var specialindex = conn.Value.Contains('\r') ? int.Parse(conn.Value.Split('\r')[1]) : -1;
            ob.GetComponentInChildren<TMP_Text>().text = username;
            ob.sprite = specialindex==-1? instance.playerIcons[Extensions.GetDeterministicStringIndex(username, instance.playerIcons.Length)] : instance.specialPlayerIcons[specialindex];
            ob.transform.GetChild(1).GetComponent<Image>().sprite = readied.Contains(conn.Key) ? instance.ready : instance.notready;
        }
    }

    //lobby and menu
    [SerializeField] private CanvasGroup LobbyUI, GameUI, TransitionScreen;

    public static void ShowLobby()
    {
        instance.LobbyUI.alpha = 1;
        instance.LobbyUI.interactable = true;
        instance.LobbyUI.blocksRaycasts = true;

        instance.GameUI.alpha = 0;
        instance.GameUI.interactable = false;
        instance.GameUI.blocksRaycasts = false;

        instance.TransitionScreen.alpha = 0;
        instance.SetLobbyCustomisationSliders();
    }

    public static void FadeToGame()
    {
        instance.LobbyUI.DOFade(0, 0.1f).onComplete += () => { instance.TransitionScreen.DOFade(0, 1.9f); };
        instance.LobbyUI.interactable = false;
        instance.LobbyUI.blocksRaycasts = false;

        instance.GameUI.alpha = 1;
        instance.GameUI.interactable = true;
        instance.GameUI.blocksRaycasts = true;

        instance.TransitionScreen.alpha = 1;
    }

    public void ReadyupButton()
    {
        GameManager.ReadyUp();
    }

    public void BackToLobby()
    {
        GameManager.BackToLobby();
    }

    [SerializeField] TMP_Text ReadyUpTimerText;
    [SerializeField] TMP_Text ReadyUpButtonText;

    public static void SetReadyUpButtonText(string text)
    {
        instance.ReadyUpButtonText.text = text;
    }

    public static void SetReadyTimerText(string text)
    {
        instance.ReadyUpTimerText.text = text;
    }

    [SerializeField] private TMP_Text RegionTitleText;
    private float regiontitlecountdown = 0;
    private string regiontitle = "";
    private Coroutine currDisplayCoroutine;

    public static void ShowRegionTitle(string title, float countdown = 3)
    {
        instance.regiontitle = title;
        instance.regiontitlecountdown = countdown;
    }

    private IEnumerator DisplayRegionTitle()
    {
        RegionTitleText.text = "";
        for (int i = 0; i < regiontitle.Length; i++)
        {
            RegionTitleText.text += regiontitle[i];
            yield return new WaitForSeconds(0.075f);
        }
        yield return new WaitForSeconds(10);
        RegionTitleText.DOFade(0, 1);
    }

    [SerializeField] private Image HeadDamageIndicator, BodyDamageIndicator, FeetDamageIndicator;

    public static void SetHeadDamage(float amount)
    {
        instance.HeadDamageIndicator.color = amount <= 0 ? Color.Lerp(Color.red, Color.black, Mathf.Abs(amount / 0.25f)) : Color.Lerp(Color.white, Color.red, 1 - amount);
    }
    public static void SetBodyDamage(float amount)
    {
        instance.BodyDamageIndicator.color = amount <= 0 ? Color.Lerp(Color.red, Color.black, Mathf.Abs(amount / 0.9f)) : Color.Lerp(Color.white, Color.red, 1 - amount);
    }

    public static void SetFeetDamage(float amount)
    {
        instance.FeetDamageIndicator.color = amount <= 0 ? Color.Lerp(Color.black, Color.red, amount+1) : Color.Lerp(Color.white, Color.red, 1 - amount);
    }

    [SerializeField] private Slider LobbyScarfR, LobbyScarfG, LobbyScarfB, LobbySkinTone;
    [SerializeField] private Image LobbyColIndicator, LobbySkinIndicator;
    [SerializeField] private List<Sprite> LobbySkinTones;

    private void SetLobbyCustomisationSliders()
    {
        LobbyScarfR.value = PlayerPrefs.GetFloat("SCARFCOL_R", 1);
        LobbyScarfG.value = PlayerPrefs.GetFloat("SCARFCOL_G", 0);
        LobbyScarfB.value = PlayerPrefs.GetFloat("SCARFCOL_B", 0);
        LobbySkinTone.value = PlayerPrefs.GetInt("SKINTEX", 0);
        LobbySkinIndicator.sprite = LobbySkinTones[PlayerPrefs.GetInt("SKINTEX", 0)];
        LobbyColIndicator.color = new Color(PlayerPrefs.GetFloat("SCARFCOL_R", 1), PlayerPrefs.GetFloat("SCARFCOL_G", 0), PlayerPrefs.GetFloat("SCARFCOL_B", 0));
    }

    public void SetScarfR(float value)
    {
        PlayerPrefs.SetFloat("SCARFCOL_R", value);
        LobbyColIndicator.color = new Color(PlayerPrefs.GetFloat("SCARFCOL_R", 1), PlayerPrefs.GetFloat("SCARFCOL_G", 0), PlayerPrefs.GetFloat("SCARFCOL_B", 0));
    }

    public void SetScarfG(float value)
    {
        PlayerPrefs.SetFloat("SCARFCOL_G", value);
        LobbyColIndicator.color = new Color(PlayerPrefs.GetFloat("SCARFCOL_R", 1), PlayerPrefs.GetFloat("SCARFCOL_G", 0), PlayerPrefs.GetFloat("SCARFCOL_B", 0));
    }

    public void SetScarfB(float value)
    {
        PlayerPrefs.SetFloat("SCARFCOL_B", value);
        LobbyColIndicator.color = new Color(PlayerPrefs.GetFloat("SCARFCOL_R", 1), PlayerPrefs.GetFloat("SCARFCOL_G", 0), PlayerPrefs.GetFloat("SCARFCOL_B", 0));
    }

    public void SetSkinTex(float value)
    {
        PlayerPrefs.SetInt("SKINTEX", (int)value);
        LobbySkinIndicator.sprite = LobbySkinTones[PlayerPrefs.GetInt("SKINTEX", 0)];
    }
}