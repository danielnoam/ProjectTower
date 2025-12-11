using System.Collections.Generic;
using UnityEngine;

public class StatusEffectComponent : MonoBehaviour
{
    private readonly List<StatusEffect> _activeStatuses = new List<StatusEffect>();
    private ICombatTarget _combatTarget;
    
    private void Awake()
    {
        _combatTarget = GetComponent<ICombatTarget>();
    }
    
    private void Update()
    {
        for (int i = _activeStatuses.Count - 1; i >= 0; i--)
        {
            var status = _activeStatuses[i];
            status.duration -= Time.deltaTime;
            
            if (status.duration <= 0)
            {
                status.OnRemove(_combatTarget);
                _activeStatuses.RemoveAt(i);
            }
            else
            {
                status.OnTick(_combatTarget, Time.deltaTime);
            }
        }
    }
    
    public void ApplyStatus(StatusEffect status)
    {
        var existing = _activeStatuses.Find(s => s.GetType() == status.GetType());
        
        if (existing != null)
        {
            if (status.CanStack)
            {
                _activeStatuses.Add(status.Clone());
                status.OnApply(_combatTarget);
            }
            else
            {
                existing.duration = status.duration;
            }
        }
        else
        {
            _activeStatuses.Add(status.Clone());
            status.OnApply(_combatTarget);
        }
    }
    
    public bool HasStatus<T>() where T : StatusEffect
    {
        return _activeStatuses.Exists(s => s is T);
    }
    
    public T GetStatus<T>() where T : StatusEffect
    {
        return _activeStatuses.Find(s => s is T) as T;
    }
    
    public void RemoveStatus<T>() where T : StatusEffect
    {
        for (int i = _activeStatuses.Count - 1; i >= 0; i--)
        {
            if (_activeStatuses[i] is T)
            {
                _activeStatuses[i].OnRemove(_combatTarget);
                _activeStatuses.RemoveAt(i);
            }
        }
    }
    
    public void ClearAllStatuses()
    {
        for (int i = _activeStatuses.Count - 1; i >= 0; i--)
        {
            _activeStatuses[i].OnRemove(_combatTarget);
        }
        _activeStatuses.Clear();
    }
}