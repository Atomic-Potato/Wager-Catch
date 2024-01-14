using UnityEngine;

public class ColorsManager : Singleton<ColorsManager>
{
    [Space, Header("Money")]
    [SerializeField] Color _affordable = new Color(0, 1, 0, 1);
    public Color Affordable => _affordable;
    [SerializeField] Color _unaffordable = new Color(1, 0, 0, 1);
    public Color Unaffordable => _unaffordable;

    public static Color GetPriceColor(int price)
    {
        return price <=  GameManager.Instance.Balance ? Instance.Affordable : Instance.Unaffordable;
    }
}
