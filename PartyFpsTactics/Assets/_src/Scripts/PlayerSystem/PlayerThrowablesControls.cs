using System.Collections.Generic;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerThrowablesControls : MonoBehaviour
    {
        // TODO подрубить UnityDictionary, сделать UnityDictionary<Enum, ProjectileController>
        // 0 - spycam; 1 - ladder; 2 - fragGrenade
        public List<ProjectileController> toolsPrefabs;
        public int selectedTool = 0;

        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;

            if (!LevelGenerator.Instance.levelIsReady)
                return;
        
            if (Player.Health.health <= 0)
                return;
            
            // TODO роутить управление централизованно
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                int i = selectedTool + 1;
                while (true)
                {
                    if (i >= toolsPrefabs.Count)
                        i = 0;
                
                    if (Player.Inventory.GetAmount(toolsPrefabs[i].toolType) > 0)
                    {
                        selectedTool = i;
                        break;
                    }
                
                    if (i == selectedTool)
                        break;
                    i++;
                }
            }
        
            if (Input.GetKeyDown(KeyCode.Q))
            {
                int i = selectedTool - 1;
                while (true)
                {
                    if (i < 0)
                        i = toolsPrefabs.Count - 1;
                
                    if (Player.Inventory.GetAmount(toolsPrefabs[i].toolType) > 0)
                    {
                        selectedTool = i;
                        break;
                    }
                
                    if (i == selectedTool)
                        break;
                    i--;
                }
            }
        
            if (Input.GetKeyDown(KeyCode.F))
            { 
                if (Player.Inventory.GetAmount(toolsPrefabs[selectedTool].toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                var newTool = Instantiate(toolsPrefabs[selectedTool]);
                newTool.transform.position = Player.Movement.headTransform.position;
                newTool.transform.rotation = Player.MainCamera.transform.rotation;
                newTool.Init(Player.Health);
                Player.Inventory.RemoveTool(toolsPrefabs[selectedTool].toolType);
            }
        }
    }
}