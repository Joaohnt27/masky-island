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

public static class KeyInventoryValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("KeyInventoryValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class KeyInventoryValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("KeyInventoryValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<KeyInventoryValidationRunner>();
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
        File.WriteAllLines("key-inventory-results.txt", results);
        SessionState.SetBool("KeyInventoryValidation", false);
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
    private T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
    private IEnumerator Tests(){
     yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
     var gm=GameManager.Instance;gm.ReadHouseNotice();gm.StartMission();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
     Check(!gm.TryCollectKeyMaterials(),"Materials cannot be collected before three sockets");Check(!gm.TryCraftLighthouseKey(),"Cannot craft without materials");
     foreach(GameManager.CrystalId id in Enum.GetValues(typeof(GameManager.CrystalId))){Check(gm.TryCollectCrystal(id),"Collect crystal "+id);Check(gm.GetItemCount((GameManager.ItemId)(int)id)==1,"Crystal stored in general inventory");Check(!gm.TryCollectCrystal(id),"No duplicate crystal reward");}
     Check(!gm.TryPlaceCrystal(0,GameManager.CrystalId.Triangle)&&gm.GetItemCount(GameManager.ItemId.Triangle)==1,"Wrong socket preserves inventory item");
     Find<MiningUI>().ClosePanel();var inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;yield return null;Check(inv.IsOpen,"Inventory opens with existing backpack control");Check(Field<Button[]>(inv,"itemButtons").Count(b=>b.interactable)==4,"Inventory lists three crystals and pickaxe");CaptureWorld("key-inventory-crystals.png");inv.Close();
     gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);Check(gm.GetItemCount(GameManager.ItemId.Circle)==0&&gm.HasCrystal(GameManager.CrystalId.Circle),"Placed crystal leaves inventory while altar history persists");gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);Check(!gm.IsCaveCacheOpen,"Two sockets do not open cache");gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);yield return new WaitForSeconds(1);
     Check(gm.IsCaveCacheOpen&&gm.HasMemory(GameManager.MemoryId.StormNight),"Third socket opens cache and retains second memory");Check(!gm.HasCollectedKeyMaterials&&gm.GetItemCount(GameManager.ItemId.KeyBlank)==0,"Opening cache does not grant materials automatically");
     Find<MiningUI>().ClosePanel();yield return null;var cache=Find<CaveKeyCache>();Check(cache.GetComponent<Collider>().enabled,"Opened cache is clickable");CaptureWorld("key-cache-open.png");Find<PlayerClickMovement>().transform.position=new Vector3(0,-3,-1);Physics.SyncTransforms();var point=cache.GetComponent<Collider>().bounds.center;Physics.Raycast(Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(point)),out var hit);Check(hit.collider==cache.GetComponent<Collider>(),"Camera ray reaches cache collider");yield return WorldClick(point);
     var dialogue=Find<DialogueController>();float deadline=Time.realtimeSinceStartup+12;while(!dialogue.IsOpen&&Time.realtimeSinceStartup<deadline)yield return null;Check(dialogue.IsOpen,"World click approaches cache and opens collection dialogue");while(dialogue.IsOpen){dialogue.NextLine();yield return null;}
     Check(gm.HasCollectedKeyMaterials&&gm.GetItemCount(GameManager.ItemId.KeyBlank)==1&&gm.GetItemCount(GameManager.ItemId.KeyMold)==1,"Materials collected into inventory");Check(!gm.TryCollectKeyMaterials(),"Cache cannot duplicate materials");Check(!gm.TryCraftLighthouseKey(),"Crafting restricted to house");
     inv.Toggle();yield return null;yield return null;Check(Field<Button[]>(inv,"itemButtons").Count(b=>b.interactable)==3,"Inventory contains materials and tool, no used crystals");var slots=Field<Button[]>(inv,"itemButtons");slots[1].onClick.Invoke();Check(Field<TMP_Text>(inv,"itemDescription").text.Contains("Peça de metal"),"Selecting material shows its description");CaptureWorld("key-inventory-materials.png");inv.Close();
     SceneManager.LoadScene("House");yield return null;yield return new WaitForSecondsRealtime(1);Check(gm.CanCraftLighthouseKey,"Materials persist after scene change");gm.SetPaused(true);Check(!gm.TryCraftLighthouseKey(),"Cannot craft while paused");gm.SetPaused(false);
     Find<ScrollInteraction>().Interact();dialogue=Find<DialogueController>();Check(dialogue.IsOpen&&Field<TMP_Text>(dialogue,"dialogueText").text.Contains("molde"),"House workbench replaces third puzzle with crafting dialogue");Check(!gm.HasLighthouseKey,"Crafting is not granted before interaction finishes");while(dialogue.IsOpen){dialogue.NextLine();yield return null;}
     Check(gm.HasLighthouseKey&&gm.GetItemCount(GameManager.ItemId.KeyBlank)==0&&gm.GetItemCount(GameManager.ItemId.KeyMold)==0,"Crafting consumes both materials and creates key");Check(gm.HasMemory(GameManager.MemoryId.Return),"Crafting advances final memory progression");Check(!gm.TryCraftLighthouseKey(),"Key cannot be crafted twice");inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;yield return null;Field<Button[]>(inv,"itemButtons")[1].onClick.Invoke();CaptureWorld("key-inventory-crafted.png");inv.Close();
     gm.CollectJournal();gm.LearnJournal();Find<QuestJournalUI>().Toggle();yield return new WaitForSeconds(3);CaptureWorld("key-journal.png");Find<QuestJournalUI>().Close();
     SceneManager.LoadScene("Cave");yield return null;yield return new WaitForSecondsRealtime(1);cache=Find<CaveKeyCache>();Check(gm.HasLighthouseKey&&gm.FilledCrystalSocketCount==3,"Key and filled altar persist on return");Check(!cache.GetComponent<Collider>().enabled&&!Field<GameObject>(cache,"loot").activeSelf,"Collected cache stays empty on return");CaptureWorld("key-cache-empty.png");
     gm.ResetMission();yield return null;Check(!gm.HasLighthouseKey&&!gm.HasCollectedKeyMaterials&&!gm.IsCaveCacheOpen&&gm.CrystalCount==0,"New game resets items, cache and sockets");Check(!gm.TryRecoverMemory(GameManager.MemoryId.Return),"Old third-puzzle path cannot grant final memory without crafted key");
    }
}



