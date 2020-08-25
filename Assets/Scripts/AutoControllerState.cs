using System;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public class AutoControllerState
{
	bool m_active;
    MapTile m_targetTile;
    PlayerManager m_target;

	public bool IsActive() {
		return m_active;
	}

	public void SetActive(bool active) {
		m_active = active;
	}

    public MapTile GetTargetTile() {
        return m_targetTile;
    }

    public void SetTargetTile(MapTile tile) {
        m_targetTile = tile;
    }

    public PlayerManager GetTarget() {
        return m_target;
    }

    public void SetTarget(PlayerManager player) {
        m_target = player;
    }
}
