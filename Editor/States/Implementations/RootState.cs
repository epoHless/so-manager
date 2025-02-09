using System;
using UnityEngine;

namespace epoHless.SOManager
{
    public class RootState : ViewState
    {
        private readonly Type[] _manageables = SOManager.GetManagedTypes();

        public override void OnGUI( SOManagerWindow window )
        {
            if ( _manageables.Length == 0 )
            {
                window.ChangeState( new NoManageablesState() );
                return;
            }

            if ( GUILayout.Button( "Initialise" ) ) SOManager.Initialize();

            foreach ( var manageable in _manageables )
            {
                if ( GUILayout.Button( "Manage " + manageable.Name ) )
                {
                    window.ChangeState( new ManageableState( manageable ) );
                }
            }
        }
    }
}