using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public enum MemoryId { LighthouseKeeper, StormNight, Return }
    public enum CrystalId { Triangle, Circle, Square }
    public enum ItemId { Triangle, Circle, Square, Pickaxe, KeyBlank, KeyMold, LighthouseKey }

    public static GameManager Instance;

    private readonly HashSet<string> visitedLocations = new HashSet<string>();
    public bool HasVisitedLocation(string scene) => visitedLocations.Contains(scene);
    public string ObjectiveScene => !HasMissionStarted ? (!HasReadHouseNotice ? "House" : "Island")
        : !HasBackpack ? "House"
        : !HasMemory(MemoryId.LighthouseKeeper) ? "House"
        : !HasMemory(MemoryId.StormNight) ? "Cave"
        : !HasCollectedKeyMaterials ? "Cave" : !HasLighthouseKey ? "House" : "LighthouseExterior";
    public static bool IsExplorationScene(string name) => name == "Beach" || name == "House" || name == "Island" || name == "Cave" || name == "Lighthouse" || name == "LighthouseExterior";
    private bool missionActive;
    private bool missionStarted;
    private bool gameWon;
    private readonly HashSet<MemoryId> recoveredMemories = new HashSet<MemoryId>();
    private readonly Dictionary<ItemId, int> items = new Dictionary<ItemId, int>();
    private readonly HashSet<int> filledCrystalSockets = new HashSet<int>();
    private static readonly CrystalId[] socketSolution = { CrystalId.Circle, CrystalId.Triangle, CrystalId.Square };
    public bool IsPaused { get; private set; }
    public bool BlocksGameplay => IsPaused || gameWon;
    public static bool IsGameplayScene => SceneManager.GetActiveScene().name == "Cave" ||
        SceneManager.GetActiveScene().name == "Island" || SceneManager.GetActiveScene().name == "Beach" ||
        SceneManager.GetActiveScene().name == "House" || SceneManager.GetActiveScene().name == "Lighthouse" || SceneManager.GetActiveScene().name == "LighthouseExterior";
    public bool HasVisitedHouse { get; private set; }
    public bool HasMap { get; private set; }
    public bool HasJournal { get; private set; }
    public bool HasReadHouseNotice { get; private set; }
    public bool HasBackpack { get; private set; }
    public bool HasPickaxe { get; private set; }
    public bool CollectPickaxe()
    {
        if (HasPickaxe || !HasMissionStarted || !HasBackpack || BlocksGameplay) return false;
        HasPickaxe = true;
        OnMissionStateChanged?.Invoke();
        return true;
    }
    public void ReadHouseNotice()
    {
        if (HasReadHouseNotice || BlocksGameplay) return;
        HasReadHouseNotice = true;
        OnMissionStateChanged?.Invoke();
    }
    public bool CollectBackpack()
    {
        if (HasBackpack || !HasMissionStarted || BlocksGameplay) return false;
        HasBackpack = true;
        OnMissionStateChanged?.Invoke();
        return true;
    }
    public bool HasLearnedJournal { get; private set; }
    public void CollectJournal()
    {
        if (HasJournal || BlocksGameplay) return;
        HasJournal = true;
        OnMissionStateChanged?.Invoke();
    }
    public void LearnJournal()
    {
        if (!HasJournal || HasLearnedJournal) return;
        HasLearnedJournal = true;
        OnMissionStateChanged?.Invoke();
    }
    public bool HasSeenBeachIntroduction { get; private set; }
    public void CompleteBeachIntroduction() => HasSeenBeachIntroduction = true;

    // A entrega é chamada pelo diálogo, nunca pelo simples início da missão.
    public bool ReceiveMapFromNpc()
    {
        if (HasMap || !HasMissionStarted || BlocksGameplay) return false;
        HasMap = true;
        OnMissionStateChanged?.Invoke();
        GameAudio.Play(GameAudio.Cue.Success);
        return true;
    }

    public event Action OnMissionStateChanged;
    public bool HasMissionStarted => missionStarted;
    public int RecoveredMemoryCount => recoveredMemories.Count;
    public int CrystalCount => (HasCrystal(CrystalId.Triangle) ? 1 : 0) + (HasCrystal(CrystalId.Circle) ? 1 : 0) + (HasCrystal(CrystalId.Square) ? 1 : 0);
    public int FilledCrystalSocketCount => filledCrystalSockets.Count;
    public bool HasAllMemories => HasMemory(MemoryId.LighthouseKeeper) && HasMemory(MemoryId.StormNight) && HasMemory(MemoryId.Return);
    public bool CanWorkOnCrystals => missionActive && HasBackpack && HasPickaxe && !BlocksGameplay && !SceneFade.IsRunning &&
        HasMemory(MemoryId.LighthouseKeeper) && !HasMemory(MemoryId.StormNight);
    public int GetItemCount(ItemId item) => item == ItemId.Pickaxe ? (HasPickaxe ? 1 : 0) : items.TryGetValue(item, out int count) ? count : 0;
    public bool HasLighthouseKey => GetItemCount(ItemId.LighthouseKey) > 0;
    public bool HasReadLighthouseClue { get; private set; }
    public bool HasUsedLighthouseKey { get; private set; }
    public bool IsLighthouseUnlocked { get; private set; }
    private bool CanUseLighthouseDoor => !BlocksGameplay && !SceneFade.IsRunning && SceneManager.GetActiveScene().name == "LighthouseExterior";
    public void ReadLighthouseClue()
    {
        if (!CanUseLighthouseDoor || HasReadLighthouseClue) return;
        HasReadLighthouseClue = true; OnMissionStateChanged?.Invoke();
    }
    public bool TryUseLighthouseKey()
    {
        if (!CanUseLighthouseDoor || !missionActive || !HasLighthouseKey || HasUsedLighthouseKey) return false;
        HasUsedLighthouseKey = true; OnMissionStateChanged?.Invoke(); return true;
    }
    public bool TryUnlockLighthouse(int code)
    {
        if (!CanUseLighthouseDoor || !missionActive || !HasLighthouseKey || !HasUsedLighthouseKey ||
            !HasReadLighthouseClue || IsLighthouseUnlocked || code != 403) return false;
        IsLighthouseUnlocked = true; OnMissionStateChanged?.Invoke(); return true;
    }
    public bool HasCollectedKeyMaterials { get; private set; }
    public bool IsCaveCacheOpen => FilledCrystalSocketCount == 3;
    public bool CanCraftLighthouseKey => missionActive && !BlocksGameplay && !SceneFade.IsRunning && HasBackpack &&
        HasMemory(MemoryId.StormNight) && !HasLighthouseKey && GetItemCount(ItemId.KeyBlank) > 0 && GetItemCount(ItemId.KeyMold) > 0;
    public bool TryCollectKeyMaterials()
    {
        if (!missionActive || BlocksGameplay || SceneFade.IsRunning || !HasBackpack || !IsCaveCacheOpen ||
            HasCollectedKeyMaterials || SceneManager.GetActiveScene().name != "Cave") return false;
        items[ItemId.KeyBlank] = 1; items[ItemId.KeyMold] = 1;
        HasCollectedKeyMaterials = true;
        OnMissionStateChanged?.Invoke();
        return true;
    }
    public bool TryCraftLighthouseKey()
    {
        if (!CanCraftLighthouseKey || SceneManager.GetActiveScene().name != "House") return false;
        items.Remove(ItemId.KeyBlank); items.Remove(ItemId.KeyMold);
        items[ItemId.LighthouseKey] = 1;
        recoveredMemories.Add(MemoryId.Return);
        OnMissionStateChanged?.Invoke();
        return true;
    }
    public bool HasCrystal(CrystalId crystal) => Enum.IsDefined(typeof(CrystalId), crystal) &&
        (GetItemCount((ItemId)(int)crystal) > 0 || IsCrystalSocketFilled(crystal == CrystalId.Circle ? 0 : crystal == CrystalId.Triangle ? 1 : 2));
    public bool IsCrystalSocketFilled(int slot) => filledCrystalSockets.Contains(slot);

    public bool TryCollectCrystal(CrystalId crystal)
    {
        if (!CanWorkOnCrystals || !Enum.IsDefined(typeof(CrystalId), crystal) || HasCrystal(crystal))
            return false;
        items[(ItemId)(int)crystal] = 1;
        OnMissionStateChanged?.Invoke();
        return true;
    }

    public bool TryPlaceCrystal(int slot, CrystalId crystal)
    {
        if (!CanWorkOnCrystals || slot < 0 || slot >= socketSolution.Length ||
            !HasCrystal(crystal) || socketSolution[slot] != crystal || !filledCrystalSockets.Add(slot))
            return false;
        items.Remove((ItemId)(int)crystal);
        if (filledCrystalSockets.Count == socketSolution.Length)
            TryRecoverMemory(MemoryId.StormNight);
        else
            OnMissionStateChanged?.Invoke();
        return true;
    }

    public bool HasMemory(MemoryId memory) => recoveredMemories.Contains(memory);

    public bool TryRecoverMemory(MemoryId memory)
    {
        if (!missionActive || BlocksGameplay || SceneFade.IsRunning ||
            !Enum.IsDefined(typeof(MemoryId), memory) ||
            (memory == MemoryId.StormNight && filledCrystalSockets.Count != socketSolution.Length) ||
            (memory == MemoryId.Return && (!HasLighthouseKey || !HasMemory(MemoryId.LighthouseKeeper) || !HasMemory(MemoryId.StormNight))) ||
            !recoveredMemories.Add(memory))
            return false;
        OnMissionStateChanged?.Invoke();
        return true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RegisterVisit(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Time.timeScale = 1f;
            Instance = null;
        }
    }

    public void StartMission()
    {
        if (missionStarted || BlocksGameplay)
            return;
        missionStarted = true;
        missionActive = true;
        OnMissionStateChanged?.Invoke();
    }

    // Somente a restauração do farol encerra a aventura, após as três lembranças.
    public void CompleteMission()
    {
        if (!missionActive || IsPaused || gameWon || SceneFade.IsRunning || !HasAllMemories || !IsLighthouseUnlocked)
            return;
        gameWon = true;
        missionActive = false;
        IsPaused = false;
        Time.timeScale = 1f;
        OnMissionStateChanged?.Invoke();
    }

    public void ResetMission()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        missionActive = false;
        missionStarted = false;
        gameWon = false;
        HasVisitedHouse = false;
        HasMap = false;
        HasJournal = false;
        HasReadHouseNotice = false;
        HasBackpack = false;
        HasPickaxe = false;
        HasLearnedJournal = false;
        HasSeenBeachIntroduction = false;
        visitedLocations.Clear();
        recoveredMemories.Clear();
        items.Clear();
        HasCollectedKeyMaterials = false;
        HasReadLighthouseClue = HasUsedLighthouseKey = IsLighthouseUnlocked = false;
        filledCrystalSockets.Clear();
        OnMissionStateChanged?.Invoke();
    }

    public bool IsMissionActive() => missionActive;
    public bool IsGameWon() => gameWon;

    public void SetPaused(bool paused)
    {
        if (paused && (!IsGameplayScene || gameWon || SceneFade.IsRunning)) return;
        if (IsPaused == paused) return;
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        OnMissionStateChanged?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Uma saída externa da cena também não pode deixar o próximo menu pausado.
        SetPaused(false);
        RegisterVisit(scene.name);
    }

    private void RegisterVisit(string scene)
    {
        if (!IsExplorationScene(scene)) return;
        if (scene == "House") HasVisitedHouse = true;
        if (visitedLocations.Add(scene)) OnMissionStateChanged?.Invoke();
    }

    public void RestartGame() => LeaveGameplay("Beach", false);
    public void ReturnToMainMenu() => LeaveGameplay("MainMenu", true);

    private void LeaveGameplay(string destination, bool returnToMenu)
    {
        if (!IsGameplayScene || !SceneFade.TryLoad(destination)) return;
        // O fade já bloqueia inputs. Limpa os persistentes antes de carregar o destino.
        ResetMission();
        if (returnToMenu) MainMenuController.ReturningFromGameplay = true;
    }
}

