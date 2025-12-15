using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI spellsText;
    [SerializeField] private Image reticleImage;
    [SerializeField] private FPCManager fpcManager;

    private readonly StringBuilder _sb = new StringBuilder();
    private string _healthText;
    private string _manaText;

    
    
    private void OnEnable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged += UpdateHealth;
        fpcManager.SpellCasterComponent.ManaChanged += UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated += UpdateInventory;
        fpcManager.FpcCaster.SpellChanged += UpdateSpells;
        fpcManager.FpcCaster.SpellAdded += UpdateSpells;
        fpcManager.FpcCaster.CastingProgressChanged += UpdateReticle;
        
        UpdateHealth(new HealthChangeData());
        UpdateMana(0);
        UpdateSpells(fpcManager.FpcCaster.CurrentSpell);
        UpdateReticle(CastMethod.Instant,0,0);
        UpdateInventory();
    }

    private void OnDisable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged -= UpdateHealth;
        fpcManager.SpellCasterComponent.ManaChanged -= UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated -= UpdateInventory;
        fpcManager.FpcCaster.SpellChanged -= UpdateSpells;
        fpcManager.FpcCaster.SpellAdded -= UpdateSpells;
        fpcManager.FpcCaster.CastingProgressChanged -= UpdateReticle;
    }

    private void UpdateReticle(CastMethod method, float current, float max)
    {
        switch (method)
        {
            case CastMethod.Instant:
            case CastMethod.Channel:
                reticleImage.fillAmount = 0;
                return;
        }
        
        if (max == 0)
        {
            reticleImage.fillAmount = 0;
        }
        else
        {
            reticleImage.fillAmount = current / max;
        }
    }

    private void UpdateHealth(HealthChangeData healthChangeData)
    {
        var health = fpcManager.HealthComponent;
        _healthText = $"Health: {health.CurrentHealth:F0}/{health.MaxHealth:F0}";
        RefreshStatsText();
    }

    private void UpdateMana(float currentMana)
    {
        var mana = fpcManager.SpellCasterComponent;
        _manaText = $"Mana: {mana.CurrentMana:F0}/{mana.MaxMana:F0}";
        RefreshStatsText();
    }

    private void UpdateSpells(SOSpell _)
    {
        var spells = fpcManager.FpcCaster.SpellsList;
        var currentSpell = fpcManager.FpcCaster.CurrentSpell;
        
        _sb.Clear();
        _sb.AppendLine("Spells:");
        
        if (spells.Count == 0)
        {
            _sb.Append("None");
        }
        else
        {
            foreach (var spell in spells)
            {
                if (spell == currentSpell)
                {
                    _sb.AppendLine($"[{spell.name}]");
                }
                else
                {
                    _sb.AppendLine(spell.name);
                }
            }
        }
        
        spellsText.text = _sb.ToString();
    }

    private void RefreshStatsText()
    {
        statsText.text = $"{_healthText}\n{_manaText}";
    }

    private void UpdateInventory()
    {
        var items = fpcManager.InventoryComponent.GetAllItems();
        
        _sb.Clear();
        _sb.AppendLine("Inventory:");
        
        if (items.Count == 0)
        {
            _sb.Append("Empty");
        }
        else
        {
            foreach (var kvp in items)
            {
                _sb.AppendLine($"{kvp.Key.Label} x{kvp.Value}");
            }
        }
        
        inventoryText.text = _sb.ToString();
    }
}