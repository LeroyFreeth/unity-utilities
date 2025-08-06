using UnityEngine.UI;
using ScrollUtilities;
using UnityEngine;

[System.Serializable] 
public struct ExampleImageData
{
    public Sprite sprite;
    public string name;

    public override string ToString()
    {
        return $"This is example data for name {name}";
    }
}

public class ExampleScrollImage : ElementScroll<ImageButton, ExampleImageData>
{
    protected override void ApplyData(int index, ExampleImageData data)
    {
        Elements[index].SetData(data);
    }
}
