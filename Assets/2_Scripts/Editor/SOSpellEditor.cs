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
        
        // Casting
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("castMethod"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manaCost"));
        
        if (spell.castMethod == CastMethod.Channel)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("channelRate"));
        }
        if (spell.castMethod == CastMethod.Charge)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeTime"));
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spellForm"));
        
        // Conjure
        if (spell.spellForm == SpellForm.Conjure)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjurePrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjureMovement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjureCollision"));
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("domains"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif