using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{

    public static SpriteManager Singleton { get; private set; }

    public Sprite arrow;
    public Sprite neutral;
    public Sprite cross;

    private void Awake()
    {
        Singleton = this;
    }


}
