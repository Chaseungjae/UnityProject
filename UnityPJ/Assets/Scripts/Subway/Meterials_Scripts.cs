using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Unity.Hierarchy;
using UnityEngine;

public class Meterials_Scripts : MonoBehaviour
{
    public Material targetMaterial;

    private Vector4 originalAlbedo; // (x, y, ?, ?) 값 저장

    public void SetTiling(float x, float y)
    {
        if (targetMaterial == null) return;

        // _Albedo Vector4 가져오기
        originalAlbedo = targetMaterial.GetVector("_Albedo");

        // x, y만 변경
        Vector4 newTiling = new Vector4(x, y, originalAlbedo.z, originalAlbedo.w);

        targetMaterial.SetVector("_Albedo", newTiling);

        Debug.Log($"[OK] _Albedo Tiling → X:{x}, Y:{y} 로 변경 완료");
    }

    public void ResetTiling()
    {
        if (targetMaterial == null) return;

        targetMaterial.SetVector("_Albedo", originalAlbedo);

        Debug.Log("[OK] _Albedo Tiling 원래 값으로 복원 완료");
    }

    public void Start()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("Material 없음");
            return;
        }

        Shader shader = targetMaterial.shader;
        int count = shader.GetPropertyCount();

        Debug.Log("=== Shader Property 목록 시작 ===");

        for (int i = 0; i < count; i++)
        {
            string name = shader.GetPropertyName(i);
            var type = shader.GetPropertyType(i);
            Debug.Log($"[{i}] 이름: {name}, 타입: {type}");
        }

        Debug.Log("=== Shader Property 목록 끝 ===");

        SetTiling(1.5f, 1.5f);
    }
}
