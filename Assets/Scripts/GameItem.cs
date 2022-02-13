using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "GameItem/CharacterSet")]
public class GameItem : ScriptableObject
{
    public enum ItemType
    {
        Borrow,
        Go
    }

    public ItemType ItemTypeRef;

    public Sprite DisplaySprite;

    public Sprite DisplayChineseText;

    public AudioClip ReadAloudClip;

    /// <summary>
    /// if false we don't load it
    /// </summary>
    public bool LoadFlag;
}
