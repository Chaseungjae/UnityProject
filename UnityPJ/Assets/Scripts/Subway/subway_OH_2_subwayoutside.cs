using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class subway_OH_2_subwayoutside : MonoBehaviour
{
    public Material newskybox;
    public float transparency = 0f;
    public Material targetMaterial;
    private Shader originalShader;
    private List<GameObject> tunnelLightedObjects = new List<GameObject>();
    private List<GameObject> wallTunnelObjects = new List<GameObject>();
    private List<GameObject> tunnelendObjects = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // 이름으로 오브젝트 찾기
        tunnelLightedObjects.AddRange(GameObject.FindGameObjectsWithTag("Tunnel_Lighted"));
        wallTunnelObjects.AddRange(GameObject.FindGameObjectsWithTag("Wall_Tunnel"));
        tunnelendObjects.AddRange(GameObject.FindGameObjectsWithTag("Tunnel_End"));
        // 비활성화
        StartCoroutine(DisableTunnelsAfterDelay(8f));
    }

    IEnumerator DisableTunnelsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var obj in tunnelLightedObjects)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in wallTunnelObjects)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in tunnelendObjects)
            if (obj != null) obj.SetActive(false);
    }
    void Start()
    {   
        originalShader = targetMaterial.shader;
        Shader standardShader = Shader.Find("Standard");
        targetMaterial.shader = standardShader;

        RenderSettings.skybox = newskybox;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        // 오브젝트 삭제될 때 Shader 되돌림
        RestoreOriginalShader();
        foreach (var obj in tunnelLightedObjects)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in wallTunnelObjects)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in tunnelendObjects)
            if (obj != null) obj.SetActive(true);

        RenderSettings.skybox = null;
    }
    void OnApplicationQuit()
    {
        RestoreOriginalShader();
        foreach (var obj in tunnelLightedObjects)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in wallTunnelObjects)
            if (obj != null) obj.SetActive(true);
        foreach (var obj in tunnelendObjects)

            if (obj != null) obj.SetActive(true);
        
        RenderSettings.skybox = null;
    }
    private void RestoreOriginalShader()
    {
        if (targetMaterial != null && originalShader != null)
        {
            targetMaterial.shader = originalShader;
            Debug.Log($"Shader restored to original: {targetMaterial.name}");
        }
    }

}
