using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace epoHless.SOManager.Utilities
{
    public enum SortPreferenceType
    {
        Alphabetical,
        CreationDate,
        ModificationDate
    }

    public interface ISortPreference
    {
        public ScriptableObject[] Sort( ScriptableObject[] array );
    }

    public class AlphabeticalSort : ISortPreference
    {
        public ScriptableObject[] Sort( ScriptableObject[] array ) => array.OrderBy( o => o.name ).ToArray();
    }

    public class CreationDateSort : ISortPreference
    {
        public ScriptableObject[] Sort( ScriptableObject[] array ) =>
            array.OrderBy( o => File.GetCreationTime( AssetDatabase.GetAssetPath( o ) ) ).ToArray();
    }

    public class ModificationDateSort : ISortPreference
    {
        public ScriptableObject[] Sort( ScriptableObject[] array ) =>
            array.OrderBy( o => File.GetLastWriteTime( AssetDatabase.GetAssetPath( o ) ) ).ToArray();
    }
}