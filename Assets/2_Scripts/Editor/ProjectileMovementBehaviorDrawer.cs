using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(ProjectileMovementBehavior), true)]
public class ProjectileMovementBehaviorDrawer : PropertyDrawer
{
    private static readonly Type[] BehaviorTypes;
    private static readonly string[] BehaviorNames;

    static ProjectileMovementBehaviorDrawer()
    {
        BehaviorTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ProjectileMovementBehavior)))
            .ToArray();

        BehaviorNames = BehaviorTypes.Select(t => FormatName(t.Name)).ToArray();
    }
    
    private static string FormatName(string name)
    {
        // Remove "Behavior" suffix if present
        if (name.EndsWith("Behavior"))
        {
            name = name.Substring(0, name.Length - 8);
        }
    
        // Add spaces before capital letters
        string result = "";
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
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
        
        int selectedIndex = -1;
        if (!string.IsNullOrEmpty(typeName))
        {
            selectedIndex = Array.FindIndex(BehaviorTypes, t => typeName.Contains(t.Name));
        }

        // Draw dropdown with label
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(dropdownRect, label.text, selectedIndex, BehaviorNames);
        
        if (EditorGUI.EndChangeCheck() && newIndex >= 0)
        {
            property.managedReferenceValue = Activator.CreateInstance(BehaviorTypes[newIndex]);
        }

        // Draw fields
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty prop = property.Copy();
            SerializedProperty endProperty = prop.GetEndProperty();
            
            prop.NextVisible(true);
            
            while (!SerializedProperty.EqualContents(prop, endProperty))
            {
                float height = EditorGUI.GetPropertyHeight(prop, true);
                Rect fieldRect = new Rect(position.x, yOffset, position.width, height);
                EditorGUI.PropertyField(fieldRect, prop, true);
                yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                
                if (!prop.NextVisible(false))
                    break;
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        
        if (property.managedReferenceValue != null)
        {
            SerializedProperty prop = property.Copy();
            SerializedProperty endProperty = prop.GetEndProperty();
            
            prop.NextVisible(true);
            
            while (!SerializedProperty.EqualContents(prop, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
                
                if (!prop.NextVisible(false))
                    break;
            }
        }
        
        return height;
    }
}