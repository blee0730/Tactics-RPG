using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatPanel : MonoBehaviour 
{
	public Panel panel;
	public Sprite allyBackground;
	public Sprite enemyBackground;
	public Image background;
	public Image avatar;
	public Text nameLabel;
	public Text hpLabel;
	public Text mpLabel;
	public Text lvLabel;

	public void Display (GameObject obj)
	{
		if (obj == null)
		{
			Clear();
			return;
		}

		Alliance alliance = obj.GetComponent<Alliance>();
		if (background != null)
		{
			bool isEnemy = alliance != null && alliance.type == Alliances.Enemy;
			background.sprite = isEnemy ? enemyBackground : allyBackground;
		}

		UnitProfile profile = obj.GetComponent<UnitProfile>();
		if (avatar != null)
		{
			Sprite portrait = profile != null ? profile.statusPortrait : null;
			avatar.sprite = portrait;
			avatar.enabled = portrait != null;
		}

		if (nameLabel != null)
			nameLabel.text = profile != null ? profile.DisplayName : obj.name;

		Stats stats = obj.GetComponent<Stats>();
		if (stats != null)
		{
			if (hpLabel != null)
				hpLabel.text = string.Format( "HP {0} / {1}", stats[StatTypes.HP], stats[StatTypes.MHP] );
			if (mpLabel != null)
				mpLabel.text = string.Format( "MP {0} / {1}", stats[StatTypes.MP], stats[StatTypes.MMP] );
			if (lvLabel != null)
				lvLabel.text = string.Format( "LV. {0}", stats[StatTypes.LVL]);
		}
		else
		{
			if (hpLabel != null)
				hpLabel.text = "HP -- / --";
			if (mpLabel != null)
				mpLabel.text = "MP -- / --";
			if (lvLabel != null)
				lvLabel.text = "LV. --";
		}
	}

	void Clear ()
	{
		if (avatar != null)
		{
			avatar.sprite = null;
			avatar.enabled = false;
		}
		if (nameLabel != null)
			nameLabel.text = "";
		if (hpLabel != null)
			hpLabel.text = "HP -- / --";
		if (mpLabel != null)
			mpLabel.text = "MP -- / --";
		if (lvLabel != null)
			lvLabel.text = "LV. --";
	}
}
