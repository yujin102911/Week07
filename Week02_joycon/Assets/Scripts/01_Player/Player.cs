using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[DisallowMultipleComponent]
public class Player : Singleton<Player>
{
    [Header("Jump / Move")]
    public float maxJumpHeight = 4f;
    public float minJumpHeight = 1f;
    public float timeToJumpApex = .4f;
    [SerializeField] float accelerationTimeAirborne = .2f;
    [SerializeField] float accelerationTimeGrounded = .1f;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float moveSpeedWeight = .1f;

    public Vector2 wallJumpClimb = new Vector2(7.5f, 16f);
    public Vector2 wallJumpOff = new Vector2(8f, 7f);
    public Vector2 wallLeap = new Vector2(18f, 17f);

    [SerializeField] float wallSlideSpeedMax = 3.0f;
    [SerializeField] float wallStickTime = 0.25f;

    [SerializeField] private PlayerInteractGeneral playerInteractGeneral;
    public static bool TryInteract() => Instance.playerInteractGeneral.TryInteract();

    [SerializeField] private PlayerInteractCarryable playerInteractCarryable;
    public static bool TryPickUp() => Instance.playerInteractCarryable.TryPickUp();
    public static bool TryDrop() => Instance.playerInteractCarryable.TryDrop();
    public static bool TryDrop(ItemName itemName) => Instance.playerInteractCarryable.TryDrop(itemName);
    public static bool TryDrop(Carryable carryable) => Instance.playerInteractCarryable.TryDrop(carryable);
    public static void TryDropAll() => Instance.playerInteractCarryable.DropAllForce();
    public static void UpdateWeight() => Instance.playerInteractCarryable.UpdateWeight();

    [SerializeField] private PlayerHatController playerHatController;
    public static void ChangeHat(HatType hatType) => Instance.playerHatController.ChangeHat(hatType);

    [SerializeField] private Controller2D controller2D;
    public static int GetFaceDir() => Instance.controller2D.collisions.faceDir;

    float timeToWallUnstick;
    float gravity;
    [SerializeField] float gravityWeight = 0.01f;
    float maxJumpVelocity;
    float minJumpVelocity;
    public Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    [Header("Ladder")]
    [SerializeField] LayerMask ladderMask = 0;
    [SerializeField, Range(1f, 12f)] float climbSpeed = 5.0f;
    [SerializeField, Range(0.05f, 1.0f)] float attachProbeHalfWidth = 0.25f;
    [SerializeField, Range(0.6f, 2.0f)] float attachProbeHeight = 1.4f;
    [SerializeField, Range(0.1f, 20f)] float snapSpeed = 12f;
    [SerializeField] bool snapToCenterX = true;
    [SerializeField, Range(0f, 10f)] float detachPush = 3.0f;

    bool onLadder;
    Collider2D _ladderCol;
    float _ladderCenterX;

    [Header("Ladder Jump")]
    [SerializeField] bool allowLadderJump = true;
    [SerializeField, Range(0f, 24f)] float ladderJumpUp = 14f;
    [SerializeField] Vector2 ladderJumpSide = new Vector2(10f, 12f);
    [SerializeField, Range(0f, 0.25f)] float ladderCoyoteTime = 0.08f;

    [SerializeField, Range(0f, 0.2f)] float jumpBuffer = 0.12f;
    [SerializeField, Range(0f, 0.25f)] float ladderReattachBlock = 0.12f;

    float _ladderCoyoteTimer;
    float _jumpBufferTimer;
    bool _didJumpThisFrame;
    float _ladderAttachBlockTimer;

    [Header("Solids / Resolve")]
    [SerializeField] LayerMask solidMask = 0;
    [SerializeField, Range(0.001f, 0.01f)] float cornerEpsilon = 0.004f;
    [SerializeField, Range(0f, 0.1f)] float wallLockAfterLand = 0.03f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private SpriteRenderer hatSprite;
    [SerializeField] private Transform visualsParent;

