using System.Collections.Generic;

namespace Rebirth.Server.Center.Template.DropInfo
{
    /// <summary>
    /// Contains the collection of drops for a specific mob
    /// </summary>
    public class MobDropStruct
    {
        public int MobID;
        public List<DropStruct> Drops;
    }
}
