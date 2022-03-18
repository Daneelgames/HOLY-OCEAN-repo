using Cysharp.Threading.Tasks;
using ScriptGraphPro.Attributes;
using ScriptGraphPro.Attributes.Fields;
using ScriptGraphPro.Commands;
using UnityEngine;

namespace MrPink.PhoneScripting.ScriptCommands
{
    [CommandNode("Сообщение")]
    [TabGroup(Tabs.Dialogue)]
    public class MessageCommand : Command
    {
        [NodeContent("Текст")] 
        [Searchable] 
        public string Text = "";  // TODO перевести на LocaleString


        protected override async UniTask<Return> Run()
        {
            await base.Run();
            
            Debug.Log("Собеседник печатает...");

            await UniTask.Delay(100);
            
            Debug.Log(Text);
            
            return Return.NextCommand(OutKey);
        }
    }
}