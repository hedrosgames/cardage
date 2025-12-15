using UnityEngine;
public class ManagerBoard : MonoBehaviour
{
    public CardSlot[] allSlots = new CardSlot[9];
    private CardSlot[,] grid = new CardSlot[3, 3];
    private void Awake()
    {
        foreach (CardSlot slot in allSlots)
        {
            if (slot == null)
            continue;
            Vector2Int pos = slot.gridPosition;
            if (pos.x >= 0 && pos.x < 3 && pos.y >= 0 && pos.y < 3)
            grid[pos.x, pos.y] = slot;
        }
    }
    public CardSlot GetSlot(int x, int y)
    {
        if (x < 0 || x > 2)
        return null;
        if (y < 0 || y > 2)
        return null;
        return grid[x, y];
    }
    public CardSlot[] GetAllSlots()
    {
        return allSlots;
    }
}

