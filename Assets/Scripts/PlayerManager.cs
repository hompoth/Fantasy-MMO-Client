using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum MovingDirection {
    Up = 1, Right, Down, Left,
}

public enum FacingDirection {
    Up = 1, Down, Left, Right,
}

public static class DirectionExtensions {
    public static Vector2 ToVector(this MovingDirection direction) {
        switch(direction) {
            case MovingDirection.Up:
                return Vector2.up;
            case MovingDirection.Right:
                return Vector2.right;
            case MovingDirection.Down:
                return Vector2.down;
            case MovingDirection.Left:
                return Vector2.left;
        }
        return Vector2.down;
    }
    public static MovingDirection ToMovingDirection(this Vector3 vector) {
        if(vector.Equals(Vector3.up)) {
            return MovingDirection.Up;
        }
        else if(vector.Equals(Vector3.right)) {
            return MovingDirection.Right;
        }
        else if(vector.Equals(Vector3.down)) {
            return MovingDirection.Down;
        }
        else if(vector.Equals(Vector3.left)) {
            return MovingDirection.Left;
        }
        return MovingDirection.Down;
    }

        public static Vector2 ToVector(this FacingDirection direction) {
        switch(direction) {
            case FacingDirection.Up:
                return Vector2.up;
            case FacingDirection.Right:
                return Vector2.right;
            case FacingDirection.Down:
                return Vector2.down;
            case FacingDirection.Left:
                return Vector2.left;
        }
        return Vector2.down;
    }
    public static FacingDirection ToFacingDirection(this Vector3 vector) {
        if(vector.Equals(Vector3.up)) {
            return FacingDirection.Up;
        }
        else if(vector.Equals(Vector3.right)) {
            return FacingDirection.Right;
        }
        else if(vector.Equals(Vector3.down)) {
            return FacingDirection.Down;
        }
        else if(vector.Equals(Vector3.left)) {
            return FacingDirection.Left;
        }
        return FacingDirection.Down;
    }
}

public class PlayerManager : MonoBehaviour
{
    public Rigidbody2D rb;
    public GearSocket[] gearSockets;

    public float movementSpeedMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public GameObject playerUIObject;
    public AsperetaTextObject nameTextObject;
    public StatBarUI statBarObject;
    public SpriteRenderer bodySpriteRenderer;
    public GameObject displayObjects;
    public Spell spell;
    public Emoticon emoticon;
    public SpellTargetUI spellTargetObject;
    public TextBubbleUI m_textBubble, m_chatBubble;

    float BASE_MOVEMENT_SPEED = 0.4f;
    float BASE_ATTACK_SPEED = 1f;
    float ATTACK_ANIMATION_SPEED = 2f;
    float HP_MP_CLEAR_TIME = 2f;
    float CHAT_BUBBLE_CLEAR_TIME = 2f;
	string SLASH = Path.DirectorySeparatorChar.ToString();
    int playerHP = 0;
    int playerMP = 0;
    int m_bodyId;
    bool m_playerInvisible, m_mainPlayerCanSeeInvisible, m_isMainPlayer;

    IEnumerator moveCoroutine, attackCoroutine, hpMpCoroutine, chatBubbleCoroutine;
    bool m_isMoving, m_isAttacking, m_isAdmin, m_isPlayerInParty;
    float m_lerpSpeed, m_moveTimer, m_attackTimer, m_spriteHeight, m_spriteWidth;
    Vector3 m_currentDirection, m_startLocation, m_targetLocation;
    int m_displayObjectPosition = 0, m_playerType, m_playerId;
    string m_playerTitle, m_playerName, m_playerSurname;
    AnimAttackType m_attackType;

    void Start() {
        m_textBubble.gameObject.SetActive(false);
        m_chatBubble.gameObject.SetActive(false);
    }

    public void MouseOver(Vector3 worldPosition) {
        if(ClientManager.GetNameFormat().Equals(NameFormat.VisibleOnHover)) {
            m_textBubble.transform.position = worldPosition;
        }
    }

    public void MouseEnter(Vector3 worldPosition) {
        if(ClientManager.GetNameFormat().Equals(NameFormat.VisibleOnHover)) {
            m_textBubble.gameObject.SetActive(true);
            m_textBubble.transform.position = worldPosition;
            UpdatePlayerName();
        }
    }

    public void MouseExit(Vector3 worldPosition) {
        m_textBubble.gameObject.SetActive(false);
    }

