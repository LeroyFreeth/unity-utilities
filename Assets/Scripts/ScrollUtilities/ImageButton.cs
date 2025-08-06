using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Button))]
public class ImageButton : Image
{
    [SerializeField] private Button _button;
    private ExampleImageData _exampleImageData;

    public void SetData(ExampleImageData exampleImageData)
    {
        sprite = exampleImageData.sprite;
        _exampleImageData = exampleImageData;
    }

    private void ClickImageButton()
    {
        Debug.Log(_exampleImageData.ToString());
    }

    protected override void Reset()
    {
        base.Reset();

        _button = GetComponent<Button>();
        _button.onClick.AddListener(ClickImageButton);
    }
}
