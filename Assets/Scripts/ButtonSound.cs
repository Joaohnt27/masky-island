using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private Button button;
    private void Awake() => button = GetComponent<Button>();
    private void OnEnable() { if (button == null) button = GetComponent<Button>(); button.onClick.AddListener(Play); }
    private void Play() { if (button.interactable) GameAudio.Play(GameAudio.Cue.Click); }
    private void OnDisable() { if (button != null) button.onClick.RemoveListener(Play); }
}
