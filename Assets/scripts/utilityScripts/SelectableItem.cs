using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum SelectableItemDataType { GRAPHIC, GAMEOBJECT, CANVASGROUP, SPRITE}

[System.Serializable]
public class SelectableItemData
{
    [HideInInspector] public string Name;
    [SerializeField] private SelectableItemDataType _type;

    [ConditionalField(nameof(_showGraphicSettings)), SerializeField] private Graphic _target;
    [ConditionalField(nameof(_showGraphicSettings)), SerializeField] private bool _useMaterials;

    [ConditionalField(nameof(_showGraphicColorSettings)), SerializeField] private Color _normalColor = Color.white;
    [ConditionalField(nameof(_showGraphicColorSettings)), SerializeField] private Color _hoveredColor = Color.white;
    [ConditionalField(nameof(_showGraphicColorSettings)), SerializeField] private Color _selectedColor = Color.white;
    [ConditionalField(nameof(_showGraphicColorSettings)), SerializeField] private Color _disabledColor = Color.white;

    [ConditionalField(nameof(_useMaterials)), SerializeField] private Material _normalMat;
    [ConditionalField(nameof(_useMaterials)), SerializeField] private Material _hoveredMat;
    [ConditionalField(nameof(_useMaterials)), SerializeField] private Material _selectedMat;
    [ConditionalField(nameof(_useMaterials)), SerializeField] private Material _disabledMat;

    [ConditionalField(nameof(_showGOSettings)), SerializeField] private GameObject _obj;
    [ConditionalField(nameof(_showGOSettings)), SerializeField] private bool _normalState = false;
    [ConditionalField(nameof(_showGOSettings)), SerializeField] private bool _hoveredState = false;
    [ConditionalField(nameof(_showGOSettings)), SerializeField] private bool _selectedState = true;
    [ConditionalField(nameof(_showGOSettings)), SerializeField] private bool _disabledState = false;

    [ConditionalField(nameof(_showGroupSettings)), SerializeField] private CanvasGroup _group;
    [ConditionalField(nameof(_showGroupSettings)), SerializeField, Range(0, 1)] private float _normalAlpha = 0.3f;
    [ConditionalField(nameof(_showGroupSettings)), SerializeField, Range(0, 1)] private float _hoveredAlpha = 0.6f;
    [ConditionalField(nameof(_showGroupSettings)), SerializeField, Range(0, 1)] private float _selectedAlpha = 1;
    [ConditionalField(nameof(_showGroupSettings)), SerializeField, Range(0, 1)] private float _disabledAlpha = 0.2f;

    [ConditionalField(nameof(_showSpriteSettings)), SerializeField] private Image _spriteTarget;
    [ConditionalField(nameof(_showSpriteSettings)), SerializeField] private Sprite _normalSprite;
    [ConditionalField(nameof(_showSpriteSettings)), SerializeField] private Sprite _hoveredSprite;
    [ConditionalField(nameof(_showSpriteSettings)), SerializeField] private Sprite _selectedSprite;
    [ConditionalField(nameof(_showSpriteSettings)), SerializeField] private Sprite _disabledSprite;

    [SerializeField, HideInInspector] private bool _showGraphicSettings;
    [SerializeField, HideInInspector] private bool _showGraphicColorSettings;
    [SerializeField, HideInInspector] private bool _showGOSettings;
    [SerializeField, HideInInspector] private bool _showGroupSettings;
    [SerializeField, HideInInspector] private bool _showSpriteSettings;

    private bool _isGraphic => _type == SelectableItemDataType.GRAPHIC;
    private bool _isGameObject => _type == SelectableItemDataType.GAMEOBJECT;
    private bool _isGroup => _type == SelectableItemDataType.CANVASGROUP;
    private bool _isSprite => _type == SelectableItemDataType.SPRITE;
    private bool _usingMaterials => _isGraphic && _useMaterials;

