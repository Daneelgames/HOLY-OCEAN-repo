using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Unity.VisualScripting;
using UnityEngine;

public class BillboardGenerator : MonoBehaviour
{
    public string currentBillboardSign = "welcome to dust";
     Vector3Int BillboardDirection = Vector3Int.right;
    public Vector3 lettersEuler = Vector3.zero;
     Vector3 BillboardStartPos = Vector3.zero;
    public int wallSize = 10;
    public int lettersSpacingInTilesHorizontal = 3;
    public int lettersSpacingInTilesVertical  = 2;
    public List<BillboardLetter> letters;

    public List<TileHealth> spawnedLetters = new List<TileHealth>();
    [ContextMenu("GenerateBillboard")]
    public void GenerateBillboard()
    {
        for (int i = spawnedLetters.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnedLetters[i].gameObject);    
        }
        spawnedLetters.Clear();
        
        string sign = currentBillboardSign.ToUpper();
        int xxx = 0;
        int rows = 0;
        int x = 0;
        BillboardStartPos.x = +wallSize / 2;
        for (int i = 0; i < sign.Length; i++)
        {
            TileHealth letterPrefab = null;
            foreach (var billboardLetter in letters)
            {
                if (billboardLetter.letter.ToUpper()[0] == sign[i])
                {
                    letterPrefab = billboardLetter.letterPrefab;
                    break;
                }
            }

            xxx += lettersSpacingInTilesHorizontal;
            
            if (xxx >= wallSize) // new row
            {
                rows++;
                xxx = 0;
                x = 0;
                foreach (var spawnedLetter in spawnedLetters)
                {
                    spawnedLetter.transform.position += Vector3.up * lettersSpacingInTilesVertical;   
                }
            }

            x++;
            if (letterPrefab == null)
                continue;
            
            var letter = Instantiate(letterPrefab,
                transform.position + BillboardStartPos + BillboardDirection * lettersSpacingInTilesHorizontal * x + Vector3.down * lettersSpacingInTilesVertical * rows,
                Quaternion.identity);
            letter.transform.parent = transform;
            letter.transform.localRotation = Quaternion.Euler(lettersEuler);
            spawnedLetters.Add(letter);
        }
    }
    [Serializable]
    public class BillboardLetter
    {
        public string letter = "";
        public TileHealth letterPrefab;
    }
}
