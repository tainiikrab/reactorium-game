using System;
using System.Collections.Generic;
using ChemSimDiploma.SceneObjectController;
using PrimeTween;
using UnityEngine;

namespace ChemSimDiploma.Indicator
{
[RequireComponent(typeof(Draggable))]
public class IndicatorBoxController : MonoBehaviour
{
    private struct PendingReturn
    {
        public IndicatorStickController Stick;
        public Transform Slot;
        public IndicatorStickController SlotStick;
        public float StoredPh;
        public bool Dipped;
    }

    [SerializeField] private IndicatorStickController _stickPrefab;

    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private Transform _returnPoint;

    [Header("Return animation")]
    [SerializeField] private float _returnMoveDuration = 0.5f;

    [SerializeField] private Ease _returnMoveEase = Ease.InOutCubic;

    [SerializeField] private float _returnFadeDuration = 0.4f;

    [SerializeField] private Ease _returnFadeEase = Ease.InCubic;

    [SerializeField] private float _returnRevealDuration = 0.4f;

    [SerializeField] private Ease _returnRevealEase = Ease.OutCubic;

    [SerializeField] private float _stowEndScale = 0.35f;

    [SerializeField] private bool _infiniteSupply;

    [SerializeField] private Transform[] _decorativeStickRoots;

    [Tooltip("Если в иерархии нет декоративных палочек, используется это число запаса (без скрытия объектов).")]
    [SerializeField] private int _supplyWhenNoDecoratives = 3;

    private readonly HashSet<IndicatorStickController> _spawnedSticks = new();
    private readonly Dictionary<Transform, Vector3> _decorativeBaseScales = new();
    private int _sticksRemaining;
    private PendingReturn? _pendingReturn;
    private Sequence _slotRevealSequence;

    public bool CanAcceptTap => (_infiniteSupply || _sticksRemaining > 0) && !_pendingReturn.HasValue;

    public Transform ReturnPoint => _returnPoint != null ? _returnPoint : _spawnPoint;

    public float ReturnAttachDuration => _returnMoveDuration;

    public Ease ReturnAttachEase => _returnMoveEase;

    private void Awake()
    {
        if (_spawnPoint == null)
        {
            var go = new GameObject("StickSpawn");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 14f, 0f);
            _spawnPoint = go.transform;
        }

        if (_decorativeStickRoots == null || _decorativeStickRoots.Length == 0)
            CollectDecorativeSticksFromHierarchy();

        if (_decorativeStickRoots != null && _decorativeStickRoots.Length > 0)
        {
            _sticksRemaining = _decorativeStickRoots.Length;
            CacheDecorativeBaseScales();
            DisableDraggingOnDecorativeSticks();
        }
        else
        {
            _sticksRemaining = Mathf.Max(0, _supplyWhenNoDecoratives);
        }
    }

    private void OnDestroy()
    {
        if (_slotRevealSequence.isAlive)
            _slotRevealSequence.Stop();
    }

    private void CacheDecorativeBaseScales()
    {
        _decorativeBaseScales.Clear();
        foreach (Transform root in _decorativeStickRoots)
            if (root != null)
                _decorativeBaseScales[root] = root.localScale;
    }

    private void DisableDraggingOnDecorativeSticks()
    {
        foreach (Transform root in _decorativeStickRoots)
        {
            if (root == null) continue;
            foreach (Collider2D c in root.GetComponentsInChildren<Collider2D>(true))
                c.enabled = false;
            foreach (Draggable d in root.GetComponentsInChildren<Draggable>(true))
                d.enabled = false;
        }
    }

    private Vector3 GetEmergeTargetWorldPosition(Transform slot)
    {
        if (slot == null)
            return _spawnPoint.position;

        Vector3 local = slot.localPosition;
        local.y = _spawnPoint.localPosition.y;
        return transform.TransformPoint(local);
    }

    private void CollectDecorativeSticksFromHierarchy()
    {
        var list = new List<Transform>();
        foreach (IndicatorStickController stick in GetComponentsInChildren<IndicatorStickController>(true))
        {
            if (stick.transform.parent != transform)
                continue;
            list.Add(stick.transform);
        }

        list.Sort((a, b) => b.localPosition.y.CompareTo(a.localPosition.y));
        _decorativeStickRoots = list.ToArray();
        _sticksRemaining = _decorativeStickRoots.Length;
    }

