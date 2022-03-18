using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ScriptGraphPro.Attributes;
using ScriptGraphPro.Attributes.Fields;
using ScriptGraphPro.Commands;
using UnityEngine;

namespace MrPink.PhoneScripting.ScriptCommands
{
    [CommandNode("Выбор")]
    [TabGroup(Tabs.Dialogue)]
    public class ChoiceCommand : Command
    {
        [NodeContent("Варианты")] 
        [OutputDictionary]
        [Searchable] 
        public Dictionary<string, string> Choices = new Dictionary<string, string>();  // TODO перевести на LocaleString
    
        
        protected override HashSet<string> DefaultOutputKeys => new HashSet<string>();  // Нужно, чтобы изначально было пустое поле

        
        protected override async UniTask<Return> Run()
        {
            await base.Run();
            
            // Тут можно await'итить пользовательский ввод
            var randomAnswer = Choices.ElementAt(Random.Range(0, Choices.Count));
            
            Debug.Log($"> {randomAnswer.Value}");
            
            return Return.NextCommand(randomAnswer.Key);
        }
    }
}