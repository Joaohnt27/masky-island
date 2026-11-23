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

public static class BeachIntroductionValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("BeachIntroductionValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class BeachIntroductionValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("BeachIntroductionValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<BeachIntroductionValidationRunner>();
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
        File.WriteAllLines("beach-introduction-results.txt", results);
        SessionState.SetBool("BeachIntroductionValidation", false);
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
        yield return new WaitForSecondsRealtime(1);Find<MainMenuController>().PlayGame(); yield return WaitForGameplay();
        yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        var gm=GameManager.Instance;var dialogue=Find<DialogueController>();var player=Find<PlayerClickMovement>();
        Check(SceneManager.GetActiveScene().name=="Beach"&&dialogue.IsOpen,"New game opens beach introduction after fade");
        Check(!Named("StoryObjectiveCard").activeSelf,"Beach header removed during introduction");
        Check(!Named("StoryControls").activeSelf&&!Named("StoryControlsBar").activeSelf,"Removed bottom HUD remains hidden");
        Check(!player.IsMovementEnabled&&!gm.HasMissionStarted&&!gm.HasMap,"Introduction locks movement without starting NPC mission or granting map");
        Check(Named("IntroductionText").GetComponent<TMP_Text>().text.Contains("Eita"),"First thought asks why character is here");
        Check(Named("CoastalDialogueCard").GetComponent<RawImage>().texture!=null&&Named("PlayerPortrait").GetComponent<Image>().sprite!=null,"Illustrated frame and player portrait assigned");
        CheckClickable("IntroductionContinue");CaptureWorld("beach-introduction-first.png");
        gm.SetPaused(true);yield return null;yield return null;string text=Named("IntroductionText").GetComponent<TMP_Text>().text;dialogue.NextLine();
        Check(dialogue.IsOpen&&!Named("BeachIntroductionPanel").activeSelf&&Named("IntroductionText").GetComponent<TMP_Text>().text==text,"Pause hides introduction and prevents advancement");
        gm.SetPaused(false);yield return null;yield return null;Check(Named("BeachIntroductionPanel").activeSelf,"Resume restores same thought");
        Click("IntroductionContinue");Check(Named("IntroductionText").GetComponent<TMP_Text>().text.Contains("não lembro"),"Second thought establishes amnesia");
        Click("IntroductionContinue");Check(Named("IntroductionText").GetComponent<TMP_Text>().text.Contains("casa no horizonte"),"Third thought points to house");
        Click("IntroductionContinue");Check(dialogue.IsOpen&&Named("IntroductionText").GetComponent<TMP_Text>().text.Contains("me ajudar"),"Final thought sets destination before dismissing");CaptureWorld("beach-introduction-last.png");
        Click("IntroductionContinue");dialogue.NextLine();yield return null;
        Check(!dialogue.IsOpen&&gm.HasSeenBeachIntroduction&&player.IsMovementEnabled,"Final click records introduction and releases player");
        Check(!gm.HasMissionStarted&&!gm.HasMap,"Completing introduction preserves NPC mission and map gates");
        Check(!Named("StoryObjectiveCard").activeSelf,"Beach remains free of objective header after introduction");CaptureWorld("beach-after-introduction.png");
        SceneFade.TryLoad("House");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        SceneFade.TryLoad("Beach");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        Check(!Find<DialogueController>().IsOpen&&Find<PlayerClickMovement>().IsMovementEnabled,"Returning to beach does not replay introduction");
        gm.ResetMission();SceneFade.TryLoad("Beach");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        dialogue=Find<DialogueController>();Check(dialogue.IsOpen&&!gm.HasSeenBeachIntroduction,"New game resets introduction");while(dialogue.IsOpen)dialogue.NextLine();
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        dialogue=Find<DialogueController>();dialogue.StartDialogue();Check(dialogue.IsOpen&&!gm.HasMissionStarted,"NPC dialogue still opens independently");while(dialogue.IsOpen)dialogue.NextLine();
        Check(gm.HasMissionStarted&&gm.HasMap,"NPC still starts mission and delivers map after final line");
    }
}
