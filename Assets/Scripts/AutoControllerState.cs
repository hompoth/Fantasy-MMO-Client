using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoControllerState
{
  bool m_active;

    // Cache value
  public bool IsSurrounded(GameManager gameManager) {
	  return gameManager.MainPlayerIsSurrounded();
  }

  public bool IsActive() {
    return m_active;
  }

  public void SetActive(bool active) {
    m_active = active;
  }
}
