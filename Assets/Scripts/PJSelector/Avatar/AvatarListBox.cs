using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;

// The box used for displaying the content
// Must be inherited from the class ListBox
public class AvatarListBox : ListBox
{
    [SerializeField]
    private Image _image;

    private Material normalMaterial;
    private Material blackAndWhiteMaterial;

    private void Awake()
    {
        normalMaterial = _image.material;
        blackAndWhiteMaterial = new Material(_image.material);
        blackAndWhiteMaterial.SetFloat("_GrayscaleAmount", 1);
    }

    // This function is invoked by the `CircularScrollingList` for updating the list content.
    // The type of the content will be converted to `object` in the `IntListBank` (Defined later)
    // So it should be converted back to its own type for being used.
    // The original type of the content is `int`.
    protected override void UpdateDisplayContent(object content)
    {
        AvatarBox avatarBox = (AvatarBox)content;
        _image.sprite = avatarBox.GetSprite();
        _image.material = avatarBox.GetBlackAndWhite() ? blackAndWhiteMaterial : normalMaterial;
    }
}