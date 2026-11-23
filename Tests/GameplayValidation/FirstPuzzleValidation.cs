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

public static class FirstPuzzleValidation
{
    public static void Run()
    {
        SessionState.SetBool("FirstPuzzleValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class FirstPuzzleValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("FirstPuzzleValidation", false)) return;
        var host = new GameObject("First Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<FirstPuzzleValidationRunner>();
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
        File.WriteAllLines("first-puzzle-results.txt", results);
        SessionState.SetBool("FirstPuzzleValidation", false);
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
    private static Button Button(string name) => Named(name + "SymbolButton").GetComponent<Button>();
    private static void Click(string name) => Button(name).onClick.Invoke();

    // Render explícito: CaptureScreenshot não grava imagens em algumas sessões batch.
    // A mudança de Canvas existe só nesta cópia de teste e é restaurada em seguida.
    private static void CapturePanel(string filename)
    {
        var canvas = Named("ScrollPanel").GetComponentInParent<Canvas>();
        var originalMode = canvas.renderMode;
        var originalCamera = canvas.worldCamera;
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
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(filename, pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previousTarget;
            canvas.renderMode = originalMode;
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

    private IEnumerator Tests()
    {
        yield return null;
        yield return null;
        var gm = GameManager.Instance;
        var scroll = Find<ScrollInteraction>();
        var player = Find<PlayerClickMovement>();
        player.transform.position = scroll.transform.position;
        Check(!gm.HasMissionStarted && gm.RecoveredMemoryCount == 0, "New session has no memories or active mission");
        Check(!gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper), "Cannot obtain memory before NPC dialogue");
        scroll.Interact();
        var puzzle = Find<SymbolSequencePuzzle>();
        Check(puzzle != null && !player.IsMovementEnabled && !Button("Sun").interactable,
            "Existing scroll opens puzzle and locks movement; symbols unavailable before mission");
        Click("Sun"); Click("Moon"); Click("Star");
        Check(gm.RecoveredMemoryCount == 0, "Direct button callbacks cannot bypass mission prerequisite");
        scroll.CloseScroll();
        Check(player.IsMovementEnabled, "Closing scroll releases movement");

        var dialogue = Find<DialogueController>();
        var lines = (string[])typeof(DialogueController).GetField("dialogueLines", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialogue);
        dialogue.StartDialogue();
        for (int i = 0; i < lines.Length - 1; i++) dialogue.NextLine();
        Check(!gm.HasMissionStarted, "Mission waits for final introductory line");
        dialogue.NextLine();
        Check(gm.IsMissionActive() && player.IsMovementEnabled, "NPC starts mission and restores movement");
        scroll.Interact();
        Check(Button("Sun").interactable && Button("Moon").interactable && Button("Star").interactable,
            "All three authored buttons are available during mission");
        Check(Named("ScrollText").GetComponent<TMP_Text>().text ==
            "O sol se despede.\nA lua desperta.\nA estrela mostra o caminho.", "All three clue lines are saved correctly");
        yield return null;
        CapturePanel("puzzle-before.png");
        yield return new WaitForSeconds(0.3f);
        Click("Moon");
        Check(puzzle.EnteredCount == 0 && Named("PuzzleFeedback").GetComponent<TMP_Text>().text.Contains("recomeçou"),
            "Wrong first symbol resets attempt with visible feedback");
        Click("Sun");
        Check(puzzle.EnteredCount == 1, "Correct first symbol advances sequence");
        var pause = Find<PauseController>();
        pause.HandleEscape();
        Click("Moon");
        scroll.CloseScroll();
        Check(gm.IsPaused && puzzle.EnteredCount == 1 && Named("ScrollPanel").activeSelf && !Button("Sun").interactable,
            "Pause prevents symbol input and closing behind pause panel");
        pause.Resume();
        Check(!player.IsMovementEnabled && Button("Sun").interactable, "Resume retains scroll movement lock");
        Click("Moon");
        Check(puzzle.EnteredCount == 2, "Resume preserves partial sequence");
        scroll.CloseScroll();
        scroll.Interact();
        Check(puzzle.EnteredCount == 0, "Reopening unfinished puzzle resets partial attempt");
        Click("Sun"); Click("Star");
        Check(puzzle.EnteredCount == 0 && gm.RecoveredMemoryCount == 0, "Wrong later symbol also resets without reward");
        Click("Sun"); Click("Moon"); Click("Star");
        Check(puzzle.IsSolved && gm.RecoveredMemoryCount == 1 && gm.IsMissionActive() && !gm.IsGameWon(),
            "Sun Moon Star grants first memory without ending the game");
        Check(!Button("Sun").interactable && Named("PuzzleFeedback").GetComponent<TMP_Text>().text.Contains("Você cuidava da luz."),
            "Solved UI reveals memory and disables symbols");
        Click("Sun"); Click("Moon"); Click("Star");
        Check(gm.RecoveredMemoryCount == 1 && !gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper),
            "Repeated clicks and reward calls never duplicate memory");
        yield return null;
        Canvas.ForceUpdateCanvases();
        foreach (var text in Named("ScrollPanel").GetComponentsInChildren<TMP_Text>())
        {
            text.ForceMeshUpdate();
            Check(text.preferredHeight <= text.rectTransform.rect.height + 2f &&
                text.preferredWidth <= text.rectTransform.rect.width + 2f, "Text fits: " + text.name);
        }
        CapturePanel("puzzle-solved.png");
        yield return new WaitForSeconds(0.3f);
        scroll.CloseScroll();
        dialogue.StartDialogue();
        Check(Named("DialogueText").GetComponent<TMP_Text>().text.Contains("cuidava da luz"),
            "NPC acknowledges recovered memory");
        for (int i = 0; i < 10 && dialogue.IsOpen; i++) dialogue.NextLine();
        Check(player.IsMovementEnabled && gm.RecoveredMemoryCount == 1, "Repeat dialogue preserves memory and releases player");

        yield return SceneManager.LoadSceneAsync("Cave");
        yield return null;
        Check(GameManager.Instance == gm && gm.RecoveredMemoryCount == 1, "Memory persists in Cave");
        yield return SceneManager.LoadSceneAsync("Island");
        yield return null;
        scroll = Find<ScrollInteraction>();
        scroll.Interact();
        Check(Find<SymbolSequencePuzzle>().IsSolved && !Button("Sun").interactable && gm.RecoveredMemoryCount == 1,
            "Returning to Island restores solved puzzle");
        Check(Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length == 1, "No duplicated global manager");
        scroll.CloseScroll();
        gm.RestartGame();
        while (SceneFade.IsRunning) yield return null;
        Check(SceneManager.GetActiveScene().name == "Cave" && gm.RecoveredMemoryCount == 0 && !gm.HasMissionStarted,
            "Restart clears memories and mission");
        yield return SceneManager.LoadSceneAsync("Island");
        yield return null;
        scroll = Find<ScrollInteraction>();
        scroll.Interact();
        Check(!Find<SymbolSequencePuzzle>().IsSolved && !Button("Sun").interactable,
            "New game restores unsolved puzzle and NPC prerequisite");
        scroll.CloseScroll();
        gm.StartMission();
        gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        gm.ReturnToMainMenu();
        while (SceneFade.IsRunning) yield return null;
        Check(gm.RecoveredMemoryCount == 0 && !gm.HasMissionStarted, "Main menu exit also clears memories");
    }
}