    float wallLockTimer;
    bool wasGrounded;

    static readonly Collider2D[] sHits = new Collider2D[8];
    static readonly Collider2D[] _overlapHits = new Collider2D[8];
    static readonly Collider2D[] _probeHits = new Collider2D[8];

    private ContactFilter2D _solidFilter;
    private ContactFilter2D _ladderFilter;
    private Collider2D _selfCol;

#if UNITY_EDITOR

    bool _drawAttachGizmo = true;
#endif

    protected override void Awake()
    {
        base.Awake();

        controller = GetComponent<Controller2D>();
        _selfCol = GetComponent<Collider2D>();

        _solidFilter.useTriggers = false;
        _solidFilter.SetLayerMask(solidMask);
        _solidFilter.useDepth = false;

        _ladderFilter.useTriggers = true;
        _ladderFilter.SetLayerMask(ladderMask);
        _ladderFilter.useDepth = false;
    }

    void Start()
    {
        gravity = -(2f * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2f);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) transform.position = new Vector3(43, 20, 0f);

        if (Input.GetKeyDown(KeyCode.B)) QuestRuntime.Instance.SetFlag(FlagId.EnterCastle);
        if (Input.GetKeyDown(KeyCode.N))
        {
            QuestRuntime.Instance.SetFlag(FlagId.ManagingMimic);
            QuestRuntime.Instance.SetFlag(FlagId.DryingRack);
            QuestRuntime.Instance.SetFlag(FlagId.WipingDust);
            QuestRuntime.Instance.SetFlag(FlagId.PreparingFood);
            QuestRuntime.Instance.SetFlag(FlagId.InstallGarlander);
            QuestRuntime.Instance.SetFlag(FlagId.InstallBallon);
        }
        // if (Input.GetKeyDown(KeyCode.M)) QuestRuntime.Instance.SetFlag(FlagId.PlaceMimic);

        float dt = Time.deltaTime;

