using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraAspectEnforcer : MonoBehaviour
{
    public float targetAspect = 16f / 9f;
    Camera cam;
    Camera backgroundCam;
    void Awake()
    {
        cam = GetComponent<Camera>();
        CreateBackgroundCamera();
        UpdateCameraRect();
    }
    void Update()
    {
        UpdateCameraRect();
    }
    public void SetAspectRatio(float newAspect)
    {
        targetAspect = newAspect;
        UpdateCameraRect();
    }
    void CreateBackgroundCamera()
    {
        GameObject bgObj = new GameObject("BackgroundBlackBars");
        bgObj.transform.SetParent(transform);
        backgroundCam = bgObj.AddComponent<Camera>();
        backgroundCam.depth = cam.depth - 1;
        backgroundCam.clearFlags = CameraClearFlags.SolidColor;
        backgroundCam.backgroundColor = Color.black;
        backgroundCam.cullingMask = 0;
    }
    void UpdateCameraRect()
    {
        if (cam == null) return;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}

