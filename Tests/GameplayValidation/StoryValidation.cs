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

public static class StoryValidation
{
    public static void RunPresentation() {
        SessionState.SetBool("StoryValidation",true);SessionState.SetBool("StoryPresentation",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse.unity");EditorApplication.EnterPlaymode();
    }
    public static void Run()
    {
        SessionState.SetBool("StoryPresentation",false);
        SessionState.SetBool("StoryValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class StoryValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("StoryValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<StoryValidationRunner>();
    }

    private void Start()
    {
        Application.logMessageReceived += OnLog;
        StartCoroutine(Guard(SessionState.GetBool("StoryPresentation",false) ? PresentationTests() : Tests()));
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
        File.WriteAllLines(SessionState.GetBool("StoryPresentation",false) ? "story-presentation-results.txt" : "story-results.txt", results);
        SessionState.SetBool("StoryValidation", false);
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
    private IEnumerator PresentationTests()
    {
        yield return new WaitForSecondsRealtime(1);
        var gm=GameManager.Instance;gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        foreach(GameManager.CrystalId crystal in Enum.GetValues(typeof(GameManager.CrystalId))) gm.TryCollectCrystal(crystal);
        gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
        var lighthouse=Find<LighthouseInteraction>();lighthouse.Interact();lighthouse.Activate();yield return null;yield return null;
        Canvas.ForceUpdateCanvases();var bar=Named("QteTime").GetComponent<Slider>();Check(bar.fillRect.rect.width<=bar.GetComponent<RectTransform>().rect.width+1,"QTE progress stays inside panel");CaptureWorld("story-qte.png");
        gm.ResetMission();yield return null;SceneFade.TryLoad("Ending");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;
        yield return new WaitForSecondsRealtime(2);Check(Find<BeaconBeamGraphic>().GetComponent<CanvasRenderer>()!=null,"Cinematic beam has renderer");CaptureWorld("story-ending.png");
    }
    private IEnumerator WaitForGameplay()
    {
        float deadline=Time.realtimeSinceStartup+90;
        while(SceneManager.GetActiveScene().name!="Beach"&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(SceneManager.GetActiveScene().name=="Beach","Opening video reaches beach");
    }
    private IEnumerator Tests()
    {
        yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        Find<MainMenuController>().PlayGame(); yield return WaitForGameplay(); yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name=="Beach","Menu starts on beach");
        var gm=GameManager.Instance; Check(!gm.HasMissionStarted,"Mission waits for NPC");
        var opening=Find<DialogueController>();if(opening!=null)while(opening.IsOpen)opening.NextLine();
        Check(GameObject.Find("Background")==null || GameObject.Find("Background").transform.parent!=null,"Legacy world background removed");
        CaptureWorld("story-beach.png");
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("StoryKeyboard");
        var player=Find<PlayerClickMovement>();var before=player.transform.position;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.D));
        yield return new WaitForSecondsRealtime(.4f);
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return null;
        Check(player.transform.position.x>before.x+.3f,$"WASD moves player: {before} -> {player.transform.position}, enabled={player.IsMovementEnabled}, fade={SceneFade.IsRunning}");
        gm.SetPaused(true);before=player.transform.position;yield return PressKey(keyboard,UnityEngine.InputSystem.Key.W);Check(player.transform.position==before,"Pause blocks WASD");gm.SetPaused(false);
        yield return WorldClick(Named("PathToHouse").transform.Find("DestinationLabel").position);yield return new WaitForSecondsRealtime(4); while(SceneFade.IsRunning) yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name=="House","Click on destination label approaches and changes scene");
        Check(gm.HasVisitedHouse,"House visit recorded");CaptureWorld("story-house.png");
        yield return WorldClick(Find<ScrollInteraction>().transform.position);yield return new WaitForSecondsRealtime(1.5f);Check(Named("ScrollPanel").activeSelf,"Click on house desk approaches and opens note");Find<ScrollInteraction>().CloseScroll();
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        CaptureWorld("story-center.png");Check(Find<NPCInteraction>()!=null,"NPC lives at center");
        var dialogue=Find<DialogueController>();yield return WorldClick(Find<NPCInteraction>().transform.position);yield return new WaitForSecondsRealtime(1.3f);Check(dialogue.IsOpen,"Click on NPC starts dialogue");while(dialogue.IsOpen)dialogue.NextLine();Check(gm.IsMissionActive(),"NPC starts mission once");gm.StartMission();Check(gm.RecoveredMemoryCount==0,"Repeated mission start preserves progress");
        SceneFade.TryLoad("Lighthouse");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        var lighthouse=Find<LighthouseInteraction>();lighthouse.Interact();lighthouse.Activate();Check(!lighthouse.IsRestoring,"Farol rejects missing memories");lighthouse.Close();
        SceneFade.TryLoad("House");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        Find<ScrollInteraction>().Interact();Click("SunSymbolButton");Click("MoonSymbolButton");Click("StarSymbolButton");Check(gm.RecoveredMemoryCount==1,"House sky puzzle recovers first memory");Find<ScrollInteraction>().CloseScroll();
        SceneFade.TryLoad("Cave");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        CaptureWorld("story-cave.png");Check(Find<NPCInteraction>()==null,"NPC not duplicated in cave");
        var spot=Find<MiningSpot>();player=Find<PlayerClickMovement>();player.transform.position=spot.ApproachPosition;
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,0f);
        spot.StartDiscovery();spot.StartDiscovery();yield return new WaitForSeconds(4.3f);Check(gm.CrystalCount==0&&!spot.IsMining,"Failed mining finishes without a crystal");
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,1f);
        for(int i=0;i<3;i++) {spot.StartDiscovery();Check(spot.IsMining&&!player.IsMovementEnabled,"Mining locks movement");yield return new WaitForSeconds(4.3f);}
        Check(gm.CrystalCount==3,"Three completed extractions populate inventory");
        Click("CloseCrystalPanel");Check(!((GameObject)typeof(MiningUI).GetField("miningPanel",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Find<MiningUI>())).activeSelf,"Crystal panel can be dismissed");
        yield return WorldClick(Named("CrystalSockets").transform.position);
        var socketPanel=(GameObject)typeof(MiningUI).GetField("miningPanel",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Find<MiningUI>());
        float socketDeadline=Time.realtimeSinceStartup+12;
        while(!socketPanel.activeInHierarchy && Time.realtimeSinceStartup<socketDeadline) yield return null;
        yield return null;yield return null;
        var ray=Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());Physics.Raycast(ray,out var socketHit);
        CaptureWorld("story-sockets-reopen.png");
        Check(((GameObject)typeof(MiningUI).GetField("miningPanel",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Find<MiningUI>())).activeSelf,$"Clicking sockets reopens crystal puzzle: player={player.transform.position}, enabled={player.IsMovementEnabled}, hit={socketHit.collider}, pointerUI={UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()}");
        Check(!Named("StoryObjectiveCard").activeSelf&&!Named("StoryHover").activeSelf,"Puzzle panel hides overlapping HUD");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);Check(Find<CrystalInventoryUI>().IsOpen,"I opens inventory");CaptureWorld("story-inventory.png");yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);
        Click("CrystalTriangleButton");Click("CrystalSocket1");Check(gm.FilledCrystalSocketCount==0,"Wrong socket preserves crystals");
        Click("CrystalTriangleButton");Click("CrystalSocket2");Click("CrystalCircleButton");Click("CrystalSocket1");Click("CrystalSquareButton");Click("CrystalSocket3");
        Check(gm.RecoveredMemoryCount==2,"Crystal puzzle recovers storm memory");Click("CloseCrystalPanel");
        SceneFade.TryLoad("House");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        Find<ScrollInteraction>().Interact();yield return null;Check(Named("ReturnMemoryPanel").activeSelf&&!Named("ScrollPanel").activeSelf,"Returning home reveals second reading on same desk");
        Click("BoatSymbolButton");Click("FishSymbolButton");Click("LighthouseSymbolButton");Check(gm.HasAllMemories&&!gm.IsGameWon(),"Return puzzle unlocks finale without winning");Find<ScrollInteraction>().CloseScroll();
        SceneFade.TryLoad("Lighthouse");yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;
        CaptureWorld("story-lighthouse.png");lighthouse=Find<LighthouseInteraction>();lighthouse.Interact();lighthouse.Activate();lighthouse.Activate();yield return null;
        Check(lighthouse.IsRestoring&&lighthouse.QteStep==0,"QTE activation is single");
        gm.SetPaused(true);float remaining=lighthouse.Remaining;yield return new WaitForSecondsRealtime(.3f);Check(lighthouse.Remaining==remaining,"Pause freezes QTE time");gm.SetPaused(false);
        lighthouse.SubmitQteKey(UnityEngine.InputSystem.Key.D);Check(!lighthouse.IsRestoring&&!gm.IsGameWon(),"Wrong key allows retry without victory");
        lighthouse.Activate();yield return new WaitForSeconds(2.7f);Check(!lighthouse.IsRestoring&&!gm.IsGameWon(),"QTE timeout permits retry");
        lighthouse.Activate();CaptureWorld("story-qte.png");
        foreach(var key in new[]{UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.D,UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.Space}) yield return PressKey(keyboard,key);
        yield return new WaitForSecondsRealtime(1.5f);Check(gm.IsGameWon(),"Correct timed keys complete mission");Check(SceneManager.GetActiveScene().name=="Ending","Victory transitions to cutscene");
        yield return new WaitForSecondsRealtime(2);CaptureWorld("story-ending.png");
        yield return new WaitForSecondsRealtime(16);Check(SceneManager.GetActiveScene().name=="Credits","Cutscene automatically reaches credits");
        Find<CreditsController>().BackToMenu();yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;Check(SceneManager.GetActiveScene().name=="MainMenu","Credits return to menu");
        Find<MainMenuController>().PlayGame(); yield return WaitForGameplay();yield return new WaitForSecondsRealtime(1); while(SceneFade.IsRunning) yield return null; yield return null; yield return null;Check(SceneManager.GetActiveScene().name=="Beach"&&!gm.HasMissionStarted&&gm.CrystalCount==0&&!gm.IsGameWon(),"Replay clears progress and returns to beach");
        Check(Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length==1,"Single persistent GameManager");
    }
}