    public void DisplayChatBubble(string message) {
        if(!string.IsNullOrEmpty(message)) {
            if(chatBubbleCoroutine != null) {
                StopCoroutine(chatBubbleCoroutine);
                chatBubbleCoroutine = null;
            }
            m_chatBubble.gameObject.SetActive(true);
            m_chatBubble.UpdateBubbleText(message);
            chatBubbleCoroutine = ClearChatBubble();
            StartCoroutine(chatBubbleCoroutine);
        }
    }

    IEnumerator ClearChatBubble() {
        yield return new WaitForSeconds(CHAT_BUBBLE_CLEAR_TIME);
        chatBubbleCoroutine = null;
        m_chatBubble.gameObject.SetActive(false);
    }

    public void DestroyAllButPlayerUI() {
        gameObject.name = "Destroyed";
        foreach (GearSocket gear in gearSockets) {
            gear.gameObject.SetActive(false);
        }
        playerUIObject.SetActive(false);
        Destroy(rb);
        Destroy(GetComponent<BoxCollider2D>());
        Destroy(this);
        Destroy(gameObject, 2f);
    }

    public void LoadPlayerSpell(AnimationClip anim) {
        spell.PlayAnimation(anim);
    }

    public void LoadPlayerEmoticon(AnimationClip anim) {
        emoticon.PlayAnimation(anim);
    }

    public void SetPlayerId(int playerId) {
        m_playerId = playerId;
    }

    public void SetPlayerHPPercent(int hpPercent) {
        int previousHP = playerHP;
        statBarObject.SetHPBar(hpPercent);
        playerHP = hpPercent;
        UpdateStatBarSize();

        if(!(hpPercent == 0 || previousHP == 0)) {
            UpdateHPMPActive();
        }
    }

    public int GetPlayerHPPercent() {
        return playerHP;
    }

    public void SetPlayerMPPercent(int mpPercent) {
        int previousMP = playerMP;
        statBarObject.SetMPBar(mpPercent);
        playerMP = mpPercent;
        UpdateStatBarSize();

        if(!(mpPercent == 0 || previousMP == 0)) {
            UpdateHPMPActive();
        }
    }

    public int GetPlayerMPPercent() {
        return playerMP;
    }
 
    public void SetPlayerInvisible(bool invis) {
        m_playerInvisible = invis;
        UpdatePlayerVisibility();
    }

    public void SetMainPlayerCanSeeInvisible(bool invis) {
        m_mainPlayerCanSeeInvisible = invis;
    }

    bool IsPlayerVisible() {
        return !m_playerInvisible || m_mainPlayerCanSeeInvisible || m_isMainPlayer;
    }

    public void UpdatePlayerVisibility() {
        bool canSeeInvisible = IsPlayerVisible();
        foreach (GearSocket gear in gearSockets) {
            gear.SetInvisible(m_playerInvisible, canSeeInvisible);
        }
        UpdateNameVisibility();
        UpdateHealthManaVisibility();
    }

    public void UpdateNameVisibility() {
        bool isActive = IsPlayerVisible() && ClientManager.GetNameFormat().Equals(NameFormat.Visible);
        nameTextObject.gameObject.SetActive(isActive);
    }

    public void UpdateHealthManaVisibility() {
        bool isActive = IsPlayerVisible();
        if(isActive) {
            HealthManaFormat healthManaFormat = ClientManager.GetHealthManaFormat();
            switch(healthManaFormat) {
                case HealthManaFormat.Hidden:
                    isActive = false;
                    break;
                case HealthManaFormat.VisibleOnUpdate:
                    if(playerHP == 100 && (playerMP == 100 || playerMP == 0) && hpMpCoroutine == null) {
                        isActive = false;
                    }
                    break;
            }
        }
        statBarObject.gameObject.SetActive(isActive);
    }

    public void SetPlayerType(int type) {
        m_playerType = type;
        UpdatePlayerNameColor();
    }

    public void SetPlayerGuild(string guild) {
        // TODO Do something with the guild
    }

    public void SetPlayerTitle(string title) {
        m_playerTitle = title.Trim();
        UpdatePlayerName();
    }
    
    public void SetPlayerName(string name) {
        m_playerName = name.Trim();
        UpdatePlayerName();
    }

    public string GetPlayerName() {
        string name = m_playerName;
        if(!string.IsNullOrEmpty(m_playerTitle)) {
            name = m_playerTitle + " " + name;
        }
        if(!string.IsNullOrEmpty(m_playerSurname)) {
            name = name + " " + m_playerSurname;
        }
        return name;
    }

