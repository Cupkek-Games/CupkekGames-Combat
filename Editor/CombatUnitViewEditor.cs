#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Combat;

using CupkekGames.VFX;

namespace CupkekGames.Combat.Editor
{
    [CustomEditor(typeof(CombatUnitView))]
    public class CombatUnitViewEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // Create the root element
            var root = new VisualElement();

            // Add the default inspector
            var defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);

            // Add component setup button
            VisualElement setupContainer = new VisualElement();
            setupContainer.style.marginTop = 10f;
            setupContainer.style.marginBottom = 10f;
            root.Add(setupContainer);

            Label setupTitle = new Label("Component Setup");
            setupTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            setupTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            setupTitle.style.fontSize = 16f;
            setupTitle.style.marginBottom = 5f;
            setupContainer.Add(setupTitle);

            Button addComponentsButton = new Button(() =>
            {
                CombatUnitView targetObj = (CombatUnitView)target;
                bool added = false;

                // Add CombatUnitHighlightController if missing
                if (targetObj.GetComponent<CombatUnitHighlightController>() == null)
                {
                    targetObj.gameObject.AddComponent<CombatUnitHighlightController>();
                    added = true;
                    Debug.Log($"Added CombatUnitHighlightController to {targetObj.gameObject.name}");
                }

                // Add FadeableOutlineSize if missing
                FadeableOutline fadeableOutline = targetObj.GetComponent<FadeableOutline>();
                if (fadeableOutline == null)
                {
                    fadeableOutline = targetObj.gameObject.AddComponent<FadeableOutline>();

                    // Set outline type to Wide using SerializedObject
                    SerializedObject serializedOutline = new SerializedObject(fadeableOutline);
                    SerializedProperty outlineTypeProp = serializedOutline.FindProperty("_outlineType");
                    if (outlineTypeProp != null)
                    {
                        outlineTypeProp.enumValueIndex = 0; // 0 = Wide
                        serializedOutline.ApplyModifiedProperties();
                    }

                    added = true;
                    Debug.Log($"Added FadeableOutlineSize to {targetObj.gameObject.name}");
                }
                else
                {
                    // Ensure outline type is set to Wide
                    SerializedObject serializedOutline = new SerializedObject(fadeableOutline);
                    SerializedProperty outlineTypeProp = serializedOutline.FindProperty("_outlineType");
                    if (outlineTypeProp != null && outlineTypeProp.enumValueIndex != 0)
                    {
                        outlineTypeProp.enumValueIndex = 0; // 0 = Wide
                        serializedOutline.ApplyModifiedProperties();
                    }
                }

                if (added)
                {
                    EditorUtility.SetDirty(targetObj);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetObj);
                }
                else
                {
                    Debug.Log($"All required components already exist on {targetObj.gameObject.name}");
                }
            })
            {
                text = "Add Missing Components"
            };
            addComponentsButton.style.height = 30f;
            addComponentsButton.style.marginTop = 5f;
            addComponentsButton.style.marginBottom = 5f;
            setupContainer.Add(addComponentsButton);

            Label title = new Label("CombatUnitView Debug");
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 24f;
            root.Add(title);

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceEvenly;
            root.Add(container);

            CombatUnitView source = (CombatUnitView)target;

            // Add buttons
            Button button = new Button(() => source.TakeDamageSquashAndStretch(.1f))
            {
                text = "Squash and Stretch 1"
            };
            container.Add(button);
            button = new Button(() => source.TakeDamageSquashAndStretch(.2f))
            {
                text = "Squash and Stretch 2"
            };
            container.Add(button);

            title = new Label("CombatUnit Debug");
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 24f;
            root.Add(title);

            container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceEvenly;
            root.Add(container);

            button = new Button(() => source.CombatUnit.Mana.Increase(100))
            {
                text = "Add Mana"
            };
            container.Add(button);

            return root;
        }
    }
}
#endif