using System.Text;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private FPCManager fpcManager;

    private readonly StringBuilder _sb = new StringBuilder();
    private string _healthText;
    private string _manaText;
    private string _spellText;

    private void OnEnable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged += UpdateHealth;
        fpcManager.ManaSourceComponent.ManaChanged += UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated += UpdateInventory;
        fpcManager.FpcCaster.SpellChanged += UpdateSpell;
        
        UpdateHealth(0);
        UpdateMana(0);
        UpdateSpell(null);
        UpdateInventory();
    }

    private void OnDisable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged -= UpdateHealth;
        fpcManager.ManaSourceComponent.ManaChanged -= UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated -= UpdateInventory;
        fpcManager.FpcCaster.SpellChanged -= UpdateSpell;
    }

    private void UpdateHealth(float currentHealth)
    {
        var health = fpcManager.HealthComponent;
        _healthText = $"Health: {health.CurrentHealth:F0}/{health.MaxHealth:F0}";
        RefreshStatsText();
    }

    private void UpdateMana(float currentMana)
    {
        var mana = fpcManager.ManaSourceComponent;
        _manaText = $"Mana: {mana.CurrentMana:F0}/{mana.MaxMana:F0}";
        RefreshStatsText();
    }

    private void UpdateSpell(SOSpell spell)
    {
        _spellText = $"Active Spell: {(spell ? spell.name : "None")}";
        RefreshStatsText();
    }

    private void RefreshStatsText()
    {
        statsText.text = $"{_healthText}\n{_manaText}\n{_spellText}";
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
                _sb.AppendLine($"{kvp.Key.name} x{kvp.Value}");
            }
        }
        
        inventoryText.text = _sb.ToString();
    }
}