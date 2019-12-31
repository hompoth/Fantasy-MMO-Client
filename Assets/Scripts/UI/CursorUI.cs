using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorType { Fairy, Baton, Crosshairs, Sword, Fist, Grab };

public class CursorUI : MonoBehaviour
{	
    public SpriteRenderer m_spriteRenderer;
    public Texture2D m_cursors;
	public CursorType m_cursorType;
    public SlotUI m_heldItem;
    private SlotUI m_selectedSlot;
    private WindowUI m_selectedWindow;
    private Vector3 m_selectedWindowOffset;
    private List<GameObject> m_mousedOverObjects;

	const int CURSOR_WIDTH = 24, CURSOR_HEIGHT = 24, UNIT_SIZE = 32;
    
    void Awake() {
        UpdateCursor(m_cursorType);
        m_mousedOverObjects = new List<GameObject>();
    }

    void Update() {
        UpdateMouseWindowPosition();
        MouseMoveEvent();
    }

    void UpdateMouseWindowPosition() {
        // TODO Test larger unit values to see if this fixes the shaking mouse cursor
        if(TryGetMouseWorldPosition(out Vector3 worldPosition, true)) {
            transform.localPosition = worldPosition;
            
            if(m_selectedWindow != null) {
                Vector3 windowPosition = windowPosition = worldPosition + m_selectedWindowOffset;
                GameObject windowObject = m_selectedWindow.gameObject;
                windowObject.transform.localPosition = windowPosition;
            }
        }
    }

    public void LeftMouseUp(bool controlKeyPressed) {
        SetCursor(m_cursorType);
        if(m_selectedSlot != null) {
            if(TryGetSelectSlot(out WindowUI window, out SlotUI slot)) {
                SwapSelectedSlot(slot, controlKeyPressed);
            }
            else {
                DropSelectedSlot(window, controlKeyPressed);
            }
            m_selectedSlot = null;
        }
        m_selectedWindow = null;
        m_selectedWindowOffset = Vector3.zero;
        ClearCursorSlot();
    }

    public void LeftMouseDown() {
        SetCursor(CursorType.Grab);
        if(TryGetSelectSlot(out WindowUI window, out SlotUI slot)) {
            WindowType windowType = window.GetWindowType();
            if(windowType.Equals(WindowType.BuffBar)) {
                int index = slot.GetSlotIndex();
                GameManager.instance?.SendMessageToServer(Packet.KillBuff(index));
            }
            else {
                int graphicId = slot.GetSlotGraphicId();
                if(graphicId > 0) {
                    Color slotColor = slot.GetSlotColor();
                    m_selectedSlot = slot;
                    UpdateItemGraphic(graphicId, slotColor);
                }
            }
        }
        else if(window != null) {
            if(TryGetSelectedButton(window, out ButtonUI button)) {
                button.UseButton();
            }
            else {
                if(TryGetMouseWorldPosition(out Vector3 worldPosition, true)) {
                    Vector3 windowPosition = window.gameObject.transform.localPosition;
                    m_selectedWindow = window;
                    m_selectedWindowOffset = windowPosition - worldPosition;
                }
            }
        }
        else if(TryGetMouseServerPosition(out int x, out int y)) {
            GameManager.instance?.SendMessageToServer(Packet.LeftClick(x, y));
        }
        if(window != null) {
            PlayerState.MoveWindowToFront(window.gameObject);
        }
    }

    public void LeftDoubleClick() {
        if(TryGetSelectSlot(out WindowUI window, out SlotUI slot)) {
            int index = slot.GetSlotIndex();
            int npcId = slot.GetWindow().GetNPCId();
            WindowType windowType = window.GetWindowType();
            if(windowType.Equals(WindowType.InventoryWindow)) {
                GameManager.instance?.SendMessageToServer(Packet.Use(index));
            }
            else if(windowType.Equals(WindowType.VendorWindow)) {
                GameManager.instance?.SendMessageToServer(Packet.VendorPurchaseToInventory(npcId, index));
            }
            else if(windowType.Equals(WindowType.CommandBar)) {
                // TODO - Use a spell
            }
            else if(windowType.Equals(WindowType.SpellsWindow)) {
                // TODO - Use a spell
            }
        }
    }

