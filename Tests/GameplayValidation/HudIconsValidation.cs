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

public static class HudIconsValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("HudIconsValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class HudIconsValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("HudIconsValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<HudIconsValidationRunner>();
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
        File.WriteAllLines("hud-icons-results.txt", results);
        SessionState.SetBool("HudIconsValidation", false);
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
        pointer.position = RectTransformUtility.WorldToScreenPoint(null, button.transform.TransformPoint(((RectTransform)button.transform).rect.center));
        var hits = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && (hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform)),
            "Pointer reaches " + name + "; hits=" + string.Join(",",hits.ConvertAll(h=>h.gameObject.name)) + "; screen=" + pointer.position);
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
    private IEnumerator Tests()
    {
        yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;
        Check(!Named("OpenMap").activeSelf&&!Named("OpenInventory").activeSelf,"Icons respect initial mission and NPC map gates");
        gm.StartMission();yield return null;Check(Named("OpenInventory").activeSelf&&!Named("OpenMap").activeSelf,"Inventory unlocks with mission; map still waits for NPC gift");gm.ReceiveMapFromNpc();
        foreach(string scene in new[]{"House","Island","Cave","Lighthouse","Beach"}){
            if(SceneManager.GetActiveScene().name!=scene){SceneFade.TryLoad(scene);yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;}
            yield return null;
            var map=Button("OpenMap");var bag=Button("OpenInventory");var mr=(RectTransform)map.transform;var br=(RectTransform)bag.transform;
            Check(map.gameObject.activeSelf&&bag.gameObject.activeSelf,scene+": both icons visible after unlock");
            Check(mr.anchorMin==Vector2.zero&&br.anchorMin==Vector2.zero&&mr.anchoredPosition.x==br.anchoredPosition.x&&br.anchoredPosition.y>mr.anchoredPosition.y+mr.rect.height,scene+": inventory above map at lower-left");
            Check(map.GetComponent<Image>().sprite!=null&&bag.GetComponent<Image>().sprite!=null,scene+": PNG sprites assigned");
            CaptureWorld("hud-icons-debug.png");CheckClickable("OpenInventory");CheckClickable("OpenMap");
            Click("OpenInventory");yield return null;Check(Find<CrystalInventoryUI>().IsOpen&&!map.gameObject.activeSelf&&!bag.gameObject.activeSelf,scene+": inventory icon opens inventory and hides HUD icons");
            Find<CrystalInventoryUI>().Close();yield return null;yield return null;
            Click("OpenMap");yield return null;Check(Find<IslandMapUI>().IsOpen&&!bag.gameObject.activeSelf,scene+": map icon opens map");Find<IslandMapUI>().Close();yield return null;yield return null;
            Check(!Named("StoryControlsBar").activeSelf&&!Named("StoryControls").activeSelf,scene+": bottom controls bar stays removed");
            yield return new WaitForSecondsRealtime(.3f);
            if(scene=="House")CaptureWorld("hud-icons-house.png");
            if(scene=="Cave"){
                gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);yield return null;Canvas.ForceUpdateCanvases();
                var progress=(RectTransform)Named("WorldMiningPanel").transform;
                Vector3[] corners=new Vector3[4];progress.GetWorldCorners(corners);Vector3[] iconCorners=new Vector3[4];br.GetWorldCorners(iconCorners);
                Check(corners[0].x>iconCorners[2].x,"Mining feedback does not overlap HUD icons");CaptureWorld("hud-icons-cave.png");
            }
        }
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keys=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("HudKeyboard");
        yield return PressKey(keys,UnityEngine.InputSystem.Key.I);Check(Find<CrystalInventoryUI>().IsOpen,"I shortcut still opens inventory");yield return PressKey(keys,UnityEngine.InputSystem.Key.I);
        yield return PressKey(keys,UnityEngine.InputSystem.Key.M);Check(Find<IslandMapUI>().IsOpen,"M shortcut still opens map");yield return PressKey(keys,UnityEngine.InputSystem.Key.M);
        gm.SetPaused(true);yield return null;yield return null;Check(!Named("OpenInventory").activeSelf&&!Named("OpenMap").activeSelf,"Icons hidden during pause");gm.SetPaused(false);
    }
}
