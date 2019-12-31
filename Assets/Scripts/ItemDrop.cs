using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public TextBubbleUI m_textBubble;
	string SLASH = Path.DirectorySeparatorChar.ToString();
    string m_itemName;
    int m_itemCount;

    public void MouseOver(Vector3 worldPosition) {
        m_textBubble.transform.position = worldPosition;
    }

    public void MouseEnter(Vector3 worldPosition) {
        m_textBubble.gameObject.SetActive(true);
        m_textBubble.transform.position = worldPosition;
    }

    public void MouseExit(Vector3 worldPosition) {
        m_textBubble.gameObject.SetActive(false);
    }

    public void UpdateItem(int spriteId, string itemName, int count, Color color) {
        Sprite sprite = Resources.Load<Sprite>("Sprites" + SLASH + spriteId);
        float flashAmount = color.a;
        Color flashColor = new Color(color.r, color.g, color.b, 1f);
        spriteRenderer.material.SetFloat("_FlashAmount", flashAmount);
        spriteRenderer.material.SetColor("_FlashColor", flashColor);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
        m_itemName = itemName;
        m_itemCount = count;
        m_textBubble.UpdateBubbleText(GetItemText());
        m_textBubble.gameObject.SetActive(false);
    }

    public string GetItemText() {
        return m_itemName + " (" + m_itemCount + ")";
    }
}
