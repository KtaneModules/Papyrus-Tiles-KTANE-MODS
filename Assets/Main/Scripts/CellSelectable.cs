using UnityEngine;

[RequireComponent(typeof(KMSelectable), typeof(MeshRenderer))]
public class CellSelectable : MonoBehaviour
{
    private MeshRenderer _renderer;
    private Color _color;

    private bool _isHighlighted;

    public KMSelectable Selectable { get; set; }
    public Color Color {
        set {
            _color = value;
            if (!_isHighlighted)
                _renderer.material.color = _color;
        }
    }

    private void Awake() {
        Selectable = GetComponent<KMSelectable>();
        _renderer = GetComponent<MeshRenderer>();

        Selectable.OnHighlight += () => { _isHighlighted = true; _renderer.material.color = Color.white; };
        Selectable.OnHighlightEnded += () => { _isHighlighted = false; _renderer.material.color = _color; };
    }
}
