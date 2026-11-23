using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class GameplayVideoValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("GameplayVideoValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class GameplayVideoValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;
    private bool restoreName, hadName;
    private string oldName;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("GameplayVideoValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<GameplayVideoValidationRunner>();
    }

    private void Start()
    {
        Application.logMessageReceived += OnLog;
        StartCoroutine(Guard(Tests()));
    }

    private void OnLog(string message, string trace, LogType type)
    {
        // Falha conhecida do índice de busca do Editor em cópias batch recém-criadas.
        // Registra a ocorrência sem ocultar exceções originadas no gameplay.
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchDatabase") &&
            !trace.Contains("Assets/Scripts"))
        {
            results.Add("EDITOR NOTE: search index startup exception: " + message);
            return;
        }
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            runtimeError = message + "\n" + trace;
    }

    private IEnumerator Guard(IEnumerator test)
    {
        while (true)
        {
            object current = null;
            bool more;
            try
            {
                more = test.MoveNext();
                if (runtimeError != null) throw new Exception(runtimeError);
                if (more) current = test.Current;
            }
            catch (Exception ex)
            {
                results.Add("FAIL: " + ex);
                Finish(1);
                yield break;
            }
            if (!more) break;
            yield return current;
        }
        results.Add("ALL PASSED");
        Finish(0);
    }

    private void Finish(int code)
    {
        if(restoreName){if(hadName)PlayerPrefs.SetString("PlayerNickname",oldName);else PlayerPrefs.DeleteKey("PlayerNickname");PlayerPrefs.Save();}
        File.WriteAllLines("gameplay-video-results.txt", results);
        SessionState.SetBool("GameplayVideoValidation", false);
        EditorApplication.Exit(code);
    }

    private void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        results.Add("PASS: " + message);
    }

    private static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
    private static GameObject Named(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            if (go.name == name) return go;
        throw new Exception("Missing object: " + name);
    }
    private static Button Button(string name) => Named(name).GetComponent<Button>();
    private static void Click(string name) => Button(name).onClick.Invoke();

    private void CheckClickable(string name)
    {
        var button = Button(name);
        Canvas.ForceUpdateCanvases();
        var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        pointer.position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
        var hits = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && (hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform)),
            "Pointer reaches " + name);
    }

    private static void CaptureWorld(string filename)
    {
        var camera = Camera.main;
        GameObject captureCamera = null;
        if(camera==null) {captureCamera=new GameObject("CaptureCamera",typeof(Camera));camera=captureCamera.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;}
        var target = new RenderTexture(1280,720,24); target.Create();
        var oldTarget = camera.targetTexture;
        var oldActive = RenderTexture.active;
        var canvases = Object.FindObjectsByType<Canvas>();
        var modes = new RenderMode[canvases.Length];
        var cameras = new Camera[canvases.Length];
        var distances = new float[canvases.Length];
        var texture = new Texture2D(1280,720,TextureFormat.RGB24,false);
        try {
            camera.targetTexture=target;
            for(int i=0;i<canvases.Length;i++) {
                modes[i]=canvases[i].renderMode; cameras[i]=canvases[i].worldCamera; distances[i]=canvases[i].planeDistance;
                if(modes[i]==RenderMode.ScreenSpaceOverlay) {
                    canvases[i].renderMode=RenderMode.ScreenSpaceCamera; canvases[i].worldCamera=camera; canvases[i].planeDistance=1;
                }
            }
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest { destination=target });
            RenderTexture.active=target; texture.ReadPixels(new Rect(0,0,1280,720),0,0); texture.Apply();
            File.WriteAllBytes(filename,texture.EncodeToPNG());
        } finally {
            for(int i=0;i<canvases.Length;i++) { canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i]; }
            camera.targetTexture=oldTarget;RenderTexture.active=oldActive;target.Release();Destroy(target);Destroy(texture);if(captureCamera!=null)Destroy(captureCamera);
            Canvas.ForceUpdateCanvases();
        }
    }
    private static IEnumerator PressKey(UnityEngine.InputSystem.Keyboard keyboard,UnityEngine.InputSystem.Key key) {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
        yield return null;yield return null;
    }
    private static UnityEngine.InputSystem.Mouse testMouse;
    private static IEnumerator WorldClick(Vector3 point)
    {
        if(testMouse==null) testMouse=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("StoryMouse");
        var mouse=testMouse;
        foreach(var device in UnityEngine.InputSystem.InputSystem.devices)
            if(device is UnityEngine.InputSystem.Mouse && device!=mouse) UnityEngine.InputSystem.InputSystem.DisableDevice(device);
        mouse.MakeCurrent();
        Vector2 screen = Camera.main.WorldToScreenPoint(point);
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen });
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen }.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left));
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen });
        yield return null;
    }
    private IEnumerator WaitForGameplay()
    {
        float deadline=Time.realtimeSinceStartup+90;
        while(SceneManager.GetActiveScene().name!="Beach"&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(SceneManager.GetActiveScene().name=="Beach","Opening video reaches beach");
    }
    private IEnumerator Tests()
    {
        yield return new WaitForSecondsRealtime(1);
        var menu=Find<MainMenuController>();var video=menu.transitionVideoPlayer;
        Check(video.clip!=null&&video.clip.name=="transition","Nickname transition clip remains configured separately");
        hadName=PlayerPrefs.HasKey("PlayerNickname");oldName=PlayerPrefs.GetString("PlayerNickname","");restoreName=true;
        PlayerPrefs.SetString("PlayerNickname","Maré");
        menu.PlayGame();menu.PlayGame();
        Check(menu.IsTransitioning&&video.clip.name=="intro_gameplay","Play selects gameplay video and blocks repeated clicks");
        float deadline=Time.realtimeSinceStartup+20;while(!video.isPlaying&&Time.realtimeSinceStartup<deadline&&SceneManager.GetActiveScene().name=="MainMenu")yield return null;
        Check(video!=null&&video.isPlaying&&SceneManager.GetActiveScene().name=="MainMenu","Real gameplay video plays before loading beach");
        yield return new WaitForSecondsRealtime(1);
        Check(video.frame>0&&video.texture!=null,"Video decoder advances and produces picture");CaptureWorld("gameplay-intro-video.png");
        yield return WaitForGameplay();while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        var gm=GameManager.Instance;var dialogue=Find<DialogueController>();
        string speaker=Named("IntroductionSpeaker").GetComponent<TMP_Text>().text;
        if(hadName)PlayerPrefs.SetString("PlayerNickname",oldName);else PlayerPrefs.DeleteKey("PlayerNickname");PlayerPrefs.Save();
        Check(dialogue.IsOpen&&speaker=="Maré","Saved player nickname replaces You in beach dialogue");
        Check(!Named("IntroductionSpeaker").GetComponent<TMP_Text>().richText,"Player name is displayed literally");
        Check(!gm.HasMissionStarted&&!gm.HasMap,"Opening preserves NPC mission and map gates");
        CaptureWorld("named-beach-dialogue.png");
        dialogue.NextLine();Check(Named("IntroductionText").GetComponent<TMP_Text>().text.Contains("não lembro"),"Named speaker still expresses memory loss");while(dialogue.IsOpen)dialogue.NextLine();
        Check(gm.HasSeenBeachIntroduction&&Find<PlayerClickMovement>().IsMovementEnabled,"Beach intro still completes and releases player");
        SceneFade.TryLoad("MainMenu");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return new WaitForSecondsRealtime(1);
        menu=Find<MainMenuController>();menu.nicknamePanel.SetActive(false);menu.mainMenuPanel.SetActive(true);
        typeof(MainMenuController).GetField("gameplayIntroClip",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(menu,null);
        PlayerPrefs.SetString("PlayerNickname","   ");menu.PlayGame();yield return WaitForGameplay();while(SceneFade.IsRunning)yield return null;yield return null;
        Check(SceneManager.GetActiveScene().name=="Beach","Missing clip falls back to gameplay without a soft lock");
        yield return null;yield return null;
        Check(Named("IntroductionSpeaker").GetComponent<TMP_Text>().text=="Você","Empty nickname falls back to You");
    }
}
