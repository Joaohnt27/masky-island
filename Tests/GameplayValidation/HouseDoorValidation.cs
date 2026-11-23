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

public static class HouseDoorValidation
{
    public static void Run()
    {
        SessionState.SetBool("HouseDoorValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity"); EditorApplication.EnterPlaymode();
    }
}
public class HouseDoorValidationRunner : MonoBehaviour
{
    readonly List<string> results = new List<string>(); string error; Mouse mouse; Keyboard keyboard;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("HouseDoorValidation", false)) return;
        var go = new GameObject("House door validation"); DontDestroyOnLoad(go); go.AddComponent<HouseDoorValidationRunner>();
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
        Time.timeScale = 1; File.WriteAllLines("house-door-results.txt", results);
        SessionState.SetBool("HouseDoorValidation", false); EditorApplication.Exit(code);
    }
    void Check(bool ok, string text) { if (!ok) throw new Exception(text); results.Add("PASS: " + text); }
    static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
    static DoorInteraction HouseDoor() => Object.FindObjectsByType<DoorInteraction>(FindObjectsInactive.Exclude).First(d => d.SceneName == "House");
    Vector2 Screen(DoorInteraction door) => Camera.main.WorldToScreenPoint(door.GetComponent<Collider>().bounds.center);
    void Point(Vector2 point, bool click = false) => InputSystem.QueueStateEvent(mouse,
        click ? new MouseState { position = point }.WithButton(MouseButton.Left) : new MouseState { position = point });
    IEnumerator Tests()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        mouse = InputSystem.AddDevice<Mouse>(); keyboard = InputSystem.AddDevice<Keyboard>(); Point(Vector2.zero);
        yield return new WaitForSecondsRealtime(1);
        var door = HouseDoor(); var animation = door.GetComponent<AnimatedDoor>(); var player = Find<PlayerClickMovement>();
        Check(animation != null && animation.Progress == 0 && !door.ReadyToEnter, "House door starts closed");
        Check(Resources.Load<AudioClip>("CartoonSfx/DoorOpen") != null && Resources.Load<AudioClip>("CartoonSfx/DoorClose") != null, "Door sounds imported");
        Capture("house-door-center-closed.png");
        Point(Screen(door)); yield return new WaitForSeconds(.7f);
        Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hoverHit);
        Check(animation.IsFullyOpen && player.HoveredDoor == door, "Hover opens illustrated door; progress=" + animation.Progress +
            "; hover=" + player.HoveredDoor + "; hit=" + hoverHit.collider + "; enabled=" + player.IsMovementEnabled +
            "; mouse=" + Mouse.current.position.ReadValue() + "; expected=" + Screen(door) +
            "; UI=" + UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
        Check(SceneManager.GetActiveScene().name == "Island" && player.TargetDoor == null, "Hover does not move player or change scene");
        Check(GameAudio.Instance.LastCue == GameAudio.Cue.DoorOpen, "Opening uses door sound");
        Capture("house-door-center-open.png");
        Point(Vector2.zero); yield return new WaitForSecondsRealtime(.55f);
        Check(animation.Progress == 0 && GameAudio.Instance.LastCue == GameAudio.Cue.DoorClose, "Mouse exit closes door with sound");
        Point(Screen(door)); yield return new WaitForSecondsRealtime(.16f);
        Check(animation.Progress > 0 && animation.Progress < 1, "Opening has intermediate frames");
        Point(Vector2.zero); yield return new WaitForSecondsRealtime(.15f);
        Point(Screen(door)); yield return new WaitForSecondsRealtime(.6f);
        Check(animation.IsFullyOpen, "Rapid hover reversal recovers without restarting a coroutine");
        GameManager.Instance.SetPaused(true); yield return null; yield return null;
        float paused = animation.Progress; Point(Vector2.zero); yield return new WaitForSecondsRealtime(.5f);
        Check(Mathf.Approximately(animation.Progress, paused), "Pause freezes door animation");
        GameManager.Instance.SetPaused(false); yield return new WaitForSecondsRealtime(.5f);
        Check(animation.Progress == 0, "Door responds again after resume");
        player.AcquireMovementLock(this); Point(Screen(door)); yield return new WaitForSecondsRealtime(.6f);
        Check(animation.Progress == 0 && player.HoveredDoor == null, "Dialogue/inventory lock blocks door hover");
        Point(Vector2.zero); player.ReleaseMovementLock(this); yield return null;
        player.transform.position = door.ApproachPosition + Vector3.right * 3;
        Point(Screen(door), true); yield return null; Point(Vector2.zero);
        yield return new WaitForSecondsRealtime(.65f);
        Check(player.TargetDoor == door && animation.IsFullyOpen && SceneManager.GetActiveScene().name == "Island", "Click keeps door open while approaching with pointer elsewhere");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); yield return new WaitForSecondsRealtime(.08f);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSecondsRealtime(.5f);
        Check(player.TargetDoor == null && animation.Progress == 0 && SceneManager.GetActiveScene().name == "Island", "WASD cancels entry and closes unhovered door");
        player.transform.position = door.ApproachPosition;
        Point(Screen(door), true); yield return null; Point(Vector2.zero);
        yield return new WaitForSecondsRealtime(.08f);
        Check(SceneManager.GetActiveScene().name == "Island" && !SceneFade.IsRunning, "Arrival waits until door finishes opening");
        float deadline = Time.realtimeSinceStartup + 8;
        while ((SceneManager.GetActiveScene().name != "House" || SceneFade.IsRunning) && Time.realtimeSinceStartup < deadline) yield return null;
        Check(SceneManager.GetActiveScene().name == "House" && !SceneFade.IsRunning, "Fully open door transitions once to House");
        Check(Find<DoorInteraction>().ReadyToEnter, "Other exits keep existing behavior");
        SceneManager.LoadScene("Beach"); yield return new WaitForSecondsRealtime(.6f);
        var dialogue = Find<DialogueController>(); int guard = 0;
        while (dialogue != null && dialogue.IsOpen && guard++ < 20) dialogue.NextLine();
        Point(Vector2.zero); yield return new WaitForSecondsRealtime(.5f);
        door = HouseDoor(); animation = door.GetComponent<AnimatedDoor>();
        Check(animation != null && animation.Progress == 0, "Beach house has closed animated entrance");
        Capture("house-door-beach-closed.png");
        Point(Screen(door)); yield return new WaitForSecondsRealtime(.7f);
        Check(animation.IsFullyOpen, "Beach entrance opens on hover"); Capture("house-door-beach-open.png");
        Point(Vector2.zero); SceneManager.LoadScene("Island"); yield return new WaitForSecondsRealtime(.6f);
        Check(HouseDoor().GetComponent<AnimatedDoor>().Progress == 0, "Returning to center resets door closed");
        Check(Find<NPCInteraction>().GetComponentInParent<Animator>() != null && Find<PlayerSpriteAnimation>() != null, "NPC idle and player animation preserved");
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
