#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class HouseExitValidation
{
    public static void Run()
    {
        SessionState.SetBool("HouseExitValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/House.unity"); EditorApplication.EnterPlaymode();
    }
}
public class HouseExitValidationRunner : MonoBehaviour
{
    readonly List<string> results = new List<string>(); string error; Mouse mouse; Keyboard keyboard;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("HouseExitValidation", false)) return;
        var go = new GameObject("House exit validation"); DontDestroyOnLoad(go); go.AddComponent<HouseExitValidationRunner>();
    }
    void Start() { Application.logMessageReceived += Log; StartCoroutine(Guard(Tests())); }
    void Log(string message, string trace, LogType type)
    {
        if (trace.Contains("UnityEditor.Search.SearchDatabase") && !trace.Contains("Assets/Scripts")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = message + trace;
    }
    IEnumerator Guard(IEnumerator tests)
    {
        while (true)
        {
            bool more; object current = null;
            try { more = tests.MoveNext(); if (error != null) throw new Exception(error); if (more) current = tests.Current; }
            catch (Exception ex) { results.Add("FAIL: " + ex); Finish(1); yield break; }
            if (!more) break; yield return current;
        }
        results.Add("ALL PASSED"); Finish(0);
    }
    void Finish(int code)
    {
        Time.timeScale = 1; File.WriteAllLines("house-exit-results.txt", results);
        SessionState.SetBool("HouseExitValidation", false); EditorApplication.Exit(code);
    }
    void Check(bool ok, string text) { if (!ok) throw new Exception(text); results.Add("PASS: " + text); }
    static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
    Vector2 Screen(DoorInteraction door) => Camera.main.WorldToScreenPoint(door.GetComponent<Collider>().bounds.center);
    void Point(Vector2 point, bool click = false) => InputSystem.QueueStateEvent(mouse,
        click ? new MouseState { position = point }.WithButton(MouseButton.Left) : new MouseState { position = point });
    IEnumerator Tests()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        mouse = InputSystem.AddDevice<Mouse>(); keyboard = InputSystem.AddDevice<Keyboard>(); Point(Vector2.zero);
        yield return new WaitForSeconds(1);
        var door = Find<DoorInteraction>(); var animation = door.GetComponent<AnimatedDoor>(); var player = Find<PlayerClickMovement>();
        Check(door.SceneName == "Island" && animation != null && animation.Progress == 0, "Inside exit starts closed and leads to center");
        Capture("house-exit-closed.png"); yield return null; yield return null;
        Point(Screen(door)); yield return new WaitForSeconds(.7f);
        Check(animation.IsFullyOpen && player.HoveredDoor == door, "Hover opens door from inside");
        Check(SceneManager.GetActiveScene().name == "House" && player.TargetDoor == null, "Hover alone does not leave house");
        Check(GameAudio.Instance.LastCue == GameAudio.Cue.DoorOpen, "Inside door reuses opening sound");
        Capture("house-exit-open.png");
        Point(Vector2.zero); yield return new WaitForSeconds(.55f);
        Check(animation.Progress == 0 && GameAudio.Instance.LastCue == GameAudio.Cue.DoorClose, "Mouse exit closes door with sound");
        player.AcquireMovementLock(this); Point(Screen(door)); yield return new WaitForSeconds(.6f);
        Check(animation.Progress == 0, "Reading and inventory movement locks block hover");
        player.ReleaseMovementLock(this); yield return new WaitForSeconds(.7f);
        GameManager.Instance.SetPaused(true); yield return null; yield return null;
        float paused = animation.Progress; Point(Vector2.zero); yield return new WaitForSecondsRealtime(.5f);
        Check(Mathf.Approximately(animation.Progress, paused), "Pause freezes inside door");
        GameManager.Instance.SetPaused(false); yield return new WaitForSeconds(.55f);
        Check(animation.Progress == 0, "Closing resumes after pause");
        player.transform.position = door.ApproachPosition + Vector3.right * 3;
        Point(Screen(door), true); yield return null; Point(Vector2.zero); yield return new WaitForSeconds(.65f);
        Check(player.TargetDoor == door && animation.IsFullyOpen && SceneManager.GetActiveScene().name == "House", "Click holds door open during approach");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); yield return new WaitForSeconds(.08f);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSeconds(.5f);
        Check(player.TargetDoor == null && animation.Progress == 0, "WASD cancels exit and closes door");
        player.transform.position = door.ApproachPosition; Point(Screen(door), true); yield return null; Point(Vector2.zero);
        yield return new WaitForSeconds(.08f);
        Check(SceneManager.GetActiveScene().name == "House" && !SceneFade.IsRunning, "Arrival waits for complete opening");
        float deadline = Time.realtimeSinceStartup + 10;
        while ((SceneManager.GetActiveScene().name != "Island" || SceneFade.IsRunning) && Time.realtimeSinceStartup < deadline) yield return null;
        Check(SceneManager.GetActiveScene().name == "Island" && !SceneFade.IsRunning, "Inside door transitions to center");
        Check(Find<NPCInteraction>() != null && Find<PlayerSpriteAnimation>() != null, "Center NPC and player animation preserved");
        var outside = Object.FindObjectsByType<DoorInteraction>(FindObjectsInactive.Exclude).First(d => d.SceneName == "House");
        player = Find<PlayerClickMovement>(); player.transform.position = outside.ApproachPosition;
        Point(Screen(outside), true); yield return null; Point(Vector2.zero);
        deadline = Time.realtimeSinceStartup + 10;
        while ((SceneManager.GetActiveScene().name != "House" || SceneFade.IsRunning) && Time.realtimeSinceStartup < deadline) yield return null;
        Check(SceneManager.GetActiveScene().name == "House" && !SceneFade.IsRunning, "Outside door still returns player to house");
        Check(Find<AnimatedDoor>().Progress == 0 && Find<ScrollInteraction>() != null && Find<BackpackPickup>() != null, "Return restores closed door and keeps house interactions");
    }
    static void Capture(string path)
    {
        var camera = Camera.main; var old = camera.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(1672, 941, 24); camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
        var texture = new Texture2D(1672, 941, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1672, 941), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
        camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(texture); rt.Release(); Object.Destroy(rt);
    }
}
#endif