    public bool TrySpawnStick(out IndicatorStickController stick)
    {
        stick = null;
        if (_stickPrefab == null || !CanAcceptTap) return false;

        Vector3 emergeFrom = _spawnPoint.position;
        Transform decorativeToHide = null;
        IndicatorStickController slotState = null;

        if (!_infiniteSupply)
        {
            if (_sticksRemaining <= 0) return false;

            int hideIndex = _sticksRemaining - 1;
            if (_decorativeStickRoots != null && hideIndex >= 0 && hideIndex < _decorativeStickRoots.Length)
            {
                decorativeToHide = _decorativeStickRoots[hideIndex];
                if (decorativeToHide != null)
                {
                    emergeFrom = decorativeToHide.position;
                    decorativeToHide.TryGetComponent(out slotState);
                }
            }

            _sticksRemaining--;
            if (decorativeToHide != null)
                decorativeToHide.gameObject.SetActive(false);
        }

        Vector3 targetPos = GetEmergeTargetWorldPosition(decorativeToHide);
        stick = Instantiate(_stickPrefab, emergeFrom, Quaternion.identity);
        stick.name = "IndicatorStick (spawned)";
        stick.ResetToDryVisual();
        stick.SetSpawnedFrom(this);

        if (slotState != null)
            stick.CopyIndicatorStateFrom(slotState);

        stick.PlayEmergeFrom(emergeFrom, targetPos, stick.HasBeenDipped);

        _spawnedSticks.Add(stick);
        return true;
    }

    public void NotifyStickDestroyed(IndicatorStickController stick)
    {
        _spawnedSticks.Remove(stick);
        if (_pendingReturn.HasValue && _pendingReturn.Value.Stick == stick)
            _pendingReturn = null;
    }

    public Transform GetReturnAnchorFor(IndicatorStickController stick)
    {
        if (TryGetPendingRestoreSlot(out Transform slot, out _, out _) && slot != null)
            return slot;
        return ReturnPoint;
    }

    public bool TryReturnStick(IndicatorStickController stick)
    {
        if (stick == null || !stick.CanReturnTo(this)) return false;

        CompletePendingReturnImmediately();

        _spawnedSticks.Remove(stick);
        ClearDraggableRelations(stick);

        float storedPh = stick.StoredPh;
        bool dipped = stick.HasBeenDipped;

        TryGetPendingRestoreSlot(out Transform slot, out Vector3 pos, out Quaternion rot);
        TryResolveSlotVisual(slot, out IndicatorStickController slotStick, out SpriteRenderer slotRenderer,
            out Vector3 slotBaseScale);

        _pendingReturn = new PendingReturn
        {
            Stick = stick,
            Slot = slot,
            SlotStick = slotStick,
            StoredPh = storedPh,
            Dipped = dipped
        };

        stick.BeginReturnAnimation();

        var slotAnim = new IndicatorReturnAnimation(
            slot,
            slotStick,
            slotRenderer,
            slotBaseScale,
            pos,
            rot,
            storedPh,
            dipped,
            _returnMoveDuration,
            _returnFadeDuration,
            _returnRevealDuration,
            _stowEndScale,
            _returnMoveEase,
            _returnFadeEase,
            _returnRevealEase);

        stick.PlayReturnToBox(
            pos,
            rot,
            _returnMoveDuration,
            _returnFadeDuration,
            _stowEndScale,
            _returnMoveEase,
            _returnFadeEase,
            () => BeginSlotReveal(slotAnim, CompletePendingReturn));

        return true;
    }

