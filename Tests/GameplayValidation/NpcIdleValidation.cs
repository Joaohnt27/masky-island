#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class NpcIdleValidation
{
    public static void Run()
    {
        SessionState.SetBool("NpcIdleValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        EditorApplication.EnterPlaymode();
    }
}
public class NpcIdleValidationRunner : MonoBehaviour
{
    readonly List<string> results = new List<string>();
    string error;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("NpcIdleValidation", false)) return;
        var host = new GameObject("NPC idle validation");
        DontDestroyOnLoad(host); host.AddComponent<NpcIdleValidationRunner>();
    }
    void Start() { Application.logMessageReceived += Log; StartCoroutine(Guard(Tests())); }
    void Log(string text, string stack, LogType type)
    {
        if (stack.Contains("UnityEditor.Search.SearchDatabase") && !stack.Contains("Assets/Scripts")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = text + stack;
    }
    IEnumerator Guard(IEnumerator tests)
    {
        while (true)
        {
            object current = null; bool more;
            try { more = tests.MoveNext(); if (error != null) throw new Exception(error); if (more) current = tests.Current; }
            catch (Exception ex) { results.Add("FAIL: " + ex); Finish(1); yield break; }
            if (!more) break;
            yield return current;
        }
        results.Add("ALL PASSED"); Finish(0);
    }
    void Finish(int code)
    {
        Time.timeScale = 1; File.WriteAllLines("npc-idle-results.txt", results);
        SessionState.SetBool("NpcIdleValidation", false); EditorApplication.Exit(code);
    }
    void Check(bool condition, string text) { if (!condition) throw new Exception(text); results.Add("PASS: " + text); }
    static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
    IEnumerator Tests()
    {
        yield return new WaitForSecondsRealtime(1);
        var npc = Find<NPCInteraction>(); var renderer = npc.GetComponentInParent<SpriteRenderer>();
        var animator = renderer.GetComponent<Animator>();
        Check(animator != null && animator.runtimeAnimatorController != null && !animator.applyRootMotion, "NPC has idle controller without root motion");
        Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Idle starts automatically");
        var origin = renderer.transform.position; var scale = renderer.transform.localScale;
        var colliderBounds = npc.GetComponent<Collider>().bounds;
        var seen = new HashSet<Sprite>();
        float deadline = Time.realtimeSinceStartup + 4;
        while (Time.realtimeSinceStartup < deadline) { seen.Add(renderer.sprite); yield return null; }
        Check(seen.Count == 4, "All four breathing frames play");
        Check(renderer.transform.position == origin && renderer.transform.localScale == scale, "Idle keeps NPC position and scale");
        Check(npc.GetComponent<Collider>().bounds == colliderBounds, "Click collider remains fixed");
        Capture("npc-idle-center.png");
        GameManager.Instance.SetPaused(true); yield return null; yield return null;
        var frozen = renderer.sprite; float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        yield return new WaitForSecondsRealtime(.7f);
        Check(renderer.sprite == frozen && Mathf.Approximately(time, animator.GetCurrentAnimatorStateInfo(0).normalizedTime), "Pause freezes NPC idle");
        GameManager.Instance.SetPaused(false); yield return new WaitForSecondsRealtime(.6f);
        Check(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > time, "Idle resumes after pause");
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = Find<PlayerClickMovement>(); player.transform.position = npc.ApproachPosition + Vector3.down * .8f;
        var mouse = InputSystem.AddDevice<Mouse>();
        Vector2 screen = Camera.main.WorldToScreenPoint(npc.GetComponent<Collider>().bounds.center);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }); yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left)); yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
        deadline = Time.realtimeSinceStartup + 6;
        while (!Find<DialogueController>().IsOpen && Time.realtimeSinceStartup < deadline) yield return null;
        var dialogue = Find<DialogueController>();
        Check(dialogue.IsOpen && !player.IsMovementEnabled, "Click still opens NPC dialogue and locks player");
        time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        yield return new WaitForSecondsRealtime(.6f);
        Check(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > time, "NPC breathes during conversation");
        int guard = 0; while (dialogue.IsOpen && guard++ < 30) dialogue.NextLine();
        Check(!dialogue.IsOpen && GameManager.Instance.HasMissionStarted, "Dialogue still starts mission");
        Check(GameManager.Instance.HasMap, "NPC still delivers map");
        SceneManager.LoadScene("House"); yield return new WaitForSecondsRealtime(.4f);
        SceneManager.LoadScene("Island"); yield return new WaitForSecondsRealtime(.6f);
        renderer = Find<NPCInteraction>().GetComponentInParent<SpriteRenderer>();
        Check(renderer.sprite.name.StartsWith("NpcIdle_") && renderer.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Idle restored on return to center");
        Check(Find<PlayerSpriteAnimation>() != null, "Player animations preserved");
    }
    static void Capture(string path)
    {
        var cam = Camera.main; var old = cam.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(1440, 810, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var image = new Texture2D(1440, 810, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1440, 810), 0, 0); image.Apply(); File.WriteAllBytes(path, image.EncodeToPNG());
        cam.targetTexture = old; RenderTexture.active = active; Object.Destroy(image); rt.Release(); Object.Destroy(rt);
    }
}
#endif
