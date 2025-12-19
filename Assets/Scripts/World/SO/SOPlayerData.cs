using UnityEngine;
public enum PlayerGender { Male, Female }
[CreateAssetMenu(menuName = "Config/PlayerData")]
public class SOPlayerData : ScriptableObject
{
    public string playerName;
    public float moveSpeed;
    public PlayerGender gender = PlayerGender.Male;
    public Color characterColor = Color.white;
}

