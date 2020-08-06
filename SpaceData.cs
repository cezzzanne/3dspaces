using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public struct Player {
    public int playerID;

    public float[] position;
}
[System.Serializable]

public struct Item {
    public string name;
    public float[] pos;
}
[System.Serializable]

public struct Terrain {
    public Item[] items;
}

[System.Serializable]
public struct World {
    public Terrain terrain;
    public Player player;
}

[System.Serializable]
public class SpaceData {
    // Data Class (Not Mono because not used as a component)
    // public float[] playerPosition;

    // public int characterID;
    
    // public float[][] itemPositions;

    // public string[] itemNames;

    public  World world;

    public SpaceData(GameObject terrain, Transform player, int id) {
        // 4TH ELEMENT IS ROTATION
        // characterID = id;
        float[] playerPosition = new float[4];
        playerPosition[0] = player.position.x;
        playerPosition[1] = player.position.y;
        playerPosition[2] = player.position.z;
        playerPosition[3] = player.eulerAngles.y;
        Player playerDict = new Player(){playerID = id, position = playerPosition};
        int childCount = terrain.transform.childCount;
        // itemPositions = new float[childCount][];
        // itemNames = new string[childCount];
        Terrain tempTerrain = new Terrain() {items = new Item[childCount]};
        for (int i = 0; i < childCount; i++) {
            Transform child = terrain.transform.GetChild(i);
            // itemPositions[i] = new float[4];
            float[] temp = new float[4];
            temp[0] = child.position.x;
            temp[1] = child.position.y;
            temp[2] = child.position.z;
            temp[3] = child.eulerAngles.y;
            Item item = new Item() {name = child.name, pos = temp};
            tempTerrain.items[i] = item;
            // itemPositions[i] = temp;
            // itemNames[i] = child.name;
        }
        world = new World(){player = playerDict, terrain = tempTerrain};
    }
}
