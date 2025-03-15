using System;
using System.IO;
using epoHless.SOManager.Utilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace epoHless.SOManager
{
    public class ManageableState : ViewState
    {
        private SOInfo[] _infos;

        private readonly Type _manageable;
        
        private string _infoName;
        private int _sortIndex = 0;
        private bool _wantsToRename;
        
        private Vector2 _scrollPosition;
        private string _newName;

        public ManageableState( Type manageable )
        {
            _manageable = manageable;

            var scriptableObjects = SOManager.GetAll( manageable );

            _infos = new SOInfo[ scriptableObjects.Length ];

            for ( var index = 0; index < _infos.Length; index++ )
            {
                var info = scriptableObjects[ index ];
                _infos[ index ] = new SOInfo( info );
            }
        }

        public override void OnGUI( SOManagerWindow window )
        {
            using ( new GUILayout.HorizontalScope(GUI.skin.box) )
            {
                RenderToolbar( window );
            }
            
            using ( new GUILayout.HorizontalScope(GUI.skin.box) )
            {
                RenderAllItemsInfo();
            }

            using ( new GUILayout.HorizontalScope() )
            {
                using ( new GUILayout.VerticalScope() )
                {
                    using ( var scrollView = new EditorGUILayout.ScrollViewScope( _scrollPosition ) )
                    {
                        _scrollPosition = scrollView.scrollPosition;
                        
                        RenderObjects();
                    }
                }
            }
        }
        
        private bool _showAllItemsInfo = false;

        private void RenderAllItemsInfo()
        {
            var showItemsString = _showAllItemsInfo ? "Hide All Items Info" : "Show All Items Info";
            
            if ( GUILayout.Button( showItemsString, GUILayout.Width( GetSize( showItemsString ) ) ) ) _showAllItemsInfo = !_showAllItemsInfo;

            if ( _showAllItemsInfo ) RenderAllItemsInfoContent();
            
            var itemsString = "Total Items: " + _infos.Length;
            EditorGUILayout.LabelField( itemsString, GUILayout.Width( GetSize( itemsString ) ) );
            
            var totalSize = 0L;
            
            foreach ( var info in _infos )
            {
                totalSize += info.Info.Length;
            }
            
            var totalSizeString = " -  Total Size: " + ( totalSize / 1024 ) + " KB";
            EditorGUILayout.LabelField( totalSizeString, GUILayout.Width( GetSize( totalSizeString ) ) );
        }

        private void RenderAllItemsInfoContent()
        {
            for ( var index = 0; index < _infos.Length; index++ ) _infos[ index ].IsFolded = _showAllItemsInfo;
        }

        private void RenderEditor( int index )
        {
            if ( index >= _infos.Length ) return;
            
            var info = _infos[ index ];
            
            if ( info.Instance == null ) return;

            var serializedObject = new SerializedObject( info.Instance );

            if ( !info.IsFolded )
            {
                using ( new EditorGUILayout.VerticalScope( GUI.skin.box ) )
                {
                    serializedObject.Update();
                    serializedObject.DrawInspectorExcept( "m_Script" );
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void RenderObjects()
        {
            for ( var index = 0; index < _infos.Length; index++ )
            {
                var info = _infos[ index ];
                
                EditorGUI.BeginChangeCheck();

                using ( new GUILayout.VerticalScope( GUI.skin.box ) )
                {
                    using ( new GUILayout.HorizontalScope() )
                    {
                        RenderInstanceInfo( info, index, _infos[ index ].Info );
                    }

                    using ( new GUILayout.HorizontalScope() )
                    {
                        RenderEditor( index );
                    }
                }
                
                if ( EditorGUI.EndChangeCheck())
                {
                    if ( info.Instance == null )
                    {
                        continue;
                    }

                    info.Info = new FileInfo( AssetDatabase.GetAssetPath( info.Instance ) );
                }
            }
        }

        private void RenderInstanceInfo( SOInfo info, int index, FileInfo fileInfo )
        {
            if ( GUILayout.Button( info.IsFolded ? ">" : "v" , GUILayout.Width( 20 ) ) )
                ToggleEditor( index );
            
            AutoSizedLabel( "Name: " + info.Instance.name );
            AutoSizedLabel("Created: " + File.GetCreationTime( AssetDatabase.GetAssetPath( info.Instance ) ) );
            AutoSizedLabel("Size: " + fileInfo.Length + " bytes");
            
           if(GUILayout.Button( "[R]", GUILayout.Width( GetSize("[R]") ) ))
                _wantsToRename = !_wantsToRename;
            
            if ( _wantsToRename )
            {
                _newName = info.Instance.name;
                _newName = EditorGUILayout.TextField( _newName );

                if ( GUILayout.Button( "Save", GUILayout.Width( 50 ) ) )
                {
                    AssetDatabase.RenameAsset( AssetDatabase.GetAssetPath( info.Instance ), _newName );
                    AssetDatabase.SaveAssets();
                    
                    _infos[ index ].Info = new FileInfo( AssetDatabase.GetAssetPath( info.Instance ) );
                    _infos[ index ].Instance.name = _newName;
                    
                    EditorUtility.SetDirty( info.Instance );
                    
                    _wantsToRename = false;
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if ( GUILayout.Button( "[P]", GUILayout.Width( 25 ) ) )
                EditorGUIUtility.PingObject( info.Instance );

            if ( GUILayout.Button( "[X]", GUILayout.Width( 25 ) ) )
            {
                SOManager.Delete( info.Instance );

                ArrayUtility.Remove( ref _infos, info );
            }
        }

        private void ToggleEditor( int index ) => _infos[ index ].IsFolded = !_infos[ index ].IsFolded;

        private void RenderToolbar( SOManagerWindow window )
        {
            if ( GUILayout.Button( "<-", GUILayout.Width( 30 ) ) )
                window.ChangeState( new RootState() );

            _infoName = GUILayout.TextField( _infoName );

            if ( CheckInputEvent( EventType.KeyDown, KeyCode.Space ) && !string.IsNullOrEmpty( _infoName ) )
                CreateInstance();

            if ( GUILayout.Button( "Create", GUILayout.Width( 100 ) ) && !string.IsNullOrEmpty( _infoName ) )
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
                _infos = ( _sortIndex ) switch
                {
                    0 => new AlphabeticalSort().Sort( _infos ),
                    1 => new CreationDateSort().Sort( _infos ),
                    2 => new ModificationDateSort().Sort( _infos ),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private bool CheckInputEvent( EventType keyDown, KeyCode keyCode ) =>
            Event.current.type == keyDown && Event.current.keyCode == keyCode;

        private void CreateInstance()
        {
            var info = SOManager.Create( _infoName, _manageable );

            _infoName = string.Empty;

            ArrayUtility.Add( ref _infos, new SOInfo( info ) );
        }

        internal struct SOInfo
        {
            public readonly ScriptableObject Instance;
            public bool IsFolded;
            public FileInfo Info;

            public SOInfo( ScriptableObject info )
            {
                Instance = info;
                IsFolded = true;
                Info = new FileInfo( AssetDatabase.GetAssetPath( info ) );
            }
        }

        private float GetSize( string input ) => GUI.skin.label.CalcSize( new GUIContent( input ) ).x + 5;
        private void AutoSizedLabel( string input ) => EditorGUILayout.LabelField( input, GUILayout.Width( GetSize( input ) ) );
    }
}