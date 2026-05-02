#if UNITY_EDITOR
using UnityEditor;

namespace CupkekGames.Combat.Editor
{
    [CustomEditor(typeof(StatusEffectManager))]
    public class StatusEffectManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
#endif
