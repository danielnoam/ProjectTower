#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SOSpell))]
public class SOSpellEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SOSpell spell = (SOSpell)target;
        
        // Spell Info
        EditorGUILayout.PropertyField(serializedObject.FindProperty("label"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        
        EditorGUILayout.Space();
        
        // Casting
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("castType"));
        bool castTypeChanged = EditorGUI.EndChangeCheck();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manaCost"));
        
        if (spell.castType == CastType.Channeled)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("channelRate"));
        }
        
        if (spell.castType == CastType.Charged)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeTime"));
        }
        
        EditorGUILayout.Space();
        
        // Targeting
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetingType"));
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("deliveryMethod"));
        bool deliveryMethodChanged = EditorGUI.EndChangeCheck();
        
        EditorGUILayout.Space();
        
        // Show appropriate sections based on delivery method
        if (spell.deliveryMethod == DeliveryMethod.Instant)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
        }
        else if (spell.deliveryMethod == DeliveryMethod.Projectile)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectilePrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileMovement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnEffects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitEffects"), true);
        }
        
        serializedObject.ApplyModifiedProperties();
        
        if (castTypeChanged || deliveryMethodChanged)
        {
            Repaint();
        }
    }
}
#endif