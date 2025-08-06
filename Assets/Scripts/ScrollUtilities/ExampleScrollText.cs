using UnityEngine.UI;
using ScrollUtilities;

public class ExampleScrollText : ElementScroll<Text, string>
{
    protected override void ApplyData(int index, string data)
    {
        Elements[index].text = data;
    }
}
