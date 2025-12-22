public class SOCardInstance
{
    public SOCardData cardData;
    public int ownerId;
    public SOCardInstance(SOCardData data, int owner)
    {
        cardData = data;
        ownerId = owner;
    }
}

