using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(SpellEffect), true)]
public class SpellEffectDrawer : PropertyDrawer
{
    private static readonly Type[] EffectTypes;
    private static readonly string[] EffectNames;

    static SpellEffectDrawer()
    {
        EffectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(SpellEffect)))
            .ToArray();

        EffectNames = EffectTypes.Select(t => FormatName(t.Name)).ToArray();
    }
    
    private static string FormatName(string name)
    {
        // Remove "Effect" suffix
        if (name.EndsWith("Effect"))
        {
            name = name.Substring(0, name.Length - 6);
        }
    
        // Add spaces before capital letters
        string result = "";
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                result += " ";
            }
            result += name[i];
        }
    
        return result;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get current type
        string typeName = property.managedReferenceFullTypename;
        
        int selectedIndex = Array.FindIndex(EffectTypes, t => typeName.Contains(t.Name));

        // Draw dropdown
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int newIndex = EditorGUI.Popup(dropdownRect, selectedIndex, EffectNames);

        if (newIndex != selectedIndex && newIndex >= 0)
        {
            property.managedReferenceValue = Activator.CreateInstance(EffectTypes[newIndex]);
        }

        // Draw fields directly without foldout
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // Get a copy and enter into children
            SerializedProperty prop = property.Copy();
            SerializedProperty endProperty = prop.GetEndProperty();
            
            // Enter into the first child
            prop.NextVisible(true);
            
            // Draw all children
            do
            {
                if (SerializedProperty.EqualContents(prop, endProperty))
                    break;
                    
                float height = EditorGUI.GetPropertyHeight(prop, true);
                Rect fieldRect = new Rect(position.x, yOffset, position.width, height);
                EditorGUI.PropertyField(fieldRect, prop, true);
                yOffset += height + EditorGUIUtility.standardVerticalSpacing;
            }
            while (prop.NextVisible(false));
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        float height = EditorGUIUtility.singleLineHeight; // Dropdown
        float extraSpace = 5;
        height += extraSpace;
        
        if (property.managedReferenceValue != null)
        {

            SerializedProperty prop = property.Copy();
            SerializedProperty endProperty = prop.GetEndProperty();
            
            prop.NextVisible(true);
            
            do
            {
                if (SerializedProperty.EqualContents(prop, endProperty))
                    break;
                    
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            while (prop.NextVisible(false));
        }
        
        return height;
    }
}