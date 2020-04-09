using System;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public class AutoControllerState
{
	bool m_active, m_regroupRequired;
    float m_regroupExpireTime;
    int m_totalFollowTargets;
    MapTile m_regroupPointTile, m_followPointTile;
    GameManager m_regroupPointManager;

	// Cache value?
	public bool IsSurrounded(GameManager gameManager) {
		return gameManager.MainPlayerIsSurrounded();
	}

	public bool IsActive() {
		return m_active;
	}

	public void SetActive(bool active) {
		m_active = active;
	}

    public float GetRegroupPointExpireTime() {
        return m_regroupExpireTime;
    }

    public void SetRegroupPointExpireTime(float expireTime) {
        m_regroupExpireTime = expireTime;
    }

    public MapTile GetRegroupPoint() {
        if(m_regroupPointManager != null) {
            m_regroupPointManager.GetMainPlayerPosition(out int map, out int x, out int y);
            return Tuple.Create(map, x, y);
        }
        else {
            return m_regroupPointTile;
        }
    }

    public void SetRegroupPoint(MapTile tile) {
        m_regroupPointTile = tile;
        m_regroupPointManager = null;
    }

    public void SetRegroupPoint(GameManager manager) {
        m_regroupPointTile = null;
        m_regroupPointManager = manager;
    }

    public bool GetRegroupRequired() {
        return m_regroupRequired;
    }

    public void SetRegroupRequired(bool required) {
        m_regroupRequired = required;
    }

    public MapTile GetFollowPoint() {
        return m_followPointTile;
    }

    public void SetFollowPoint(MapTile tile) {
        m_followPointTile = tile;
    }
}
