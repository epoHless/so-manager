using UnityEngine;

namespace epoHless.SOManager
{
    public class SOSettings : ScriptableObject
    {
        [field: SerializeField, Tooltip( "Do not include `/` for opening and closing paths." )]
        public string DefaultPath { get; private set; } = "Resources";

        [field: SerializeField] public bool UseClassPrefixForNaming { get; private set; } = true;
    }
}