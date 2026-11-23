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

public static class LighthouseExteriorValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("LighthouseExteriorValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class LighthouseExteriorValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("LighthouseExteriorValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<LighthouseExteriorValidationRunner>();
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
        File.WriteAllLines("lighthouse-exterior-results.txt", results);
        SessionState.SetBool("LighthouseExteriorValidation", false);
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

    private IEnumerator Tests(){
      yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;
      UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
      UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
      var gm=GameManager.Instance;gm.ReadHouseNotice();gm.CollectJournal();gm.LearnJournal();gm.StartMission();gm.ReceiveMapFromNpc();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
      foreach(GameManager.CrystalId id in Enum.GetValues(typeof(GameManager.CrystalId)))gm.TryCollectCrystal(id);
      gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);Check(gm.TryCollectKeyMaterials(),"Cache materials collected");
      Check(SceneFade.TryLoad("Lighthouse"),"Legacy interior route accepted safely");yield return new WaitForSecondsRealtime(1.5f);Check(SceneManager.GetActiveScene().name=="LighthouseExterior","Locked route redirects to exterior");
      Check(GameManager.IsGameplayScene&&GameManager.IsExplorationScene("LighthouseExterior"),"Exterior supports gameplay systems");Check(gm.HasVisitedLocation("LighthouseExterior"),"Exterior visit registered");
      var gate=Find<LighthouseGate>();var player=Find<PlayerClickMovement>();CaptureWorld("lighthouse-exterior.png");
      player.transform.position=new Vector3(.8f,-2.2f,-1);Physics.SyncTransforms();yield return WorldClick(Find<LighthouseInscription>().transform.position);yield return null;yield return null;
      Check(gate.IsOpen&&gm.HasReadLighthouseClue,"World plaque click opens readable clue");Check(!player.IsMovementEnabled,"Lock panel blocks movement");CheckClickable("ReadInscription");
      gate.UseKey();Check(!gm.HasUsedLighthouseKey,"Cannot use missing key");gate.ChangeDigit(0,4);gate.ChangeDigit(2,3);gate.Submit();Check(!gm.IsLighthouseUnlocked,"Correct code cannot bypass crafted key");gate.Enter();Check(!SceneFade.IsRunning,"Entry blocked before unlock");
      Find<PauseController>().HandleEscape();Check(!gate.IsOpen&&player.IsMovementEnabled,"Escape returns control");
      Find<QuestJournalUI>().Toggle();yield return null;Check(((TMP_Text)typeof(QuestJournalUI).GetField("notes",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Find<QuestJournalUI>())).text.Contains("Inscrição do farol"),"Clue is recorded in journal");Find<QuestJournalUI>().Close();
      SceneManager.LoadScene("House");yield return null;yield return new WaitForSecondsRealtime(1);Check(gm.TryCraftLighthouseKey(),"Crafted key available for lock");gm.CompleteMission();Check(!gm.IsGameWon(),"Cannot win before unlocking door");
      SceneManager.LoadScene("Island");yield return null;yield return new WaitForSecondsRealtime(1);var map=Find<IslandMapUI>();map.Toggle();yield return null;map.SelectLocation("Lighthouse");Check(map.SelectedLocation=="LighthouseExterior"&&map.CanTravel,"Map legacy pin selects visited exterior");map.Travel();yield return new WaitForSecondsRealtime(1.5f);Check(SceneManager.GetActiveScene().name=="LighthouseExterior","Fast travel lands outside");
      gate=Find<LighthouseGate>();player=Find<PlayerClickMovement>();Physics.SyncTransforms();yield return WorldClick(gate.transform.position);float approachDeadline=Time.realtimeSinceStartup+10;while(!gate.IsOpen&&Time.realtimeSinceStartup<approachDeadline)yield return null;yield return null;yield return null;Check(gate.IsOpen,"Door click walks to entrance and opens lock interface");CheckClickable("UseLighthouseKey");Click("UseLighthouseKey");Check(gm.HasUsedLighthouseKey&&gm.HasLighthouseKey,"Key used without removing inventory keepsake");
      gm.SetPaused(true);gate.ChangeDigit(0,4);gate.Submit();Check(gate.Code==0&&!gm.IsLighthouseUnlocked,"Pause blocks lock inputs");gm.SetPaused(false);
      CheckClickable("UnlockLighthouse");Click("UnlockLighthouse");Check(!gm.IsLighthouseUnlocked,"Wrong combination does not unlock");
      for(int i=0;i<4;i++)Click("LockPlus0");Click("LockMinus1");Check(gate.Code==490,"Digit decrements wrap to nine");Click("LockPlus1");for(int i=0;i<3;i++)Click("LockPlus2");Check(gate.Code==403,"Buttons form intended combination");CaptureWorld("lighthouse-lock-puzzle.png");
      Click("UnlockLighthouse");Check(gm.IsLighthouseUnlocked,"Correct combination unlocks after key");Check(!Named("DoorPadlock").activeSelf,"World padlock removed after success");Check(!gm.TryUnlockLighthouse(403),"Cannot repeat reward");yield return null;yield return null;CheckClickable("EnterLighthouse");CaptureWorld("lighthouse-lock-open.png");Click("EnterLighthouse");yield return new WaitForSecondsRealtime(1.5f);Check(SceneManager.GetActiveScene().name=="Lighthouse","Unlocked entry loads original interior");
      Check(Find<DoorInteraction>().SceneName=="LighthouseExterior","Interior exit returns outside");SceneFade.TryLoad("LighthouseExterior");yield return new WaitForSecondsRealtime(1.5f);Check(gm.IsLighthouseUnlocked&&gm.HasReadLighthouseClue&&gm.HasUsedLighthouseKey,"Lock and clue persist after leaving");Check(!Named("DoorPadlock").activeSelf,"Padlock remains absent on return");
      gate=Find<LighthouseGate>();player=Find<PlayerClickMovement>();player.transform.position=gate.ApproachPosition;gate.Interact();gate.Enter();yield return new WaitForSecondsRealtime(1.5f);
      var lamp=Find<LighthouseInteraction>();lamp.Interact();lamp.Activate();Check(lamp.IsRestoring,"Original QTE still starts after entry");for(int adjustment=0;adjustment<3;adjustment++){yield return new WaitForSeconds(.4f);float deadline=Time.realtimeSinceStartup+8;while(Mathf.Abs(lamp.CursorValue-lamp.TargetCenter)>.02f&&Time.realtimeSinceStartup<deadline)yield return null;Check(Time.realtimeSinceStartup<deadline,"Precision target reachable");lamp.SubmitPulse();Check(lamp.QteStep==adjustment+1,"Precision adjustment completed");}
      yield return new WaitForSecondsRealtime(2);Check(gm.IsGameWon(),"QTE completes mission");Check(SceneManager.GetActiveScene().name=="Ending","QTE retains ending video route");
      gm.ResetMission();Check(!gm.IsLighthouseUnlocked&&!gm.HasReadLighthouseClue&&!gm.HasUsedLighthouseKey,"New game resets lock and clue state");
    }
}


