using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackingCursor : MonoBehaviour
{
    public float maxRadius = 2f;
    public GameObject cursorObject;
    public int circleSegments = 64;
    public Color circleColor = Color.white;
    public Color cursorColor = Color.red;

    private LineRenderer _lineRenderer;
    private Renderer _cursorRenderer;

    // Шейдер который рисует поверх всего
    public const string XRayShader = @"
        Shader ""Custom/XRay""
        {
            Properties { _Color (""Color"", Color) = (1,1,1,1) }
            SubShader
            {
                Tags { ""RenderType""=""Transparent"" ""Queue""=""Overlay+100"" }
                Pass
                {
                    ZTest Always
                    ZWrite Off
                    Blend SrcAlpha OneMinusSrcAlpha
                    Color [_Color]
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include ""UnityCG.cginc""
                    fixed4 _Color;
                    struct appdata { float4 vertex : POSITION; };
                    struct v2f { float4 pos : SV_POSITION; };
                    v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
                    fixed4 frag(v2f i) : SV_Target { return _Color; }
                    ENDCG
                }
            }
        }";

    void Start()
    {
        Cursor.visible = false;

        Shader shader = ShaderUtil.CreateShaderAsset(XRayShader);
        
        Material circleMat = new Material(shader);
        circleMat.color = circleColor;

        GameObject circle = new GameObject("RadiusCircle");
        circle.transform.SetParent(transform);
        _lineRenderer = circle.AddComponent<LineRenderer>();
        _lineRenderer.loop = true;
        _lineRenderer.positionCount = circleSegments;
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.material = circleMat;
        _lineRenderer.startColor = circleColor;
        _lineRenderer.endColor = circleColor;
        
        if (cursorObject != null)
        {
            _cursorRenderer = cursorObject.GetComponent<Renderer>();
            if (_cursorRenderer != null)
            {
                Material cursorMat = new Material(shader);
                cursorMat.color = cursorColor;
                _cursorRenderer.material = cursorMat;
            }
        }
    }

    void Update()
    {
        DrawCircle();

        var mousePos = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 offset = hitPoint - transform.position;
            offset.y = 0f;

            if (offset.magnitude > maxRadius)
                offset = offset.normalized * maxRadius;

            Vector3 clampedPos = transform.position + offset;

            if (cursorObject != null)
                cursorObject.transform.position = clampedPos;

            if (offset != Vector3.zero)
                transform.LookAt(transform.position + offset);
        }
    }

    void DrawCircle()
    {
        float angleStep = 360f / circleSegments;
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = transform.position.x + Mathf.Cos(angle) * maxRadius;
            float z = transform.position.z + Mathf.Sin(angle) * maxRadius;
            float y = transform.position.y + 0.05f;
            _lineRenderer.SetPosition(i, new Vector3(x, y, z));
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}