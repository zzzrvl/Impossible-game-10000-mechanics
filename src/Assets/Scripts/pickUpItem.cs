using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PickUpItem : MonoBehaviour
{
    [Header("Респавн")]
    [Tooltip("Prefab из окна Project (с этим же скриптом). После подбора инстанс в сцене удаляется, при выбросе создаётся клон префаба.")]
    [SerializeField] private GameObject prefabSource;

    [Header("Выброс")]
    [SerializeField] private float throwSpawnForward = 0.45f;
    [SerializeField] private float throwSpawnUp = 0.55f;
    [SerializeField] private float rigidbodyHorizontalSpeed = 10f;
    [SerializeField] private float rigidbodyUpwardSpeed = 3.5f;

    [Header("Без динамического Rigidbody — баллистика по transform")]
    [SerializeField] private float ballisticSpeed = 11f;
    [SerializeField] private float ballisticUpFactor = 0.25f;
    [SerializeField] private float ballisticGravity = 26f;
    [SerializeField] private float ballisticMaxTime = 4f;

    [Header("Столкновения (баллистика — SphereCast по траектории)")]
    [SerializeField] private LayerMask collisionMask = Physics.DefaultRaycastLayers;
    [Tooltip("≤ 0 — взять радиус из первого коллайдера (примерно по bounds).")]
    [SerializeField] private float ballisticCastRadius = -1f;
    [SerializeField] [Range(0f, 1f)] private float wallBounce = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float groundBounce = 0.12f;
    [SerializeField] private float collisionSkin = 0.1f;
    [SerializeField] private float minBounceSpeed = 0.35f;
    [Tooltip("Нормаль «пола»: выше — считаем приземление и гасим вертикальную скорость.")]
    [SerializeField] private float groundNormalDot = 0.55f;

    private const float MaxVerticalSpeed = 10f;
    private const float MaxTotalSpeed = 30f;

    private TrackingCursor _trackingCursor;

    private Collider[] _colliders;
    private bool[] _colliderEnabledBackup;

    private bool _hadRigidbody;
    private bool _rbWasKinematic;
    private bool _rbUsedGravity;

    private Coroutine _ballisticCoroutine;
    private Coroutine _pickupMoveCoroutine;
    private bool _isBeingPickedUp;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
        _colliderEnabledBackup = new bool[_colliders.Length];
        for (var i = 0; i < _colliders.Length; i++)
            _colliderEnabledBackup[i] = _colliders[i].enabled;
    }

    private void Start()
    {
        RefreshTrackingCursorIfNeeded();
    }

    /// <summary>
    /// Спавн клона префаба у рук игрока и выброс по прицелу.
    /// Если на ассете префаба нет PickUpItem (как у готовых меш-префабов), компонент добавляется здесь — лучше повесить скрипт на префаб, чтобы задать силы броска.
    /// </summary>
    public static PickUpItem SpawnThrownPickup(PlayerEntity holder, GameObject prefabAsset)
    {
        if (holder == null || prefabAsset == null)
            return null;

        GameObject clone = Instantiate(prefabAsset);

        PickUpItem pick =
            clone.GetComponent<PickUpItem>() ??
            clone.GetComponentInChildren<PickUpItem>(true);

        if (pick == null)
        {
            pick = clone.AddComponent<PickUpItem>();
#if UNITY_EDITOR
            Debug.Log(
                $"{nameof(SpawnThrownPickup)}: на префабе «{prefabAsset.name}» не было {nameof(PickUpItem)} — добавлен во время игры. " +
                $"Открой префаб в Project и добавь компонент {nameof(PickUpItem)}, чтобы настроить параметры выброса.",
                prefabAsset);
#endif
        }

        pick.BindPrefabSourceForThrownClone(prefabAsset);

        if (!clone.activeSelf)
            clone.SetActive(true);

        pick.RefreshTrackingCursorIfNeeded();
        pick.ApplyThrowFromHolder(holder);
        return pick;
    }

    /// <summary>Чтобы выпавший объект снова можно было подбирать (цепочка Instantiate).</summary>
    private void BindPrefabSourceForThrownClone(GameObject spawnTemplate)
    {
        if (spawnTemplate != null)
            prefabSource = spawnTemplate;
    }

    /// <summary>
    /// Подбор из InputReader по Interact — ближайший предмет в радиусе курсора.
    /// </summary>
    public static bool TryPickClosestInCursorRadius(PlayerEntity player)
    {
        if (player == null || player.equippedItem != null)
            return false;

        RefreshCursorStatic(ref _staticCursorCached);
        TrackingCursor cursor = _staticCursorCached;
        Vector3 pivot = cursor != null ? cursor.transform.position : player.transform.position;
        float limit = cursor != null ? cursor.maxRadius * 0.8f : 1.8f; // slightly reduced radius

        // Compute aim direction
        Vector3 aimDir = GetAimDirectionHorizontalStatic(player, cursor);
        const float coneAngle = 90f;
        float cosCone = Mathf.Cos(coneAngle * Mathf.Deg2Rad);

        PickUpItem chosen = null;
        float best = limit;

        foreach (PickUpItem item in FindObjectsByType<PickUpItem>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (!item.isActiveAndEnabled)
                continue;

            // Distance from pivot (cursor or player)
            float d = Vector3.Distance(Flat(pivot), Flat(item.transform.position));
            if (!(d <= limit))
                continue;

            // Direction from player to item (horizontal)
            Vector3 toItem = Flat(item.transform.position - player.transform.position);
            if (toItem.sqrMagnitude < 1e-6f)
                continue;
            Vector3 dirToItem = toItem.normalized;
            
            // Cone check: item must be within cone of aim direction
            if (Vector3.Dot(aimDir, dirToItem) < cosCone)
                continue;

            // Choose closest
            if (!(d < best))
                continue;
            best = d;
            chosen = item;
        }

        if (chosen == null)
            return false;

        if (chosen.GetEffectiveSpawnPrefab() == null)
        {
            Debug.LogError(
                $"{chosen.name}: назначь в инспекторе поле «Prefab Source» (ссылку на префаб из окна Project) " +
                "или помести в сцену экземпляр префаба из Project — в редакторе он подхватится автоматически.",
                chosen);
            return false;
        }

        chosen.PickUpInternal(player);
        return true;
    }

    private static Vector3 GetAimDirectionHorizontalStatic(PlayerEntity player, TrackingCursor cursor)
    {
        if (cursor != null && cursor.cursorObject != null)
        {
            Vector3 delta = cursor.cursorObject.transform.position - player.transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 1e-6f)
                return delta.normalized;
        }

        if (cursor != null)
        {
            Vector3 f = cursor.transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 1e-6f)
                return f.normalized;
        }

        Vector3 fb = player.transform.forward;
        fb.y = 0f;
        return fb.sqrMagnitude > 1e-6f ? fb.normalized : Vector3.forward;
    }

    private static TrackingCursor _staticCursorCached;

    private static void RefreshCursorStatic(ref TrackingCursor cached)
    {
        if (cached == null)
            cached = FindFirstObjectByType<TrackingCursor>();
    }

    private void PickUpInternal(PlayerEntity player)
    {
        if (player == null || _isBeingPickedUp)
            return;

        // Check if we have a valid prefab source for throwing later
        GameObject spawnPrefab = GetEffectiveSpawnPrefab();
        if (spawnPrefab == null)
        {
            Debug.LogError(
                $"{name}: нужен источник для Instantiate при выбросе: назначь «Prefab Source» префабом из Project " +
                "или используй объект, размещённый из префаба (только редактор).",
                this);
            return;
        }

        // Stop any existing pickup movement
        if (_pickupMoveCoroutine != null)
        {
            StopCoroutine(_pickupMoveCoroutine);
            _pickupMoveCoroutine = null;
        }

        if (player.equippedItem != null)
            player.equippedItem.Throw();

        CancelBallisticIfAny();

        _isBeingPickedUp = true;
        
        // Start smooth movement to hand
        _pickupMoveCoroutine = StartCoroutine(MoveToHandAndCompletePickup(player));
    }

    private IEnumerator MoveToHandAndCompletePickup(PlayerEntity player)
    {
        Transform handPoint = player.ItemHoldPoint;
        
        // Move to hand
        yield return MoveToHandRoutine(handPoint, 1f);
        
        // Get prefab source for throwing
        GameObject spawnPrefab = GetEffectiveSpawnPrefab();
        if (spawnPrefab == null)
        {
            Debug.LogError(
                $"{name}: нужен источник для Instantiate при выбросе: назначь «Prefab Source» префабом из Project " +
                "или используй объект, размещённый из префаба (только редактор).",
                this);
            _isBeingPickedUp = false;
            _pickupMoveCoroutine = null;
            yield break;
        }
        
        // Create held handle with the prefab (original behavior)
        player.equippedItem = new HeldPickupHandle(player, spawnPrefab);
        
        // Destroy the original object (original behavior)
        Destroy(gameObject);
        
        _isBeingPickedUp = false;
        _pickupMoveCoroutine = null;
    }

    /// <summary>
    /// Ручное поле или префаб-asset, восстановленный из экземпляра префаба на сцене (Play Mode в Unity Editor).
    /// В финальной сборке игры без поля объект должен быть с явнымPrefab Source.
    /// </summary>
    public GameObject GetEffectiveSpawnPrefab()
    {
        if (prefabSource != null)
            return prefabSource;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return null;

        UnityEngine.GameObject linked =
            PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject) as GameObject;

        return linked != null ? linked : null;
