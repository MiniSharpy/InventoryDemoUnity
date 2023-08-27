using UnityEngine;

public abstract class Item : ScriptableObject
{
    public Texture2D Icon;
    public string Name;
    public string Description;
    private int _dateAdded;
}