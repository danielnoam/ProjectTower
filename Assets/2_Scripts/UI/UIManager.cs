using System;
using DNExtensions.InputSystem;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private PlayerHUD hud;
    [SerializeField] private SpellCraftingUI spellCraftingUI;
    [SerializeField] private SpellCraftingStation spellCraftingStation;
    [SerializeField] private GameObject gameFinishedUI;


    private void Awake()
    {
        spellCraftingUI.gameObject.SetActive(false);
        hud.gameObject.SetActive(true);
        gameFinishedUI.SetActive(false);
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
    
    public void ShowGameFinishedUI()
    {
        hud.gameObject.SetActive(false);
        gameFinishedUI.SetActive(true);
        InputManager.Instance?.EnableUIInput();
    }
}