    private void BeginSlotReveal(IndicatorReturnAnimation anim, Action onComplete)
    {
        if (_slotRevealSequence.isAlive)
            _slotRevealSequence.Stop();

        if (anim.SlotRoot != null)
        {
            anim.SlotRoot.gameObject.SetActive(true);
            anim.SlotRoot.localScale = anim.SlotBaseLocalScale * anim.EndScaleFactor;
        }

        if (anim.SlotStick != null)
        {
            anim.SlotStick.ApplyVisualStateImmediate(anim.HasBeenDipped, anim.StoredPh);
        }
        else if (anim.SlotRenderer != null)
        {
            Color color = anim.HasBeenDipped
                ? IndicatorPhColor.ForPh(anim.StoredPh)
                : new Color(0.99f, 0.78f, 0.34f, 1f);
            anim.SlotRenderer.color = new Color(color.r, color.g, color.b, 0f);
        }

        if (anim.SlotRoot == null || anim.SlotRenderer == null)
        {
            onComplete?.Invoke();
            return;
        }

        _slotRevealSequence = Sequence.Create()
            .Chain(Tween.Scale(anim.SlotRoot, anim.SlotBaseLocalScale, anim.RevealDuration, anim.RevealEase))
            .Group(CreateSlotRevealAlphaTween(anim))
            .OnComplete(onComplete);
    }

    private static Tween CreateSlotRevealAlphaTween(IndicatorReturnAnimation anim)
    {
        SpriteRenderer renderer = anim.SlotRenderer;
        return Tween.Custom(renderer, 0f, 1f, anim.RevealDuration, (sr, alpha) =>
        {
            Color rgb = anim.HasBeenDipped
                ? IndicatorPhColor.ForPh(anim.StoredPh)
                : anim.SlotStick != null
                    ? anim.SlotStick.IndicatorColor
                    : new Color(0.99f, 0.78f, 0.34f, 1f);
            rgb.a = alpha;
            sr.color = rgb;
        }, anim.RevealEase);
    }

    private void CompletePendingReturn()
    {
        if (!_pendingReturn.HasValue) return;

        PendingReturn pending = _pendingReturn.Value;
        _pendingReturn = null;

        CommitSlotState(pending.SlotStick, pending.Dipped, pending.StoredPh);
        FinishReturn(pending.Stick, pending.Slot);
    }

    private void CompletePendingReturnImmediately()
    {
        if (!_pendingReturn.HasValue) return;

        if (_slotRevealSequence.isAlive)
            _slotRevealSequence.Stop();

        CompletePendingReturn();
    }

    private void CommitSlotState(IndicatorStickController slotStick, bool dipped, float storedPh)
    {
        if (slotStick == null) return;

        slotStick.ApplyVisualStateImmediate(dipped, storedPh);
        slotStick.EnsureFullOpacity();
    }

    private void FinishReturn(IndicatorStickController stick, Transform restoredSlot)
    {
        if (stick != null)
            Destroy(stick.gameObject);

        if (!_infiniteSupply && restoredSlot != null)
            _sticksRemaining++;
    }

    private bool TryGetPendingRestoreSlot(out Transform slot, out Vector3 worldPos, out Quaternion worldRot)
    {
        slot = null;
        worldPos = ReturnPoint.position;
        worldRot = Quaternion.identity;

        if (_infiniteSupply || _decorativeStickRoots == null || _sticksRemaining >= _decorativeStickRoots.Length)
            return false;

        slot = _decorativeStickRoots[_sticksRemaining];
        if (slot == null) return false;

        worldPos = slot.position;
        worldRot = slot.rotation;
        return true;
    }

    private void TryResolveSlotVisual(
        Transform slot,
        out IndicatorStickController slotStick,
        out SpriteRenderer slotRenderer,
        out Vector3 slotBaseScale)
    {
        slotStick = null;
        slotRenderer = null;
        slotBaseScale = Vector3.one;

        if (slot == null) return;

        slot.TryGetComponent(out slotStick);
        if (slotStick != null)
            slotRenderer = slotStick.GetComponentInChildren<SpriteRenderer>(true);
        else
            slotRenderer = slot.GetComponentInChildren<SpriteRenderer>(true);

        if (_decorativeBaseScales.TryGetValue(slot, out Vector3 cached))
            slotBaseScale = cached;
    }

    private static void ClearDraggableRelations(IndicatorStickController stick)
    {
        if (!stick.TryGetComponent(out Draggable draggable)) return;

        if (draggable.Receiver != null)
            draggable.Receiver.Sender = null;
        if (draggable.Sender != null)
            draggable.Sender.Receiver = null;
        draggable.Receiver = null;
        draggable.Sender = null;
    }
}
}