using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public GameObject[] RocketHolder;
    public PlayerController playerController;

    void Start()
    {

    }

    void Update()
    {
        RocketSprite();
    }

    void RocketSprite()
    {
        for (int i = 0; i < RocketHolder.Length; i++)
        {
            RocketHolder[i].SetActive(i < playerController.Ammo);
        }
    }
}
