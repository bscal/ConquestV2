using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
using Conquest;

// ---------------
//  String => Int
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryDrawer : SerializableDictionaryDrawer<string, int>
{
    protected override SerializableKeyValueTemplate<string, int> GetTemplate()
    {
        return GetGenericTemplate<SerializableStringIntTemplate>();
    }
}
internal class SerializableStringIntTemplate : SerializableKeyValueTemplate<string, int> { }

// ---------------
//  GameObject => Float
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
public class GameObjectFloatDictionaryDrawer : SerializableDictionaryDrawer<GameObject, float>
{
    protected override SerializableKeyValueTemplate<GameObject, float> GetTemplate()
    {
        return GetGenericTemplate<SerializableGameObjectFloatTemplate>();
    }
}
internal class SerializableGameObjectFloatTemplate : SerializableKeyValueTemplate<GameObject, float> { }

// ---------------
//  Hex => TileObject
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
public class HexTileObjectDictionaryDrawer : SerializableDictionaryDrawer<Hex, TileObject>
{
    protected override SerializableKeyValueTemplate<Hex, TileObject> GetTemplate()
    {
        return GetGenericTemplate<SerializableHexTileObjectTemplate>();
    }
}
internal class SerializableHexTileObjectTemplate : SerializableKeyValueTemplate<Hex, TileObject> { }
