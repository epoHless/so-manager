using UnityEditor;
using UnityEngine;

namespace epoHless.SOManager
{
    public class NoSettingsState : ViewState
    {
        public override void OnGUI( SOManagerWindow window )
        {
            EditorGUILayout.HelpBox( "SOSettings not found!", MessageType.Error );

            if ( GUILayout.Button( "Create Settings" ) )
            {
                var settings = ScriptableObject.CreateInstance< SOSettings >();

                if ( !AssetDatabase.IsValidFolder( "Assets/Resources" ) )
                {
                    AssetDatabase.CreateFolder( "Assets", "Resources" );
                }

                AssetDatabase.CreateAsset( settings, "Assets/Resources/SOSettings.asset" );
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                SOManager.Initialize();

                window.ChangeState( new RootState() );
            }
        }
    }
}