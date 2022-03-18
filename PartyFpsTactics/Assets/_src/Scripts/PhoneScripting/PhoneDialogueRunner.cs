using System;
using System.Collections.Generic;
using ScriptGraphPro.Runtime;
using Sirenix.OdinInspector;

namespace MrPink.PhoneScripting
{
    
    public class PhoneDialogueRunner : ScriptGraphRunner<PhoneDialogue>
    {
        protected override Dictionary<string, HashSet<Guid>> LoadVisitedCommands()
            => new Dictionary<string, HashSet<Guid>>();
        
        [Button]
        public void Run()
            => RunScript(LoadVisitedCommands());
    }
}