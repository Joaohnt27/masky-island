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

public static class BackpackFlowValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("BackpackFlowValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Beach.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class BackpackFlowValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("BackpackFlowValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<BackpackFlowValidationRunner>();
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
        File.WriteAllLines("backpack-flow-results.txt", results);
        SessionState.SetBool("BackpackFlowValidation", false);
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
 yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;FinishDialogue();Find<JournalPickup>().Interact();FinishDialogue();
 var journal=Find<QuestJournalUI>();journal.Toggle();yield return null;Check(gm.HasLearnedJournal&&Named("JournalTasks").GetComponent<TMP_Text>().text.Contains("Procurar ajuda na casa."),"Diary immediately records house goal");journal.Close();
 UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
 UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 yield return Load("House");CaptureWorld("house-backpack.png");
 yield return ClickObject(Find<BackpackPickup>().transform);FinishDialogue();Check(!gm.HasBackpack,"Cannot take backpack before NPC permission");
 Find<CrystalInventoryUI>().Toggle();Check(!Find<CrystalInventoryUI>().IsOpen&&!Named("OpenInventory").activeSelf,"Inventory unavailable before collecting backpack");
 var scroll=Find<ScrollInteraction>();var hit=scroll.GetComponent<Collider>();yield return WorldClick(hit.bounds.center);float deadline=Time.realtimeSinceStartup+12;while(!Find<DialogueController>().IsOpen&&Time.realtimeSinceStartup<deadline)yield return null;
 Check(Find<DialogueController>().IsOpen,"Table click shows house notice");CaptureWorld("house-notice.png");FinishDialogue();Check(gm.HasReadHouseNotice&&!gm.HasMissionStarted&&gm.ObjectiveScene=="Island","Notice directs player to center without starting mission");
 journal=Find<QuestJournalUI>();journal.Toggle();yield return null;Check(Named("JournalTasks").GetComponent<TMP_Text>().text.Contains("trabalhador no centro"),"Journal records notice destination");journal.Close();
 yield return Load("Island");Find<DialogueController>().StartDialogue();FinishDialogue();Check(gm.HasMissionStarted&&!gm.HasBackpack&&gm.ObjectiveScene=="House","NPC starts mission and sends player back for backpack");
 gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);Check(!gm.TryCollectCrystal(GameManager.CrystalId.Circle),"Cannot collect crystals without backpack");
 yield return Load("House");yield return ClickObject(Find<BackpackPickup>().transform);FinishDialogue();yield return null;yield return null;
 Check(gm.HasBackpack&&!Named("BackpackOnFloor").GetComponent<SpriteRenderer>().enabled,"Authorized pickup removes backpack from house");
 var inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;Check(inv.IsOpen,"Collected backpack unlocks inventory");inv.Close();
 foreach(GameManager.CrystalId c in System.Enum.GetValues(typeof(GameManager.CrystalId)))Check(gm.TryCollectCrystal(c),"Backpack permits storing "+c);
 gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
 journal=Find<QuestJournalUI>();journal.Toggle();while(journal.IsWriting)yield return null;yield return null;CaptureWorld("journal-nine-tasks.png");Check(!Named("JournalTasks").GetComponent<TMP_Text>().isTextOverflowing&&!Named("JournalNotes").GetComponent<TMP_Text>().isTextOverflowing,"Nine tasks and notes fit journal");journal.Close();
 yield return Load("Island");yield return Load("House");Check(gm.HasBackpack&&!Named("BackpackOnFloor").GetComponent<SpriteRenderer>().enabled,"Backpack stays collected on revisit");
 gm.ResetMission();yield return null;yield return null;Check(!gm.HasBackpack&&!gm.HasReadHouseNotice&&Named("BackpackOnFloor").GetComponent<SpriteRenderer>().enabled,"New game resets notice and backpack");
 }
}