    public void RightMouseUp() {
        SetCursor(m_cursorType);
        ClearCursorSlot();
    }

    public void RightMouseDown(bool controlKeyPressed) {
        SetCursor(CursorType.Grab);
        if(TryGetSelectSlot(out WindowUI window, out SlotUI slot)) {
            int graphicId = slot.GetSlotGraphicId();
            if(graphicId > 0) {
                int index = slot.GetSlotIndex();
                int slotId = slot.GetSlotId();
                bool viewOnlySlot = slot.ViewOnly();
                WindowType windowType = window.GetWindowType();
                if(windowType.Equals(WindowType.InventoryWindow) || windowType.Equals(WindowType.CharacterWindow) 
                    || IsCombineWindow(windowType) || windowType.Equals(WindowType.VendorWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.ItemInfo(slotId));
                }
                else if(windowType.Equals(WindowType.SpellsWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.SpellInfo(slotId));
                }
                else if(windowType.Equals(WindowType.TradeWindow)) {
                    if(viewOnlySlot) {
                        GameManager.instance?.SendMessageToServer(Packet.ItemInfo(slotId));
                    }
                    else {
                        GameManager.instance?.SendMessageToServer(Packet.DeleteTradeItem(index));
                    }
                }
                else if(windowType.Equals(WindowType.CommandBar)) {
                    PlayerState.ClearCommandSlot(index);
                }
            }
        }
        else if(window == null) {
            if(TryGetMouseServerPosition(out int x, out int y)) {
                if(controlKeyPressed) {
                    GameManager.instance?.SendMessageToServer(Packet.TradeWith(x, y));
                }
                else {
                    GameManager.instance?.SendMessageToServer(Packet.RightClick(x, y));
                }
            }
        }
    }

