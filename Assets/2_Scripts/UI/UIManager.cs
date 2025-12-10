using System;
using DNExtensions.InputSystem;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private PlayerHUD hud;
    [SerializeField] private SpellCraftingUI spellCraftingUI;
    [SerializeField] private SpellCraftingStation spellCraftingStation;


    private void Awake()
    {
        spellCraftingUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        spellCraftingStation.Opened += OnSpellCraftingStationOpened;
        spellCraftingUI.Closed += OnSpellCraftingStationClosed;
    }
    
    private void OnDisable()
    {
        spellCraftingStation.Opened -= OnSpellCraftingStationOpened;
        spellCraftingUI.Closed -= OnSpellCraftingStationClosed;
    }

    private void OnSpellCraftingStationClosed()
    {
        hud.gameObject.SetActive(true);
        spellCraftingUI.gameObject.SetActive(false);
        InputManager.Instance?.EnablePlayerInput();
    }

    private void OnSpellCraftingStationOpened()
    {
        hud.gameObject.SetActive(false);
        spellCraftingUI.gameObject.SetActive(true);
        spellCraftingUI.ResetData();
        InputManager.Instance?.EnableUIInput();
    }
}
