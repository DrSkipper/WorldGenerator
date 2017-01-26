using UnityEngine;

public class WorldGenStarter : MonoBehaviour
{
    public WorldGenManager WorldGenManager;
    public WorldGenSpecs Specs;

    void Start()
    {
        this.WorldGenManager.InitiateGeneration(this.Specs);
    }
}
