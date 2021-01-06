using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Field;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Scripts.Map
{
    public class MapScript : ScriptBase
    {
        public MapScriptContext Context { get; }
        
        public bool IsFirstUserEnterScript { get; }
        public CField Field { get; }

        // normal field scripts (not FUE) are tied to the client instance
        public MapScript(string sScriptName, string sScriptContents, WvsGameClient client) : base(sScriptName, sScriptContents, client)
        {
            Context = new MapScriptContext(this);
            IsFirstUserEnterScript = false;
            Field = client.Character.Field;
        }

        // first user enter scripts are tied to the field instance
        public MapScript(string sScriptName, string sScriptContents, CField field, WvsGameClient client) : base(sScriptName, sScriptContents, client)
        {
            Context = new MapScriptContext(this);
            IsFirstUserEnterScript = true;
            Field = field;
        }

        public override void InitLocals()
        {
            Engine.Set("ctx", Context);
        }

        public override void Dispose()
        {
            base.Dispose(); // no need to do anything here - client expects nothing
        }
    }
}
