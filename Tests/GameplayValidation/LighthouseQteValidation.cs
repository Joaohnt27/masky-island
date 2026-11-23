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

public static class LighthouseQteValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("LighthouseQteValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class LighthouseQteValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("LighthouseQteValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<LighthouseQteValidationRunner>();
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
        File.WriteAllLines("lighthouse-qte-results.txt", results);
        SessionState.SetBool("LighthouseQteValidation", false);
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

 private float Cooldown(LighthouseInteraction lamp)=>(float)typeof(LighthouseInteraction).GetField("cooldown",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(lamp);
 private IEnumerator Tests(){
 yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;gm.StartMission();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);foreach(GameManager.CrystalId id in Enum.GetValues(typeof(GameManager.CrystalId)))gm.TryCollectCrystal(id);gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryCollectKeyMaterials();
 SceneManager.LoadScene("House");yield return null;yield return new WaitForSecondsRealtime(1);gm.TryCraftLighthouseKey();SceneManager.LoadScene("LighthouseExterior");yield return null;yield return new WaitForSecondsRealtime(1);gm.ReadLighthouseClue();gm.TryUseLighthouseKey();gm.TryUnlockLighthouse(403);
 SceneManager.LoadScene("Lighthouse");yield return null;yield return new WaitForSecondsRealtime(1);var lamp=Find<LighthouseInteraction>();lamp.Interact();yield return null;yield return null;Check(lamp.IsOpen&&!Find<PlayerClickMovement>().IsMovementEnabled,"Panel opens and locks movement");CheckClickable("ActivateLighthouse");CaptureWorld("lighthouse-qte-ready.png");Click("ActivateLighthouse");Check(lamp.IsRestoring&&lamp.QteStep==0,"Prepare starts precision challenge");lamp.SubmitPulse();Check(lamp.QteStep==0,"Mistimed click does not advance");
 yield return new WaitForSeconds(.4f);gm.SetPaused(true);float frozen=lamp.CursorValue,remaining=lamp.Remaining;yield return new WaitForSecondsRealtime(.4f);lamp.SubmitPulse();Check(lamp.CursorValue==frozen&&lamp.Remaining==remaining&&lamp.QteStep==0,"Pause freezes cursor timer and actions");gm.SetPaused(false);
 Find<PauseController>().HandleEscape();Check(!lamp.IsOpen&&!lamp.IsRestoring&&Find<PlayerClickMovement>().IsMovementEnabled,"Escape cancels and restores movement");lamp.Interact();lamp.Activate();yield return new WaitForSeconds(8.3f);Check(lamp.IsRestoring&&lamp.QteStep==0&&lamp.Remaining>6,"Timeout retries current stage");
 UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("LighthouseKeyboard");
 for(int i=0;i<3;i++){
 float deadline=Time.realtimeSinceStartup+8;while((Mathf.Abs(lamp.CursorValue-lamp.TargetCenter)>.02f||Cooldown(lamp)>0)&&Time.realtimeSinceStartup<deadline)yield return null;Check(Time.realtimeSinceStartup<deadline,"Target reachable "+i);
 if(i==1)yield return PressKey(keyboard,UnityEngine.InputSystem.Key.Space);else {CheckClickable("LighthouseAdjust");Click("LighthouseAdjust");}
 Check(lamp.QteStep==i+1,"Timed action advances stage "+i);lamp.SubmitPulse();Check(lamp.QteStep==i+1,"Repeated action cannot skip stage");if(i==0){yield return new WaitForSeconds(.4f);CaptureWorld("lighthouse-qte-playing.png");yield return new WaitForSeconds(8.3f);Check(lamp.QteStep==1&&lamp.IsRestoring,"Timeout preserves completed stage");}
 }
 yield return null;yield return null;Check(gm.IsGameWon(),"Three adjustments restore lighthouse");CaptureWorld("lighthouse-qte-success.png");yield return new WaitForSecondsRealtime(2);Check(SceneManager.GetActiveScene().name=="Ending","Success starts ending scene");
 float videoDeadline=Time.realtimeSinceStartup+20;UnityEngine.Video.VideoPlayer video=null;while(Time.realtimeSinceStartup<videoDeadline){video=Find<UnityEngine.Video.VideoPlayer>();if(video!=null&&video.isPlaying&&video.frame>0)break;yield return null;}Check(video!=null&&video.isPlaying&&video.frame>0,"Final gameplay video still plays");
 }
}