    bool TryGetMouseWorldPosition(out Vector3 worldPosition, bool useLocalPosition = false) {
        worldPosition = default(Vector3);
        Camera mainCamera = Camera.main;
        if(mainCamera != null) {
            worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition); 
            if(useLocalPosition) {
                worldPosition = worldPosition - mainCamera.transform.position;
            }
            worldPosition = SnapToGrid(worldPosition);
            worldPosition.z = 0;
            return true;
        }
        return false;
    }

    bool TryGetMouseServerPosition(out int x, out int y) {
        x = 0; 
        y = 0;
        if(TryGetMouseWorldPosition(out Vector3 worldPosition)) {
            GameManager.ServerPosition(worldPosition, out x, out y);
            return true;
        }
        return false;
    }

    void DropSelectedSlot(WindowUI window, bool controlKeyPressed) {
        if(m_selectedSlot != null) {
            int index = m_selectedSlot.GetSlotIndex();
            int amount = m_selectedSlot.GetSlotAmount(controlKeyPressed);
            int windowId = m_selectedSlot.GetWindow().GetWindowId();
            WindowType windowType = m_selectedSlot.GetWindow().GetWindowType();
            if(window == null) {
                if(windowType.Equals(WindowType.InventoryWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.Drop(index, amount));
                }
                else if(windowType.Equals(WindowType.CharacterWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.Use(index));
                }
                else if(IsCombineWindow(windowType)) {
                    GameManager.instance?.SendMessageToServer(Packet.DropFromWindow(windowId, index, amount));
                }
            }
            else {
                WindowType newWindowType = window.GetWindowType();
                if(newWindowType.Equals(WindowType.CharacterWindow)) {
                    if(windowType.Equals(WindowType.InventoryWindow)) {
                        GameManager.instance?.SendMessageToServer(Packet.Use(index));
                    }
                    else if(windowType.Equals(WindowType.CharacterWindow)) {
                        GameManager.instance?.SendMessageToServer(Packet.Use(index));
                    }
                }
                else if(newWindowType.Equals(WindowType.DiscardButton)) {
                    if(windowType.Equals(WindowType.InventoryWindow) || windowType.Equals(WindowType.CharacterWindow)) {
                        GameManager.instance?.SendMessageToServer(Packet.DiscardItem(index));
                    }
                    else if(windowType.Equals(WindowType.SpellsWindow)) {
                        GameManager.instance?.SendMessageToServer(Packet.DiscardSpell(index));
                    }
                }
            }
        }
    }

    bool IsCombineWindow(WindowType windowType) {
        return windowType.Equals(WindowType.Combine2) || windowType.Equals(WindowType.Combine4)
            || windowType.Equals(WindowType.Combine6) || windowType.Equals(WindowType.Combine8)
            || windowType.Equals(WindowType.Combine10);
    }

    void SwapSelectedSlot(SlotUI newSlot, bool controlKeyPressed) {        
        if(m_selectedSlot != null && newSlot != null) {
            int index = m_selectedSlot.GetSlotIndex();
            int newSlotIndex = newSlot.GetSlotIndex();
            int amount = m_selectedSlot.GetSlotAmount(controlKeyPressed);
            int windowId = m_selectedSlot.GetWindow().GetWindowId();
            int newWindowId = newSlot.GetWindow().GetWindowId();
            int npcId = m_selectedSlot.GetWindow().GetNPCId();
            int newNpcId = newSlot.GetWindow().GetNPCId();
            bool viewOnlySlot = m_selectedSlot.ViewOnly() || newSlot.ViewOnly();
            WindowType windowType = m_selectedSlot.GetWindow().GetWindowType();
            WindowType newWindowType = newSlot.GetWindow().GetWindowType();
            if(windowType.Equals(WindowType.InventoryWindow)) {
                if(windowType.Equals(newWindowType) && !m_selectedSlot.Equals(newSlot)) {
                    if(controlKeyPressed) {
                        GameManager.instance?.SendMessageToServer(Packet.Split(index, newSlotIndex));
                    }
                    else {
                        GameManager.instance?.SendMessageToServer(Packet.Change(index, newSlotIndex));
                    }
                }
                else if(newWindowType.Equals(WindowType.CharacterWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.Use(index));
                }
                else if(IsCombineWindow(newWindowType)) {
                    GameManager.instance?.SendMessageToServer(Packet.InventoryToWindow(index, newWindowId, newSlotIndex));
                }
                else if(newWindowType.Equals(WindowType.VendorWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.InventorySellToVendor(newNpcId, index, amount));
                }
                else if(newWindowType.Equals(WindowType.TradeWindow) && !viewOnlySlot) {
                    GameManager.instance?.SendMessageToServer(Packet.AddTradeItem(newSlotIndex, index));
                }
                else if(newWindowType.Equals(WindowType.CommandBar)) {
                    PlayerState.AddCommandSlot(newSlotIndex, m_selectedSlot);
                }
            }
            else if(windowType.Equals(WindowType.CharacterWindow)) {
                if(windowType.Equals(newWindowType)) {
                    GameManager.instance?.SendMessageToServer(Packet.Use(index));
                }
                else if(newWindowType.Equals(WindowType.InventoryWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.Use(index));
                }
                else if(newWindowType.Equals(WindowType.VendorWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.InventorySellToVendor(newNpcId, index, amount));
                }
                else if(newWindowType.Equals(WindowType.CommandBar)) {
                    PlayerState.AddCommandSlot(newSlotIndex, m_selectedSlot);
                }
            }
            else if(windowType.Equals(WindowType.SpellsWindow)) {
                if(windowType.Equals(newWindowType) && !m_selectedSlot.Equals(newSlot)) {
                    GameManager.instance?.SendMessageToServer(Packet.Swap(index, newSlotIndex));
                }
                else if(newWindowType.Equals(WindowType.CommandBar)) {
                    PlayerState.AddCommandSlot(newSlotIndex, m_selectedSlot);
                }
            }
            else if(IsCombineWindow(windowType)) {
                if(IsCombineWindow(newWindowType)) {
                    GameManager.instance?.SendMessageToServer(Packet.WindowToWindow(windowId, index, newWindowId, newSlotIndex));
                }
                else if(newWindowType.Equals(WindowType.InventoryWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.WindowToInventory(windowId, index, newSlotIndex));
                }
                else if(newWindowType.Equals(WindowType.VendorWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.WindowSellToVendor(newNpcId, windowId, index, amount));
                }
            }
            else if(windowType.Equals(WindowType.VendorWindow)) {
                if(newWindowType.Equals(WindowType.InventoryWindow)) {
                    GameManager.instance?.SendMessageToServer(Packet.VendorPurchaseToInventory(npcId, index));
                }
            }
        }
    }

    void UpdateItemGraphic(int itemId, Color itemColor) {
        m_heldItem.UpdateSlotGraphic(itemId, itemColor);
    }

    void ClearCursorSlot() {
        m_heldItem.ClearSlot();
    }

    bool SlotSelected(WindowUI window, SlotUI slot) {
        if(window != null && slot != null) {
            int slotWindowId = slot.GetWindow().GetWindowId();
            int expectedWindowId = window.GetWindowId();
            if(slotWindowId.Equals(expectedWindowId)) {
                return true;
            }
        }
        return false;
    }

    bool ButtonSelected(WindowUI window, ButtonUI button) {
        if(window != null && button != null) {
            int buttonWindowId = button.GetWindow().GetWindowId();
            int expectedWindowId = window.GetWindowId();
            if(buttonWindowId.Equals(expectedWindowId)) {
                return true;
            }
        }
        return false;
    }

    bool TryGetSelectSlot(out WindowUI window, out SlotUI slot) {
        window = default(WindowUI);
        slot = default(SlotUI);
        if(TryGetMouseWorldPosition(out Vector3 worldPosition)) {
            window = GetWindowAtPoint(worldPosition);
            slot = GetSlotAtPoint(worldPosition);
            return SlotSelected(window, slot);
        }
        return false;
    }

    bool TryGetSelectedButton(WindowUI window, out ButtonUI button) {
        button = GetButtonAtPoint();
        return ButtonSelected(window, button);
    }

    SlotUI GetSlotAtPoint(Vector3 worldPosition = default(Vector3)) {
        return GetUIElementAtPoint<SlotUI>(1 << 13, worldPosition); 
    }

    WindowUI GetWindowAtPoint(Vector3 worldPosition = default(Vector3)) {
        return GetUIElementAtPoint<WindowUI>(1 << 5, worldPosition); 
    }

    ButtonUI GetButtonAtPoint(Vector3 worldPosition = default(Vector3)) {
        return GetUIElementAtPoint<ButtonUI>(1 << 14, worldPosition); 
    }

    T GetUIElementAtPoint<T>(int layerMask, Vector3 worldPosition) {
        T element = default(T);
        int maxSiblingIndex = -1;
        if(worldPosition != default(Vector3) || TryGetMouseWorldPosition(out worldPosition)) {
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(worldPosition, Vector3.forward, Mathf.Infinity, layerMask);
            foreach(RaycastHit2D hit in raycastHits) {
                GameObject gameObject = hit.collider?.gameObject;
                int currentSiblingIndex;
                if(typeof(T) == typeof(SlotUI) || typeof(T) == typeof(ButtonUI)) {
                    currentSiblingIndex = gameObject.transform.parent.GetSiblingIndex();
                }
                else {
                    currentSiblingIndex = gameObject.transform.GetSiblingIndex();
                }
                if(gameObject != null && currentSiblingIndex > maxSiblingIndex) {
                    T currentElement = gameObject.GetComponent<T>();
                    if(currentElement != null) {
                        maxSiblingIndex = currentSiblingIndex;
                        element = currentElement;
                    }
                }
            }
        }
        return element;
    }

    void MouseMoveEvent() {
        if(TryGetMouseWorldPosition(out Vector3 worldPosition)) {
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(worldPosition, Vector3.forward, Mathf.Infinity, 1 | (1 << 5) | (1 << 8) | (1 << 11) | (1 << 13));
            List<GameObject> currentMousedOverObjects = new List<GameObject>();
            foreach(RaycastHit2D hit in raycastHits) {
                GameObject gameObject = hit.collider?.gameObject;
                if(m_mousedOverObjects.Contains(gameObject)) {
                    MouseOver(gameObject, worldPosition);
                }
                else {
                    MouseEnter(gameObject, worldPosition);
                    m_mousedOverObjects.Add(gameObject);
                }
                currentMousedOverObjects.Add(gameObject);
            }
            for (int i = m_mousedOverObjects.Count - 1; i >= 0; i--) {
                GameObject gameObject = m_mousedOverObjects[i];
                if(!currentMousedOverObjects.Contains(gameObject)) {
                    MouseExit(gameObject, worldPosition);
                    m_mousedOverObjects.RemoveAt(i);
                }
            }
        }
    }

    void MouseOver(GameObject gameObject, Vector3 worldPosition) {
        PlayerManager player;
        WindowUI window;
        SlotUI slot;
        ItemDrop item;
        if((player = gameObject.GetComponent<PlayerManager>()) != null) {
            player.MouseOver(worldPosition);
        }
        else if((window = gameObject.GetComponent<WindowUI>()) != null) {
            window.MouseOver(worldPosition);
        }
        else if((slot = gameObject.GetComponent<SlotUI>()) != null) {
            slot.MouseOver(worldPosition);
        }
        else if((item = gameObject.GetComponent<ItemDrop>()) != null) {
            item.MouseOver(worldPosition);
        }
    }

    void MouseEnter(GameObject gameObject, Vector3 worldPosition) {
        PlayerManager player;
        WindowUI window;
        SlotUI slot;
        ItemDrop item;
        if((player = gameObject.GetComponent<PlayerManager>()) != null) {
            player.MouseEnter(worldPosition);
        }
        else if((window = gameObject.GetComponent<WindowUI>()) != null) {
            window.MouseEnter(worldPosition);
        }
        else if((slot = gameObject.GetComponent<SlotUI>()) != null) {
            slot.MouseEnter(worldPosition);
        }
        else if((item = gameObject.GetComponent<ItemDrop>()) != null) {
            item.MouseEnter(worldPosition);
        }
    }

    void MouseExit(GameObject gameObject, Vector3 worldPosition) {
        PlayerManager player;
        WindowUI window;
        SlotUI slot;
        ItemDrop item;
        if((player = gameObject.GetComponent<PlayerManager>()) != null) {
            player.MouseExit(worldPosition);
        }
        else if((window = gameObject.GetComponent<WindowUI>()) != null) {
            window.MouseExit(worldPosition);
        }
        else if((slot = gameObject.GetComponent<SlotUI>()) != null) {
            slot.MouseExit(worldPosition);
        }
        else if((item = gameObject.GetComponent<ItemDrop>()) != null) {
            item.MouseExit(worldPosition);
        }
    }

    void SetCursor(CursorType cursorType) {
		UpdateCursor(cursorType);
	}

	void UpdateCursor(CursorType cursorType) {
		Sprite cursorSprite = GetCursorSprite(m_cursors, cursorType);
        m_spriteRenderer.sprite = cursorSprite;
        m_spriteRenderer.color = Color.white;
	}

	Sprite GetCursorSprite(Texture2D cursorSheetTexture, CursorType cursorType) {
		int cursorIndex = EnumHelper.GetIndex<CursorType>(cursorType);
        int width = cursorSheetTexture.width, height = cursorSheetTexture.height;
        int x = cursorIndex * CURSOR_WIDTH, y = 0;
        Color[] data = cursorSheetTexture.GetPixels(x, -y, CURSOR_WIDTH, CURSOR_HEIGHT);
        Texture2D cursorTexture = new Texture2D(CURSOR_WIDTH, CURSOR_HEIGHT);
        cursorTexture.filterMode = FilterMode.Point;
        cursorTexture.SetPixels(data);
        cursorTexture.Apply();
		Sprite cursorSprite = Sprite.Create(cursorTexture, new Rect(0.0f, 0.0f, CURSOR_WIDTH, CURSOR_HEIGHT), new Vector2(0.0f, 1.0f), UNIT_SIZE);
        return cursorSprite;
    }

    Vector3 SnapToGrid(Vector3 position) {
        position.x = Mathf.Round(position.x * UNIT_SIZE) / UNIT_SIZE;
        position.y = Mathf.Round(position.y * UNIT_SIZE) / UNIT_SIZE;
        position.z = Mathf.Round(position.z * UNIT_SIZE) / UNIT_SIZE;
        return position;
    }
}