    public void OnValidate()
    {
        if ((_isGraphic && !_target) || (_isGameObject && !_obj) || (_isGroup && !_group) || (_isSprite && !_spriteTarget)) Initialize();
        else SetName();

        if (!_isGraphic) _useMaterials = false;
        _showGraphicSettings = _isGraphic;
        _showGOSettings = _isGameObject;
        _showGroupSettings = _isGroup;
        _showSpriteSettings = _isSprite;
        _showGraphicColorSettings = _showGraphicSettings && !_useMaterials;
    }

    private void SetName()
    {
        if (_isGraphic) Name = _target.gameObject.name;
        if (_isGameObject) Name = _obj.name;
        if (_isGroup) Name = _group.gameObject.name;
        if (_isSprite) Name = _spriteTarget.gameObject.name;
    }

    private void Initialize()
    {
        _normalColor = Color.white;
        _hoveredColor = Color.white;
        _selectedColor = Color.white;
        _disabledColor = Color.white;

        _normalState = false;
        _hoveredState = false;
        _selectedState = true;
        _disabledState = false;

        _normalAlpha = 0.3f;
        _hoveredAlpha = 0.6f;
        _selectedAlpha = 1;
        _disabledAlpha = 0.2f;

        _normalSprite = null;
        _hoveredSprite = null;
        _selectedSprite = null;
        _disabledSprite = null;
    }

    public void Update(bool selected, bool hovered, bool disabled)
    {
        if (!IsValid()) return;
        if (disabled) Disable();
        else if (selected) Select();
        else if (hovered) Hover();
        else Deselect();
    }

    private bool IsValid()
    {
        if (_isGraphic) return _target;
        if (_isGroup) return _group;
        if (_isGameObject) return _obj;
        if (_isSprite) return _spriteTarget;
        return false;
    }

    private void Select()
    {
        if (_isGraphic && !_usingMaterials) _target.color = _selectedColor;
        else if (_usingMaterials) _target.material = _selectedMat;

        if (_isGroup) _group.alpha = _selectedAlpha;
        if (_isGameObject) _obj.SetActive(_selectedState);
        if (_isSprite) _spriteTarget.sprite = _selectedSprite;
    }

    private void Hover()
    {
        if (_isGraphic && !_usingMaterials) _target.color = _hoveredColor;
        else if (_usingMaterials) _target.material = _hoveredMat;

        if (_isGroup) _group.alpha = _hoveredAlpha;
        if (_isGameObject) _obj.SetActive(_hoveredState);
        if (_isSprite) _spriteTarget.sprite = _hoveredSprite;
    }

    private void Deselect()
    {
        if (_isGraphic && !_usingMaterials) _target.color = _normalColor;
        else if (_usingMaterials) _target.material = _normalMat;

        if (_isGroup) _group.alpha = _normalAlpha;
        if (_isGameObject) _obj.SetActive(_normalState);
        if (_isSprite) _spriteTarget.sprite = _normalSprite;
    }

    private void Disable()
    {
        if (_isGraphic && !_usingMaterials) _target.color = _disabledColor;
        else if (_usingMaterials) _target.material = _disabledMat;

        if (_isGroup) _group.alpha = _disabledAlpha;
        if (_isGameObject) _obj.SetActive(_disabledState);
        if (_isSprite) _spriteTarget.sprite = _disabledSprite;
    }

}

