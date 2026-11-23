using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlatformerPlayer : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 4.5f;
    public float jumpSpeed = 12f;
    public float fallLimit = -5f;

    [Header("Mensaje al caer")]
    public string fallMessage = "INGE, POR FAVOR, NO ME REPRUEBE";
    public float fallMessageDuration = 2.2f;

    [Header("Ladrillos")]
    public string brickNamePrefix = "ladrillo";
    public AudioClip brickHitClip;
    [Range(0f, 1f)] public float brickHitVolume = 0.9f;

    Rigidbody2D body;
    Collider2D hitbox;
    AudioSource audioSource;
    Canvas fallMessageCanvas;

    Vector3 spawnPoint;
    float horizontalInput;
    bool jumpQueued;
    bool isGrounded;
    bool isFalling;
    float lastGroundedTime = -10f;
    float lastBrickHitTime = -10f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<Collider2D>();
        spawnPoint = transform.position;

        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (brickHitClip == null)
            brickHitClip = Resources.Load<AudioClip>("sonidohit");

        CreateFallMessage();
    }

    void Update()
    {
        if (isFalling)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        horizontalInput = 0f;
        if (keyboard.spaceKey.isPressed) horizontalInput += 1f;
        if (keyboard.upArrowKey.isPressed) horizontalInput -= 1f;

        if (keyboard.pKey.wasPressedThisFrame)
            jumpQueued = true;

        if (transform.position.y < fallLimit)
            StartCoroutine(FallSequence());
    }

    void FixedUpdate()
    {
        if (isFalling)
            return;

        isGrounded = CheckGrounded();
        if (isGrounded)
            lastGroundedTime = Time.time;

        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;

        if (jumpQueued)
        {
            if (Time.time - lastGroundedTime <= 0.1f)
            {
                velocity.y = jumpSpeed;
                isGrounded = false;
                lastGroundedTime = -10f;
            }
            jumpQueued = false;
        }

        body.linearVelocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.name.StartsWith(brickNamePrefix, System.StringComparison.OrdinalIgnoreCase))
            return;

        if (Time.time - lastBrickHitTime < 0.08f)
            return;

        if (brickHitClip == null)
            return;

        lastBrickHitTime = Time.time;
        audioSource.PlayOneShot(brickHitClip, brickHitVolume);
    }

    bool CheckGrounded()
    {
        Bounds bounds = hitbox.bounds;
        Vector2 feet = new Vector2(bounds.center.x, bounds.min.y - 0.03f);
        Vector2 size = new Vector2(bounds.size.x * 0.8f, 0.06f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(feet, size, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit != null && hit != hitbox && !hit.isTrigger)
                return true;
        }
        return false;
    }

    void CreateFallMessage()
    {
        Canvas canvas = new GameObject("Fall Message Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("Fall Message Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(40f, 40f);
        rect.offsetMax = new Vector2(-40f, -40f);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = fallMessage;
        text.fontSize = 54;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.95f, 0.2f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        fallMessageCanvas = canvas;
        fallMessageCanvas.gameObject.SetActive(false);
    }

    IEnumerator FallSequence()
    {
        isFalling = true;
        horizontalInput = 0f;
        jumpQueued = false;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;

        if (fallMessageCanvas != null)
            fallMessageCanvas.gameObject.SetActive(true);

        yield return new WaitForSeconds(fallMessageDuration);

        if (fallMessageCanvas != null)
            fallMessageCanvas.gameObject.SetActive(false);

        Respawn();
        body.simulated = true;
        isFalling = false;
    }

    void Respawn()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.position = spawnPoint;
        transform.position = spawnPoint;
        jumpQueued = false;
        isGrounded = false;
        lastGroundedTime = -10f;
    }

}
