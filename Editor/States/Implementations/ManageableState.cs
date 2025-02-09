using System;
using epoHless.SOManager.Utilities;
using UnityEditor;
using UnityEngine;

namespace epoHless.SOManager
{
    public class ManageableState : ViewState
    {
        private ScriptableObject[] _instances;

        private readonly Type _manageable;

        private string _instanceName;

        private Editor _editor;

        private int _sortIndex = 0;

        public ManageableState( Type manageable )
        {
            _manageable = manageable;
            _instances = SOManager.GetAll( manageable );

            if ( _instances.Length > 0 )
            {
                _editor = Editor.CreateEditor( _instances[ 0 ] );
            }
        }

        public override void OnGUI( SOManagerWindow window )
        {
            using ( new GUILayout.HorizontalScope() )
            {
                RenderToolbar( window );
            }

            using ( new GUILayout.HorizontalScope() )
            {
                using ( new GUILayout.VerticalScope() )
                {
                    RenderObjects();
                }

                RenderEditor();
            }
        }

        private void RenderEditor()
        {
            if ( _editor != null )
            {
                using ( new EditorGUILayout.VerticalScope( GUI.skin.box ) )
                {
                    _editor?.OnInspectorGUI();
                }
            }
        }

        private void RenderObjects()
        {
            for ( var index = 0; index < _instances.Length; index++ )
            {
                var instance = _instances[ index ];

                using ( new GUILayout.HorizontalScope() )
                {
                    if ( GUILayout.Button( instance.name, GUILayout.Width( 300 ) ) )
                        _editor = Editor.CreateEditor( instance );

                    if ( GUILayout.Button( "[P]", GUILayout.Width( 25 ) ) )
                        EditorGUIUtility.PingObject( instance );

                    if ( GUILayout.Button( "[X]", GUILayout.Width( 25 ) ) )
                    {
                        SOManager.Delete( instance );
                        ArrayUtility.Remove( ref _instances, instance );
                        _editor = null;
                    }
                }
            }
        }

        private void RenderToolbar( SOManagerWindow window )
        {
            if ( GUILayout.Button( "<-", GUILayout.Width( 30 ) ) )
                window.ChangeState( new RootState() );

            _instanceName = GUILayout.TextField( _instanceName );

            if ( CheckInputEvent( EventType.KeyDown, KeyCode.Space ) && !string.IsNullOrEmpty( _instanceName ) )
                CreateInstance();

            if ( GUILayout.Button( "Create", GUILayout.Width( 100 ) ) && !string.IsNullOrEmpty( _instanceName ) )
                CreateInstance();

            RenderSortOption();
        }

        private void RenderSortOption()
        {
            EditorGUI.BeginChangeCheck();

            _sortIndex = EditorGUILayout.Popup( _sortIndex,
                new[] { "Alphabetical", "Creation Date", "Modification Date" }, GUILayout.Width( 200 ) );

            if ( EditorGUI.EndChangeCheck() )
            {
                _instances = ( _sortIndex ) switch
                {
                    0 => new AlphabeticalSort().Sort( _instances ),
                    1 => new CreationDateSort().Sort( _instances ),
                    2 => new ModificationDateSort().Sort( _instances ),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private bool CheckInputEvent( EventType keyDown, KeyCode keyCode ) =>
            Event.current.type == keyDown && Event.current.keyCode == keyCode;

        private void CreateInstance()
        {
            SOManager.Create( _instanceName, _manageable );

            _instanceName = string.Empty;

            _instances = SOManager.GetAll( _manageable );
        }
    }
}