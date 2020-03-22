using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emote {
    Heart = 1, Question, Grumble, Ellipsis, Poop, Surprise, Sleep, Angry, Cry, Music, Dollar, Wink
}

public class PlayerController : MonoBehaviour
{
    const float TIMER_WAIT_TIME = 0.5f, DOUBLE_CLICK_WAIT_TIME = 0.5f;
    const string VERTICAL_INPUT = "Vertical";
    const string HORIZONTAL_INPUT = "Horizontal";

    public PlayerState m_state;
    public AutoController m_autoController;

    Dictionary<KeyCode, float> keyCodeTimerDictionary;
    float m_clickTimer;

    void Awake() {
        keyCodeTimerDictionary = new Dictionary<KeyCode, float>();
    }

    void Update() {   
        if (Input.GetMouseButtonUp(0)) {
            bool controlKeyPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            m_state.LeftMouseUp(controlKeyPressed);
        }
         
        if (Input.GetMouseButtonDown(0)) {
            m_state.LeftMouseDown();
            float now = Time.time;
            if(now - m_clickTimer < DOUBLE_CLICK_WAIT_TIME) {
                m_state.LeftDoubleClick();
            }
            m_clickTimer = now;
        }

        if(Input.GetMouseButtonUp(1)) {
            m_state.RightMouseUp();
        }

        if (Input.GetMouseButtonDown(1)) {
            bool controlKeyPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            m_state.RightMouseDown(controlKeyPressed);
        }

        bool shiftKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if(CanSendCommand(KeyCode.Alpha0) || CanSendCommand(KeyCode.Keypad0)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Wink);
            }
            else {
                m_state.UseCommandSlot(10);
            }
        }
        if(CanSendCommand(KeyCode.Alpha1) || CanSendCommand(KeyCode.Keypad1)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Heart);
            }
            else {
                m_state.UseCommandSlot(1);
            }
        }
        if(CanSendCommand(KeyCode.Alpha2) || CanSendCommand(KeyCode.Keypad2)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Question);
            }
            else {
                m_state.UseCommandSlot(2);
            }
        }
        if(CanSendCommand(KeyCode.Alpha3) || CanSendCommand(KeyCode.Keypad3)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Ellipsis);
            }
            else {
                m_state.UseCommandSlot(3);
            }
        }
        if(CanSendCommand(KeyCode.Alpha4) || CanSendCommand(KeyCode.Keypad4)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Poop);
            }
            else {
                m_state.UseCommandSlot(4);
            }
        }
        if(CanSendCommand(KeyCode.Alpha5) || CanSendCommand(KeyCode.Keypad5)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Surprise);
            }
            else {
                m_state.UseCommandSlot(5);
            }
        }
        if(CanSendCommand(KeyCode.Alpha6) || CanSendCommand(KeyCode.Keypad6)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Sleep);
            }
            else {
                m_state.UseCommandSlot(6);
            }
        }
        if(CanSendCommand(KeyCode.Alpha7) || CanSendCommand(KeyCode.Keypad7)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Angry);
            }
            else {
                m_state.UseCommandSlot(7);
            }
        }
        if(CanSendCommand(KeyCode.Alpha8) || CanSendCommand(KeyCode.Keypad8)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Cry);
            }
            else {
                m_state.UseCommandSlot(8);
            }
        }
        if(CanSendCommand(KeyCode.Alpha9) || CanSendCommand(KeyCode.Keypad9)) {
            if(shiftKeyPressed) {
                m_state.UseEmote(Emote.Music);
            }
            else {
                m_state.UseCommandSlot(9);
            }
        }

        if(CanSendCommand(KeyCode.Tab)) {
            if(shiftKeyPressed) {
                m_state.LaunchNewPlayer();
            }
            else {
                m_state.SwitchPlayer();
            }
        }
        
        if(CanSendCommand(KeyCode.Minus)) {
            m_autoController.Disable();
        }
        
        if(shiftKeyPressed && CanSendCommand(KeyCode.Equals)) {
            m_autoController.Enable();
        }

        if(CanSendCommand(KeyCode.Home)) {
            m_state.Home();
        }

        if(CanSendCommand(KeyCode.Return) || CanSendCommand(KeyCode.KeypadEnter)) {
            m_state.Enter();
        }

        if(CanSendCommand(KeyCode.Slash)) {
            m_state.EnableChat();
        }
        
        if (CanSendCommand(KeyCode.Escape)) {
            m_state.Escape();
        }
        else {
            m_state.AddInputText(Input.inputString);
        }

        if(CanMovePlayer(out Vector3 inputVector)) {
            m_state.HandlePlayerMovement(inputVector);
            m_autoController.Disable();
        }

        if(CanSendCommand(KeyCode.LeftArrow)) {
            m_state.LeftArrow();
            m_autoController.Disable();
            // TODO: Move into m_state, and only trigger on move
        }

        if(CanSendCommand(KeyCode.RightArrow)) {
            m_state.RightArrow();
            m_autoController.Disable();
        }

        if(CanSendCommand(KeyCode.UpArrow)) {
            m_state.UpArrow();
            m_autoController.Disable();
        }

        if(CanSendCommand(KeyCode.DownArrow)) {
            m_state.DownArrow();
            m_autoController.Disable();
        }

        if (CanSendCommand(KeyCode.G)) {
            m_state.AddGuildText();
        }

        if (CanSendCommand(KeyCode.T)) {
            m_state.AddTellText();
        }

        if (CanSendCommand(KeyCode.R)) {
            m_state.AddReplyText();
        }

        if (CanSendCommand(KeyCode.PageUp)) {
            m_state.PageUp();
        }

        if (CanSendCommand(KeyCode.PageDown)) {
            m_state.PageDown();
        }
        
        if (CanSendCommand(KeyCode.Space)) {
            m_state.Attack();
        }

        if(CanSendCommand(KeyCode.Comma)) {
            m_state.PickUp();
        }
        
        if(CanSendCommand(KeyCode.S)) {
            m_state.ToggleSpellsWindow();
        }
        
        if(CanSendCommand(KeyCode.I)) {
            m_state.ToggleInventory();
        }
        
        if(CanSendCommand(KeyCode.C)) {
            m_state.ToggleCharacterWindow();
        }

        if(CanSendCommand(KeyCode.P)) {
            m_state.TogglePartyWindow(true);
        }

        if(CanSendCommand(KeyCode.F1)) {
            m_state.MinimizeScreen();
        }
    
        if(CanSendCommand(KeyCode.F2)) {
            m_state.ToggleCommandBar();
        }

        if(CanSendCommand(KeyCode.F3)) {
            m_state.ToggleBuffBar();
        }

        if(CanSendCommand(KeyCode.F4)) {
            m_state.ToggleChatWindow();
        }
    
        if(CanSendCommand(KeyCode.F5)) {
            m_state.ToggleFPSBar();
        }

        if(CanSendCommand(KeyCode.F6)) {
            m_state.ToggleHealthBar();
        }

        if(CanSendCommand(KeyCode.F7)) {
            m_state.ToggleManaBar();
        }

        if(CanSendCommand(KeyCode.F8)) {
            m_state.ToggleSpiritBar();
        }
    
        if(CanSendCommand(KeyCode.F9)) {
            m_state.ToggleExperienceBar();
        }

        if(CanSendCommand(KeyCode.F10)) {
            m_state.TogglePartyWindow(false);
        }
    
        if(CanSendCommand(KeyCode.F11)) {
            m_state.ToggleOptionsBar();
        }
    
        if(CanSendCommand(KeyCode.F12)) {
            m_state.ToggleDiscardButton();
        }
    }

    bool IsWindowBlockingMovement() {
        if(m_state.GetWindowTypeCount(WindowType.VendorWindow) > 0) {
            return true;
        }
        else if(m_state.GetWindowTypeCount(WindowType.TradeWindow) > 0) {
            return true;
        }
        return false;
    }

    private bool CanMovePlayer(out Vector3 inputVector) {
        float vertical = Input.GetAxisRaw(VERTICAL_INPUT);
        float horizontal = Input.GetAxisRaw(HORIZONTAL_INPUT);
        if(!IsWindowBlockingMovement()) {
            if(vertical != 0) {
                inputVector = new Vector3(0, vertical, 0);
                return true;
            }
            else if(horizontal != 0) {
                inputVector = new Vector3(horizontal, 0, 0);
                return true;
            }
        }
        inputVector = Vector3.zero;
        return false;
    }

    private bool CanSendCommand(KeyCode keyCode) {
        if(Input.GetKeyDown(keyCode)) {
            keyCodeTimerDictionary[keyCode] = Time.time;
            return true;
        }
        else if(Input.GetKey(keyCode)) {
            if(keyCodeTimerDictionary.TryGetValue(keyCode, out float timer)) {
                if(Time.time - timer < TIMER_WAIT_TIME) {
                    return false;
                }
            }
            else {
                keyCodeTimerDictionary[keyCode] = Time.time;
            }
            return true;
        }
        return false;
    }
}
