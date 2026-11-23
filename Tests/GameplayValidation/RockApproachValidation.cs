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

public static class RockApproachValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("RockApproachValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class RockApproachValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("RockApproachValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<RockApproachValidationRunner>();
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
        File.WriteAllLines("rock-results.txt", results);
        SessionState.SetBool("RockApproachValidation", false);
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
    private IEnumerator Tests()
    {
        yield return new WaitForSecondsRealtime(1);
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("RockKeyboard");
        var gm=GameManager.Instance;gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        var spot=Find<MiningSpot>();var player=Find<PlayerClickMovement>();
        var rocks=new List<Collider>();foreach(var c in spot.GetComponentsInChildren<Collider>())if(c.name=="MineralVein")rocks.Add(c);
        rocks.Sort((a,b)=>a.transform.position.x.CompareTo(b.transform.position.x));
        Check(rocks.Count==3,"Three clickable mining rocks configured");
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,1f);
        for(int i=0;i<3;i++){
            player.transform.position=new Vector3(0,-3,-1);spot.SelectTarget(rocks[i]);spot.StartDiscovery();
            Check(!spot.IsMining,"Rock "+(i+1)+": cannot mine remotely");
            yield return WorldClick(rocks[i].bounds.center);
            Check(!spot.IsMining,"Rock "+(i+1)+": approach takes time");
            float deadline=Time.realtimeSinceStartup+12;while(!spot.IsMining&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(spot.IsMining,"Rock "+(i+1)+": world click walks to rock and starts mining");
            Check(Vector3.Distance(player.transform.position,spot.ApproachPosition)<=.08f,"Rock "+(i+1)+": reaches dedicated approach point");
            Check(!player.IsMovementEnabled,"Rock "+(i+1)+": mining locks movement");
            Vector3 before=player.transform.position;yield return PressKey(keyboard,UnityEngine.InputSystem.Key.S);Check(player.transform.position==before,"Rock "+(i+1)+": WASD cannot move during mining");
            Check(!spot.SelectTarget(rocks[(i+1)%3]),"Rock "+(i+1)+": cannot retarget active mining");
            CaptureWorld("rock-"+(i+1)+"-mining.png");
            deadline=Time.realtimeSinceStartup+6;while(spot.IsMining&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(!spot.IsMining&&gm.CrystalCount==i+1,"Rock "+(i+1)+": timed extraction adds crystal to inventory");
        }
        Find<MiningUI>().ClosePanel();yield return WorldClick(Named("CrystalSockets").transform.position);
        float end=Time.realtimeSinceStartup+10;var panel=(GameObject)typeof(MiningUI).GetField("miningPanel",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Find<MiningUI>());
        while(!panel.activeSelf&&Time.realtimeSinceStartup<end)yield return null;
        Check(panel.activeSelf,"Socket puzzle remains reachable after collecting three crystals");
        Find<MiningUI>().ClosePanel();
        player.transform.position=new Vector3(-3.7f,.8f,-1);yield return PressKey(keyboard,UnityEngine.InputSystem.Key.W);
        Check(player.transform.position.y>.8f,"WASD can reach the upper floor near rocks");
    }
}
