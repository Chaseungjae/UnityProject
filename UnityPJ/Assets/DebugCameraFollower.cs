using UnityEngine;

public class DebugCameraFollower : MonoBehaviour
{
    public Transform cameraRoot; // Inspector에 CameraRoot 지정
    void LateUpdate()
    {
        if (cameraRoot == null) return;
        // 위치와 회전 강제 동기화 (테스트 전용)
        transform.position = cameraRoot.position;
        transform.rotation = cameraRoot.rotation;
    }
}