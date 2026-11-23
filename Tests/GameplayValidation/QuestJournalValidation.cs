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

public static class QuestJournalValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("QuestJournalValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class QuestJournalValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("QuestJournalValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<QuestJournalValidationRunner>();
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
        File.WriteAllLines("quest-journal-results.txt", results);
        SessionState.SetBool("QuestJournalValidation", false);
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
        yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;var journal=Find<QuestJournalUI>();
        Check(Named("OpenJournal").activeSelf,"Notebook available after house visit");var icon=(RectTransform)Named("OpenJournal").transform;Check(icon.anchorMin==new Vector2(1,0)&&icon.anchoredPosition.x<0&&icon.anchoredPosition.y>0,"Notebook icon anchored bottom-right");
        journal.Toggle();yield return new WaitForSecondsRealtime(.3f);CaptureWorld("journal-initial.png");CheckClickable("CloseJournal");
        var tasks=Named("JournalTasks").GetComponent<TMP_Text>();Check(tasks.text.Contains("<s>Procurar ajuda na casa.")&&!tasks.text.Contains("cristais"),"Completed visit crossed out; future crystal mission not revealed");
        Check(!Find<PlayerClickMovement>().IsMovementEnabled,"Journal locks movement");Find<IslandMapUI>().Toggle();Find<CrystalInventoryUI>().Toggle();Check(!Find<IslandMapUI>().IsOpen&&!Find<CrystalInventoryUI>().IsOpen,"Journal prevents conflicting panels");
        gm.StartMission();gm.ReceiveMapFromNpc();Check(tasks.text.Contains("<s>Conversar com o estranho")&&tasks.text.Contains("Decifrar os símbolos"),"NPC progress crosses task and adds next entry");
        gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);Check(tasks.text.Contains("Encontrar os três cristais"),"First memory reveals mining task");
        foreach(GameManager.CrystalId crystal in System.Enum.GetValues(typeof(GameManager.CrystalId)))gm.TryCollectCrystal(crystal);
        Check(tasks.text.Contains("<s>Encontrar os três cristais")&&tasks.text.Contains("Usar os cristais"),"Collecting crystals crosses mining task and records sockets");
        gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
        yield return null;Canvas.ForceUpdateCanvases();CaptureWorld("journal-progress.png");Check(tasks.text.Contains("<s>Voltar à casa")&&tasks.text.Contains("Restaurar a luz"),"Recovered memories lead to lighthouse task");
        Check(!tasks.isTextOverflowing&&!Named("JournalNotes").GetComponent<TMP_Text>().isTextOverflowing,"All seven tasks and notes fit pages");
        Find<PauseController>().HandleEscape();Check(!journal.IsOpen&&!gm.IsPaused&&Find<PlayerClickMovement>().IsMovementEnabled,"Escape closes notebook without pausing and releases player");
        foreach(string scene in new[]{"Island","Cave","Lighthouse","Beach"}){SceneFade.TryLoad(scene);yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
            var mining=Find<MiningUI>();if(mining!=null&&mining.enabled)mining.ClosePanel();journal=Find<QuestJournalUI>();journal.Toggle();yield return null;Check(journal.IsOpen&&Named("JournalTasks").GetComponent<TMP_Text>().text.Contains("Restaurar a luz"),scene+": notebook retains progress");journal.Close();}
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keys=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("JournalKeyboard");yield return PressKey(keys,UnityEngine.InputSystem.Key.J);Check(journal.IsOpen,"J opens notebook");yield return PressKey(keys,UnityEngine.InputSystem.Key.J);Check(!journal.IsOpen,"J closes notebook");
        gm.ResetMission();yield return null;journal.Toggle();Check(!journal.IsOpen&&!Named("OpenJournal").activeSelf,"Reset clears availability and previous notes cannot be opened");
    }
}
