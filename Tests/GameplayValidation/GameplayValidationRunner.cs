using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using Object = UnityEngine.Object;

public class GameplayValidationRunner : MonoBehaviour
{
    private static readonly List<string> results = new List<string>();
    private static string error;
    private bool nicknameLoadedCovered;
    private bool caveLoadedCovered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || SessionState.GetBool("FirstPuzzleValidation", false) ||
            SessionState.GetBool("CrystalPuzzleValidation", false))
            return;
        var go = new GameObject("Validation Runner");
        DontDestroyOnLoad(go);
        go.AddComponent<GameplayValidationRunner>();
    }

    private void Start()
    {
        Application.logMessageReceived += OnLog;
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(Guard(Tests()));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // sceneLoaded ocorre após Awake e antes de Start, independente da duração do frame batch.
        if (scene.name == "MainMenu")
            nicknameLoadedCovered = Find<MainMenuController>().IsTransitioning &&
                Named("Fade Shade").GetComponent<Image>().color.a == 1f;
        if (scene.name == "Cave" && SceneFade.IsRunning)
            caveLoadedCovered = Named("Scene Fade Shade").GetComponent<Image>().color.a == 1f;
    }

    private void OnLog(string message, string trace, LogType type)
    {
        // Unity 6000.6 pode falhar ao iniciar o índice de busca em um projeto batch novo.
        // Registrar a falha do Editor sem ocultar exceções dos scripts do jogo.
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchDatabase") &&
            !trace.Contains("Assets/Scripts"))
        {
            results.Add("EDITOR NOTE: startup search index exception: " + message);
            return;
        }
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            error = message + "\n" + trace;
    }

    private IEnumerator Guard(IEnumerator test)
    {
        while (true)
        {
            bool more;
            object current = null;
            try
            {
                more = test.MoveNext();
                if (more) current = test.Current;
                if (error != null) throw new Exception(error);
            }
            catch (Exception ex)
            {
                File.WriteAllText("validation-results.txt", string.Join("\n", results) + "\nFAIL: " + ex);
                EditorApplication.Exit(1);
                yield break;
            }
            if (!more) break;
            yield return current;
        }
        File.WriteAllText("validation-results.txt", string.Join("\n", results) + "\nALL PASSED");
        EditorApplication.Exit(0);
    }

    private static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();
    private static GameObject Named(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (go.name == name) return go;
        throw new Exception("Missing object " + name);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        results.Add("PASS: " + message);
    }
    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static void NewMission()
    {
        GameManager.Instance.ResetMission();
        GameManager.Instance.StartMission();
    }

    private IEnumerator Tests()
    {
        yield return null;
        yield return null;
        Check(SceneManager.GetActiveScene().name == "Splash" && Find<MainMenuController>() == null,
            "Splash starts without nickname or gameplay");
        var settings = SettingsManager.Instance;
        Check(settings != null, "Settings bootstrap exists in Splash");
        Check(Find<SplashParticles>() != null && !Find<SplashParticles>().raycastTarget,
            "Splash has Canvas particles without blocking input");
        float clickStarted = Time.realtimeSinceStartup;
        Find<SplashController>().ContinueToMenu();
        Find<SplashController>().ContinueToMenu();
        Check(SceneManager.GetActiveScene().name == "Splash", "Click waits for animation before changing scenes");
        while (SceneManager.GetActiveScene().name != "MainMenu") yield return null;
        Check(Time.realtimeSinceStartup - clickStarted >= 0.8f, "Splash click animation completes before MainMenu");
        yield return null;
        var menu = Find<MainMenuController>();
        Check(menu.nicknamePanel.activeSelf && !menu.mainMenuPanel.activeSelf, "Splash leads to nickname");
        Check(menu.transitionVideoPlayer.clip != null && menu.transitionVideoPlayer.targetTexture != null,
            "Original video and RenderTexture remain assigned");
        var shade = Named("Fade Shade").GetComponent<Image>();
        Check(nicknameLoadedCovered, "Nickname starts covered by fade-in");
        Time.timeScale = 0f;
        while (menu.IsTransitioning) yield return null;
        Check(shade.color.a == 0f && !shade.raycastTarget, "Nickname fade completes while time is paused and releases input");
        string savedNickname = PlayerPrefs.GetString("PlayerNickname", "");
        menu.nicknameInput.text = "Unsaved nickname";
        Named("CancelButton").GetComponent<Button>().onClick.Invoke();
        menu.CancelarNickname();
        Check(menu.IsTransitioning && SceneFade.IsRunning, "Cancel button starts one transition to Splash");
        while (SceneFade.IsRunning) yield return null;
        Check(SceneManager.GetActiveScene().name == "Splash" && Find<SplashController>() != null,
            "Cancel nickname returns to Splash");
        Check(PlayerPrefs.GetString("PlayerNickname", "") == savedNickname,
            "Cancel does not save unfinished nickname");
        Find<SplashController>().ContinueToMenu();
        while (SceneManager.GetActiveScene().name != "MainMenu") yield return null;
        menu = Find<MainMenuController>();
        while (menu.IsTransitioning) yield return null;
        shade = Named("Fade Shade").GetComponent<Image>();
        Check(menu.nicknamePanel.activeSelf && !menu.mainMenuPanel.activeSelf,
            "Splash can reopen nickname after cancelling");
        menu.nicknameInput.text = "Validation Player";
        menu.ConfirmarNickname();
        Check(menu.nicknamePanel.activeSelf && !menu.transitionPanel.activeSelf && menu.IsTransitioning,
            "Continue keeps nickname visible during fade-out");
        menu.ConfirmarNickname();
        menu.PlayGame();
        Check(SceneManager.GetActiveScene().name == "MainMenu" && !menu.mainMenuPanel.activeSelf,
            "Repeated confirmation and Play cannot bypass intro");
        while (!menu.transitionPanel.activeSelf) yield return null;
        Check(!menu.nicknamePanel.activeSelf && shade.color.a == 1f,
            "Video panel opens only after nickname is fully covered");
        // Sem GPU, simula o término e interrompe a preparação antes da decodificação.
        menu.transitionVideoPlayer.Stop();
        typeof(MainMenuController).GetMethod("OnTransitionVideoFinished", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(menu, new object[] { menu.transitionVideoPlayer });
        while (menu.IsTransitioning) yield return null;
        Time.timeScale = 1f;
        Check(menu.mainMenuPanel.activeSelf && !menu.nicknamePanel.activeSelf && !menu.transitionPanel.activeSelf,
            "Intro completion opens menu without starting gameplay");
        Check(shade.color.a == 0f && menu.mainMenuPanel.GetComponent<CanvasGroup>().interactable,
            "Menu fade-in finishes and restores interaction");
        Named("CreditsButton").GetComponent<Button>().onClick.Invoke();
        while (SceneFade.IsRunning) yield return null;
        Check(SceneManager.GetActiveScene().name == "Credits" && Find<CreditsController>() != null,
            "Credits button loads saved credits scene");
        var credits = Find<CreditsController>();
        var creditText = Named("CreditsText").GetComponent<TMP_Text>();
        Check(!string.IsNullOrWhiteSpace(creditText.text), "Credits retains editable nonempty text");
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        float creditsY = creditText.rectTransform.anchoredPosition.y;
        yield return new WaitForSecondsRealtime(0.2f);
        Check(creditText.rectTransform.anchoredPosition.y > creditsY, "Credits scroll upward with paused game time");
        Named("BackButton").GetComponent<Button>().onClick.Invoke();
        while (SceneFade.IsRunning) yield return null;
        menu = Find<MainMenuController>();
        while (menu.IsTransitioning) yield return null;
        Check(menu.mainMenuPanel.activeSelf && !menu.nicknamePanel.activeSelf && !menu.transitionPanel.activeSelf,
            "Back from credits restores menu without nickname or intro");
        menu.OpenCredits();
        while (SceneFade.IsRunning) yield return null;
        credits = Find<CreditsController>();
        Set(credits, "scrollSpeed", 100000f);
        Set(credits, "finalDelay", 0f);
        float creditsDeadline = Time.realtimeSinceStartup + 8f;
        while (SceneManager.GetActiveScene().name == "Credits" && Time.realtimeSinceStartup < creditsDeadline)
            yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenu", "Credits return automatically after final line");
        while (SceneFade.IsRunning) yield return null;
        menu = Find<MainMenuController>();
        while (menu.IsTransitioning) yield return null;
        Time.timeScale = 1f;
        var settingsUI = Find<SettingsMenuUI>();
        Check(Named("SettingsPanel").scene == SceneManager.GetActiveScene(), "Settings panel belongs to MainMenu scene");
        Named("SettingsButton").GetComponent<Button>().onClick.Invoke();
        Check(menu.IsTransitioning && menu.mainMenuPanel.activeSelf && !settingsUI.IsOpen,
            "Settings opening fades out menu before switching panels");
        settingsUI.Open(); settingsUI.Close();
        Time.timeScale = 0f;
        while (settingsUI.IsTransitioning) yield return null;
        Time.timeScale = 1f;
        Check(settingsUI.IsOpen && !menu.mainMenuPanel.activeSelf, "Settings button opens panel");
        Check(Named("SettingsPanel").GetComponentsInChildren<Graphic>(true).Length > 0,
            "Settings retains editable scene graphics regardless of object names");
        Named("Volume Geral").GetComponent<Slider>().value = 0.6f;
        Named("Música").GetComponent<Slider>().value = 0.4f;
        Named("Efeitos Sonoros").GetComponent<Slider>().value = 0.2f;
        Check(Mathf.Approximately(AudioListener.volume, 0.6f) && Mathf.Approximately(settings.MusicVolume, 0.4f)
            && Mathf.Approximately(settings.SFXVolume, 0.2f), "Audio sliders apply centralized volumes");
        Check(Mathf.Approximately(menu.transitionVideoPlayer.GetDirectAudioVolume(0), 0.24f),
            "Direct video audio respects master and music");
        var testAudio = new GameObject("TestAudio");
        var source = testAudio.AddComponent<AudioSource>();
        source.volume = 0.8f;
        var channel = testAudio.AddComponent<SettingsAudioChannel>();
        Check(Mathf.Approximately(source.volume, 0.32f), "Future music AudioSource respects category");
        channel.enabled = false;
        Set(channel, "category", SettingsManager.AudioCategory.SoundEffects);
        channel.enabled = true;
        Check(Mathf.Approximately(source.volume, 0.16f), "Future SFX AudioSource respects category");
        Object.Destroy(testAudio);
        settingsUI.Close();
        Check(settingsUI.IsTransitioning && settingsUI.IsOpen, "Settings closing keeps panel visible during fade-out");
        settingsUI.Close();
        while (settingsUI.IsTransitioning) yield return null;
        Check(!settingsUI.IsOpen && menu.mainMenuPanel.activeSelf && SceneManager.GetActiveScene().name == "MainMenu",
            "Back returns to same menu without reloading");
        Check(Mathf.Approximately(PlayerPrefs.GetFloat("MasterVolume"), 0.6f) &&
            Mathf.Approximately(PlayerPrefs.GetFloat("MusicVolume"), 0.4f), "Audio preferences saved");
        var sizes = settings.Resolutions;
        Check(sizes.Count > 0 && sizes[settings.ResolutionIndex].x > 0, "Resolution choices populated from display/current window");
        settings.ApplyVideo(settings.ResolutionIndex, false, QualitySettings.GetQualityLevel());
        Check(PlayerPrefs.GetInt("Fullscreen", 1) == 0 && PlayerPrefs.HasKey("QualityLevel") &&
            PlayerPrefs.GetInt("ResolutionWidth") > 0, "Video preferences applied and persisted");
        Object.Destroy(settings.gameObject);
        yield return null;
        settings = new GameObject("SettingsReloadTest").AddComponent<SettingsManager>();
        Check(Mathf.Approximately(settings.MasterVolume, 0.6f) && Mathf.Approximately(settings.MusicVolume, 0.4f)
            && Mathf.Approximately(settings.SFXVolume, 0.2f) && !settings.Fullscreen, "Settings reload stored preferences");
        menu.PlayGame();
        Check(SceneFade.IsRunning && menu.mainMenuPanel.activeSelf && menu.IsTransitioning,
            "Play keeps menu visible while quick fade blocks repeated input");
        Check(!SceneFade.TryLoad("Island"), "Concurrent scene transitions are rejected");
        Time.timeScale = 0f;
        while (SceneManager.GetActiveScene().name != "Cave") yield return null;
        Check(caveLoadedCovered,
            "Cave loads under opaque cover");
        while (SceneFade.IsRunning) yield return null;
        Time.timeScale = 1f;
        Check(Find<SceneFade>() == null && Find<PlayerClickMovement>().IsMovementEnabled,
            "Quick fade completes while paused and releases destination controls");
        Check(SceneManager.GetActiveScene().name == "Cave", "Play enters Cave after intro and menu");
        Check(Find<MiningSpot>() == null && Find<MiningUI>() == null, "Cave has no mining components");
        Check(SettingsManager.Instance == settings, "Settings survive scene change");
        var gm = GameManager.Instance;
        Check(gm != null && !gm.HasMissionStarted, "Cave starts with inactive story mission");
        CheckRetiredUI();
        gm.CompleteMission();
        Check(!gm.IsGameWon(), "Cannot finish before starting story");
        var pause = Find<PauseController>();
        pause.HandleEscape();
        Check(gm.IsPaused && Time.timeScale == 0f && !Find<PlayerClickMovement>().IsMovementEnabled,
            "Pause still blocks movement before mission");
        pause.OpenSettings();
        Check(Find<SettingsMenuUI>().IsOpen && gm.IsPaused, "Pause settings remain functional");
        pause.HandleEscape();
        pause.Resume();
        Check(!gm.IsPaused && Time.timeScale == 1f, "Resume restores gameplay");

        yield return SceneManager.LoadSceneAsync("Island");
        yield return null;
        CheckRetiredUI();
        Check(gm == GameManager.Instance && Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length == 1,
            "Single persistent GameManager survives scene change");
        Check(Find<MiningSpot>() != null && Find<MiningUI>() != null && !Find<MiningSpot>().CanMine,
            "Crystal mining is unavailable before first memory");
        var dialogue = Find<DialogueController>();
        var lines = (string[])typeof(DialogueController).GetField("dialogueLines", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialogue);
        dialogue.StartDialogue();
        for (int i = 0; i < lines.Length - 1; i++) dialogue.NextLine();
        Check(!gm.HasMissionStarted, "Story waits for last dialogue line");
        dialogue.NextLine();
        Check(gm.IsMissionActive() && Find<PlayerClickMovement>().IsMovementEnabled,
            "Dialogue starts story without currency manager and releases movement");
        int notifications = 0;
        Action onState = () => notifications++;
        gm.OnMissionStateChanged += onState;
        gm.StartMission();
        Check(notifications == 0, "Repeated start is ignored");
        gm.OnMissionStateChanged -= onState;
        Check(typeof(GameManager).GetField("currentTime", BindingFlags.Instance | BindingFlags.NonPublic) == null &&
            typeof(GameManager).GetMethod("GetRemainingTime") == null, "Mission has no countdown");
        yield return new WaitForSeconds(0.25f);
        Check(gm.IsMissionActive() && !gm.IsGameWon(), "Exploration does not automatically end mission");
        pause = Find<PauseController>();
        pause.HandleEscape();
        gm.CompleteMission();
        Check(!gm.IsGameWon(), "Final objective cannot finish while paused");
        pause.Resume();

        // Crystal excavation becomes available after the first memory.
        gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        Named("MiningSpot").SetActive(true);
        yield return null;
        var spot = Find<MiningSpot>();
        var player = Find<PlayerClickMovement>();
        player.transform.position = spot.transform.position;
        var completed = (UnityEngine.Events.UnityEvent)typeof(MiningSpot).GetField("onMiningCompleted",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spot);
        int completions = 0;
        completed.AddListener(() => completions++);
        spot.StartMining();
        spot.StartMining();
        Check(spot.IsMining && !player.IsMovementEnabled, "Excavation still prevents parallel attempts and movement");
        yield return new WaitForSeconds(1f);
        Check(spot.Progress > 0f && spot.Progress < 1f && completions == 0, "Excavation progresses before completion");
        pause.HandleEscape();
        float progress = spot.Progress;
        yield return new WaitForSecondsRealtime(0.2f);
        Check(spot.Progress == progress, "Pause freezes excavation progress");
        pause.Resume();
        yield return new WaitForSeconds(2.2f);
        Check(completions == 1 && !spot.IsMining && player.IsMovementEnabled && !gm.IsGameWon(),
            "Excavation completes once without currency or victory");
        spot.StartMining();
        yield return SceneManager.LoadSceneAsync("Cave");
        yield return null;
        Check(completions == 1 && gm.IsMissionActive() && Find<PlayerClickMovement>().IsMovementEnabled,
            "Scene change cancels excavation and preserves story state");
        CheckRetiredUI();
        gm.CompleteMission();
        yield return null;
        Check(gm.IsGameWon() && !gm.IsMissionActive() && Named("MissionResultPanel").activeSelf,
            "Explicit final objective shows completion panel");
        Check(Named("EndGameMessage").GetComponent<TMP_Text>().text == "Você concluiu sua jornada pela ilha.",
            "Completion feedback contains no currency or deadline");
        Named("RestartButton").GetComponent<Button>().onClick.Invoke();
        while (SceneFade.IsRunning) yield return null;
        Check(!gm.HasMissionStarted && !gm.IsGameWon() && !gm.IsPaused && Time.timeScale == 1f,
            "Restart resets story and pause state");
        CheckRetiredUI();
        NewMission();
        pause = Find<PauseController>();
        pause.HandleEscape();
        pause.MainMenu();
        while (SceneFade.IsRunning) yield return null;
        menu = Find<MainMenuController>();
        while (menu.IsTransitioning) yield return null;
        Check(menu.mainMenuPanel.activeSelf && !menu.nicknamePanel.activeSelf && !gm.HasMissionStarted &&
            !gm.IsPaused && Time.timeScale == 1f, "Return to main menu resets session correctly");
    }

    private static void CheckRetiredUI()
    {
        var retired = new HashSet<string> { "MoneyManager", "MoneyUI", "MoneyHUD", "MoneyText", "TimerUI", "TimerText" };
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (retired.Contains(go.name)) throw new Exception("Retired object still exists: " + go.name);
            foreach (var component in go.GetComponents<Component>())
                if (component == null) throw new Exception("Missing script on " + go.name);
        }
        Check(true, "Scene has no currency/countdown objects or missing scripts");
    }
}