    public void SetPlayerSurname(string surname) {
        m_playerSurname = surname.Trim();
        UpdatePlayerName();
    }

    public void SetPlayerFacing(int facing) {
        // Uses MovingDirection on load for some reason
        MovingDirection direction = EnumHelper.GetValueName<MovingDirection>(facing);
        Face(direction.ToVector(), false);
    }

    public void SetPlayerInParty(bool isPlayerInParty) {
        m_isPlayerInParty = isPlayerInParty;
        UpdatePlayerNameColor();
    }

    public void SetPlayerAdmin(bool isAdmin) {
        m_isAdmin = isAdmin;
        UpdatePlayerNameColor();
        if(isAdmin) {
            gameObject.layer = 12;
        }
        else {
            if(m_isMainPlayer) {
                gameObject.layer = 11;
            }
            else {
                gameObject.layer = 0;
            }
        }
    }

    public FacingDirection GetPlayerFacingDirection() {
        return m_currentDirection.ToFacingDirection();
    }
    
    public MovingDirection GetPlayerMovingDirection() {
        return m_currentDirection.ToMovingDirection();
    }

    public void GetPlayerPosition(GameManager manager, out int x, out int y) {
        Vector3 position = new Vector3(m_targetLocation.x, m_targetLocation.y + 0.5f, 0);
        manager.ServerPosition(position, out x, out y);
    }

    public void SetPlayerPosition(Vector3 newPosition, bool moveTowards = false) {
        Vector3 position, difference, direction = Vector3.down;

        if(moveTowards) {
            if(!m_targetLocation.Equals(Vector3.zero)) {
                position = m_targetLocation;
            }
            else {
                position = rb.transform.position;
            }
            difference = newPosition - position;
            if(Mathf.Abs(difference.x) > Mathf.Abs(difference.y)) {
                if (difference.x > 0) {
                    direction = Vector3.right;
                }
                else {
                    direction = Vector3.left;
                }
            }
            else {
                if (difference.y > 0) {
                    direction = Vector3.up;
                }
                else {
                    direction = Vector3.down;
                }
            }
            Face(direction, false);
            MoveFrom(newPosition - direction, false);
        }
        else {
            m_startLocation = newPosition;
            ResetLocation();
        }
    }

    public void SetPlayerAttackSpeed(int weaponSpeed) {
        BASE_ATTACK_SPEED = weaponSpeed / 1000f;
    }

    public void SetPlayerAttacking() {
        m_isAttacking = false;            
        AnimatorAttacking(false);
        if(attackCoroutine != null) {
            StopCoroutine(attackCoroutine);
        }
        Attack(false);
    }

    public void UpdatePlayerAppearance(int bodyId, int poseId, int hairId, Color hairColor, int chestId, Color chestColor, int helmId, Color helmColor, int pantsId, 
		Color pantsColor, int shoesId, Color shoesColor, int shieldId, Color shieldColor, int weaponId, Color weaponColor, bool invis, int faceId) {
        
        SetPlayerPose(poseId);
        SetPlayerInvisible(invis);
        SetPlayerBody(bodyId);
        SetPlayerFace(faceId);
        SetPlayerHair(hairId, hairColor);
        SetPlayerHelm(helmId, helmColor);
        SetPlayerChest(chestId, chestColor);
        SetPlayerPants(pantsId, pantsColor);
        SetPlayerShoes(shoesId, shoesColor);
        SetPlayerShield(shieldId, shieldColor);
        SetPlayerWeapon(weaponId, weaponColor);   
    }

    bool ShowPlayerEquipment() {
        return m_bodyId < 100;
    }

    void ClearPlayerAppearance() {
        foreach (GearSocket gear in gearSockets) {
            gear.Equip(null, m_attackType);
        }
    }

    public void SetPlayerPose(int poseId) {
        if(ShowPlayerEquipment()) {
            SetPlayerPoseEnum(EnumHelper.GetValueName<AnimAttackType>(poseId));
        }
        else {
            SetPlayerPoseEnum(AnimAttackType.Fist);
        }
    }

    void SetPlayerPoseEnum(AnimAttackType attackType) {
        m_attackType = attackType;
        foreach (GearSocket gear in gearSockets) {
            gear.SetAttackType(attackType);
        }
    }

