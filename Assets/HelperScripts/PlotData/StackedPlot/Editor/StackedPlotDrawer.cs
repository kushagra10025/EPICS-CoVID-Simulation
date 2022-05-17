using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace CSimHelper.Data
{
    // https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
    [CustomPropertyDrawer(typeof(StackedPlotAttribute))]
    public class StackedPlotDrawer : PropertyDrawer
    {
        StackedPlot stackedPlot = null;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            EditorGUI.PropertyField(position, property);

            if (!property.isExpanded)
                return;

            position.y += PropertyHeight;

            StackedPlotAttribute StackedAttribute = attribute as StackedPlotAttribute;
            StackedData data = fieldInfo.GetValue(property.serializedObject.targetObject) as StackedData;

            if (stackedPlot == null)
                stackedPlot = new StackedPlot(data, StackedAttribute);

            
            stackedPlot.OnGUI(position);
            //scatterPlot.OnInspectorGUI(position);
        }

        
        public const float PropertyHeight = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //ScatterPlotAttribute scatterPlot = attribute as ScatterPlotAttribute;
            //return property.isExpanded ? scatterPlot.Height : PropertyHeight;
            //return property.isExpanded ? 16 : PropertyHeight;
            StackedPlotAttribute plotAttribute = attribute as StackedPlotAttribute;
            //return PropertyHeight + plotAttribute.Height;
            return property.isExpanded ? PropertyHeight + plotAttribute.Height : PropertyHeight;
        }
        
    }
}