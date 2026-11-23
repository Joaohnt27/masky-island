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

public static class ThirdPuzzleValidation
{
    public static void Run()
    {
        SessionState.SetBool("ThirdPuzzleValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class ThirdPuzzleValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("ThirdPuzzleValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<ThirdPuzzleValidationRunner>();
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
        File.WriteAllLines("third-puzzle-results.txt", results);
        SessionState.SetBool("ThirdPuzzleValidation", false);
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
    private static IEnumerator PressKey(UnityEngine.InputSystem.Keyboard keyboard,UnityEngine.InputSystem.Key key) {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
        yield return null;yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
        yield return null;yield return null;
    }
    private IEnumerator Tests()
    {
        yield return null;yield return null;
        var gm=GameManager.Instance;var player=Find<PlayerClickMovement>();var note=Named("Scroll_Interactable").GetComponent<ScrollInteraction>();
        var puzzle=Named("ScrollPanel").GetComponent<SymbolSequencePuzzle>();
        Check(!Named("ScrollPanel").activeSelf,"Note panel starts closed");
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("ThirdPuzzleKeyboard");
        player.transform.position=note.transform.position+Vector3.right*.8f;yield return null;
        Check(Named("InteractionPrompt").activeSelf,"Nearby note shows E prompt");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.E);
        Check(Named("ScrollPanel").activeSelf && !player.IsMovementEnabled,"E opens note and blocks walking");
        Check(!Button("BoatSymbolButton").interactable,"Puzzle locked before mission");
        Check(Named("ScrollText").GetComponent<TMP_Text>().text.Contains("capuz vermelho"),"Opening note still points to the NPC");
        Click("BoatSymbolButton");Check(puzzle.EnteredCount==0,"Direct callback cannot bypass inactive mission");
        Click("CloseButton");Check(player.IsMovementEnabled,"Closing note restores movement");
        gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);note.Interact();yield return null;
        Check(!Button("BoatSymbolButton").interactable && Named("ReturnPuzzleFeedback").GetComponent<TMP_Text>().text.Contains("cristais"),"First memory alone cannot unlock third puzzle");
        Check(!gm.TryRecoverMemory(GameManager.MemoryId.Return),"GameManager rejects early third memory");
        Click("BoatSymbolButton");Click("FishSymbolButton");Click("LighthouseSymbolButton");Check(gm.RecoveredMemoryCount==1,"Locked sequence grants no reward");
        Click("CloseButton");
        gm.TryCollectCrystal(GameManager.CrystalId.Triangle);gm.TryCollectCrystal(GameManager.CrystalId.Circle);gm.TryCollectCrystal(GameManager.CrystalId.Square);
        gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);
        Check(gm.RecoveredMemoryCount==2,"Crystal puzzle setup grants second memory");
        CaptureWorld("third-puzzle-note.png");note.Interact();yield return null;
        Check(Button("BoatSymbolButton").interactable,"Second memory unlocks note sequence");
        CheckClickable("BoatSymbolButton");CheckClickable("FishSymbolButton");CheckClickable("LighthouseSymbolButton");CheckClickable("CloseButton");
        foreach(var text in Named("ScrollPanel").GetComponentsInChildren<TMP_Text>()) {text.ForceMeshUpdate();Check(text.preferredHeight<=text.rectTransform.rect.height+2,"Text fits: "+text.name);}
        CaptureWorld("third-puzzle-clue.png");
        Find<CrystalInventoryUI>().Toggle();Check(!Find<CrystalInventoryUI>().IsOpen,"Inventory cannot overlap note interaction");
        Click("BoatSymbolButton");Check(puzzle.EnteredCount==1,"Boat starts correct sequence");
        Click("LighthouseSymbolButton");Check(puzzle.EnteredCount==0 && gm.RecoveredMemoryCount==2,"Wrong symbol resets attempt without erasing memories");
        Click("BoatSymbolButton");gm.SetPaused(true);Click("FishSymbolButton");Click("CloseButton");
        Check(puzzle.EnteredCount==1 && Named("ScrollPanel").activeSelf,"Pause blocks symbols and note close");gm.SetPaused(false);
        Click("CloseButton");note.Interact();yield return null;Check(puzzle.EnteredCount==0,"Closing and reopening clears partial sequence");
        Click("BoatSymbolButton");Click("FishSymbolButton");Click("LighthouseSymbolButton");yield return null;
        Check(puzzle.IsSolved && gm.RecoveredMemoryCount==3 && !gm.IsGameWon(),"Correct sequence recovers third memory without prematurely ending game");
        Check(Named("ReturnPuzzleFeedback").GetComponent<TMP_Text>().text.Contains("Você voltou para terminar"),"Third memory reveals reason for return");
        Check(!Button("BoatSymbolButton").interactable,"Solved puzzle disables symbol buttons");
        Click("BoatSymbolButton");Click("FishSymbolButton");Click("LighthouseSymbolButton");Check(gm.RecoveredMemoryCount==3,"Repeated callbacks cannot duplicate reward");
        CaptureWorld("third-memory.png");Click("CloseButton");
        SceneManager.LoadScene("Island");yield return null;yield return null;
        Check(gm.HasMemory(GameManager.MemoryId.Return),"Third memory persists on Island");
        var dialogue=Find<DialogueController>();dialogue.StartDialogue();
        var textField=(TMP_Text)typeof(DialogueController).GetField("dialogueText",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(dialogue);
        Check(textField.text.Contains("terminar o que começou"),"NPC acknowledges third memory");
        while(dialogue.IsOpen)dialogue.NextLine();
        SceneManager.LoadScene("Cave");yield return null;yield return null;
        note=Named("Scroll_Interactable").GetComponent<ScrollInteraction>();puzzle=Named("ScrollPanel").GetComponent<SymbolSequencePuzzle>();note.Interact();yield return null;
        Check(puzzle.IsSolved && !Button("BoatSymbolButton").interactable,"Revisited note remains solved");
        gm.ResetMission();yield return null;Check(!puzzle.IsSolved && puzzle.EnteredCount==0 && gm.RecoveredMemoryCount==0,"Reset clears all memories and puzzle progress");
        Click("CloseButton");Check(Find<PlayerClickMovement>().IsMovementEnabled,"Reset and close leave no movement lock");
    }
}
