



using UnityEngine;

[CreateAssetMenu(fileName = "Geometric", menuName = "Scriptable Objects/Conjure Geometric")]
public class SOConjureGeometric : ScriptableObject
{
    [Header("Info")]
    public string label = "Shape";
    public Sprite icon;
    
    [Header("Properties")]
    public float costMultiplier = 1.0f;
    public Conjure prefab;
    
}