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

public static class FinalGameplayValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("FinalGameplayValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class FinalGameplayValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("FinalGameplayValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<FinalGameplayValidationRunner>();
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
        File.WriteAllLines("final-gameplay-results.txt", results);
        SessionState.SetBool("FinalGameplayValidation", false);
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
    private IEnumerator Load(string scene){SceneFade.TryLoad(scene);yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;}
    private IEnumerator ClickObject(Transform target){yield return WorldClick(target.position);float deadline=Time.realtimeSinceStartup+12;while(!Find<DialogueController>().IsOpen&&Time.realtimeSinceStartup<deadline)yield return null;Check(Find<DialogueController>().IsOpen,"World click reaches "+target.name);}
    private void FinishDialogue(){var talk=Find<DialogueController>();int guard=0;while(talk.IsOpen&&guard++<30)talk.NextLine();Check(!talk.IsOpen,"Dialogue completes without repetition");}
    private IEnumerator Tests(){
 yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;gm.StartMission();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);foreach(GameManager.CrystalId c in System.Enum.GetValues(typeof(GameManager.CrystalId)))gm.TryCollectCrystal(c);gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
 var lighthouse=Find<LighthouseInteraction>();lighthouse.Interact();lighthouse.Activate();lighthouse.SubmitQteKey(UnityEngine.InputSystem.Key.W);yield return null;Check(!gm.IsGameWon()&&SceneManager.GetActiveScene().name=="Lighthouse","Failed QTE does not start ending video");
 lighthouse.Activate();foreach(var key in new[]{UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.D,UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.Space})lighthouse.SubmitQteKey(key);
 float deadline=Time.realtimeSinceStartup+20;while(SceneManager.GetActiveScene().name!="Ending"&&Time.realtimeSinceStartup<deadline)yield return null;while(SceneFade.IsRunning)yield return null;
 Check(gm.IsGameWon()&&SceneManager.GetActiveScene().name=="Ending","Successful QTE loads ending");
 UnityEngine.Video.VideoPlayer video=null;deadline=Time.realtimeSinceStartup+25;while(Time.realtimeSinceStartup<deadline){video=Find<UnityEngine.Video.VideoPlayer>();if(video!=null&&video.isPlaying&&video.frame>0)break;yield return null;}
 Check(video!=null&&video.isPlaying&&video.clip.name=="final_gameplay"&&video.texture!=null,"User final_gameplay video decodes and plays");
 yield return null;CaptureWorld("final-gameplay-playing.png");Check(!Named("CaptionBackground").activeSelf,"Video replaces illustrated captions");CheckClickable("SkipAnimationButton");
 deadline=Time.realtimeSinceStartup+Mathf.Max(30,(float)video.length+20);while(SceneManager.GetActiveScene().name!="Credits"&&Time.realtimeSinceStartup<deadline)yield return null;Check(SceneManager.GetActiveScene().name=="Credits","Video completion opens credits automatically");while(SceneFade.IsRunning)yield return null;
 yield return Load("Ending");float end=Time.realtimeSinceStartup+10;while(!Named("CutsceneSkip").activeSelf&&Time.realtimeSinceStartup<end)yield return null;
 Click("SkipAnimationButton");Click("SkipAnimationButton");yield return new WaitForSecondsRealtime(1.5f);while(SceneFade.IsRunning)yield return null;Check(SceneManager.GetActiveScene().name=="Credits","Skip button advances final video to credits once");
 }
}
