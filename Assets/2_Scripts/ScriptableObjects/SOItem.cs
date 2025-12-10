using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Item")]
public class SOItem : ScriptableObject
{
    [SerializeField] private string label = "New Item";
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField, Range(1,99)] private int maxStack = 99;
    [SerializeField] private CollectibleItem worldPrefab;

    public string Label => label;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => maxStack;
    public CollectibleItem WorldPrefab => worldPrefab;
}