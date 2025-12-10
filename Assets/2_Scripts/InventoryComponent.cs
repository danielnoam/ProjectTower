using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    private readonly Dictionary<SOItem, int> _items = new Dictionary<SOItem, int>();

    public event Action InventoryUpdated;

    public void AddItem(SOItem item, int amount)
    {
        if (_items.ContainsKey(item))
        {
            _items[item] = Mathf.Min(_items[item] + amount, item.MaxStack);
        }
        else
        {
            _items[item] = Mathf.Min(amount, item.MaxStack);
        }
        
        InventoryUpdated?.Invoke();
    }

    public void RemoveItem(SOItem item, int amount)
    {
        if (!_items.ContainsKey(item)) return;

        _items[item] -= amount;
        
        if (_items[item] <= 0)
        {
            _items.Remove(item);
        }
        
        InventoryUpdated?.Invoke();
    }
    
    public void DropItem(SOItem item, int amount, Vector3 position)
    {
        RemoveItem(item, amount);
        var collectibleItem = Instantiate(item.WorldPrefab, position, Quaternion.identity);
        collectibleItem.SetAmount(amount);
    }
    
    public void EmptyInventory()
    {
        _items.Clear();
        InventoryUpdated?.Invoke();
    }

    public int GetItemAmount(SOItem item)
    {
        return _items.GetValueOrDefault(item, 0);
    }
    
    public bool HasItem(SOItem item)
    {
        return _items.ContainsKey(item) && _items[item] >= 1;
    }
}