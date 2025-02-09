using System;
using System.Linq;
using epoHless.SOManager.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace epoHless.SOManager
{
    [InitializeOnLoad]
    public static class SOManager
    {
        private static SOSettings _settings;
        private static Type[] _managedTypes;

        static SOManager() => Initialize();

        public static void Initialize()
        {
            _managedTypes = AppDomain.CurrentDomain.GetAssemblies()
                                     .SelectMany( assembly => assembly.GetTypes() )
                                     .Where( type => type.IsSubclassOf( typeof(ScriptableObject) ) )
                                     .Where( type =>
                                         type.GetCustomAttributes( typeof(ManageableAttribute), false ).Length > 0 )
                                     .ToArray();

            foreach ( var type in _managedTypes )
            {
                Debug.Log( $"Initializing {type.Name}" );
            }

            _settings = AssetDatabase.FindAssets( "t:SOSettings" )
                                     .Select( AssetDatabase.GUIDToAssetPath )
                                     .Select( AssetDatabase.LoadAssetAtPath< SOSettings > )
                                     .FirstOrDefault();
        }

        public static Type[] GetManagedTypes() => _managedTypes;
        public static bool HasSettings() => _settings != null;

        public static void Create( string name, Type manageable, bool useClassPrefix = true )
        {
            var instance = ScriptableObject.CreateInstance( manageable );

            instance.name = useClassPrefix ? $"{manageable.Name}_{name}" : name;

            var finalPath = $"Assets/{_settings.DefaultPath}/{manageable.Name}/{instance.name}.asset";

            if ( !AssetDatabase.IsValidFolder( $"Assets/{_settings.DefaultPath}/{manageable.Name}" ) )
            {
                AssetDatabase.CreateFolder( $"Assets/{_settings.DefaultPath}", manageable.Name );
            }

            Undo.RegisterCreatedObjectUndo( instance, "Create Scriptable Object Of Type " + manageable.Name );

            AssetDatabase.CreateAsset( instance, finalPath );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ScriptableObject[] GetAll( Type manageable )
        {
            return AssetDatabase.FindAssets( $"t:{manageable.Name}" )
                                .Select( AssetDatabase.GUIDToAssetPath )
                                .Select( AssetDatabase.LoadAssetAtPath< ScriptableObject > )
                                .ToArray();
        }

        public static void Delete( ScriptableObject instance )
        {
            Undo.RecordObject( instance, "Delete Scriptable Object Of Type " + instance.GetType().Name );

            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( instance ) );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ScriptableObject[] SortBy( SortPreferenceType preferenceType, ScriptableObject[] array )
        {
            ISortPreference sortOperation = ( preferenceType ) switch
            {
                SortPreferenceType.Alphabetical => new AlphabeticalSort(),
                SortPreferenceType.CreationDate => new CreationDateSort(),
                SortPreferenceType.ModificationDate => new ModificationDateSort(),
                _ => throw new ArgumentOutOfRangeException( nameof( preferenceType ), preferenceType, null )
            };

            return sortOperation.Sort( array );
        }
    }
}