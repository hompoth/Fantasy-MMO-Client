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

    Dictionary<KeyCode, float> keyCodeTimerDictionary;
    float m_clickTimer;

    void Awake() {
        keyCodeTimerDictionary = new Dictionary<KeyCode, float>();
    }

    void Update() {   
        if(Input.GetKey(KeyCode.Minus) && CanSendCommand(KeyCode.Equals)) {
            AutoController.ToggleActive();
        }

        if (Input.GetMouseButtonUp(0)) {
            bool controlKeyPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            PlayerState.LeftMouseUp(controlKeyPressed);
        }
         
        if (Input.GetMouseButtonDown(0)) {
            PlayerState.LeftMouseDown();
            float now = Time.time;
            if(now - m_clickTimer < DOUBLE_CLICK_WAIT_TIME) {
                PlayerState.LeftDoubleClick();
            }
            m_clickTimer = now;
        }

        if(Input.GetMouseButtonUp(1)) {
            PlayerState.RightMouseUp();
        }

        if (Input.GetMouseButtonDown(1)) {
            bool controlKeyPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            PlayerState.RightMouseDown(controlKeyPressed);
        }

        bool shiftKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if(CanSendCommand(KeyCode.Alpha0) || CanSendCommand(KeyCode.Keypad0)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Wink);
            }
            else {
                PlayerState.UseCommandSlot(10);
            }
        }
        if(CanSendCommand(KeyCode.Alpha1) || CanSendCommand(KeyCode.Keypad1)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Heart);
            }
            else {
                PlayerState.UseCommandSlot(1);
            }
        }
        if(CanSendCommand(KeyCode.Alpha2) || CanSendCommand(KeyCode.Keypad2)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Question);
            }
            else {
                PlayerState.UseCommandSlot(2);
            }
        }
        if(CanSendCommand(KeyCode.Alpha3) || CanSendCommand(KeyCode.Keypad3)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Ellipsis);
            }
            else {
                PlayerState.UseCommandSlot(3);
            }
        }
        if(CanSendCommand(KeyCode.Alpha4) || CanSendCommand(KeyCode.Keypad4)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Poop);
            }
            else {
                PlayerState.UseCommandSlot(4);
            }
        }
        if(CanSendCommand(KeyCode.Alpha5) || CanSendCommand(KeyCode.Keypad5)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Surprise);
            }
            else {
                PlayerState.UseCommandSlot(5);
            }
        }
        if(CanSendCommand(KeyCode.Alpha6) || CanSendCommand(KeyCode.Keypad6)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Sleep);
            }
            else {
                PlayerState.UseCommandSlot(6);
            }
        }
        if(CanSendCommand(KeyCode.Alpha7) || CanSendCommand(KeyCode.Keypad7)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Angry);
            }
            else {
                PlayerState.UseCommandSlot(7);
            }
        }
        if(CanSendCommand(KeyCode.Alpha8) || CanSendCommand(KeyCode.Keypad8)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Cry);
            }
            else {
                PlayerState.UseCommandSlot(8);
            }
        }
        if(CanSendCommand(KeyCode.Alpha9) || CanSendCommand(KeyCode.Keypad9)) {
            if(shiftKeyPressed) {
                PlayerState.UseEmote(Emote.Music);
            }
            else {
                PlayerState.UseCommandSlot(9);
            }
        }

        if(CanSendCommand(KeyCode.Home)) {
            PlayerState.Home();
        }

        if(CanSendCommand(KeyCode.Return)) {
            PlayerState.Enter();
        }

        if(CanSendCommand(KeyCode.Slash)) {
            PlayerState.EnableChat();
        }
        
        if (CanSendCommand(KeyCode.Escape)) {
            PlayerState.Escape();
        }
        else {
            PlayerState.AddInputText(Input.inputString);
        }

        if(CanMovePlayer(out Vector3 inputVector)) {
            PlayerState.HandlePlayerMovement(inputVector);
            AutoController.Disable();
        }

        if(CanSendCommand(KeyCode.LeftArrow)) {
            PlayerState.LeftArrow();
            AutoController.Disable();
        }

        if(CanSendCommand(KeyCode.RightArrow)) {
            PlayerState.RightArrow();
            AutoController.Disable();
        }

        if(CanSendCommand(KeyCode.UpArrow)) {
            PlayerState.UpArrow();
            AutoController.Disable();
        }

        if(CanSendCommand(KeyCode.DownArrow)) {
            PlayerState.DownArrow();
            AutoController.Disable();
        }

        if (CanSendCommand(KeyCode.G)) {
            PlayerState.AddGuildText();
        }

        if (CanSendCommand(KeyCode.T)) {
            PlayerState.AddTellText();
        }

        if (CanSendCommand(KeyCode.R)) {
            PlayerState.AddReplyText();
        }

        if (CanSendCommand(KeyCode.PageUp)) {
            PlayerState.PageUp();
        }

        if (CanSendCommand(KeyCode.PageDown)) {
            PlayerState.PageDown();
        }
        
        if (CanSendCommand(KeyCode.Space)) {
            PlayerState.Attack();
        }

        if(CanSendCommand(KeyCode.Comma)) {
            PlayerState.PickUp();
        }
        
        if(CanSendCommand(KeyCode.S)) {
            PlayerState.ToggleSpellsWindow();
        }
        
        if(CanSendCommand(KeyCode.I)) {
            PlayerState.ToggleInventory();
        }
        
        if(CanSendCommand(KeyCode.C)) {
            PlayerState.ToggleCharacterWindow();
        }

        if(CanSendCommand(KeyCode.P)) {
            PlayerState.TogglePartyWindow(true);
        }

        if(CanSendCommand(KeyCode.F1)) {
            PlayerState.MinimizeScreen();
        }
    
        if(CanSendCommand(KeyCode.F2)) {
            PlayerState.ToggleCommandBar();
        }

        if(CanSendCommand(KeyCode.F3)) {
            PlayerState.ToggleBuffBar();
        }

        if(CanSendCommand(KeyCode.F4)) {
            PlayerState.ToggleChatWindow();
        }
    
        if(CanSendCommand(KeyCode.F5)) {
            PlayerState.ToggleFPSBar();
        }

        if(CanSendCommand(KeyCode.F6)) {
            PlayerState.ToggleHealthBar();
        }

        if(CanSendCommand(KeyCode.F7)) {
            PlayerState.ToggleManaBar();
        }

        if(CanSendCommand(KeyCode.F8)) {
            PlayerState.ToggleSpiritBar();
        }
    
        if(CanSendCommand(KeyCode.F9)) {
            PlayerState.ToggleExperienceBar();
        }

        if(CanSendCommand(KeyCode.F10)) {
            PlayerState.TogglePartyWindow(false);
        }
    
        if(CanSendCommand(KeyCode.F11)) {
            PlayerState.ToggleOptionsBar();
        }
    
        if(CanSendCommand(KeyCode.F12)) {
            PlayerState.ToggleDiscardButton();
        }
    }

    private bool CanMovePlayer(out Vector3 inputVector) {
        float vertical = Input.GetAxisRaw(VERTICAL_INPUT);
        float horizontal = Input.GetAxisRaw(HORIZONTAL_INPUT);

        if(vertical != 0) {
            inputVector = new Vector3(0, vertical, 0);
            return true;
        }
        else if(horizontal != 0) {
            inputVector = new Vector3(horizontal, 0, 0);
            return true;
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
