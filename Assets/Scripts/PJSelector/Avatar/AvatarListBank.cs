using AirFishLab.ScrollingList;
using System.Collections.Generic;

public class AvatarListBank : BaseListBank
{
    private List<AvatarBox> _contents = new List<AvatarBox>();

    public void AddToContents(AvatarBox avatarBox)
    {
        _contents.Add(avatarBox);
    }

    public override object GetListContent(int index)
    {
        return _contents[index];
    }

    public override int GetListLength()
    {
        return _contents.Count;
    }
}