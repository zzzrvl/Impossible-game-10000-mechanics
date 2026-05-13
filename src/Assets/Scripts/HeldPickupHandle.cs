using UnityEngine;

/// <summary>
/// Обёртка над экипированным предметом: в мире объект уничтожен, слот держит префаб для спавна при выбросе.
/// </summary>
public sealed class HeldPickupHandle : IInteractable
{
    private readonly PlayerEntity _owner;
    private readonly GameObject _prefabSource;
    private GameObject _visualInstance;

    public HeldPickupHandle(PlayerEntity owner, GameObject prefabSource)
    {
        _owner = owner;
        _prefabSource = prefabSource;
        CreateVisual();
    }

    private void CreateVisual()
    {
        if (_prefabSource == null || _owner == null)
            return;

        _visualInstance = Object.Instantiate(_prefabSource);
        _visualInstance.name = _prefabSource.name + " (Held)";
        
        // Disable any PickUpItem component to prevent picking up the held instance
        var pickup = _visualInstance.GetComponent<PickUpItem>();
        if (pickup != null)
            pickup.enabled = false;
        
        // Disable colliders
        foreach (var col in _visualInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;
        
        // Attach to player's hold point
        Transform holdPoint = _owner.ItemHoldPoint;
        _visualInstance.transform.SetParent(holdPoint, false);
        _visualInstance.transform.localPosition = Vector3.zero;
        _visualInstance.transform.localRotation = Quaternion.identity;
        
        // Ensure it's active
        _visualInstance.SetActive(true);
    }

    private void DestroyVisual()
    {
        if (_visualInstance != null)
            Object.Destroy(_visualInstance);
        _visualInstance = null;
    }

    public void Use(Entity owner)
    {
    }

    public void Throw()
    {
        if (_owner == null)
            return;

        if (_prefabSource == null)
        {
            if (ReferenceEquals(_owner.equippedItem, this))
                _owner.equippedItem = null;
            return;
        }

        PickUpItem.SpawnThrownPickup(_owner, _prefabSource);
        DestroyVisual();

        if (ReferenceEquals(_owner.equippedItem, this))
            _owner.equippedItem = null;
    }
}
