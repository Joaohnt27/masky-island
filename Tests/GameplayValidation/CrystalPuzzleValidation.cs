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

public static class CrystalPuzzleValidation
{
    public static void Run()
    {
        SessionState.SetBool("CrystalPuzzleValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class CrystalPuzzleValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("CrystalPuzzleValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<CrystalPuzzleValidationRunner>();
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
        File.WriteAllLines("crystal-puzzle-results.txt", results);
        SessionState.SetBool("CrystalPuzzleValidation", false);
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

    private IEnumerator Tests()
    {
        yield return null; yield return null;
        var gm = GameManager.Instance;
        var player = Find<PlayerClickMovement>();
        var spot = Find<MiningSpot>();
        var ui = Find<MiningUI>();
        player.transform.position = spot.transform.position;
        yield return null;
        Check(Named("MinigPanel").activeSelf && !Button("MinigButton").interactable &&
            Named("CrystalStepTitle").GetComponent<TMP_Text>().text.Contains("pergaminho"), "Locked HUD explains the prerequisite");
        Check(!Named("CrystalSockets").activeSelf && !Named("MemoryReveal").activeSelf, "Future steps stay hidden");
        CheckTextFits(); CapturePanel("crystal-hud-locked.png");
        gm.StartMission();
        Check(!spot.CanMine && !gm.TryCollectCrystal(GameManager.CrystalId.Triangle), "Mission alone cannot unlock collection");
        gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        yield return null;
        Check(Button("MinigButton").interactable && Named("CollectionActions").activeSelf &&
            Button("MinigButton").GetComponentInChildren<TMP_Text>().text.Contains("TRIÂNGULO"), "Primary action names the selected crystal");
        Check(Button("CrystalTriangleButton").GetComponentInChildren<CrystalSymbolGraphic>().Shape == GameManager.CrystalId.Triangle &&
            Button("CrystalCircleButton").GetComponentInChildren<CrystalSymbolGraphic>().Shape == GameManager.CrystalId.Circle &&
            Button("CrystalSquareButton").GetComponentInChildren<CrystalSymbolGraphic>().Shape == GameManager.CrystalId.Square,
            "Cards show all three vector shapes");
        CheckClickable("CrystalTriangleButton"); CheckClickable("CrystalCircleButton");
        CheckClickable("CrystalSquareButton"); CheckClickable("MinigButton"); CheckClickable("CloseCrystalPanel");
        CheckTextFits(); CapturePanel("crystal-hud-collect.png");
        Click("MinigButton"); Click("MinigButton"); Click("CrystalCircleButton");
        Check(spot.IsMining && !player.IsMovementEnabled && spot.SelectedCrystal == GameManager.CrystalId.Triangle,
            "Mining still blocks duplicate attempts, selection and movement");
        yield return new WaitForSeconds(1f);
        Check(spot.Progress > 0.1f && spot.Progress < 0.6f && gm.CrystalCount == 0 && Named("MinigProgressBar").activeSelf,
            "Extraction progress is visible without early reward");
        CheckTextFits(); CapturePanel("crystal-hud-mining.png");
        gm.SetPaused(true); float progress = spot.Progress;
        ui.ClosePanel();
        yield return new WaitForSecondsRealtime(0.2f);
        Check(spot.Progress == progress && spot.IsMining && Named("MinigPanel").activeSelf, "Pause freezes extraction and prevents closing behind pause");
        gm.SetPaused(false);
        yield return new WaitForSeconds(2.2f);
        Check(gm.CrystalCount == 1 && gm.HasCrystal(GameManager.CrystalId.Triangle) && player.IsMovementEnabled,
            "Three-second extraction grants one crystal");
        Check(spot.SelectedCrystal == GameManager.CrystalId.Circle && Button("MinigButton").interactable &&
            Button("MinigButton").GetComponentInChildren<TMP_Text>().text.Contains("CÍRCULO"), "Next uncollected crystal is selected automatically");
        Click("CrystalTriangleButton");
        Check(spot.SelectedCrystal == GameManager.CrystalId.Circle && !Button("CrystalTriangleButton").interactable,
            "Collected vein cannot be selected again during collection");
        ui.PlaceFirst();
        Check(gm.FilledCrystalSocketCount == 0 && !Named("CrystalSockets").activeSelf,
            "Hidden socket callbacks cannot skip collection phase");
        Click("CrystalSquareButton");
        Check(spot.SelectedCrystal == GameManager.CrystalId.Square &&
            Button("MinigButton").GetComponentInChildren<TMP_Text>().text.Contains("QUADRADO"), "Explicit choice updates primary action");
        Click("MinigButton");
        yield return new WaitForSeconds(0.2f);
        Click("CloseCrystalPanel");
        Check(!spot.IsMining && player.IsMovementEnabled && gm.CrystalCount == 1 && !Named("MinigPanel").activeSelf,
            "Cancel and leave releases movement without reward");
        ui.SelectCircle();
        Check(spot.SelectedCrystal == GameManager.CrystalId.Square, "Closed HUD rejects callbacks");
        player.transform.position += Vector3.left * 5; yield return null;
        player.transform.position = spot.transform.position; yield return null;
        Click("MinigButton");
        player.transform.position += Vector3.left * 5; yield return null; yield return null;
        Check(!spot.IsMining && gm.CrystalCount == 1 && !Named("MinigPanel").activeSelf, "Leaving area cancels extraction and hides HUD");
        player.transform.position = spot.transform.position; yield return null;
        Click("MinigButton");
        yield return SceneManager.LoadSceneAsync("Cave"); yield return null;
        Check(gm.CrystalCount == 1 && Find<PlayerClickMovement>().IsMovementEnabled, "Scene change cancels partial extraction");
        yield return SceneManager.LoadSceneAsync("Island"); yield return null;
        spot = Find<MiningSpot>(); player = Find<PlayerClickMovement>(); ui = Find<MiningUI>();
        player.transform.position = spot.transform.position; yield return null;
        Check(spot.SelectedCrystal == GameManager.CrystalId.Circle && gm.CrystalCount == 1, "Returning resumes correct collection objective");
        Click("MinigButton"); yield return new WaitForSeconds(3.2f);
        Check(gm.CrystalCount == 2 && spot.SelectedCrystal == GameManager.CrystalId.Square, "Second collection advances to last vein");
        Click("MinigButton"); yield return new WaitForSeconds(3.2f);
        Check(gm.CrystalCount == 3 && Named("CrystalSockets").activeSelf && !Named("CollectionActions").activeSelf,
            "All crystals automatically reveal matching step and hide mining controls");
        Check(spot.SelectedCrystal == GameManager.CrystalId.Triangle &&
            Named("CrystalInstructions").GetComponent<TMP_Text>().text.Contains("TRIÂNGULO"), "Matching instruction names selected shape");
        CheckClickable("CrystalSocket1"); CheckClickable("CrystalSocket2"); CheckClickable("CrystalSocket3");
        CheckTextFits(); CapturePanel("crystal-hud-fit.png");
        Click("CrystalSocket1");
        Check(gm.FilledCrystalSocketCount == 0 && gm.CrystalCount == 3 &&
            Named("MinigResultText").GetComponent<TMP_Text>().text.Contains("Não é esse"), "Wrong match explains retry without losing crystal");
        CheckTextFits(); CapturePanel("crystal-hud-error.png");
        Click("CrystalSocket2");
        Check(gm.FilledCrystalSocketCount == 1 && spot.SelectedCrystal == GameManager.CrystalId.Circle &&
            Button("CrystalSocket2").GetComponentInChildren<CrystalSymbolGraphic>().gameObject.activeSelf,
            "Correct socket shows its shape and selects next available crystal");
        Click("CrystalSocket2");
        Check(gm.FilledCrystalSocketCount == 1, "Completed sockets cannot duplicate progress");
        yield return SceneManager.LoadSceneAsync("Cave"); yield return null;
        yield return SceneManager.LoadSceneAsync("Island"); yield return null;
        spot = Find<MiningSpot>(); player = Find<PlayerClickMovement>(); ui = Find<MiningUI>();
        player.transform.position = spot.transform.position; yield return null;
        Check(Named("CrystalSockets").activeSelf && gm.FilledCrystalSocketCount == 1 && spot.SelectedCrystal == GameManager.CrystalId.Circle,
            "Matching stage and partial sockets survive scene round trip");
        Click("CrystalSquareButton"); Click("CrystalSocket3");
        Check(gm.FilledCrystalSocketCount == 2 && gm.RecoveredMemoryCount == 1, "Two matches do not finish puzzle");
        Click("CrystalSocket1"); yield return null;
        Check(gm.HasMemory(GameManager.MemoryId.StormNight) && gm.RecoveredMemoryCount == 2 && !gm.IsGameWon(),
            "Final match grants second memory once without ending adventure");
        Check(Named("MemoryReveal").activeSelf && !Named("CrystalCards").activeSelf && !Named("CrystalSockets").activeSelf &&
            !Named("CollectionActions").activeSelf, "Completion replaces controls with memory presentation");
        Check(Named("MinigResultText").GetComponent<TMP_Text>().text.Contains("Na noite da tempestade") &&
            Named("MemoryNextStep").GetComponent<TMP_Text>().text.Contains("estranho"), "Completion gives story and concrete next step");
        CheckClickable("CloseCrystalPanel");
        CheckTextFits(); CapturePanel("crystal-hud-complete.png");
        Click("CrystalSocket1"); Click("MinigButton");
        Check(gm.RecoveredMemoryCount == 2 && gm.CrystalCount == 3, "Hidden completed controls cannot duplicate reward");
        ui.ClosePanel();
        var dialogue = Find<DialogueController>(); dialogue.StartDialogue();
        Check(Named("DialogueText").GetComponent<TMP_Text>().text.Contains("tempestade"), "Suggested NPC conversation acknowledges second memory");
        for (int i = 0; i < 10 && dialogue.IsOpen; i++) dialogue.NextLine();
        gm.RestartGame(); while (SceneFade.IsRunning) yield return null;
        Check(gm.CrystalCount == 0 && gm.FilledCrystalSocketCount == 0 && gm.RecoveredMemoryCount == 0, "Restart clears all story progress");
        gm.StartMission(); gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        gm.TryCollectCrystal(GameManager.CrystalId.Triangle);
        gm.ReturnToMainMenu(); while (SceneFade.IsRunning) yield return null;
        Check(gm.CrystalCount == 0 && gm.RecoveredMemoryCount == 0, "Menu exit resets inventory and memories");
    }
}