    public void SetPlayerBody(int bodyId) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Body" + SLASH + bodyId);
        if(animGroup != null) {
            m_spriteHeight = animGroup.GetSpriteHeight();
            m_spriteWidth = animGroup.GetSpriteWidth();
            UpdatePlayerNamePosition(m_spriteHeight);
            UpdateStatBarSize();
            UpdateSpellTargetSize();
            m_bodyId = bodyId;
            if(!ShowPlayerEquipment()) {
                SetPlayerPoseEnum(AnimAttackType.Fist);
                ClearPlayerAppearance();
            }
            GearSocket gearSocket = GetGearSocketWithName("Body");
            if (gearSocket != null) {
                gearSocket.Equip(animGroup, m_attackType);
            }
        }
    }

    public void SetPlayerFace(int faceId) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Face" + SLASH + faceId);
        GearSocket gearSocket = GetGearSocketWithName("Face");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType);
        }
    }

    public void SetPlayerHair(int hairId, Color hairColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Face" + SLASH + hairId);
        GearSocket gearSocket = GetGearSocketWithName("Hair");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, hairColor);
        }
    }

    public void SetPlayerHelm(int helmId, Color helmColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Helm" + SLASH + helmId);
        GearSocket gearSocket = GetGearSocketWithName("Helm");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, helmColor);
        }
    }

    public void SetPlayerChest(int chestId, Color chestColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Top" + SLASH + chestId);
        GearSocket gearSocket = GetGearSocketWithName("Top");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, chestColor);
        }
    }

    public void SetPlayerPants(int pantsId, Color pantsColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Bottom" + SLASH + pantsId);
        GearSocket gearSocket = GetGearSocketWithName("Bottom");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, pantsColor);
        }
    }

    public void SetPlayerShoes(int shoesId, Color shoesColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Shoes" + SLASH + shoesId);
        GearSocket gearSocket = GetGearSocketWithName("Shoes");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, shoesColor);
        }
    }

    public void SetPlayerShield(int shieldId, Color shieldColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Weapon" + SLASH + shieldId);
        GearSocket gearSocket = GetGearSocketWithName("Shield");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, shieldColor);
        }
    }

    public void SetPlayerWeapon(int weaponId, Color weaponColor) {
        AnimGroup animGroup = Resources.Load<AnimGroup>("Animations" + SLASH + "Groupings" + SLASH + "Weapon" + SLASH + weaponId);
        GearSocket gearSocket = GetGearSocketWithName("Weapon");
        if (gearSocket != null && ShowPlayerEquipment()) {
            gearSocket.Equip(animGroup, m_attackType, weaponColor);
        }
    }
    
    public void AddSpreadOutDisplayObject(string displayText, Color displayColor, string characterName) {
        float xOffset, yOffset;
        int totalDisplayObjects = displayObjects.transform.childCount;
        
        if(totalDisplayObjects < 18) {
            if(totalDisplayObjects == 0) {
                m_displayObjectPosition = 0;
            }
            else {
                m_displayObjectPosition = (m_displayObjectPosition + 1) % 9;
            }
            yOffset = m_spriteHeight - Mathf.Min(Mathf.Floor(totalDisplayObjects / 3), 2) * (8f/ 32f);

            if(m_displayObjectPosition % 3 == 0) {
                xOffset = 4f / 32f;
            }
            else if(m_displayObjectPosition % 3 == 1) {
                xOffset = -4f / 32f;
            }
            else {
                xOffset = 12f / 32f;
            }
            InstantiateDisplayObject(xOffset, yOffset, displayText, displayColor);
        }
    }

	public void AddCenteredDisplayObject(string displayText, Color displayColor, string characterName) {
        int totalDisplayObjects = displayObjects.transform.childCount;
        
        if(totalDisplayObjects < 18) {
            InstantiateDisplayObject(4f / 32f, m_spriteHeight / 2, displayText, displayColor);
        }
    }

    public GearSocket GetGearSocketWithName(string name) {
        foreach (GearSocket gear in gearSockets) {
            if(gear.gameObject.name.Equals(name)) {
                return gear;
            } 
        }
        return null;
    }

    public bool IsFacingDirection(Vector3 direction) {
        return direction.Equals(m_currentDirection);
    }

    public bool Face(Vector3 direction, bool fromKeyboard) {
        if(!m_isMoving || !fromKeyboard) {
            m_currentDirection = direction;
            AnimatorFacing(direction.x, direction.y);
            return true;
        }
        return false;
    }

    public bool Move(bool fromKeyboard) {
        return MoveFrom(rb.transform.position, fromKeyboard);
    }

    public bool MoveFrom (Vector3 position, bool fromKeyboard) {
        if(!fromKeyboard || (!m_isMoving && (m_moveTimer > GetMovementSpeed(false)))) {
            if(!fromKeyboard || !GameManager.IsPositionBlocked(position + m_currentDirection, m_isAdmin)) {
                if(moveCoroutine != null) {
                    StopCoroutine(moveCoroutine);
                    AnimatorMoving(false);
                }
                m_moveTimer = 0;
                m_isMoving = true;
                m_lerpSpeed = 0;
                m_startLocation = position;
                m_targetLocation = m_startLocation + m_currentDirection;
                AnimatorMoving(true);
                moveCoroutine = MoveOverTime();
                StartCoroutine(moveCoroutine);
                return true;
            }
        }
        return false;
    }

    public bool Attack(bool fromKeyboard) {
        if(!fromKeyboard || (!m_isAttacking && (m_attackTimer > GetAttackSpeed(false)))) {
            if(attackCoroutine != null) {
                StopCoroutine(attackCoroutine);
                AnimatorAttacking(false);
            }
            m_attackTimer = 0;
            m_isAttacking = true;
            AnimatorAttacking(true);
            attackCoroutine = AttackOverTime();
            StartCoroutine(attackCoroutine);
            return true;
        }
        return false;
    }

    void InstantiateDisplayObject(float x, float y, string displayText, Color displayColor) {
        DisplayObject displayObject = Resources.Load<DisplayObject>("Prefabs" + SLASH + "DisplayObject");
        AsperetaTextObject asperetaTextObject = Resources.Load<AsperetaTextObject>("Prefabs" + SLASH + "TextObject");
        if(displayObject != null) {
            Vector3 displayPosition = displayObjects.transform.position;
            DisplayObject display = Instantiate(displayObject, displayPosition, Quaternion.identity);
            AsperetaTextObject asperetaText = Instantiate(asperetaTextObject, displayPosition + new Vector3(x, y, 0), Quaternion.identity); 
            display.SetTextObject(asperetaText); 
            display.SetYOffset(y);
            asperetaText.SetText(displayText);
            asperetaText.SetTextColor(displayColor);
            Transform displayTransform = display.gameObject.transform;
            Transform textTransform = asperetaText.gameObject.transform;
            displayTransform.SetParent(displayObjects.transform);
            textTransform.SetParent(displayTransform);
        }
    }

    public void SetIsMainPlayer(bool isMainPlayer) {
        m_isMainPlayer = isMainPlayer;
    }

    public bool GetIsMainPlayer() {
        return m_isMainPlayer;
    }

    public bool IsSurrounded() {
        //TODO Move to gameManager and instead getLocation from mainPlayer instead
        if(GameManager.IsPositionBlocked(m_targetLocation + Vector3.up, m_isAdmin) &&
            GameManager.IsPositionBlocked(m_targetLocation + Vector3.down, m_isAdmin) &&
            GameManager.IsPositionBlocked(m_targetLocation + Vector3.left, m_isAdmin) &&
            GameManager.IsPositionBlocked(m_targetLocation + Vector3.right, m_isAdmin)) {
            return true;
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerManager collidedPlayer = collision.gameObject.GetComponent<PlayerManager>();  // Assumes that moving targets have PlayerManagers
        if (m_isMainPlayer && (collidedPlayer == null || collidedPlayer.IsAtPosition(m_targetLocation))) {
            ResetLocation();
        }
        Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision.collider);
    }

    bool IsAtPosition(Vector3 positionToCheck) {
        Vector3 targetPosition = m_targetLocation;
        return SnapPosition(positionToCheck).Equals(SnapPosition(targetPosition));
    }

    void ResetLocation() {
        if(rb != null) {
            rb.MovePosition(m_startLocation);
            m_moveTimer = 0;
            m_targetLocation = m_startLocation;
            m_lerpSpeed = 1f;
            m_isMoving = false;
            AnimatorMoving(false);
            if(moveCoroutine != null) {
                StopCoroutine(moveCoroutine);
            }
        }
    }

    public void SetPlayerTarget(bool enabled) {
        spellTargetObject.gameObject.SetActive(enabled);
    }

    void UpdatePlayerName() {
        string name = GetPlayerName();
        nameTextObject.SetText(name);
        m_textBubble.UpdateBubbleText(name);
    }

    void UpdatePlayerNameColor() {
        Color color;
        if(m_isAdmin) {
            color = AsperetaTextColor.blue;
        }
        else if(m_isPlayerInParty) {
            color = AsperetaTextColor.yellow;
        }
        else if(m_playerType == 12) { // GM
            color = AsperetaTextColor.purple;
        }
        else if(m_playerType == 11) { // EM
            color = AsperetaTextColor.green;
        }
        else {
            color = AsperetaTextColor.white;
        }
        nameTextObject.SetTextColor(color);
    }

    void UpdatePlayerNamePosition(float playerNameHeight) {
        Transform nameTextTransform = playerUIObject.transform;
        Vector3 pos = nameTextTransform.localPosition;

        pos.y = playerNameHeight + (17f/32f);
        nameTextTransform.localPosition = pos;
    }

    void UpdateStatBarSize() {
        statBarObject.UpdateStatBarWidth(m_spriteWidth);
    }

    void UpdateSpellTargetSize() {
        spellTargetObject.UpdateTargetSize(m_spriteWidth, m_spriteHeight);
    }

    void FixedUpdate() {
        m_moveTimer += Time.deltaTime;
        m_attackTimer += Time.deltaTime;

        if (m_isMoving) {
            m_lerpSpeed = Mathf.Clamp(m_lerpSpeed + Time.fixedDeltaTime  / GetMovementSpeed(false), 0, 1);
            Vector3 currentPos = Vector3.Lerp(m_startLocation, m_targetLocation, m_lerpSpeed);
            rb.MovePosition(currentPos);
        }
        else {
            SnapToGrid();
        }
    }

    void AnimatorFacing(float horizontal, float vertical) {
        foreach (GearSocket gear in gearSockets) {
            gear.AnimatorFacing(horizontal, vertical);
        }
    }

    void AnimatorMoving(bool isMoving) {
        foreach (GearSocket gear in gearSockets) {
            gear.AnimatorMoving(isMoving);
            gear.AnimatorMovementSpeed(GetMovementSpeed(true));
        }
    }

    void AnimatorAttacking(bool isAttacking) {
        foreach (GearSocket gear in gearSockets) {
            gear.AnimatorAttacking(isAttacking);
            gear.AnimatorAttackSpeed(GetAttackSpeed(true));
        }
    }

    IEnumerator MoveOverTime() {
        yield return new WaitForSeconds(GetMovementSpeed(false));
        AnimatorMoving(false);
        moveCoroutine = null;
        m_isMoving = false;
    }

    IEnumerator AttackOverTime() {
        float attackTime = GetAttackSpeed(false);
        float animationAttackTime = 1 / ATTACK_ANIMATION_SPEED;
        if(attackTime <= animationAttackTime) {
            yield return new WaitForSeconds(attackTime);
        }
        else {
            yield return new WaitForSeconds(animationAttackTime);
            //yield return new WaitForSeconds(attackTime - animationAttackTime);
        }
        AnimatorAttacking(false);
        attackCoroutine = null;
        m_isAttacking = false;
    }

    IEnumerator ClearHPMP() {
        yield return new WaitForSeconds(HP_MP_CLEAR_TIME);
        hpMpCoroutine = null;
        UpdateHealthManaVisibility();
    }

    private void UpdateHPMPActive() {
        if(hpMpCoroutine != null) {
            StopCoroutine(hpMpCoroutine);
            hpMpCoroutine = null;
        }
        if(playerHP == 100 && (playerMP == 100 || playerMP == 0)) {
            hpMpCoroutine = ClearHPMP();
            StartCoroutine(hpMpCoroutine);
        }
        UpdateHealthManaVisibility();
    }

    void SnapToGrid() {
        Vector3 currentPos = m_targetLocation;
        if(currentPos.Equals(Vector3.zero)) {
            currentPos = rb.transform.position;
        }
        currentPos = SnapPosition(currentPos);
        rb.MovePosition(currentPos);
    }

    Vector3 SnapPosition(Vector3 position) {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        position.z = Mathf.Round(position.z);
        return position;
    }

    float GetMovementSpeed(bool forAnimation){
        if (forAnimation) {
            return BASE_MOVEMENT_SPEED / movementSpeedMultiplier;
        }
        return BASE_MOVEMENT_SPEED * movementSpeedMultiplier;
    }

    float GetAttackSpeed(bool forAnimation){
        if (forAnimation) {
            return ATTACK_ANIMATION_SPEED;
        }
        return BASE_ATTACK_SPEED * attackSpeedMultiplier;
    }
}