#else
        return null;
#endif
    }

    /// <remarks> Вызывается у только что созданного клона при выбросе из рук.</remarks>
    private void ApplyThrowFromHolder(PlayerEntity holder)
    {
        Vector3 aimDir = holder != null
            ? GetAimDirectionHorizontal(holder.transform)
            : Vector3.forward;
        Vector3 startPos =
            holder != null ? ComputeThrowOrigin(holder) : FlatThrowOrigin(transform);
        ApplyThrowSpawnAndVelocity(startPos, aimDir);
    }

    private static Vector3 FlatThrowOrigin(Transform t)
    {
        Vector3 fwd = Flat(t.forward);
        if (fwd.sqrMagnitude < 1e-6f)
            fwd = Vector3.forward;
        return t.position + fwd * 0.45f + Vector3.up * 0.55f;
    }

    private static void SnapTransformAndRigidbodyTo(Vector3 worldPos, Transform t, Rigidbody rbNullable)
    {
        t.position = worldPos;
        if (rbNullable != null)
        {
            rbNullable.position = worldPos;
            rbNullable.linearVelocity = Vector3.zero;
            rbNullable.angularVelocity = Vector3.zero;
        }
        Physics.SyncTransforms();
    }

    private void ApplyThrowSpawnAndVelocity(Vector3 startWorld, Vector3 aimDirHorizontal)
    {
        AwakeRebuildCollidersIfNeeded();

        Rigidbody rb = GetComponent<Rigidbody>();

        bool hadRb = rb != null;
        bool rbWasKin = false;
        bool rbUseGrav = true;
        if (hadRb)
        {
            rbWasKin = rb.isKinematic;
            rbUseGrav = rb.useGravity;
        }

        _hadRigidbody = hadRb;
        _rbWasKinematic = rbWasKin;
        _rbUsedGravity = rbUseGrav;

        SnapTransformAndRigidbodyTo(startWorld, transform, rb);

        Vector3 impulse =
            Flat(aimDirHorizontal).normalized * rigidbodyHorizontalSpeed +
            Vector3.up * rigidbodyUpwardSpeed;
        impulse = ClampVelocity(impulse);

        if (_hadRigidbody && rb != null)
        {
            bool canPhysThrow = !_rbWasKinematic;
            if (canPhysThrow)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.linearVelocity = impulse;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                rb.isKinematic = true;
                rb.useGravity = _rbUsedGravity;
                BeginBallistics(startWorld, aimDirHorizontal);
            }

            return;
        }

        BeginBallistics(startWorld, aimDirHorizontal);
    }

    private void AwakeRebuildCollidersIfNeeded()
    {
        if (_colliders != null && _colliders.Length > 0)
            return;

        _colliders = GetComponentsInChildren<Collider>(true);
        _colliderEnabledBackup = new bool[_colliders.Length];
        for (var i = 0; i < _colliders.Length; i++)
            _colliderEnabledBackup[i] = _colliders[i].enabled;
    }

    private void CancelBallisticIfAny()
    {
        if (_ballisticCoroutine == null)
            return;
        StopCoroutine(_ballisticCoroutine);
        _ballisticCoroutine = null;
    }

    private void BeginBallistics(Vector3 startWorld, Vector3 aimDirHorizontal)
    {
        CancelBallisticIfAny();
        _ballisticCoroutine = StartCoroutine(BallisticsRoutine(startWorld, aimDirHorizontal));
    }

    private IEnumerator BallisticsRoutine(Vector3 startWorld, Vector3 aimDirHorizontal)
    {
        Vector3 h = Flat(aimDirHorizontal);
        h = h.sqrMagnitude > 1e-6f ? h.normalized : Vector3.forward;

        transform.position = startWorld;

        Vector3 vel = h * ballisticSpeed + Vector3.up * (ballisticSpeed * ballisticUpFactor);
        vel = ClampVelocity(vel);
        float t = 0f;

        var rbKin = GetComponent<Rigidbody>();
        var syncKinRb = rbKin != null && rbKin.isKinematic;
        float radius = GetBallisticCastRadius();

        while (t < ballisticMaxTime)
        {
            t += Time.deltaTime;

            vel.y -= ballisticGravity * Time.deltaTime;
            Vector3 delta = vel * Time.deltaTime;
            float dist = delta.magnitude;
            if (dist > 1e-5f)
            {
                Vector3 dir = delta / dist;
                Vector3 origin = GetBallisticCastOrigin();

                if (Physics.SphereCast(
                        origin,
                        radius,
                        dir,
                        out RaycastHit hit,
                        dist + collisionSkin,
                        collisionMask,
                        QueryTriggerInteraction.Ignore) &&
                    hit.collider != null &&
                    !IsPartOfThisItem(hit.collider.transform))
                {
                    float travel = Mathf.Max(0f, hit.distance - collisionSkin);
                    Vector3 displacement = dir * Mathf.Min(travel, dist);

                    ApplyBallisticDisplacement(displacement, syncKinRb);
                    RespondToBallisticHit(ref vel, hit);
                }
                else
                {
                    ApplyBallisticDisplacement(delta, syncKinRb);
                }
            }

            if (vel.sqrMagnitude < minBounceSpeed * minBounceSpeed &&
                Physics.Raycast(
                    transform.position + Vector3.up * 0.12f,
                    Vector3.down,
                    out RaycastHit settleHit,
                    0.35f + radius,
                    collisionMask,
                    QueryTriggerInteraction.Ignore) &&
                settleHit.distance < 0.2f + radius &&
                Vector3.Dot(settleHit.normal, Vector3.up) > groundNormalDot)
                break;

            yield return null;
        }

        _ballisticCoroutine = null;
    }

    private Vector3 GetBallisticCastOrigin()
    {
        if (_colliders == null || _colliders.Length == 0 ||
            !_colliders[0].enabled || !_colliders[0].gameObject.activeInHierarchy)
            return transform.position;

        return _colliders[0].bounds.center;
    }

    private float GetBallisticCastRadius()
    {
        if (ballisticCastRadius > 0f)
            return ballisticCastRadius;

        AwakeRebuildCollidersIfNeeded();
        if (_colliders == null || _colliders.Length == 0)
            return 0.15f;

        Bounds b = _colliders[0].bounds;
        for (var i = 1; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null && _colliders[i].enabled)
                b.Encapsulate(_colliders[i].bounds);
        }

        float r = Mathf.Max(b.extents.x, b.extents.y, b.extents.z) * 0.8f;
        return Mathf.Clamp(r, 0.15f, 1.5f);
    }

    private void ApplyBallisticDisplacement(Vector3 displacement, bool syncKinRb)
    {
        transform.position += displacement;
        if (syncKinRb)
        {
            var rbKin = GetComponent<Rigidbody>();
            if (rbKin != null && rbKin.isKinematic)
                rbKin.MovePosition(transform.position);
        }
        ResolveFloorOverlap();
    }

    private void ResolveFloorOverlap()
    {
        float radius = GetBallisticCastRadius();
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);
        float maxDistance = radius + 0.2f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > groundNormalDot && hit.distance < radius + 0.1f)
            {
                transform.position = hit.point + Vector3.up * (radius + collisionSkin);
                var rbKin = GetComponent<Rigidbody>();
                if (rbKin != null && rbKin.isKinematic)
                    rbKin.MovePosition(transform.position);
            }
        }
    }

    private void RespondToBallisticHit(ref Vector3 vel, RaycastHit hit)
    {
        Vector3 n = hit.normal;
        if (n.sqrMagnitude < 1e-8f)
            return;

        n.Normalize();

        float vn = Vector3.Dot(vel, n);
        if (vn >= 0f)
            return;

        float upDot = Vector3.Dot(n, Vector3.up);

        if (upDot > groundNormalDot)
        {
            vel -= vn * n;
            float horizontalDamp = Mathf.Clamp01(1f - groundBounce * 2.5f);
            vel.x *= horizontalDamp;
            vel.z *= horizontalDamp;
            vel.y = groundBounce > 0.01f ? Mathf.Max(vel.y + (-vn) * groundBounce, 0f) : Mathf.Min(vel.y, 0f);
            if (vel.y < minBounceSpeed * 0.25f && vel.y > -minBounceSpeed * 0.25f)
                vel.y = 0f;
        }
        else
        {
            vel = Vector3.Reflect(vel, n) * wallBounce;
            if (vel.sqrMagnitude < minBounceSpeed * minBounceSpeed)
                vel = Vector3.zero;
        }
    }

    private bool IsPartOfThisItem(Transform t)
    {
        return t != null && (t == transform || t.IsChildOf(transform));
    }

    private Vector3 ComputeThrowOrigin(PlayerEntity holder)
    {
        Vector3 fwd = Flat(GetAimDirectionHorizontal(holder.transform));
        if (fwd.sqrMagnitude < 1e-6f)
            fwd = Vector3.forward;

        Vector3 pivotWorld = holder.ItemHoldPoint.position;
        return pivotWorld + fwd * throwSpawnForward + Vector3.up * throwSpawnUp;
    }

    private void RefreshTrackingCursorIfNeeded()
    {
        if (_trackingCursor == null)
            _trackingCursor = FindFirstObjectByType<TrackingCursor>();
    }

    private Vector3 GetAimDirectionHorizontal(Transform playerRoot)
    {
        RefreshTrackingCursorIfNeeded();

        if (_trackingCursor != null && _trackingCursor.cursorObject != null)
        {
            Vector3 delta = _trackingCursor.cursorObject.transform.position - playerRoot.position;
            delta.y = 0f;
            float maxDist = (_trackingCursor.maxRadius + 0.5f) * 1.5f; // allow some margin
            if (delta.sqrMagnitude > 1e-6f && delta.sqrMagnitude <= maxDist * maxDist)
                return delta.normalized;
            // if cursor is too far, ignore it and fall through
        }

        if (_trackingCursor != null)
        {
            Vector3 f = _trackingCursor.transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 1e-6f)
                return f.normalized;
        }

        Vector3 fb = playerRoot.forward;
        fb.y = 0f;
        return fb.sqrMagnitude > 1e-6f ? fb.normalized : Vector3.forward;
    }

    private Vector3 ClampVelocity(Vector3 velocity)
    {
        // Clamp vertical component
        float vertical = velocity.y;
        if (Mathf.Abs(vertical) > MaxVerticalSpeed)
        {
            velocity.y = Mathf.Sign(vertical) * MaxVerticalSpeed;
        }

        // Clamp total speed
        float horizontalSqr = velocity.x * velocity.x + velocity.z * velocity.z;
        float totalSqr = horizontalSqr + velocity.y * velocity.y;
        if (totalSqr > MaxTotalSpeed * MaxTotalSpeed)
        {
            float scale = MaxTotalSpeed / Mathf.Sqrt(totalSqr);
            velocity.x *= scale;
            velocity.y *= scale;
            velocity.z *= scale;
        }

        return velocity;
    }

    /// <summary>
    /// Плавное перемещение предмета к точке удержания в руке игрока.
    /// </summary>
    private IEnumerator MoveToHandRoutine(Transform handPoint, float duration = 0.3f)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        // Отключаем физику во время движения
        Rigidbody rb = GetComponent<Rigidbody>();
        bool hadRb = rb != null;
        if (hadRb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Отключаем коллайдеры
        foreach (var col in _colliders)
            col.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Квадратичное замедление в конце
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, handPoint.position, t);
            transform.rotation = Quaternion.Slerp(startRotation, handPoint.rotation, t);

            yield return null;
        }

        // Фиксируем окончательную позицию
        transform.position = handPoint.position;
        transform.rotation = handPoint.rotation;

        // Прикрепляем к точке удержания
        transform.SetParent(handPoint, true);

        // Восстанавливаем коллайдеры (остаются отключенными, так как предмет в руке)
        // Rigidbody остается kinematic
    }

    private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);
}
