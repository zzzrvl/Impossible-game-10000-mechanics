using UnityEngine;
using UnityEngine.InputSystem;

public class TrackingCursor : MonoBehaviour
{
    public float maxRadius = 2f;
    public GameObject cursorObject;
    public int circleSegments = 64;
    public Color circleColor = Color.white;
    public Color cursorColor = Color.red;
    public Shader xrayShader;

    private LineRenderer _lineRenderer;
    private Renderer _cursorRenderer;

    void Start()
    {
        Cursor.visible = false;

        // Находим шейдер в проекте по его имени
        // Это позволяет коду работать и в редакторе, и в билде
        //Shader xrayShader = Shader.Find("Custom/XRay");

        if (xrayShader == null)
        {
            Debug.LogError("Шейдер 'Custom/XRay' не найден! Проверьте имя в файле .shader");
            // Фолбэк на стандартный шейдер, чтобы не было розовых текстур
            xrayShader = Shader.Find("Sprites/Default");
        }

        // Настройка круга радиуса
        var circle = new GameObject("RadiusCircle");
        circle.transform.SetParent(transform);
        _lineRenderer = circle.AddComponent<LineRenderer>();
        _lineRenderer.loop = true;
        _lineRenderer.positionCount = circleSegments;
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.useWorldSpace = true;

        // Создаем материал на основе найденного шейдера
        var circleMat = new Material(xrayShader);
        circleMat.SetColor("_Color", circleColor); // Установка цвета через свойство шейдера
        _lineRenderer.material = circleMat;

        if (cursorObject != null)
        {
            _cursorRenderer = cursorObject.GetComponent<Renderer>();
            if (_cursorRenderer != null)
            {
                var cursorMat = new Material(xrayShader);
                cursorMat.SetColor("_Color", cursorColor);
                _cursorRenderer.material = cursorMat;
            }
        }
    }

    void Update()
    {
        DrawCircle();

        if (Mouse.current == null) return;

        var mousePos = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePos);
        var groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out var enter))
        {
            var hitPoint = ray.GetPoint(enter);
            var offset = hitPoint - transform.position;
            offset.y = 0f;

            if (offset.magnitude > maxRadius)
            {
                offset = offset.normalized * maxRadius;
            }

            var clampedPos = transform.position + offset;

            if (cursorObject != null)
            {
                cursorObject.transform.position = clampedPos;
            }

            if (offset != Vector3.zero)
            {
                transform.LookAt(transform.position + offset);
            }
        }
    }

    void DrawCircle()
    {
        var angleStep = 360f / circleSegments;
        for (var i = 0; i < circleSegments; i++)
        {
            var angle = i * angleStep * Mathf.Deg2Rad;
            var x = transform.position.x + Mathf.Cos(angle) * maxRadius;
            var z = transform.position.z + Mathf.Sin(angle) * maxRadius;
            var y = transform.position.y + 0.05f;
            _lineRenderer.SetPosition(i, new Vector3(x, y, z));
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}