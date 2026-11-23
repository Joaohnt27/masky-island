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

public static class MapAudioValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("MapAudioValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Beach.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class MapAudioValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("MapAudioValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<MapAudioValidationRunner>();
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
        File.WriteAllLines("map-audio-results.txt", results);
        SessionState.SetBool("MapAudioValidation", false);
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
        var gm=GameManager.Instance;
        var opening=Find<DialogueController>();if(opening!=null)while(opening.IsOpen)opening.NextLine();
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("MapKeyboard");
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var map=Find<IslandMapUI>();var player=Find<PlayerClickMovement>();
        Check(gm.HasVisitedLocation("Beach")&&!gm.HasVisitedLocation("Cave"),"Only current area initially visited");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.M);
        map.Toggle();map.SelectLocation("House");map.Travel();
        Check(!gm.HasMap&&!map.IsOpen&&!Named("OpenMap").activeSelf&&!SceneFade.IsRunning,"M, button and direct travel remain locked before NPC gift");
        Check(!Named("StoryControls").GetComponent<TMP_Text>().text.Contains("M  mapa"),"Locked HUD does not advertise map key");
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        var dialogue=Find<DialogueController>();dialogue.StartDialogue();dialogue.NextLine();dialogue.gameObject.SetActive(false);dialogue.gameObject.SetActive(true);
        Check(!gm.HasMap&&!gm.HasMissionStarted,"Interrupted dialogue does not grant map or start mission");
        dialogue.StartDialogue();bool lockedThroughout=true;while(dialogue.IsOpen){lockedThroughout &= !gm.HasMap;dialogue.NextLine();}
        Check(lockedThroughout&&gm.HasMap&&gm.HasMissionStarted,"NPC grants map only after final initial line");
        Check(!gm.ReceiveMapFromNpc(),"Map reward is idempotent");
        dialogue.StartDialogue();while(dialogue.IsOpen)dialogue.NextLine();Check(gm.HasMap,"Reminder dialogue preserves gift");
        SceneFade.TryLoad("Beach");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        map=Find<IslandMapUI>();player=Find<PlayerClickMovement>();
        Check(gm.HasMap&&Named("OpenMap").activeSelf,"Gift persists across scenes and reveals button");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.M);yield return new WaitForSecondsRealtime(.3f);
        Check(map.IsOpen&&!player.IsMovementEnabled,"M opens gifted map and locks movement");
        Check(Named("IllustratedIslandMap").GetComponent<RawImage>().texture!=null,"Illustrated map texture assigned");
        foreach(var location in IslandMapGraphic.Locations)CheckClickable("MapPin"+location);
        Check(!Named("StoryObjectiveCard").activeSelf,"Map hides overlapping objective HUD");
        CheckClickable("MapPinHouse");CheckClickable("CloseMap");
        Click("MapPinHouse");Check(map.SelectedLocation=="House"&&!map.CanTravel&&!Button("MapTravel").interactable,"Unvisited pin shows details but cannot travel");
        map.Travel();Check(!SceneFade.IsRunning,"Direct call cannot bypass visit requirement");CaptureWorld("map-first-visit.png");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.Escape);
        Check(!map.IsOpen&&!gm.IsPaused&&player.IsMovementEnabled,"Escape closes map without pausing and releases player");
        SceneFade.TryLoad("House");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        map=Find<IslandMapUI>();map.Toggle();Check(!map.CanTravel,"Current location cannot travel to itself");
        map.SelectLocation("Beach");Check(map.CanTravel,"Visited beach enables travel");map.Travel();map.Travel();
        yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        Check(SceneManager.GetActiveScene().name=="Beach"&&gm.HasVisitedLocation("House"),"Fast travel changes scene once and preserves visits");
        gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);gm.TryCollectCrystal(GameManager.CrystalId.Triangle);
        map=Find<IslandMapUI>();map.Toggle();yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);Check(!Find<CrystalInventoryUI>().IsOpen,"Inventory cannot overlap map");
        map.SelectLocation("House");map.Travel();yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        Check(gm.CrystalCount==1&&gm.RecoveredMemoryCount==1,"Fast travel preserves inventory and memories");
        map=Find<IslandMapUI>();Find<CrystalInventoryUI>().Toggle();map.Toggle();Check(!map.IsOpen,"Map blocked by inventory");Find<CrystalInventoryUI>().Close();
        Find<ScrollInteraction>().Interact();map.Toggle();Check(!map.IsOpen,"Map blocked by puzzle");Find<ScrollInteraction>().CloseScroll();
        gm.SetPaused(true);map.Toggle();Check(!map.IsOpen,"Map blocked during pause");gm.SetPaused(false);
        map.Toggle();map.SelectLocation("Cave");yield return new WaitForSecondsRealtime(.3f);CaptureWorld("map-objective.png");map.Close();
        var audio=GameAudio.Instance;Check(Object.FindObjectsByType<GameAudio>(FindObjectsSortMode.None).Length==1,"One persistent audio service across trips");
        foreach(GameAudio.Cue cue in Enum.GetValues(typeof(GameAudio.Cue))){var clip=Resources.Load<AudioClip>("CartoonSfx/"+cue);Check(clip!=null&&clip.samples>0,"Audio clip loads: "+cue);}
        int count=audio.PlayedCount;GameAudio.Play(GameAudio.Cue.Confirm);Check(audio.PlayedCount==count+1&&audio.LastCue==GameAudio.Cue.Confirm,"Action cues routed to audio service");
        gm.SetPaused(true);count=audio.PlayedCount;GameAudio.Play(GameAudio.Cue.Pickaxe);Check(audio.PlayedCount==count,"Paused world suppresses new strikes");GameAudio.Play(GameAudio.Cue.Click);Check(audio.PlayedCount==count+1,"UI remains audible during pause");gm.SetPaused(false);
        var settings=SettingsManager.Instance;float volume=settings.SFXVolume;
        bool muted;try {settings.SetSFXVolume(0);muted=true;foreach(var source in audio.GetComponentsInChildren<AudioSource>())muted &= source.volume==0;}finally{settings.SetSFXVolume(volume);}
        Check(muted,"Existing SFX control mutes both channels and restored preference");
        SceneFade.TryLoad("Cave");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        map=Find<IslandMapUI>();player=Find<PlayerClickMovement>();var mining=Find<MiningSpot>();player.transform.position=mining.ApproachPosition;
        mining.StartDiscovery();map.Toggle();Check(mining.IsMining&&!map.IsOpen,"Map cannot interrupt mining");yield return new WaitForSecondsRealtime(.9f);Check(audio.LastCue==GameAudio.Cue.Pickaxe,"Mining produces timed pickaxe feedback");mining.CancelMining();
        gm.ResetMission();SceneFade.TryLoad("Beach");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;
        Check(gm.HasVisitedLocation("Beach")&&!gm.HasVisitedLocation("House")&&!gm.HasVisitedLocation("Cave"),"New game clears visited destinations");
        map=Find<IslandMapUI>();yield return PressKey(keyboard,UnityEngine.InputSystem.Key.M);
        Check(!gm.HasMap&&!map.IsOpen&&!Named("OpenMap").activeSelf,"New game removes map and locks access again");
        gm.StartMission();map.Toggle();Check(!map.IsOpen&&!gm.HasMap,"Starting mission alone does not award NPC map");
    }
}
