using System.Collections.Generic;
using UnityEngine;

public class BlockCountManager : MonoBehaviour
{
    public Transform spawnPos;
    public GameObject[] blocks;
    public GameObject afBlock, glBlock, mrBlock, sxBlock, yzBlock;
    private Dictionary<string, GameObject> blockStringMapping;

    void Start()
    {
        blockStringMapping = new Dictionary<string, GameObject>
        {
            { "A-F Block", afBlock },
            { "G-L Block", glBlock },
            { "M-R Block", mrBlock },
            { "S-X Block", sxBlock },
            { "Y-Z Block", yzBlock }
        };
    }

    public void ReplaceBlock(GameObject blockToRemove)
    {
        for (int i = 0; i < 10; i++)
        {
            if (blocks[i] == blockToRemove)
            {
                float ranX = Random.Range(-0.3f, 0.3f);
                float ranZ = Random.Range(-0.3f, 0.3f);
                blocks[i] = Instantiate(blockStringMapping[blockToRemove.name], spawnPos.position + new Vector3(ranX, 0, ranZ), transform.rotation, transform);
            }
        }
    }
}