        if (_ladderAttachBlockTimer > 0f) _ladderAttachBlockTimer = Mathf.Max(0f, _ladderAttachBlockTimer - dt);
        if (_ladderCoyoteTimer > 0f) _ladderCoyoteTimer = Mathf.Max(0f, _ladderCoyoteTimer - dt);
        if (_jumpBufferTimer > 0f) _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - dt);

        HandleSpriteFlip(directionalInput.x);

        CalculateVelocityBase(dt);

        _didJumpThisFrame = TryConsumeJump();

        if (!onLadder)
            HandleWallSliding();

        Vector2 move = velocity * dt;

        if (onLadder)
        {

            if (!_didJumpThisFrame)
            {
                ApplyLadderMotion(ref move, dt);
            }

            controller.Move(move, directionalInput);

            if (controller.collisions.above && velocity.y > 0f) velocity.y = 0f;
            if (controller.collisions.below && velocity.y < 0f) velocity.y = 0f;

            CornerLockOnLanding(dt);

            if (!StillOnSameLadder())
                DetachFromLadder();
        }
        else
        {

            if (!_didJumpThisFrame)
                TryAttachLadder();

            controller.Move(move, directionalInput);
            CornerLockOnLanding(dt);

            if (controller.collisions.above || controller.collisions.below)
            {
                if (controller.collisions.slidingDownMaxSlope)
                    velocity.y += controller.collisions.slopeNormal.y * -gravity * dt;
                else
                    velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input) => directionalInput = input;

    public void OnJumpInputDown()
    {
        _jumpBufferTimer = jumpBuffer;
    }

    bool TryConsumeJump()
    {
        if (_jumpBufferTimer <= 0f) return false;

        if (onLadder && allowLadderJump)
        {
            _jumpBufferTimer = 0f;
            LadderJump();
            return true;
        }

        if (!onLadder && _ladderCoyoteTimer > 0f && allowLadderJump)
        {
            _jumpBufferTimer = 0f;
            _ladderCoyoteTimer = 0f;
            velocity.y = Mathf.Max(velocity.y, ladderJumpUp);
            return true;
        }

        if (wallSliding)
        {
            if (playerInteractCarryable.GetTotalWeight() > 0) return false;
            _jumpBufferTimer = 0f;

            if (wallDirX == Mathf.RoundToInt(directionalInput.x))
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (Mathf.Abs(directionalInput.x) < 0.001f)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
            return true;
        }

        if (controller.collisions.below)
        {

            _jumpBufferTimer = 0f;
            maxJumpVelocity = 2f * maxJumpHeight / timeToJumpApex / (1 + playerInteractCarryable.GetTotalWeight() * gravityWeight);

            if (controller.collisions.slidingDownMaxSlope)
            {
                if (Mathf.RoundToInt(directionalInput.x) != -Mathf.Sign(controller.collisions.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
            return true;
        }

        return false;
    }

    void LadderJump()
    {
        DetachFromLadder();

        float ax = directionalInput.x;
        bool side = Mathf.Abs(ax) > 0.25f;

        if (side)
        {
            velocity.x = Mathf.Sign(ax) * ladderJumpSide.x;
            velocity.y = ladderJumpSide.y;
        }
        else
        {
            velocity.x = 0f;
            velocity.y = ladderJumpUp;
        }

        wallLockTimer = 0f;
        _ladderAttachBlockTimer = ladderReattachBlock;
    }

    public void OnJumpInputUp()
    {
        if (onLadder) return;
        if (velocity.y > minJumpVelocity)
            velocity.y = minJumpVelocity;
    }

    void CalculateVelocityBase(float dt)
    {
        float targetVelocityX = directionalInput.x * moveSpeed

                     / (1f + playerInteractCarryable.GetTotalWeight() * moveSpeedWeight);

        velocity.x = Mathf.SmoothDamp(
         velocity.x, targetVelocityX, ref velocityXSmoothing,
         (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        if (!onLadder)
            velocity.y += gravity * dt;
    }

    void HandleWallSliding()
    {
        wallDirX = controller.collisions.left ? -1 : 1;
        wallSliding = false;

        if ((controller.collisions.left || controller.collisions.right) &&
         !controller.collisions.below && velocity.y < 0f)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
                velocity.y = -wallSlideSpeedMax;

            if (timeToWallUnstick > 0f)
            {
                velocityXSmoothing = 0f;
                velocity.x = 0f;

                if (Mathf.Abs(directionalInput.x) > 0.001f &&
                 Mathf.RoundToInt(directionalInput.x) != wallDirX)
                    timeToWallUnstick -= Time.deltaTime;
                else
                    timeToWallUnstick = wallStickTime;
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    void TryAttachLadder()
    {
        if (_ladderAttachBlockTimer > 0f) return;
        if (directionalInput.y <= 0.1f) return;

        var bestLadder = FindBestLadderAt(transform.position);
        if (bestLadder != null)
        {
            AttachToLadder(bestLadder);
        }
    }

    void AttachToLadder(Collider2D ladder)
    {
        onLadder = true;
        _ladderCol = ladder;
        _ladderCenterX = ladder.bounds.center.x;
        velocity = Vector3.zero;
        _ladderCoyoteTimer = 0f;
    }

    void DetachFromLadder()
    {
        onLadder = false;
        _ladderCol = null;
        _ladderCoyoteTimer = ladderCoyoteTime;
    }

    bool StillOnSameLadder()
    {
        if (!_ladderCol) return false;

        var bestLadder = FindBestLadderAt(transform.position);

        if (bestLadder == null)
        {

            return false;
        }

        _ladderCol = bestLadder;
        _ladderCenterX = bestLadder.bounds.center.x;
        return true;
    }

    void ApplyLadderMotion(ref Vector2 move, float dt)
    {

        float ax = directionalInput.x;
        if (Mathf.Abs(ax) > 0.25f)
        {
            bool isAtVeryTop = false;

            if (directionalInput.y > 0.1f && _ladderCol != null)
            {
                float ladderTop = _ladderCol.bounds.max.y;

                if (transform.position.y >= ladderTop - 0.05f)
                {

                    Vector2 lookAheadCenter = new Vector2(_ladderCenterX, ladderTop + 0.05f);
                    Vector2 lookAheadSize = new Vector2(attachProbeHalfWidth * 2f, 0.1f);
                    int futureHits = Physics2D.OverlapBox(lookAheadCenter, lookAheadSize, 0f, _ladderFilter, sHits);

                    if (futureHits == 0)
                    {

                        isAtVeryTop = true;
                    }
                    else
                    {

                        bool foundRealLadder = false;
                        for (int i = 0; i < futureHits; i++)
                        {
                            if (sHits[i] == null) continue;

                            if (Mathf.Abs(sHits[i].bounds.center.x - _ladderCenterX) < 0.1f)
                            {
                                foundRealLadder = true;
                                break;
                            }
                        }

                        if (!foundRealLadder)
                        {
                            isAtVeryTop = true;
                        }
                    }
                }
            }

            if (Mathf.Abs(directionalInput.y) < 0.3f || (directionalInput.y > 0.1f && isAtVeryTop))
            {
                DetachFromLadder();
                velocity.x = Mathf.Sign(ax) * detachPush;
                return;
            }
        }

        float vy = directionalInput.y * climbSpeed;

        if (vy > 0f && _ladderCol != null)
        {
            float ladderTop = _ladderCol.bounds.max.y;
            float playerCenter = transform.position.y;
            float nextY = playerCenter + (vy * dt);

            if (nextY >= ladderTop)
            {

                Vector2 lookAheadCenter = new Vector2(_ladderCenterX, ladderTop + 0.05f);
                Vector2 lookAheadSize = new Vector2(attachProbeHalfWidth * 2f, 0.1f);
                int futureHits = Physics2D.OverlapBox(lookAheadCenter, lookAheadSize, 0f, _ladderFilter, sHits);

                bool foundRealLadderAbove = false;
                if (futureHits > 0)
                {
                    for (int i = 0; i < futureHits; i++)
                    {
                        if (sHits[i] == null) continue;
                        if (Mathf.Abs(sHits[i].bounds.center.x - _ladderCenterX) < 0.1f)
                        {
                            foundRealLadderAbove = true;
                            break;
                        }
                    }
                }

                if (foundRealLadderAbove)
                {

                    move.y = vy * dt;
                }
                else
                {

                    vy = 0f;
                    move.y = ladderTop - playerCenter;
                }
            }
            else
            {
                move.y = vy * dt;
            }
        }
        else
        {
            move.y = vy * dt;
        }

        velocity.y = vy;

        if (snapToCenterX)
        {
            float x = transform.position.x;
            float newX = Mathf.MoveTowards(x, _ladderCenterX, snapSpeed * dt);
            float dx = Mathf.Clamp(newX - x, -0.2f, 0.2f);
            move.x = dx;
            velocity.x = (dt > 1e-6f) ? dx / dt : 0f;
        }
        else
        {
            move.x = 0f;
            velocity.x = 0f;
        }

        if (directionalInput.y < -0.85f)
        {

        }
    }

    void ResolveContactsAndClampVelocity()
    {
        var col = controller.collisions;

        if (col.left && velocity.x < 0f) { velocity.x = 0f; velocityXSmoothing = 0f; }
        if (col.right && velocity.x > 0f) { velocity.x = 0f; velocityXSmoothing = 0f; }

        if (!_selfCol) return;

        int n = _selfCol.Overlap(_solidFilter, _overlapHits);
        for (int i = 0; i < n; ++i)
        {
            var other = _overlapHits[i];
            if (!other) continue;

            var d = _selfCol.Distance(other);
            if (!d.isOverlapped) continue;

            Vector2 pushOut = d.normal * (-d.distance + 0.001f);
            transform.Translate(pushOut, Space.World);

            float vn = Vector2.Dot((Vector2)velocity, d.normal);
            if (vn > 0f) velocity -= (Vector3)(d.normal * vn);
        }
    }

    void CornerLockOnLanding(float dt)
    {
        var col = controller.collisions;
        bool grounded = col.below;
        bool walling = col.left || col.right;

        if (grounded && !wasGrounded && walling)
        {
            wallLockTimer = wallLockAfterLand;
            ZeroX();
            MicroSeparateFromWall();
        }
        else if (grounded && wallLockTimer > 0f && walling)
        {
            wallLockTimer -= dt;
            ZeroX();
            MicroSeparateFromWall();
        }

        wasGrounded = grounded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ZeroX()
    {
        velocity.x = 0f;
        velocityXSmoothing = 0f;
    }

    void MicroSeparateFromWall()
    {
        if (!_selfCol) return;

        var b = _selfCol.bounds;
        Vector2 center = b.center;
        Vector2 size = new Vector2(b.size.x + cornerEpsilon * 2f, b.size.y + cornerEpsilon * 2f);

        int n = Physics2D.OverlapBox(center, size, 0f, _solidFilter, _probeHits);
        for (int i = 0; i < n; ++i)
        {
            var other = _probeHits[i];
            if (!other) continue;

            var d = _selfCol.Distance(other);
            if (d.distance <= cornerEpsilon)
            {
                Vector2 push = d.normal * (cornerEpsilon - d.distance + 0.0005f);
                transform.Translate(push, Space.World);

                float vn = Vector2.Dot((Vector2)velocity, d.normal);
                if (vn > 0f) velocity -= (Vector3)(d.normal * vn);
            }
        }
    }

#if UNITY_EDITOR

    void OnDrawGizmosSelected()
    {
        if (!_drawAttachGizmo) return;
        Gizmos.color = onLadder ? Color.green : Color.yellow;
        Vector2 center = transform.position;
        Vector3 size = new Vector3(attachProbeHalfWidth * 2f, attachProbeHeight, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
#endif

    Collider2D FindBestLadderAt(Vector2 center)
    {
        Vector2 size = new Vector2(attachProbeHalfWidth * 2f, attachProbeHeight);

        int hitCount = Physics2D.OverlapBox(center, size, 0f, _ladderFilter, sHits);
        if (hitCount <= 0) return null;

        int bestIdx = -1;
        float bestDx = float.MaxValue;
        for (int i = 0; i < hitCount; ++i)
        {
            var c = sHits[i]; if (!c) continue;
            float ladderX = c.bounds.center.x;
            float dx = Mathf.Abs(ladderX - center.x);
            if (dx < bestDx) { bestDx = dx; bestIdx = i; }
        }

        return (bestIdx >= 0) ? sHits[bestIdx] : null;
    }

    public Sprite CurrentPlayerSprite
    {
        get { return playerSprite.sprite; }
    }

    public void ChangeSprite(Sprite newSprite)
    {
        if (newSprite == null)
        {
            GameLogger.Instance.LogError(this, $"변경할 스프라이트가 없음");
            return;
        }
        if (playerSprite.sprite != null)
        {
            playerSprite.sprite = newSprite;
            GameLogger.Instance.LogDebug(this, $"플레이어 sprite {newSprite.name}으로 변경 성공");
        }
        else
        {
            GameLogger.Instance.LogError(this, "플레이어의 playerSprite가 비어있음");
        }
    }

    private void HandleSpriteFlip(float horizontalInput)
    {
        if (horizontalInput == 0) return;

        bool shouldFlip = horizontalInput < 0;

        if (visualsParent != null)
        {
            visualsParent.localScale = new Vector3(shouldFlip ? -1f : 1f, 1, 1);
        }
        else
        {
            if (playerSprite != null) playerSprite.flipX = shouldFlip;
            if (hatSprite != null) hatSprite.flipX = shouldFlip;
        }
    }
}