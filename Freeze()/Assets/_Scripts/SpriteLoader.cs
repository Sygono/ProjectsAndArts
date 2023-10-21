using System.Collections.Generic;
using UnityEngine;

public class SpriteLoader : MonoBehaviour
{
    public string spriteFolder = "Sprites/NodeSprites"; // The folder path in Resources.

    private Dictionary<NodeType, Sprite> spriteDictionary = new Dictionary<NodeType, Sprite>();

    private void Awake()
    {
        LoadSpritesFromFolder();
    }

    private void LoadSpritesFromFolder()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(spriteFolder);
        foreach (Sprite sprite in loadedSprites)
        {
            string nodeName = sprite.name; // Use the sprite name as the node name.

            if (System.Enum.TryParse(nodeName, out NodeType nodeType))
            {
                spriteDictionary[nodeType] = sprite;
            }
        }
    }

    public Sprite GetSprite(NodeType nodeType)
    {
        if (spriteDictionary.TryGetValue(nodeType, out Sprite sprite))
        {
            return sprite;
        }
        return spriteDictionary[NodeType.Node]; // Return a default sprite or handle missing sprites as needed.
    }
}
