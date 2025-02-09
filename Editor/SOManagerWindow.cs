using UnityEditor;
using UnityEngine;

namespace epoHless.SOManager
{
    public class SOManagerWindow : EditorWindow
    {
        [MenuItem( "Tools/epoHless/SOManager" )]
        private static void ShowWindow()
        {
            var window = GetWindow< SOManagerWindow >();
            window.titleContent = new GUIContent( "SOManager" );
            window.Show();
        }

        private ViewState _state;

        private void OnEnable() => _state = SOManager.HasSettings() ? new RootState() : new NoSettingsState();

        private void OnGUI()
        {
            if ( _state != null )
            {
                _state.OnGUI( this );
            }
        }

        public void ChangeState( ViewState state ) => _state = state;
    }
}