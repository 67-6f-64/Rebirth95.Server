using System;
using Rebirth.Field;

namespace Rebirth.Server.Game
{
    public class CFieldItem : IDisposable
    {
        public CField Field { get; private set; }
        public DateTime LastUpdate { get; set; }

        public CFieldItem(CField field)
        {
            Field = field;
            LastUpdate = DateTime.Now;
        }

        public void Dispose() => Field.Dispose();
    }
}
