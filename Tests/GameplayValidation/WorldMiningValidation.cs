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

public static class WorldMiningValidation
{
    public static void Run()
    {
        SessionState.SetBool("WorldMiningValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class WorldMiningValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("WorldMiningValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<WorldMiningValidationRunner>();
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
        File.WriteAllLines("world-mining-results.txt", results);
        SessionState.SetBool("WorldMiningValidation", false);
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

    // Render explícito: CaptureScreenshot não grava imagens em algumas sessões batch.
    // A mudança de Canvas existe só nesta cópia de teste e é restaurada em seguida.
    private static void CapturePanel(string filename)
    {
        var canvas = Named("MinigPanel").GetComponentInParent<Canvas>();
        var originalMode = canvas.renderMode;
        var originalCamera = canvas.worldCamera;
        int originalLayer = canvas.gameObject.layer;
        float originalDistance = canvas.planeDistance;
        var cameraObject = new GameObject("Puzzle Preview Camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.10f, 0.13f);
        camera.cullingMask = 1 << 5;
        camera.orthographic = true;
        camera.orthographicSize = 360;
        camera.transform.position = new Vector3(0, 0, -100);
        var target = new RenderTexture(1280, 720, 24);
        target.Create();
        camera.targetTexture = target;
        var previousTarget = RenderTexture.active;
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            canvas.gameObject.layer = 5;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            var colors = pixels.GetPixels32();
            if (!Array.Exists(colors, color => !color.Equals(colors[0])))
                throw new Exception("Panel capture is blank");
            File.WriteAllBytes(filename, pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previousTarget;
            canvas.renderMode = originalMode;
            canvas.gameObject.layer = originalLayer;
            canvas.worldCamera = originalCamera;
            canvas.planeDistance = originalDistance;
            camera.targetTexture = null;
            target.Release();
            Destroy(target);
            Destroy(pixels);
            Destroy(cameraObject);
            Canvas.ForceUpdateCanvases();
        }
    }

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

    private void CheckTextFits()
    {
        Canvas.ForceUpdateCanvases();
        foreach (var text in Named("MinigPanel").GetComponentsInChildren<TMP_Text>())
        {
            text.ForceMeshUpdate();
            if (text.preferredWidth > text.rectTransform.rect.width + 2f && text.textWrappingMode == TextWrappingModes.NoWrap)
                throw new Exception("Unwrapped text overflow: " + text.name);
            if (text.preferredHeight > text.rectTransform.rect.height + 2f)
                throw new Exception("Text overflow: " + text.name + " " + text.preferredHeight + "/" + text.rectTransform.rect.height);
        }
        Check(true, "Active HUD text fits its containers");
        foreach (var symbol in Named("MinigPanel").GetComponentsInChildren<CrystalSymbolGraphic>())
            Check(symbol.GetComponent<CanvasRenderer>() != null, "Symbol has rendering component: " + symbol.name);
    }

    private static void CaptureWorld(string filename)
    {
        var camera = Camera.main;
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
            camera.targetTexture=oldTarget;RenderTexture.active=oldActive;target.Release();Destroy(target);Destroy(texture);
            Canvas.ForceUpdateCanvases();
        }
    }
    private static void Chance(float value) => typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(Find<MiningSpot>(),value);
    private static IEnumerator PressKey(UnityEngine.InputSystem.Keyboard keyboard,UnityEngine.InputSystem.Key key) {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
        yield return null;yield return null;
    }
    private IEnumerator Tests()
    {
        yield return null; yield return null;
        var gm=GameManager.Instance;
        var player=Find<PlayerClickMovement>(); var spot=Find<MiningSpot>();
        Check(!Named("PlayerInventory").activeSelf,"Inventory hidden before mission");
        player.transform.position=spot.ApproachPosition;
        yield return null;
        player.RequestMining(spot); yield return null;
        Check(!spot.IsMining && gm.CrystalCount==0,"Rock locked before first memory");
        Check(!Named("MinigPanel").activeSelf,"Collection no longer opens the shape selection panel");
        gm.StartMission(); gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper); yield return null;
        Check(!Named("PlayerInventory").activeSelf && Named("InventoryKeyHint").activeSelf,"Inventory starts closed with key hint");
        Check(Mathf.Approximately((float)typeof(MiningSpot).GetField("miningDuration",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(spot),4f),"Default attempts take four seconds");
        Check(Mathf.Approximately((float)typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(spot),.35f),"Default discovery chance is 35 percent");
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("InventoryTestKeyboard");
        var inventory=Find<CrystalInventoryUI>();
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);
        Check(inventory.IsOpen && Named("PlayerInventory").activeSelf && !player.IsMovementEnabled,"I opens inventory and blocks world interactions");
        CheckClickable("CloseInventoryButton");
        CaptureWorld("keyboard-inventory-open.png");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.Escape);
        Check(!inventory.IsOpen && !gm.IsPaused && player.IsMovementEnabled,"Escape closes inventory without pausing game");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.I);
        Check(!inventory.IsOpen && player.IsMovementEnabled,"I also closes inventory");
        inventory.Toggle();Click("CloseInventoryButton");
        Check(!inventory.IsOpen && player.IsMovementEnabled,"Close button releases movement");
        inventory.Toggle();gm.SetPaused(true);yield return null;
        Check(!inventory.IsOpen && gm.IsPaused,"External pause closes inventory safely");
        gm.SetPaused(false);inventory.Toggle();inventory.enabled=false;
        Check(player.IsMovementEnabled,"Disabling inventory releases its movement lock");inventory.enabled=true;
        Chance(1f);
        Check(!Named("InventoryIcon0").activeSelf,"Empty slot does not reveal the crystal");
        // O mesmo raycast usado pelo movimento deve atingir a rocha, e não o chão.
        var screen=Camera.main.WorldToScreenPoint(spot.transform.position+new Vector3(0,0,-.5f));
        Check(Physics.Raycast(Camera.main.ScreenPointToRay(screen),out var hit) && hit.collider.GetComponentInParent<MiningSpot>()==spot,"Camera ray reaches mineable rock collider");
        player.transform.position=spot.ApproachPosition+Vector3.left*.6f;
        var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position=screen };
        var uiHits = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,uiHits);
        Check(uiHits.Count==0,"Rock click is not covered by UI"+(uiHits.Count>0 ? ": "+uiHits[0].gameObject.name : ""));
        // O Editor batch fica em segundo plano; mantém os eventos sintéticos habilitados.
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var mouse=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("WorldMiningTestMouse");
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen });
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen }.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left));
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState { position=screen });
        yield return new WaitForSeconds(.5f);
        Check(spot.IsMining && Vector3.Distance(player.transform.position,spot.ApproachPosition)<.08f,"Character walks to rock and starts extraction (position="+player.transform.position+", pointer="+mouse.position.ReadValue()+")");
        Check(!player.IsMovementEnabled,"Mining locks movement");
        inventory.Toggle(); Check(!inventory.IsOpen,"Inventory cannot interrupt active mining");
        Check(Named("MiningPickaxe").activeSelf,"World pickaxe visible during extraction");
        var rotation=Named("MiningPickaxe").transform.rotation;
        yield return new WaitForSeconds(.15f);
        Check(Quaternion.Angle(rotation,Named("MiningPickaxe").transform.rotation)>1,"Pickaxe swings while mining");
        Check(gm.CrystalCount==0 && spot.Progress>0 && spot.Progress<1,"No reward before extraction completes");
        spot.StartDiscovery(); player.RequestMining(spot);
        gm.SetPaused(true); float progress=spot.Progress;
        yield return new WaitForSecondsRealtime(.2f);
        Check(Mathf.Approximately(progress,spot.Progress),"Pause freezes extraction"); gm.SetPaused(false);
        CaptureWorld("world-mining-action.png");
        Click("CancelMiningButton"); yield return null;
        Check(!spot.IsMining && player.IsMovementEnabled && gm.CrystalCount==0,"Cancel restores movement without granting crystal");
        Check(!Named("MiningPickaxe").activeSelf,"Cancel hides pickaxe");
        Chance(0f);player.RequestMining(spot);yield return new WaitForSeconds(4.8f);
        Check(gm.CrystalCount==0 && !spot.IsMining && player.IsMovementEnabled && spot.Result.Contains("Nenhum cristal"),"Unsuccessful attempt gives feedback and no item");
        CaptureWorld("mining-unsuccessful.png");Chance(1f);
        player.RequestMining(spot); yield return new WaitForSeconds(4.8f);
        Check(gm.CrystalCount==1 && gm.HasCrystal(GameManager.CrystalId.Triangle),"First extraction deposits first crystal (count="+gm.CrystalCount+", mining="+spot.IsMining+", progress="+spot.Progress+", movement="+player.IsMovementEnabled+", position="+player.transform.position+")");
        Check(Named("InventoryIcon0").activeSelf && Named("InventoryItem0").GetComponent<TMP_Text>().text=="TRIÂNGULO","Collected crystal appears in inventory");
        Check(Named("WorldMiningMessage").GetComponent<TMP_Text>().text.Contains("inventário"),"Collection confirms inventory deposit");
        Find<MiningUI>().SelectSquare();
        Check(spot.SelectedCrystal!=GameManager.CrystalId.Square,"Hidden shape selection cannot choose the next reward");
        Check(!inventory.IsOpen,"Finding a crystal does not force inventory open");
        inventory.Toggle();CaptureWorld("world-mining-inventory.png");inventory.Close();
        SceneManager.LoadScene("Cave"); yield return null; yield return null;
        Check(gm.CrystalCount==1 && Named("InventoryIcon0").activeSelf && !Named("PlayerInventory").activeSelf,"Inventory survives Cave scene transition and stays closed");
        Find<CrystalInventoryUI>().Toggle();Check(Named("PlayerInventory").activeSelf,"Inventory opens in Cave too");
        SceneManager.LoadScene("Island"); yield return null; yield return null;
        player=Find<PlayerClickMovement>();spot=Find<MiningSpot>();player.transform.position=spot.ApproachPosition;Chance(1f);
        player.RequestMining(spot);yield return new WaitForSeconds(4.8f);
        Check(gm.CrystalCount==2 && gm.HasCrystal(GameManager.CrystalId.Circle),"Second extraction finds a different crystal");
        player.RequestMining(spot);yield return new WaitForSeconds(.2f);
        SceneManager.LoadScene("Cave");yield return null;yield return null;
        Check(gm.CrystalCount==2 && Find<PlayerClickMovement>().IsMovementEnabled,"Leaving scene cancels unfinished extraction");
        SceneManager.LoadScene("Island");yield return null;yield return null;
        player=Find<PlayerClickMovement>();spot=Find<MiningSpot>();player.transform.position=spot.ApproachPosition;Chance(1f);
        player.RequestMining(spot);yield return new WaitForSeconds(4.8f);
        Check(gm.CrystalCount==3 && gm.HasCrystal(GameManager.CrystalId.Square),"Third extraction completes inventory without duplicates");
        Check(Named("MinigPanel").activeSelf && Named("CrystalSockets").activeSelf && !Named("WorldMiningPanel").activeSelf,"Three crystals unlock matching stage");
        player.RequestMining(spot);yield return null;Check(!spot.IsMining,"Exhausted rock cannot grant more crystals");
        CheckTextFits();CheckClickable("CrystalSocket1");CheckClickable("CrystalCircleButton");
        Click("CrystalTriangleButton");Click("CrystalSocket1");yield return null;
        Check(gm.FilledCrystalSocketCount==0 && Named("InventoryIcon0").activeSelf,"Wrong socket preserves inventory");
        Click("CrystalSocket2");yield return null;
        Check(gm.FilledCrystalSocketCount==1 && !Named("InventoryIcon0").activeSelf && Named("InventoryItem0").GetComponent<TMP_Text>().text=="ENCAIXADO","Correct match marks item used in inventory");
        Click("CrystalCircleButton");Click("CrystalSocket1");Click("CrystalSquareButton");Click("CrystalSocket3");yield return null;
        Check(gm.HasMemory(GameManager.MemoryId.StormNight),"Using three inventory crystals recovers memory");
        Check(Named("InventoryHeading").GetComponent<TMP_Text>().text.Contains("0 CRISTAIS"),"Used crystals no longer counted as carried");
        CaptureWorld("world-mining-memory.png");
        Find<CrystalInventoryUI>().Toggle();gm.ResetMission();yield return null;
        Check(gm.CrystalCount==0 && !Named("PlayerInventory").activeSelf,"Reset clears inventory and hides HUD");
    }
}

