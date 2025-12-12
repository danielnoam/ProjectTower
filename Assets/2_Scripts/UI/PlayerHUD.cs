using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private Image reticleImage;
    [SerializeField] private FPCManager fpcManager;

    private readonly StringBuilder _sb = new StringBuilder();
    private string _healthText;
    private string _manaText;
    private string _spellText;

    
    
    private void OnEnable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged += UpdateHealth;
        fpcManager.SpellCasterComponent.ManaChanged += UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated += UpdateInventory;
        fpcManager.FpcCaster.SpellChanged += UpdateSpell;
        fpcManager.FpcCaster.SpellCastingProgressChanged += UpdateReticle;
        
        UpdateHealth(new HealthChangeData());
        UpdateMana(0);
        UpdateSpell(fpcManager.FpcCaster.CurrentSpell);
        UpdateReticle(0,0);
        UpdateInventory();
    }

    private void OnDisable()
    {
        if (!fpcManager) return;
        
        fpcManager.HealthComponent.HealthChanged -= UpdateHealth;
        fpcManager.SpellCasterComponent.ManaChanged -= UpdateMana;
        fpcManager.InventoryComponent.InventoryUpdated -= UpdateInventory;
        fpcManager.FpcCaster.SpellChanged -= UpdateSpell;
        fpcManager.FpcCaster.SpellCastingProgressChanged -= UpdateReticle;
    }

    private void UpdateReticle(float current, float max)
    {
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
                _sb.AppendLine($"{kvp.Key.Label} x{kvp.Value}");
            }
        }
        
        inventoryText.text = _sb.ToString();
    }
}