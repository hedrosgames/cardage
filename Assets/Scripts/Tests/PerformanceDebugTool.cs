using UnityEngine;
using UnityEngine.Profiling;
using System.Text;

public class PerformanceDebugTool : MonoBehaviour
{
    [Header("Configurações de Exibição")]
    public bool showUI = true;
    public int fontSize = 18;
    public Color textColor = Color.yellow;

    [Header("Detecção de Gargalos")]
    public float bottleneckThresholdMs = 33.3f; // Aprox. 30 FPS fixo como limite

    private float _deltaTime = 0.0f;
    private StringBuilder _stringBuilder = new StringBuilder();
    private GUIStyle _style = new GUIStyle();

    void Update()
    {
        // Cálculo de DeltaTime para FPS
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        // Detecção de Gargalo (Spikes)
        float currentFrameMs = Time.unscaledDeltaTime * 1000f;
        if (currentFrameMs > bottleneckThresholdMs)
        {
            Debug.LogWarning($"[Performance] Gargalo detectado! Frame levou {currentFrameMs:F2}ms. " +
                             $"Memória Alocada: {GetMemoryMb(Profiler.GetTotalAllocatedMemoryLong())}MB");
        }
    }

    private void OnGUI()
    {
        if (!showUI) return;

        // Configuração do Estilo
        _style.alignment = TextAnchor.UpperLeft;
        _style.fontSize = fontSize;
        _style.normal.textColor = textColor;

        // Coleta de Métricas
        float fps = 1.0f / _deltaTime;
        float msec = _deltaTime * 1000.0f;
        
        long allocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
        long reservedMemory = Profiler.GetTotalReservedMemoryLong();
        long monoMemory = System.GC.GetTotalMemory(false);

        // Construção da String de Debug
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"<b>--- PERFORMANCE ---</b>");
        _stringBuilder.AppendLine($"FPS: {fps:F1} ({msec:F2} ms)");
        _stringBuilder.AppendLine($"Memória Alocada: {GetMemoryMb(allocatedMemory)} MB");
        _stringBuilder.AppendLine($"Memória Reservada: {GetMemoryMb(reservedMemory)} MB");
        _stringBuilder.AppendLine($"Memória GC (Mono): {GetMemoryMb(monoMemory)} MB");
        _stringBuilder.AppendLine($"--- SISTEMA ---");
        _stringBuilder.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
        _stringBuilder.AppendLine($"Resolução: {Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRateRatio}Hz");

        // Desenhar na tela
        Rect rect = new Rect(10, 10, 500, 300);
        GUI.Label(rect, _stringBuilder.ToString(), _style);
    }

    private string GetMemoryMb(long bytes)
    {
        return (bytes / 1024f / 1024f).ToString("F2");
    }
}
