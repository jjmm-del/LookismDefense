using UnityEngine;

public class Debugger : MonoBehaviour
{
	[SerializeField] private DebugUnitCatalogPopup unitCatalogPopup;
	//골드 추가
	public void AddGold(int amount)
	{
		if(CurrencyManager.Instance == null) return;
		CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, amount);
		Debug.Log($"[디버그] 골드{amount} 추가 완료!");
	}

	//랜덤 흔함 소환권
	public void AddRandomCommon(int amount)
	{
		if(CurrencyManager.Instance == null) return;
		CurrencyManager.Instance.AddCurrency(CurrencyType.RandomCommon, amount);
		Debug.Log($"[디버그] 랜덤 흔함 소환권 {amount}개 추가 완료!");
	}
	
	//선택 흔함 소환권
	public void AddSelectCommon(int amount)
	{
		if(CurrencyManager.Instance == null) return;
		CurrencyManager.Instance.AddCurrency(CurrencyType.SelectCommon, amount);
		Debug.Log($"[디버그] 선택 흔함 소환권 {amount}개 추가 완료!");
	}

	public void OpenUnitCatalog()
	{
		if (unitCatalogPopup == null)
		{
			Debug.LogError($"[Debugger]UnitCatalogPopup이 연결되지 않았습니다.");
			return;
		}

		if (UIManager.Instance == null)
			return;

		UIManager.Instance.OpenPopup(unitCatalogPopup);
	}
}	
