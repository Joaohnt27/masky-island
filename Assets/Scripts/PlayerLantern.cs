using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// E controla equipamento e luz; a mineração aguarda o gesto de guardar terminar.
[DefaultExecutionOrder(-20)]
public class PlayerLantern : MonoBehaviour
{
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private SpriteRenderer carried;
    [SerializeField] private TMP_Text hint;
    [SerializeField] private CanvasGroup hintGroup;
    [SerializeField, Min(.1f)] private float equipDuration = .45f;
    private SpriteRenderer glow, flame, body;
    private PlayerSpriteAnimation bodyAnimation;
    private Texture2D glowTexture;
    private Sprite glowSprite;
    private Vector3 handPosition, originalScale;
    private float amount, idleTime;
    private bool targetEquipped;
    public bool IsTransitioning => !Mathf.Approximately(amount, targetEquipped ? 1f : 0f);
    public bool IsEquipped => amount > .001f;

    private void Start()
    {
        bodyAnimation = player.GetComponent<PlayerSpriteAnimation>();
        body = player.GetComponent<SpriteRenderer>();
        handPosition = carried.transform.localPosition;
        originalScale = carried.transform.localScale;
        targetEquipped = GameManager.Instance != null && GameManager.Instance.HasLantern && GameManager.Instance.IsLanternLit;
        amount = targetEquipped ? 1 : 0;
        glowTexture = new Texture2D(64,64,TextureFormat.RGBA32,false);
        var pixels = new Color[4096];
        for (int y=0;y<64;y++) for (int x=0;x<64;x++)
        {
            float radius = Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;
            pixels[y*64+x] = new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2));
        }
        glowTexture.SetPixels(pixels);glowTexture.Apply();
        glowSprite = Sprite.Create(glowTexture,new Rect(0,0,64,64),Vector2.one*.5f,64);
        glow = CreateGlow("LanternWarmGlow",4.5f,new Color(1,.65f,.12f,.4f),-1);
        flame = CreateGlow("LanternFlame",.35f,new Color(1,.85f,.35f,1),1);
    }
    private SpriteRenderer CreateGlow(string name,float size,Color color,int order)
    {
        var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
        renderer.transform.SetParent(carried.transform.parent,false);
        renderer.transform.localScale = Vector3.one * size;
        renderer.sprite = glowSprite;renderer.color = color;
        renderer.sortingLayerID = carried.sortingLayerID;renderer.sortingOrder = carried.sortingOrder + order;
        renderer.enabled = false;
        return renderer;
    }
    public bool Toggle()
    {
        var gm = GameManager.Instance;
        if (gm == null || player == null || !player.IsMovementEnabled || !gm.HasLantern || IsTransitioning) return false;
        if (!gm.SetLanternLit(!gm.IsLanternLit)) return false;
        SetTarget(gm.IsLanternLit);
        GameAudio.Play(GameAudio.Cue.Confirm,.7f);return true;
    }
    private void SetTarget(bool equipped)
    {
        targetEquipped = equipped;
        if (IsTransitioning) player.AcquireMovementLock(this);
    }
    public IEnumerator StowForMining()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetLanternLit(false);
        SetTarget(false);
        while (IsTransitioning) yield return null;
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) Toggle();
        var gm = GameManager.Instance;
        bool owned = gm != null && gm.HasLantern;
        bool wanted = owned && gm.IsLanternLit;
        if (wanted != targetEquipped) SetTarget(wanted);
        // deltaTime também congela o gesto e o balanço durante a pausa.
        float dt = gm != null && gm.IsPaused ? 0 : Time.deltaTime;
        amount = Mathf.MoveTowards(amount,targetEquipped ? 1 : 0,dt/equipDuration);
        if (!IsTransitioning) player.ReleaseMovementLock(this);
        idleTime += dt;
        if (hintGroup != null) hintGroup.alpha = owned && player.IsMovementEnabled ? 1 : 0;
        if (hint != null) hint.text = wanted ? "E  ·  GUARDAR LAMPIÃO" : "E  ·  EQUIPAR LAMPIÃO";
    }
    private void LateUpdate()
    {
        if (carried == null || body == null) return;
        bool visible = GameManager.Instance != null && GameManager.Instance.HasLantern && IsEquipped;
        carried.enabled = visible;
        var facing = bodyAnimation != null ? bodyAnimation.Facing : PlayerSpriteAnimation.Direction.Front;
        float side = facing == PlayerSpriteAnimation.Direction.Left ? -1 : 1;
        var hand = new Vector3(Mathf.Abs(handPosition.x)*side,handPosition.y,handPosition.z);
        var pocket = new Vector3(.12f*side,-.34f,handPosition.z);
        float eased = Mathf.SmoothStep(0,1,amount);
        bool walking = bodyAnimation != null && bodyAnimation.IsWalking;
        float swing = Mathf.Sin(idleTime*(walking ? 8 : 2.6f));
        carried.transform.localPosition = Vector3.Lerp(pocket,hand,eased) + Vector3.up*(Mathf.Sin(amount*Mathf.PI)*.12f + swing*.018f*eased);
        carried.transform.localScale = originalScale * Mathf.Lerp(.25f,1,eased);
        carried.transform.localRotation = Quaternion.Euler(0,0,side*((1-eased)*-40+swing*(walking ? 9 : 3)*eased));
        carried.color = new Color(1,1,1,eased);
        carried.sortingOrder = body.sortingOrder + (facing == PlayerSpriteAnimation.Direction.Back ? -1 : 1);
        bool lit = visible && GameManager.Instance.IsLanternLit;
        foreach(var lightSprite in new[]{glow,flame}) if(lightSprite != null) {
            lightSprite.enabled = lit;
            lightSprite.transform.localPosition = carried.transform.localPosition + Vector3.down*.07f;
            lightSprite.sortingOrder = carried.sortingOrder + (lightSprite == glow ? -1 : 1);
            var color = lightSprite.color;color.a=(lightSprite == glow ? .4f : 1f)*eased;lightSprite.color=color;
        }
    }
    private void OnDisable() { if(player != null) player.ReleaseMovementLock(this); }
    private void OnDestroy()
    {
        if (glow != null) Destroy(glow.gameObject);
        if (flame != null) Destroy(flame.gameObject);
        if (glowSprite != null) Destroy(glowSprite);
        if (glowTexture != null) Destroy(glowTexture);
    }
}
