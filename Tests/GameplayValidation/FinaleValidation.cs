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

public static class FinaleValidation
{
    public static void Run()
    {
        SessionState.SetBool("FinaleValidation", true);
        MainMenuController.ReturningFromGameplay = true;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class FinaleValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private string runtimeError;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("FinaleValidation", false)) return;
        var host = new GameObject("Crystal Puzzle Validation");
        DontDestroyOnLoad(host);
        host.AddComponent<FinaleValidationRunner>();
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
        File.WriteAllLines("finale-results.txt", results);
        SessionState.SetBool("FinaleValidation", false);
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
        yield return new WaitForSecondsRealtime(1f);
        var menu=Find<MainMenuController>();Check(menu!=null && menu.mainMenuPanel.activeSelf,"Start from ready main menu");
        menu.PlayGame();yield return new WaitForSecondsRealtime(1f);
        Check(SceneManager.GetActiveScene().name=="Cave","Play enters Cave");
        var gm=GameManager.Instance;Check(gm!=null && !gm.HasMissionStarted,"New game starts without mission");
        var scroll=Find<ScrollInteraction>();scroll.Interact();yield return null;Check(Named("ScrollText").GetComponent<TMP_Text>().text.Contains("capuz vermelho"),"Opening note provides initial direction");scroll.CloseScroll();
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1f);
        var player=Find<PlayerClickMovement>();var lighthouse=Find<LighthouseInteraction>();
        gm.CompleteMission();Check(!gm.IsGameWon(),"Global completion rejects missing memories");
        var dialogue=Find<DialogueController>();dialogue.StartDialogue();while(dialogue.IsOpen)dialogue.NextLine();Check(gm.IsMissionActive(),"NPC starts mission");
        player.transform.position=lighthouse.transform.position+Vector3.right;yield return null;
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("FinaleKeyboard");
        yield return PressKey(keyboard,UnityEngine.InputSystem.Key.E);
        Check(Named("LighthousePanel").activeSelf && !Button("ActivateLighthouse").interactable,"E opens lighthouse but missing memories lock activation");
        Click("ActivateLighthouse");Check(!lighthouse.IsRestoring && !gm.IsGameWon(),"Direct callback cannot bypass prerequisite");Click("CloseLighthouse");
        scroll=Find<ScrollInteraction>();scroll.Interact();Click("SunSymbolButton");Click("MoonSymbolButton");Click("StarSymbolButton");scroll.CloseScroll();
        Check(gm.RecoveredMemoryCount==1,"First puzzle completes through its buttons");
        var spot=Find<MiningSpot>();player.transform.position=spot.ApproachPosition;
        typeof(MiningSpot).GetField("crystalChance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(spot,1f);
        for(int i=0;i<3;i++){player.RequestMining(spot);yield return new WaitForSeconds(4.8f);}
        Check(gm.CrystalCount==3,"Three real extractions fill inventory (chance forced only in test)");
        Click("CrystalTriangleButton");Click("CrystalSocket2");Click("CrystalCircleButton");Click("CrystalSocket1");Click("CrystalSquareButton");Click("CrystalSocket3");Click("CloseCrystalPanel");
        Check(gm.RecoveredMemoryCount==2,"Crystal matches recover second memory");gm.CompleteMission();Check(!gm.IsGameWon(),"Two memories still cannot win");
        SceneFade.TryLoad("Cave");yield return new WaitForSecondsRealtime(1f);
        scroll=Find<ScrollInteraction>();scroll.Interact();Click("BoatSymbolButton");Click("FishSymbolButton");Click("LighthouseSymbolButton");scroll.CloseScroll();
        Check(gm.HasAllMemories && !gm.IsGameWon(),"Third puzzle makes lighthouse ready without auto-victory");
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1f);
        player=Find<PlayerClickMovement>();lighthouse=Find<LighthouseInteraction>();dialogue=Find<DialogueController>();dialogue.StartDialogue();
        string npcLines="";var dt=(TMP_Text)typeof(DialogueController).GetField("dialogueText",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(dialogue);
        while(dialogue.IsOpen){npcLines+=dt.text;dialogue.NextLine();}
        Check(npcLines.Contains("máscara") && npcLines.Contains("à esquerda"),"NPC explains final action and location");
        player.transform.position=lighthouse.transform.position+Vector3.right;yield return null;yield return PressKey(keyboard,UnityEngine.InputSystem.Key.E);
        Check(Button("ActivateLighthouse").interactable,"All memories unlock activation");CheckClickable("ActivateLighthouse");CheckClickable("CloseLighthouse");CaptureWorld("lighthouse-ready.png");
        Click("ActivateLighthouse");Click("ActivateLighthouse");yield return new WaitForSeconds(.4f);
        Check(lighthouse.IsRestoring && !gm.IsGameWon() && !player.IsMovementEnabled,"Activation is animated, single and blocks movement");
        Find<CrystalInventoryUI>().Toggle();Check(!Find<CrystalInventoryUI>().IsOpen,"Inventory cannot interrupt restoration");
        var scale=Named("LighthouseBeacon").transform.localScale;gm.SetPaused(true);yield return new WaitForSecondsRealtime(.25f);
        Check(Vector3.Distance(scale,Named("LighthouseBeacon").transform.localScale)<.001f,"Pause freezes restoration animation");Click("CloseLighthouse");Check(lighthouse.IsRestoring,"Paused close cannot cancel restoration");gm.SetPaused(false);
        yield return new WaitForSeconds(1.8f);Check(!gm.IsGameWon() && Named("LighthouseBeacon").transform.localScale.x>.6f,"Beacon visibly grows before victory");
        CaptureWorld("lighthouse-lighting.png");yield return new WaitForSeconds(1.6f);
        Check(gm.IsGameWon() && !gm.IsMissionActive() && !lighthouse.IsRestoring,"Restoration completes mission once");
        Check(Named("LighthouseBeacon").activeSelf && !Named("LighthousePanel").activeSelf,"Beacon remains lit after mechanism closes");
        Check(Named("MissionResultPanel").activeSelf && Named("EndGameMessage").GetComponent<TMP_Text>().text.Contains("partida"),"Victory presents story ending");
        Check(!player.IsMovementEnabled && !Find<MiningSpot>().CanDiscover,"Victory blocks movement and further mining");
        gm.SetPaused(true);Check(!gm.IsPaused,"Cannot pause over victory");
        foreach(var t in Named("MissionResultPanel").GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(t.preferredHeight<=t.rectTransform.rect.height+2,"Ending text fits: "+t.name);}
        CheckClickable("RestartButton");CheckClickable("MainMenuButton");CaptureWorld("island-ending.png");
        Click("RestartButton");yield return new WaitForSecondsRealtime(1f);
        Check(SceneManager.GetActiveScene().name=="Cave" && !gm.IsGameWon() && gm.RecoveredMemoryCount==0 && gm.CrystalCount==0,"Victory restart begins clean Cave session");
        // Verificação de cancelamento e rota de menu após a travessia completa acima.
        gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);
        gm.TryCollectCrystal(GameManager.CrystalId.Triangle);gm.TryCollectCrystal(GameManager.CrystalId.Circle);gm.TryCollectCrystal(GameManager.CrystalId.Square);
        gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
        SceneFade.TryLoad("Island");yield return new WaitForSecondsRealtime(1f);
        lighthouse=Find<LighthouseInteraction>();lighthouse.Interact();lighthouse.Activate();yield return new WaitForSeconds(.2f);gm.ResetMission();yield return null;
        Check(!lighthouse.IsRestoring && !gm.IsGameWon() && !Named("LighthouseBeacon").activeSelf && Find<PlayerClickMovement>().IsMovementEnabled,"Reset during restoration cancels without stale lock or light");
        gm.StartMission();gm.TryRecoverMemory(GameManager.MemoryId.LighthouseKeeper);gm.TryCollectCrystal(GameManager.CrystalId.Triangle);gm.TryCollectCrystal(GameManager.CrystalId.Circle);gm.TryCollectCrystal(GameManager.CrystalId.Square);gm.TryPlaceCrystal(0,GameManager.CrystalId.Circle);gm.TryPlaceCrystal(1,GameManager.CrystalId.Triangle);gm.TryPlaceCrystal(2,GameManager.CrystalId.Square);gm.TryRecoverMemory(GameManager.MemoryId.Return);
        lighthouse.Interact();lighthouse.Activate();yield return new WaitForSeconds(3.8f);Check(gm.IsGameWon(),"New restoration works after reset");
        Click("MainMenuButton");yield return new WaitForSecondsRealtime(1f);
        Check(SceneManager.GetActiveScene().name=="MainMenu" && !gm.IsGameWon() && gm.RecoveredMemoryCount==0,"Victory menu button resets progress and returns to menu");
    }
}
