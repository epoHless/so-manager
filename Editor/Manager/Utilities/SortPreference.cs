using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace epoHless.SOManager.Utilities
{
    internal enum SortPreferenceType
    {
        Alphabetical,
        CreationDate,
        ModificationDate
    }

    internal interface ISortPreference
    {
        public ManageableState.SOInfo[] Sort( ManageableState.SOInfo[] array );
    }

    internal class AlphabeticalSort : ISortPreference
    {
        public ManageableState.SOInfo[] Sort( ManageableState.SOInfo[] array ) => array.OrderBy( o => o.Instance.name ).ToArray();
    }

    internal class CreationDateSort : ISortPreference
    {
        public ManageableState.SOInfo[] Sort( ManageableState.SOInfo[] array ) =>
            array.OrderBy( o => File.GetCreationTime( AssetDatabase.GetAssetPath( o.Instance ) ) ).ToArray();
    }

    internal class ModificationDateSort : ISortPreference
    {
        public ManageableState.SOInfo[] Sort( ManageableState.SOInfo[] array ) =>
            array.OrderBy( o => File.GetLastWriteTime( AssetDatabase.GetAssetPath( o.Instance ) ) ).ToArray();
    }
}