public class SelectableItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Behavior")]
    [SerializeField] private bool _selectOnClick = true;
    [SerializeField, ConditionalField(nameof(_selectOnClick))] private bool _toggleOnClick;
    [SerializeField, ConditionalField(nameof(_toggleOnClick))] private bool _deselectOnClick = false;
    [SerializeField] private bool _dontHoverWhenSelected = false;
    [SerializeField] private bool _selectOnHover;
    [SerializeField, ConditionalField(nameof(_selectOnHover))] private bool _deselectOnExit = true;
    [SerializeField] private bool _hasHoverCooldown;
    [SerializeField, ConditionalField(nameof(_hasHoverCooldown))] private float _hoverCooldown = 0.05f;
    [SerializeField] private bool _deselectOnStart = true;
    [SerializeField] private bool _autoDehover = true;

    [Header("data")]
    [SerializeField] private List<SelectableItemData> _data = new List<SelectableItemData>();
    [SerializeField] private bool _hasAnimation;
    [SerializeField, ConditionalField(nameof(_hasAnimation))] private Animator _animator;
    [SerializeField, ConditionalField(nameof(_hasAnimation))] private string _animationSelectedBool = "Selected";
    [SerializeField, ConditionalField(nameof(_hasAnimation))] private string _animationHoveredBool = "Hovered";

    [Header("Sounds")]
    [SerializeField] private Sound _hoverSound;
    [SerializeField] private Sound _selectSound;
    [SerializeField] private Sound _deselectSound;

    [Header("Events")]
    public UnityEvent OnSelect;
    public UnityEvent OnHover;
    public UnityEvent OnEndHover;
    public UnityEvent OnDeselect;

    [Header("Debug")]
    [SerializeField] private bool _printSelections;
    [SerializeField, ReadOnly] private bool _disabled;

    public bool Selected { get; private set; }
    public bool Hovered { get; private set; }
    public bool Disabled { get; private set; }

    private float _lastHoverTime = 0;
    private bool _hasBeenSelected;

    private void OnValidate()
    {
        foreach (var d in _data) d.OnValidate();
        foreach (var d in _data) d.Update(false, false, false);
    }

    private void Start()
    {
        if (_hoverSound) _hoverSound = Instantiate(_hoverSound);
        if (_selectSound) _selectSound = Instantiate(_selectSound);
        if (_deselectSound) _deselectSound = Instantiate(_deselectSound);

        //Disabled = false;
        if (_deselectOnStart && !_hasBeenSelected) Deselect();
    }

    private void Update()
    {
        _disabled = Disabled;
    }

    [ButtonMethod]
    private void printTest()
    {
        print("buttonStatus: seleted: " + Selected + ", hovered: " + Hovered + ", disabled: " + Disabled);
    }

    [ButtonMethod]
    private void SetNormal()
    {
        foreach (var d in _data) d.Update(false, false, false);
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
    }

    public void ToggleSelected()
    {
        if (Disabled) return;
        Selected = !Selected;
        UpdateVisuals();
    }

    public void Select()
    {
        _hasBeenSelected = true;
        if (_printSelections) print(gameObject.name + " selected");
        if (_selectSound && _selectSound.Instantialized) _selectSound.Play(); 
        SetState(true);
        OnSelect.Invoke();
    }
    public void Deselect()
    {
        if (_printSelections) print(gameObject.name + " deselected");
        OnDeselect.Invoke();
        if (_deselectSound) _deselectSound.Play();
        SetState(false);
    }

    public void SetState(bool selected)
    {
        if (Disabled) return;
        Selected = selected;
        if (_autoDehover) Hovered = false;
        UpdateVisuals();
    }

    public void SetEnabled(bool enabled)
    {
        //print(gameObject.name + " setting enab    xled: " + enabled);
        Disabled = !enabled;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        foreach (var d in _data) d.Update(Selected, Hovered, Disabled);
        if (_animator) {
            _animator.SetBool(_animationSelectedBool, Selected);
            _animator.SetBool(_animationHoveredBool, Hovered);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Disabled) return;
        if (Selected && (!_toggleOnClick || !_deselectOnClick)) return;
        if (Selected && _dontHoverWhenSelected) return;
        OnHover.Invoke();

        if (_hasHoverCooldown) {
            var timeSinceLastHover = Time.time - _lastHoverTime;
            if (timeSinceLastHover < _hoverCooldown) return;
            _lastHoverTime = Time.time;
        }

        Hovered = true;
        if (_hoverSound) _hoverSound.Play();
        if (_selectOnHover) Select();
        else UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Disabled) return;
        if (Hovered) OnEndHover.Invoke();

        Hovered = false;
        if (_selectOnHover && _deselectOnExit) Deselect();
        else UpdateVisuals();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Input.GetMouseButton(0)) return;

        if (Disabled) return;
        if (Selected && _toggleOnClick && _deselectOnClick) {
            Deselect();
            return;
        }

        if (_selectOnClick && !Selected) {
            Select();
            if (!_toggleOnClick) Deselect();
        }
    }
}