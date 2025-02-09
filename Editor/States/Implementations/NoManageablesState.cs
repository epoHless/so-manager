using UnityEditor;

namespace epoHless.SOManager
{
    public class NoManageablesState : ViewState
    {
        public override void OnGUI( SOManagerWindow window )
        {
            EditorGUILayout.HelpBox( "No manageables found!", MessageType.Error );
            EditorGUILayout.HelpBox( "Create a new one or import one from the project.", MessageType.Info );
        }
    }
}