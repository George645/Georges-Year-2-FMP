using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateUnitSelectionCards : MonoBehaviour {
    //[SerializeField]
    //int numberOfSoldier;
    [SerializeField]
    float maxWidthOfCard;
    [SerializeField]
    float borderSize;
    [SerializeField]
    float cardBorderSize;

    CameraScript camera;

    Unit[] units;

    void Start() {
        camera = CameraScript.instance;
        ReplaceUnitCards();
    }

    void MakeUnitsArray() {
        units = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => x.playersUnit).ToArray();
    }

    void MakeNewCard(Vector2 pos, float width, bool halved, Unit unit) {
        //Debug.Log(pos + ", " + width + ", " + halved + ", " + unit);
        GameObject newChild = new GameObject();
        newChild.transform.parent = transform;
        newChild.AddComponent<RectTransform>();
        newChild.GetComponent<RectTransform>().anchoredPosition = pos;
        newChild.GetComponent<RectTransform>().sizeDelta = new Vector2(width, halved ? (197.28f - borderSize * 2) / 2 : 197.28f - borderSize * 2);
        newChild.AddComponent<PurpleCustomButtonScript>();
        newChild.GetComponent<PurpleCustomButtonScript>().Unit = unit;
        newChild.AddComponent<Image>();
        newChild.AddComponent<Button>();
    }

    public void ReplaceUnitCards() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (units == null) MakeUnitsArray();

        int numberOfSoldiersPerRow = units.Count() > 10 ? units.Count() / 2 : units.Count();

        //Debug.Log(units.Where(x => x.playersUnit).Count());
        int maxWidth = Mathf.Min((int)(maxWidthOfCard + cardBorderSize) * units.Count() + (int)borderSize, 1920);
        float widthOfCard = (maxWidth - units.Count() * cardBorderSize) / units.Count();
        widthOfCard *= units.Count() > 10 ? 0.5f : 1;

        int horizontalPosition = 0;
        int verticalPosition = 0;
        for (int i = 0; i < units.Count(); i++) {
            Vector2 positionOfCard = new Vector2((-1920 / 2 + borderSize + widthOfCard / 2 + (widthOfCard + cardBorderSize) * horizontalPosition), units.Count() > 10 ? -270 + verticalPosition * 540 : 0);

            MakeNewCard(positionOfCard, widthOfCard / 2 - borderSize, units.Count() > 10, units[i]);

            horizontalPosition++;

            if (horizontalPosition > units.Count() / 2 && units.Count() > 10) {
                verticalPosition++;
                horizontalPosition = 0;
            }
        }

    }
}
