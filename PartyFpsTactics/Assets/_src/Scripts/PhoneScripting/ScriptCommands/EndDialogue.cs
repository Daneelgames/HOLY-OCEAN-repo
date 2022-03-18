using Cysharp.Threading.Tasks;
using ScriptGraphPro.Attributes;
using ScriptGraphPro.Attributes.Enums;
using ScriptGraphPro.Commands;
using ScriptGraphPro.StyleUtils;
using UnityEngine;

namespace MrPink.PhoneScripting.ScriptCommands
{
    [CommandNode("Завершить диалог", NodeColorOption.DarkRed, outCapacity: PortCapacity.Disabled)]
    [TabGroup(Tabs.Dialogue)]
    public class EndDialogue : Command
    {
        protected override async UniTask<Return> Run()
        {
            await base.Run();
            
            Debug.Log("Диалог завершен");

            return Return.Null;
        }
    }
}