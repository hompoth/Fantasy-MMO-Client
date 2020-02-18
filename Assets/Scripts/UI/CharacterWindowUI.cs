using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWindowUI : WindowUI
{
    public SlotUI[] m_itemSlots;
    public AsperetaTextObject[] m_textObjects;
    public AsperetaTextObject m_playerName, m_guildName, m_className, m_playerLevel, m_experience, m_strength, m_stamina, m_intelligence, 
        m_dexterity, m_armor, m_fireResist, m_waterResist, m_earthResist, m_airResist, m_spiritResist, m_gold, m_health, m_mana, m_spirit;

    new void Awake() {
        base.Awake();
        for (int index = 0; index < m_itemSlots.Length; ++index) {
            m_itemSlots[index].SetSlotIndex(index + 32);
        }
    }

    void Start() {
        foreach(AsperetaTextObject textObject in m_textObjects) {
            textObject.SetTextColor(AsperetaTextColor.yellow);
        }
    }

    public SlotUI GetSlot(int index) {
        index-=32;
        if(0 <= index && index < m_itemSlots.Length) {
            return m_itemSlots[index];
        }
        return null;
    }

    public override void SetSlotId(int index, int itemId) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotId(itemId);
        }
    }
    
    public override void SetSlotDescription(int index, string itemName) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotName(itemName);
        }
    }

    public override void SetSlotAmount(int index, int amount) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotAmount(amount);
        }
    }
    
    public override void UpdateSlotGraphic(int index, int graphicId, Color color) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].UpdateSlotGraphic(graphicId, color);
        }
    }    

    public void SetPlayerName(string playerName) {
        m_playerName.SetText(playerName);
    }
    
    public void SetHealth(int curHp, int maxHp) {
        m_health.SetText(curHp + "/" + maxHp);
    }
    
    public void SetMana(int curMp, int maxMp) {
        m_mana.SetText(curMp + "/" + maxMp);
    }
    
    public void SetSpirit(int curSp, int maxSp) {
        m_spirit.SetText(curSp + "/" + maxSp);
    }
    
    public void SetGuildName(string guildName) {
        m_guildName.SetText(guildName);
    }

    public void SetClassName(string className) {
        m_className.SetText(className);
    }
    
    public void SetPlayerLevel(int level){
        m_playerLevel.SetText(level);
    }
    
    public void SetStrength(int statStr){
        m_strength.SetText(statStr);
    }
    
    public void SetStamina(int statSta){
        m_stamina.SetText(statSta);
    }
    
    public void SetIntelligence(int statInt){
        m_intelligence.SetText(statInt);
    }
    
    public void SetDexterity(int statDex){
        m_dexterity.SetText(statDex);
    }
    
    public void SetArmor(int armor){
        m_armor.SetText(armor);
    }
    
    public void SetFireResistance(int resFire){
        m_fireResist.SetText(resFire);
    }
    
    public void SetWaterResistance(int resWater){
        m_waterResist.SetText(resWater);
    }
    
    public void SetEarthResistance(int resEarth){
        m_earthResist.SetText(resEarth);
    }
    
    public void SetAirResistance(int resAir){
        m_airResist.SetText(resAir);
    }
    
    public void SetSpiritResistance(int resSpirit){
        m_spiritResist.SetText(resSpirit);
    }
    
    public void SetGold(int gold) {
        m_gold.SetText(gold);
    }

    public void SetExperience(int experience) {
        m_experience.SetText(experience);
    }
}
