using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SlotType { Spell, Item, Cursor }

public class SlotUI : MonoBehaviour
{
    public SpriteRenderer m_spriteRenderer;
    public WindowUI m_window, m_referenceWindow;
    public TextBubbleUI m_textBubble;
    public SlotType m_slotType;
    public bool m_viewOnlySlot;
    
    private int m_slotAmount = 1, m_slotGraphicId, m_slotId, m_soundId, m_slotIndex, m_referenceIndex;
    private string m_slotName, m_spellTarget;
    private Color m_slotColor;
    private bool m_isEnabled = true;

	private string SLASH = Path.DirectorySeparatorChar.ToString();

    void Start() {
        m_textBubble.gameObject.SetActive(false);
    }

    public void MouseOver(Vector3 worldPosition) {
        MouseEnter(worldPosition);
    }

    public void MouseEnter(Vector3 worldPosition) {
        if(!string.IsNullOrEmpty(GetSlotText())) {
            m_textBubble.gameObject.SetActive(true);
            m_textBubble.transform.position = worldPosition;
        }
        else {
            m_textBubble.gameObject.SetActive(false);
        }
    }

    public void MouseExit(Vector3 worldPosition) {
        m_textBubble.gameObject.SetActive(false);
    }

    public WindowUI GetWindow() {
        return m_window;
    }

    public bool ViewOnly() {
        return m_viewOnlySlot;
    }

    public void SetSlotType(SlotType type) {
        m_slotType = type;
    }

    public SlotType GetSlotType() {
        return m_slotType;
    }

    public void SetSlotId(int id) {
        m_slotId = id;
    }

    public int GetSlotId() {
        return m_slotId;
    }

    public void SetSoundId(int soundId) {
        m_soundId = soundId;
    }
    
    public int GetSoundId() {
        return m_soundId;
    }

    public void SetSlotIndex(int index) {
        m_slotIndex = index;
    }

    public void SetSpellTarget(string target) {
        m_spellTarget = target;
    }

    public string GetSpellTarget() {
        return m_spellTarget;
    }

    public void SetSlotAmount(int amount) {
        m_slotAmount = Mathf.Max(amount, 1);
        m_textBubble.UpdateBubbleText(GetSlotText());
    }

    public int GetSlotAmount(bool controlKeyPressed) {
        return (controlKeyPressed ? m_slotAmount : 1);
    }

    public void SetSlotName(string name) {
        m_slotName = name;
        m_textBubble.UpdateBubbleText(GetSlotText());
    }

    public string GetSlotName() {
        return m_slotName;
    }

    public int GetSlotIndex() {
        return m_slotIndex;
    }

    public void SetSlotEnabled(bool isEnabled) {
        m_isEnabled = isEnabled;
        UpdateSpriteColor();
    }

    public bool GetSlotEnabled() {
        return m_isEnabled;
    }

    public void UpdateSlotGraphic(int graphicId, Color color = default(Color)) {
		if(color == default(Color)) {
			color = Color.clear;
		}
        Sprite sprite = Resources.Load<Sprite>("Sprites" + SLASH + graphicId);
        m_spriteRenderer.sprite = sprite;
        m_slotGraphicId = graphicId;
        m_slotColor = color;
        UpdateSpriteColor();
    }

    void UpdateSpriteColor() {
        Color color;
        if(m_isEnabled) {
            color = m_slotColor;
        }
        else {
            color = new Color(225f / 255f, 225f / 255f, 0f, 0.5f);
        }
        float flashAmount = color.a;
        Color flashColor = new Color(color.r, color.g, color.b, 1f);
        m_spriteRenderer.material.SetFloat("_FlashAmount", flashAmount);
        m_spriteRenderer.material.SetColor("_FlashColor", flashColor);
        m_spriteRenderer.color = Color.white;
    }

    public int GetSlotGraphicId() {
        return m_slotGraphicId;
    }

    public Color GetSlotColor() {
        return m_slotColor;
    }
    
       public void SetReferenceWindow(WindowUI window) {
        m_referenceWindow = window;
    }

    public WindowUI GetReferenceWindow() {
        return m_referenceWindow;
    }

    public void SetReferenceIndex(int index) {
        m_referenceIndex = index;
    }

    public int GetReferenceIndex() {
        return m_referenceIndex;
    }

    public void CopySlot(SlotUI slot, bool copyAsReference = false) {
        int index = slot.GetSlotIndex();
        int referenceIndex = slot.GetReferenceIndex();
        int amount = slot.GetSlotAmount(true);
        int slotId = slot.GetSlotId();
        int soundId = slot.GetSoundId();
        int graphicId = slot.GetSlotGraphicId();
        Color color = slot.GetSlotColor();
        WindowUI window = slot.GetWindow();
        WindowUI referenceWindow = slot.GetReferenceWindow();
        SlotType slotType = slot.GetSlotType();
        string slotName = slot.GetSlotName();
        string spellTarget = slot.GetSpellTarget();
        bool enabled = slot.GetSlotEnabled();

        SetSlotAmount(amount);
        SetSlotId(slotId);
        SetSoundId(soundId);
        SetSlotType(slotType);
        UpdateSlotGraphic(graphicId, color);
        SetSlotName(slotName);
        SetSpellTarget(spellTarget);
        
        if(copyAsReference) {
            SetReferenceIndex(index);
            SetReferenceWindow(window);
        }
        else {
            SetReferenceIndex(referenceIndex);
            SetReferenceWindow(referenceWindow);
            SetSlotIndex(index);
            SetSlotEnabled(enabled);
        }
    }

    public void ClearSlot() {
        m_spriteRenderer.sprite = null;
        m_slotColor = Color.clear;
        m_isEnabled = true;
        m_slotName = "";
        m_spellTarget = "";
        m_slotAmount = 1;
        m_slotGraphicId = 0;
        m_slotId = 0;
        m_soundId = 0;
        m_referenceIndex = 0;
        m_referenceWindow = default(WindowUI);
    }

    public string GetSlotText() {
        return m_slotAmount > 1 ? m_slotName + " (" + m_slotAmount + ")" : m_slotName;
    }
}
