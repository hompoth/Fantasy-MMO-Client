using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsBarUI : WindowUI
{
    public AsperetaTextObject m_fps;
    public int m_updatePerSecond;
    float m_delta;
    int m_frameCount;
    
    void Start() {
        m_fps.SetTextColor(AsperetaTextColor.white);
    }

    void Update() {
        m_frameCount++;
        m_delta += Time.deltaTime;
        float updateTimePeriod = 1f / m_updatePerSecond;
        if(m_delta > updateTimePeriod) {
            int frameRate = Mathf.Min((int)(m_frameCount / m_delta), 999);
            string fpsText = String.Format("FPS {0,3}", frameRate);
            m_fps.SetText(fpsText);
            m_frameCount = 0;
            m_delta -= updateTimePeriod;
        }
    }
}
