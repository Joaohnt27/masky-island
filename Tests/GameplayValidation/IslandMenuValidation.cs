using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class IslandMenuValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("IslandMenuValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class IslandMenuValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("IslandMenuValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<IslandMenuValidationRunner>();
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
        File.WriteAllLines("island-menu-results.txt", results);
        SessionState.SetBool("IslandMenuValidation", false);
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

 private T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);
 private IEnumerator Tests(){
 yield return new WaitForSecondsRealtime(1.5f);var menu=Find<MainMenuController>();Check(menu.mainMenuPanel.activeSelf&&!menu.IsTransitioning,"Main menu ready");CaptureWorld("island-menu.png");foreach(var n in new[]{"PlayButton","SettingsButton","CreditsButton","ExitButton"})CheckClickable(n);
 Click("SettingsButton");yield return new WaitForSecondsRealtime(1.5f);var settings=Find<SettingsMenuUI>();Check(settings.IsOpen,"Settings button opens settings");var tabs=Field<Button[]>(settings,"tabs");var pages=Field<GameObject[]>(settings,"pages");Check(pages[0].activeSelf,"Audio tab active");CaptureWorld("island-settings-audio.png");
 var master=Field<Slider>(settings,"master");float old=master.value;master.value=.37f;Check(Mathf.Abs(SettingsManager.Instance.MasterVolume-.37f)<.01f,"Audio slider remains functional");master.value=old;
 tabs[1].onClick.Invoke();yield return null;yield return null;Check(pages[1].activeSelf&&!pages[0].activeSelf,"Video tab switches correctly");CaptureWorld("island-settings-video.png");var dropdown=Field<TMP_Dropdown>(settings,"screenMode");dropdown.Show();yield return null;yield return null;Check(dropdown.IsExpanded,"Video dropdown opens");settings.HandleEscape();yield return new WaitForSecondsRealtime(.4f);Check(!dropdown.IsExpanded&&settings.IsOpen,"Escape closes dropdown first");
 tabs[2].onClick.Invoke();yield return null;yield return null;Check(pages[2].activeSelf,"Controls tab available");yield return new WaitForSecondsRealtime(.2f);CaptureWorld("island-settings-controls.png");settings.Close();yield return new WaitForSecondsRealtime(1.5f);Check(menu.mainMenuPanel.activeSelf&&!settings.IsOpen,"Settings returns to menu");
 Click("CreditsButton");yield return new WaitForSecondsRealtime(1.5f);Check(SceneManager.GetActiveScene().name=="Credits","Credits button loads scene");var credits=Find<CreditsController>();var content=Field<TMP_Text>(credits,"creditsText");float before=content.rectTransform.anchoredPosition.y;yield return new WaitForSecondsRealtime(.5f);Check(content.rectTransform.anchoredPosition.y>before,"Credits still scroll");content.rectTransform.anchoredPosition=new Vector2(0,215);CaptureWorld("island-credits.png");CheckClickable("BackButton");Click("BackButton");yield return new WaitForSecondsRealtime(1.5f);menu=Find<MainMenuController>();Check(menu.mainMenuPanel.activeSelf,"Credits returns to main menu");
 menu.mainMenuPanel.SetActive(false);menu.nicknamePanel.SetActive(true);yield return null;yield return null;CheckClickable("ContinueButton");CheckClickable("CancelButton");Check(menu.nicknameInput.isActiveAndEnabled,"Nickname input preserved");CaptureWorld("island-name.png");menu.nicknamePanel.SetActive(false);menu.mainMenuPanel.SetActive(true);
 Click("PlayButton");yield return new WaitForSecondsRealtime(1);Check(menu.IsTransitioning&&menu.transitionPanel.activeSelf,"Play starts gameplay introduction");menu.SkipIntro();yield return new WaitForSecondsRealtime(2);Check(SceneManager.GetActiveScene().name=="Beach","Skipping introduction enters beach");
 }
}


