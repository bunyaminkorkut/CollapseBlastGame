using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    public float targetWidth = 10f;  
    public float targetHeight = 5f; 

    void Start()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = targetWidth / targetHeight;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = targetHeight / 2 + 2;
        }
        else
        {
            float differenceInSize = targetHeight / 2 * (targetRatio / screenRatio);
            Camera.main.orthographicSize = differenceInSize  + 2;
        }
    }

}
