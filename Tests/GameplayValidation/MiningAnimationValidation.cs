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

public static class MiningAnimationValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("MiningAnimationValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class MiningAnimationValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("MiningAnimationValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<MiningAnimationValidationRunner>();
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
        File.WriteAllLines("mining-animation-results.txt", results);
        SessionState.SetBool("MiningAnimationValidation", false);
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
    private IEnumerator Tests() {
        yield return new WaitForSeconds(1);
        var gm=GameManager.Instance;gm.StartMission();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        var spot=Find<MiningSpot>();var player=Find<PlayerClickMovement>();var anim=player.GetComponent<PlayerSpriteAnimation>();var animator=player.GetComponent<Animator>();var renderer=player.GetComponent<SpriteRenderer>();
        var rocks=new List<Collider>();foreach(var c in spot.GetComponentsInChildren<Collider>())if(c.name=="MineralVein")rocks.Add(c);rocks.Sort((a,b)=>a.transform.position.x.CompareTo(b.transform.position.x));
        Check(rocks.Count==3,"Three rocks");
        spot.SelectTarget(rocks[0]);player.transform.position=spot.ApproachPosition;spot.StartDiscovery();yield return new WaitForSeconds(.2f);
        Check(spot.IsMining&&anim.IsMiningAnimation,"Mining animation starts");spot.CancelMining();yield return null;yield return null;
        Check(!anim.IsMiningAnimation&&animator.speed==1&&player.IsMovementEnabled,"Cancellation restores idle and movement");Check(gm.CrystalCount==0,"Cancelled attempt gives no reward");
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,0f);
        spot.StartDiscovery();while(spot.IsMining)yield return null;yield return null;
        Check(gm.CrystalCount==0&&!anim.IsMiningAnimation&&animator.speed==1,"Failed extraction restores idle without reward");
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,1f);
        for(int i=0;i<3;i++) {
            spot.SelectTarget(rocks[i]);player.transform.position=spot.ApproachPosition;spot.StartDiscovery();yield return new WaitForSeconds(.1f);
            Check(spot.IsMining&&!player.IsMovementEnabled,"Rock "+i+": mining locks movement");Check(anim.IsMiningAnimation&&!anim.IsWalking,"Rock "+i+": body mining state active");
            Check(anim.Facing==(spot.FacesLeft?PlayerSpriteAnimation.Direction.Left:PlayerSpriteAnimation.Direction.Right),"Rock "+i+": faces chosen rock");
            Check(!Named("MiningPickaxe").activeSelf,"Rock "+i+": no duplicate floating tool");
            Check(!spot.SelectTarget(rocks[(i+1)%3]),"Rock "+i+": active attempt cannot retarget");
            float progress=spot.Progress;spot.StartDiscovery();Check(spot.Progress==progress,"Repeated mining click cannot restart attempt");
            if(i==0) {gm.SetPaused(true);yield return null;yield return null;var sprite=renderer.sprite;progress=spot.Progress;int impact=spot.ImpactIndex;yield return new WaitForSecondsRealtime(.3f);Check(renderer.sprite==sprite&&spot.Progress==progress&&spot.ImpactIndex==impact,"Pause freezes body, progress and impact clock");gm.SetPaused(false);}
            var frames=new HashSet<string>();bool captured=false;float deadline=Time.realtimeSinceStartup+30;
            while(spot.IsMining&&Time.realtimeSinceStartup<deadline){frames.Add(renderer.sprite.name);if(!captured&&renderer.sprite.name.EndsWith("_3")){CaptureWorld("mining-animation-rock-"+i+".png");captured=true;}yield return null;}
            Check(!spot.IsMining,"Rock "+i+": attempt completes");Check(frames.Count>=6,"Rock "+i+": six body animation poses played");yield return null;yield return null;
            Check(gm.CrystalCount==i+1,"Rock "+i+": crystal enters inventory");Check(!anim.IsMiningAnimation&&animator.speed==1&&renderer.sprite.name.StartsWith("Idle"),"Rock "+i+": returns to directional idle");
        }
        Check(player.IsMovementEnabled,"Movement unlocked after final extraction");CaptureWorld("mining-animation-idle.png");
    }
}
