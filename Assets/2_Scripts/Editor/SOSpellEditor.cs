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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("form"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("augment"));
        
        // Conjure
        if (spell.form == SpellForm.Conjure)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjurePrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjureMotion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conjureImpact"));
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("domains"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif