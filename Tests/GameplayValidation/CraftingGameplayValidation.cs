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

public static class CraftingGameplayValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("CraftingGameplayValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class CraftingGameplayValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("CraftingGameplayValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<CraftingGameplayValidationRunner>();
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
        File.WriteAllLines("crafting-gameplay-results.txt", results);
        SessionState.SetBool("CraftingGameplayValidation", false);
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
      yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;
      UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
      UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
      var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("CraftKeyboard");
      var gm=GameManager.Instance;gm.ReadHouseNotice();gm.StartMission();gm.CollectBackpack();gm.CollectPickaxe();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
      foreach(GameManager.CrystalId id in Enum.GetValues(typeof(GameManager.CrystalId)))gm.TryCollectCrystal(id);
      gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryCollectKeyMaterials();
      SceneManager.LoadScene("House");yield return null;yield return new WaitForSecondsRealtime(1);
      var inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;yield return null;Check(inv.IsOpen,"Illustrated backpack opens");
      Check(Field<bool>(inv,"illustratedBag"),"Backpack illustrated theme enabled");CheckClickable("ItemSlot1");Click("ItemSlot1");Check(Field<TMP_Text>(inv,"itemDescription").text.Contains("Peça de metal"),"Pocket selection updates description");Check(Field<RawImage>(inv,"detailIcon").texture!=null,"Detail icon assigned");CaptureWorld("crafting-bag-materials.png");inv.Close();
      var craft=Find<KeyCraftingUI>();Find<ScrollInteraction>().Interact();yield return null;yield return null;Check(craft.IsOpen&&craft.Stage==KeyCraftingUI.CraftStage.Mold,"Workbench opens minigame instead of granting key");Check(!Find<PlayerClickMovement>().IsMovementEnabled,"Workbench locks movement");
      craft.PlaceMetal();Check(craft.Stage==KeyCraftingUI.CraftStage.Mold,"Cannot place metal before mold");CheckClickable("CraftPlaceMold");Click("CraftPlaceMold");Check(craft.Stage==KeyCraftingUI.CraftStage.Metal,"Click places mold");CheckClickable("CraftPlaceMetal");Click("CraftPlaceMetal");Check(craft.Stage==KeyCraftingUI.CraftStage.Shaping,"Click fits metal into mold");
      craft.Collect();Check(!gm.HasLighthouseKey,"Cannot collect unfinished key");craft.Strike();Check(craft.SuccessfulHits==0,"Mistimed strike does not advance");Check(gm.GetItemCount(GameManager.ItemId.KeyBlank)==1&&gm.GetItemCount(GameManager.ItemId.KeyMold)==1,"Miss preserves both materials");CaptureWorld("crafting-workbench.png");
      Find<PauseController>().HandleEscape();Check(!craft.IsOpen&&Find<PlayerClickMovement>().IsMovementEnabled,"Escape closes workbench and restores movement");Check(gm.CanCraftLighthouseKey,"Cancelling keeps materials");
      Find<ScrollInteraction>().Interact();Check(craft.IsOpen&&craft.SuccessfulHits==0&&craft.Stage==KeyCraftingUI.CraftStage.Mold,"Reopening resets local minigame");Click("CraftPlaceMold");Click("CraftPlaceMetal");yield return new WaitForSeconds(.2f);
      gm.SetPaused(true);yield return null;float frozen=craft.CursorValue;craft.Strike();yield return new WaitForSecondsRealtime(.3f);Check(craft.CursorValue==frozen&&craft.SuccessfulHits==0,"Pause freezes cursor and blocks strikes");gm.SetPaused(false);
      for(int i=0;i<3;i++){
        float deadline=Time.realtimeSinceStartup+8;while((Mathf.Abs(craft.CursorValue-craft.TargetCenter)>.025f||Field<float>(craft,"cooldown")>0)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(Time.realtimeSinceStartup<deadline,"Timing target reachable "+i);
        if(i==1)yield return PressKey(keyboard,UnityEngine.InputSystem.Key.Space);else{CheckClickable("CraftAction");Click("CraftAction");}
        Check(craft.SuccessfulHits==i+1,"Correct timed strike advances "+(i+1));craft.Strike();Check(craft.SuccessfulHits==i+1,"Rapid repeated strike does not skip stage");
      }
      Check(craft.Stage==KeyCraftingUI.CraftStage.Ready&&!gm.HasLighthouseKey,"Three strikes prepare key without automatic reward");CaptureWorld("crafting-key-ready.png");Click("CraftAction");Check(gm.HasLighthouseKey&&gm.GetItemCount(GameManager.ItemId.KeyBlank)==0&&gm.GetItemCount(GameManager.ItemId.KeyMold)==0,"Collecting consumes recipe and creates key");Check(craft.Stage==KeyCraftingUI.CraftStage.Complete,"Crafting shows completion");craft.Collect();Check(gm.GetItemCount(GameManager.ItemId.LighthouseKey)==1,"No duplicate key on repeated collection");Click("CraftAction");Check(!craft.IsOpen&&Find<PlayerClickMovement>().IsMovementEnabled,"Completion closes correctly");
      inv.Toggle();yield return null;yield return null;Click("ItemSlot1");CaptureWorld("crafting-bag-key.png");Check(Field<TMP_Text>(inv,"itemDescription").text.Contains("Chave do farol"),"Crafted key visible in illustrated backpack");inv.Close();craft.Open();Check(!craft.IsOpen,"Cannot replay crafting after key completion");
      foreach(string name in new[]{"Beach","Island","Cave","Lighthouse"}){SceneManager.LoadScene(name);yield return null;yield return new WaitForSecondsRealtime(1);inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;Check(inv.IsOpen&&gm.HasLighthouseKey,"Backpack and key persist in "+name);Check(Field<bool>(inv,"illustratedBag"),"Illustrated backpack configured in "+name);inv.Close();}
    }
}
