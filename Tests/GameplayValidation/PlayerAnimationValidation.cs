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

public static class PlayerAnimationValidation
{
    public static void Run()
    {
        SessionState.SetBool("PlayerAnimationValidation", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class PlayerAnimationValidationRunner : MonoBehaviour
{
    readonly List<string> results = new List<string>();
    Keyboard keyboard;
    string error;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin()
    {
        if (!Application.isBatchMode || !SessionState.GetBool("PlayerAnimationValidation", false)) return;
        var host = new GameObject("Player animation validation");
        DontDestroyOnLoad(host);
        host.AddComponent<PlayerAnimationValidationRunner>();
    }

    void Start()
    {
        Application.logMessageReceived += OnLog;
        StartCoroutine(Guard(Tests()));
    }
    void OnLog(string message, string trace, LogType type)
    {
        if (trace.Contains("UnityEditor.Search.SearchDatabase") && !trace.Contains("Assets/Scripts")) return;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = message + "\n" + trace;
    }
    IEnumerator Guard(IEnumerator tests)
    {
        while (true)
        {
            object current = null;
            bool more;
            try { more = tests.MoveNext(); if (error != null) throw new Exception(error); if (more) current = tests.Current; }
            catch (Exception ex) { results.Add("FAIL: " + ex); Finish(1); yield break; }
            if (!more) break;
            yield return current;
        }
        results.Add("ALL PASSED"); Finish(0);
    }
    void Finish(int code)
    {
        Time.timeScale = 1;
        File.WriteAllLines("player-animation-results.txt", results);
        SessionState.SetBool("PlayerAnimationValidation", false);
        EditorApplication.Exit(code);
    }
    void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        results.Add("PASS: " + message);
    }
    static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
    void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

    IEnumerator Tests()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard = InputSystem.AddDevice<Keyboard>();
        yield return new WaitForSecondsRealtime(1);
        var player = Find<PlayerClickMovement>();
        var animation = player.GetComponent<PlayerSpriteAnimation>();
        var animator = player.GetComponent<Animator>();
        var sprite = player.GetComponent<SpriteRenderer>();
        Check(animation != null && animator.runtimeAnimatorController != null && !animator.applyRootMotion, "Controller assigned without root motion");
        Check(!animation.IsWalking && animation.Facing == PlayerSpriteAnimation.Direction.Front, "Initial idle faces front");
        Check(animator.runtimeAnimatorController.animationClips.Count(c => c.name.StartsWith("Walk") || c.name.StartsWith("Idle")) == 8, "Four walking and four directional idle clips");
        var directions = new[] { PlayerSpriteAnimation.Direction.Front, PlayerSpriteAnimation.Direction.Back,
            PlayerSpriteAnimation.Direction.Left, PlayerSpriteAnimation.Direction.Right };
        var keys = new[] { Key.S, Key.W, Key.A, Key.D };
        for (int i = 0; i < 4; i++)
        {
            player.transform.position = new Vector3(0, -2.5f, -1);
            yield return null; yield return null;
            Keys(keys[i]);
            yield return new WaitForSecondsRealtime(.16f);
            Check(animation.IsWalking && animation.Facing == directions[i], "WASD walk " + directions[i]);
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Walk" + directions[i]), "Walking state " + directions[i]);
            var first = sprite.sprite;
            yield return new WaitForSecondsRealtime(.17f);
            Check(sprite.sprite != first, "Walking changes drawn frames " + directions[i]);
            Capture("player-walk-" + directions[i] + ".png");
            Keys(); yield return new WaitForSecondsRealtime(.08f);
            Check(!animation.IsWalking && animation.Facing == directions[i] &&
                animator.GetCurrentAnimatorStateInfo(0).IsName("Idle" + directions[i]), "Idle preserves facing " + directions[i]);
        }
        var idleStart = sprite.sprite;
        yield return new WaitForSecondsRealtime(.52f);
        Check(sprite.sprite != idleStart, "Idle breathing changes frame");
        player.AcquireMovementLock(this); Keys(Key.A);
        var position = player.transform.position;
        yield return new WaitForSecondsRealtime(.2f);
        Check(!animation.IsWalking && player.transform.position == position, "Dialogue/mining movement lock stops walking");
        Keys(); player.ReleaseMovementLock(this);
        player.transform.position = new Vector3(7.4f, -2.5f, -1);
        yield return null; yield return null;
        Keys(Key.D); yield return new WaitForSecondsRealtime(.15f);
        Check(!animation.IsWalking, "Walking into map boundary stays idle");
        Keys(); yield return null;
        player.transform.position = new Vector3(0, -2.5f, -1);
        yield return null; yield return null;
        Keys(Key.W); yield return new WaitForSecondsRealtime(.1f);
        GameManager.Instance.SetPaused(true);
        // Amostra após o frame da solicitação; o Animator pode já ter avaliado esse frame.
        yield return null; yield return null;
        var frozen = sprite.sprite;
        position = player.transform.position;
        yield return new WaitForSecondsRealtime(.2f);
        Check(player.transform.position == position && sprite.sprite == frozen && !animation.IsWalking, "Pause freezes sprite and movement");
        Keys(); GameManager.Instance.SetPaused(false);
        yield return null; yield return null;
        Check(!animation.IsWalking, "Resume returns to idle");

        // Exercita a aproximação por clique com um objeto real, usando o raycast do jogo.
        var target = Find<NPCInteraction>();
        player.transform.position = target.ApproachPosition + Vector3.down * 1.5f;
        yield return null; yield return null;
        var mouse = InputSystem.AddDevice<Mouse>();
        Vector2 screen = Camera.main.WorldToScreenPoint(target.GetComponent<Collider>().bounds.center);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
        yield return new WaitForSecondsRealtime(.12f);
        Check(animation.IsWalking, "Click approach animates walking");
        float deadline = Time.realtimeSinceStartup + 6;
        while (!Find<DialogueController>().IsOpen && Time.realtimeSinceStartup < deadline) yield return null;
        yield return null;
        Check(Find<DialogueController>().IsOpen && !animation.IsWalking, "Arriving at NPC opens dialogue and stops walk");

        foreach (string scene in new[] { "Beach", "House", "Island", "Cave", "Lighthouse" })
        {
            SceneManager.LoadScene(scene);
            yield return new WaitForSecondsRealtime(.35f);
            var current = Find<PlayerSpriteAnimation>();
            Check(current != null && !current.IsWalking && current.GetComponent<Animator>().runtimeAnimatorController != null,
                "Scene arrival ready in " + scene);
            Check(Find<PlayerClickMovement>().GetComponent<SpriteRenderer>().sprite.name.StartsWith("Idle"), "Imported sprite visible in " + scene);
            if (scene == "Cave") Capture("player-animation-cave.png");
        }
    }

    static void Capture(string path)
    {
        var camera = Camera.main;
        var old = camera.targetTexture;
        var active = RenderTexture.active;
        var render = new RenderTexture(1440, 810, 24);
        camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
        var texture = new Texture2D(1440, 810, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1440, 810), 0, 0); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        camera.targetTexture = old; RenderTexture.active = active;
        Object.Destroy(texture); render.Release(); Object.Destroy(render);
    }
}
#endif
