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

public static class MineralUIValidation
{
    public static void Run()
    {
        SessionState.SetBool("UnusedMapPresentation",false);
        SessionState.SetBool("MineralUIValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class MineralUIValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("MineralUIValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<MineralUIValidationRunner>();
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
        File.WriteAllLines("mineral-ui-results.txt", results);
        SessionState.SetBool("MineralUIValidation", false);
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
        yield return new WaitForSecondsRealtime(1);var gm=GameManager.Instance;gm.StartMission();gm.ReceiveMapFromNpc();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        var inv=Find<CrystalInventoryUI>();inv.Toggle();yield return new WaitForSecondsRealtime(.3f);
        Check(inv.IsOpen&&!Find<PlayerClickMovement>().IsMovementEnabled,"Redesigned inventory opens and locks movement");
        Check(Named("InventoryItem0").GetComponent<TMP_Text>().text.Contains("NÃO ENCONTRADO"),"Empty inventory explicitly marks missing crystals");
        Check(Named("InventoryIcon0").GetComponent<CrystalSymbolGraphic>().color.a<.2f,"Missing mineral is a faint preview");
        CaptureWorld("inventory-empty.png");CheckClickable("CloseInventoryButton");
        gm.TryCollectCrystal(GameManager.CrystalId.Triangle);yield return null;
        Check(Named("InventoryIcon0").GetComponent<CrystalSymbolGraphic>().mainTexture.name=="Triangle-v1","Triangle uses generated PNG");
        Check(Named("InventoryItem0").GetComponent<TMP_Text>().text.Contains("GUARDADO")&&Named("InventoryIcon0").GetComponent<CrystalSymbolGraphic>().color.a==1,"Collected mineral becomes opaque and marked stored");
        inv.Close();var map=Find<IslandMapUI>();map.Toggle();map.SelectLocation("Cave");yield return new WaitForSecondsRealtime(.3f);CaptureWorld("map-redesigned.png");
        Check(!map.CanTravel&&!Button("MapTravel").interactable,"Redesigned map preserves unvisited travel restriction");CheckClickable("CloseMap");CheckClickable("MapPinCave");map.Close();
        gm.TryCollectCrystal(GameManager.CrystalId.Circle);gm.TryCollectCrystal(GameManager.CrystalId.Square);
        SceneFade.TryLoad("Cave");yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
        Find<MiningUI>().ClosePanel();inv=Find<CrystalInventoryUI>();inv.Toggle();yield return new WaitForSecondsRealtime(.3f);CaptureWorld("inventory-full.png");
        Check(Named("InventoryHeading").GetComponent<TMP_Text>().text.Contains("3 CRISTAIS"),"Full inventory count remains accurate");
        foreach(string shape in new[]{"Triangle","Circle","Square"}){bool found=false;foreach(var g in Object.FindObjectsByType<CrystalSymbolGraphic>(FindObjectsInactive.Include))if(g.Shape.ToString()==shape&&g.mainTexture.name==shape+"-v1")found=true;Check(found,shape+": illustration assigned in cave UI");}
        inv.Close();Find<MiningUI>().OpenSockets();Click("CrystalTriangleButton");Click("CrystalSocket1");Check(gm.FilledCrystalSocketCount==0,"Wrong socket still preserves minerals");Click("CrystalTriangleButton");Click("CrystalSocket2");
        Check(gm.FilledCrystalSocketCount==1,"Matching mineral still fills correct socket");CaptureWorld("crystal-sockets-art.png");Find<MiningUI>().ClosePanel();inv.Toggle();yield return new WaitForSecondsRealtime(.3f);CaptureWorld("inventory-used.png");
        Check(Named("InventoryItem0").GetComponent<TMP_Text>().text.Contains("USADO NO ALTAR")&&Named("InventoryIcon0").GetComponent<CrystalSymbolGraphic>().color.a<.2f,"Used mineral distinguished from stored mineral");Check(Named("InventoryHeading").GetComponent<TMP_Text>().text.Contains("2 CRISTAIS"),"Inventory available count decrements after socket");inv.Close();
        foreach(string scene in new[]{"Beach","Island","Lighthouse"}){SceneFade.TryLoad(scene);yield return new WaitForSecondsRealtime(1);while(SceneFade.IsRunning)yield return null;yield return null;yield return null;
            inv=Find<CrystalInventoryUI>();inv.Toggle();yield return null;Check(inv.IsOpen&&Named("InventoryBackdrop").activeInHierarchy,scene+": redesigned inventory wired");inv.Close();map=Find<IslandMapUI>();map.Toggle();yield return null;Check(map.IsOpen,scene+": redesigned map wired");map.Close();}
        Check(!Named("StoryControlsBar").activeSelf,"Removed bottom bar stays removed");
    }
}
