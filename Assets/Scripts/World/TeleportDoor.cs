using UnityEngine;
using UnityEngine.Events;
using Game.World;
using System.Collections.Generic;
public class TeleportDoor : MonoBehaviour
{
    public enum LandingDirection
    {
        Center,
        Down,
        Up,
        Left,
        Right
    }
    [Header("Identidade")]
    [Tooltip("Define quem é esta porta E qual configuração de câmera carregar ao chegar nela.")]
    public WorldAreaId identification;
    [Header("Destino")]
    [Tooltip("Para qual ID de porta eu mando o player?")]
    public WorldAreaId destination;
    [Tooltip("Se marcado, usa a lista de condições. Se não, usa o destino simples acima.")]
    public bool multiDestination;
    [System.Serializable]
    public class DestinationEntry
    {
        public string description;
        public WorldAreaId targetID;
        public SOZoneFlag requiredFlag;
        public bool invertCondition;
    }
    public List<DestinationEntry> conditionalDestinations = new List<DestinationEntry>();
    [Header("Ajustes de Chegada (Spawn)")]
    [Tooltip("Para qual direção o player será empurrado ao sair desta porta?")]
    public LandingDirection landingDirection = LandingDirection.Center;
    float landingDistance = 1.5f;
    [Header("Cooldown")]
    public float cooldown = 0.5f;
    [Header("Estado da Porta")]
    [Tooltip("Se marcado, a porta está trancada e o player não pode passar. Padrão: aberta (false)")]
    public bool isLocked = false;
    [Tooltip("Flag que controla se a porta está aberta. Se configurada, sobrescreve isLocked. Flag = true = porta aberta")]
    public SOZoneFlag unlockFlag;
    [Tooltip("Se marcado, a porta está trancada quando a flag é TRUE (inverte a lógica)")]
    public bool invertFlagLogic = false;
    [Tooltip("Diálogo exibido quando o player encosta na porta trancada")]
    public SODialogueSequence lockedDialogue;
    [Tooltip("Tempo de cooldown entre diálogos de porta trancada (evita spam)")]
    public float lockedDialogueCooldown = 1f;
    [Header("Eventos")]
    [Tooltip("Evento executado no final do teleporte, após tudo ter sido processado")]
    public UnityEvent onTeleportComplete;
    private static Dictionary<WorldAreaId, TeleportDoor> doorRegistry = new Dictionary<WorldAreaId, TeleportDoor>();
    private SaveClientZone saveZone;
    private bool blocked;
    private BoxCollider2D doorCollider;
    private float lastLockedDialogueTime = 0f;
    private PlayerControl playerControl;
    private SODialogueSequence activeLockedDialogue;
    public bool IsBlocked => blocked;
    void Awake()
    {
        saveZone = FindFirstObjectByType<SaveClientZone>();
        doorCollider = GetComponent<BoxCollider2D>();
        MakeInvisible();
    }
    void Start()
    {
        UpdateLockStateFromFlag();
        UpdateDoorState();
        if (saveZone != null)
        {
            saveZone.OnLoadComplete += OnSaveLoaded;
            saveZone.OnFlagChanged += OnFlagChanged;
        }
    }
    void OnEnable()
    {
        if (identification != WorldAreaId.None)
        {
            if (doorRegistry.ContainsKey(identification))
            doorRegistry[identification] = this;
            else
            doorRegistry.Add(identification, this);
        }
        GameEvents.OnDialogueFinished += OnDialogueFinished;
    }
    void OnDisable()
    {
        if (identification != WorldAreaId.None && doorRegistry.ContainsKey(identification) && doorRegistry[identification] == this)
        {
            doorRegistry.Remove(identification);
        }
        GameEvents.OnDialogueFinished -= OnDialogueFinished;
        if (saveZone != null)
        {
            saveZone.OnLoadComplete -= OnSaveLoaded;
            saveZone.OnFlagChanged -= OnFlagChanged;
        }
    }
    void OnDestroy()
    {
        if (saveZone != null)
        {
            saveZone.OnLoadComplete -= OnSaveLoaded;
            saveZone.OnFlagChanged -= OnFlagChanged;
        }
    }
    void OnValidate()
    {
        if (doorCollider == null)
        doorCollider = GetComponent<BoxCollider2D>();
        UpdateDoorState();
    }
    public void TeleportPlayer(Transform player)
    {
        if (blocked || isLocked) return;
        WorldAreaId targetID = CalculateDestination();
        if (targetID == WorldAreaId.None)
        {
            return;
        }
        if (doorRegistry.TryGetValue(targetID, out TeleportDoor targetDoor))
        {
            Vector3 finalPos = targetDoor.transform.position;
            Vector3 offsetVector = targetDoor.GetDirectionVector();
            if ( landingDirection == LandingDirection.Up )
            {
                finalPos += (offsetVector * targetDoor.landingDistance);
                finalPos.y -= 0.5f;
            }
            else
            {
                finalPos += (offsetVector * targetDoor.landingDistance);
            }
            finalPos.z = targetDoor.transform.position.z;
            player.position = finalPos;
            this.BlockExternal(cooldown);
            targetDoor.BlockExternal(cooldown);
            GameEvents.OnPlayerTeleport?.Invoke(player.position, targetDoor.identification);
            onTeleportComplete?.Invoke();
        }
        else
        {
        }
    }
    public Vector3 GetDirectionVector()
    {
        switch (landingDirection)
        {
            case LandingDirection.Down:  return Vector3.down;
            case LandingDirection.Up:    return Vector3.up;
            case LandingDirection.Left:  return Vector3.left;
            case LandingDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
    private WorldAreaId CalculateDestination()
    {
        if (!multiDestination) return destination;
        if (saveZone != null)
        {
            foreach (var entry in conditionalDestinations)
            {
                if (entry.requiredFlag == null) continue;
                bool hasFlag = saveZone.HasFlag(entry.requiredFlag);
                bool met = entry.invertCondition ? !hasFlag : hasFlag;
                if (met) return entry.targetID;
            }
        }
        return destination;
    }
    public void BlockExternal(float duration)
    {
        blocked = true;
        CancelInvoke(nameof(Unblock));
        Invoke(nameof(Unblock), duration);
    }
    void Unblock() => blocked = false;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLocked) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < lastLockedDialogueTime + lockedDialogueCooldown) return;
        if (lockedDialogue != null)
        {
            lastLockedDialogueTime = Time.time;
            activeLockedDialogue = lockedDialogue;
            GameEvents.OnRequestDialogue?.Invoke(lockedDialogue);
        }
    }
    void OnDialogueFinished(SODialogueSequence sequence)
    {
        if (sequence == activeLockedDialogue)
        {
            activeLockedDialogue = null;
        }
    }
    void MakeInvisible()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
    }
    void UpdateLockStateFromFlag()
    {
        if (unlockFlag != null && saveZone != null)
        {
            bool hasFlag = saveZone.HasFlag(unlockFlag);
            isLocked = invertFlagLogic ? hasFlag : !hasFlag;
        }
    }
    void OnSaveLoaded()
    {
        UpdateLockStateFromFlag();
        UpdateDoorState();
    }
    void OnFlagChanged(SOZoneFlag flag)
    {
        if (unlockFlag == flag)
        {
            RefreshDoorState();
        }
    }
    public void RefreshDoorState()
    {
        UpdateLockStateFromFlag();
        UpdateDoorState();
    }
    public static void RefreshAllDoorsWithFlag(SOZoneFlag flag)
    {
        if (flag == null) return;
        var allDoors = FindObjectsByType<TeleportDoor>(FindObjectsSortMode.None);
        foreach (var door in allDoors)
        {
            if (door.unlockFlag == flag)
            {
                door.RefreshDoorState();
            }
        }
    }
    void UpdateDoorState()
    {
        if (doorCollider != null)
        {
            doorCollider.isTrigger = !isLocked;
        }
    }
    public void LockDoor()
    {
        isLocked = true;
        UpdateDoorState();
    }
    public void UnlockDoor()
    {
        isLocked = false;
        UpdateDoorState();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 dir = GetDirectionVector();
        Vector3 spawnPos = transform.position + (dir * landingDistance);
        Gizmos.DrawLine(transform.position, spawnPos);
        Gizmos.DrawWireSphere(spawnPos, 0.3f);
        if (landingDirection != LandingDirection.Center)
        {
            Vector3 arrowTip = spawnPos + (dir * 0.2f);
            Gizmos.DrawLine(spawnPos, arrowTip);
        }
    }
}

