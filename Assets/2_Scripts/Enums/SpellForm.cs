using UnityEngine;

public enum SpellForm
{
    [InspectorName("Imbue Self")]
    Imbue = 0,
    
    [InspectorName("Invoke Target")]
    Invoke = 1,
    
    [InspectorName("Conjure Projectile")]
    Conjure = 2
}