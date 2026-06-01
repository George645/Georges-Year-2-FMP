using System;
using System.Linq;
using UnityEngine;
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
    [SerializeField]
    int widthOfUnitCardBackground;
    [SerializeField]
    int thicknessHeightOfUnitCardBackground;
    [SerializeField]
    int positionHeightOfUnitCardBackground;

    CameraScript thisCamera;

    Unit[] units;

    void Start() {
        thisCamera = CameraScript.instance;
        ReplaceUnitCards();
    }
    private void Update() {
        if (!Array.TrueForAll(units, x => x != null)) {
            ReplaceUnitCards();
        }
    }

    void MakeUnitsArray() {
        units = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => x.playersUnit && x != null).ToArray();
    }

    void MakeNewCard(Vector2 pos, float width, bool halved, Unit unit) {
        //Debug.Log(pos + ", " + width + ", " + halved + ", " + unit);
        GameObject newChild = new();
        newChild.transform.parent = transform;
        newChild.AddComponent<RectTransform>();
        newChild.GetComponent<RectTransform>().anchoredPosition = pos;
        newChild.GetComponent<RectTransform>().localScale = Vector3.one;
        newChild.GetComponent<RectTransform>().sizeDelta = new Vector2(width, halved ? (thicknessHeightOfUnitCardBackground - borderSize * 4) / 2 : thicknessHeightOfUnitCardBackground - borderSize * 4);
        newChild.AddComponent<PurpleCustomButtonScript>();
        newChild.GetComponent<PurpleCustomButtonScript>().Unit = unit;
        newChild.AddComponent<Image>();
        newChild.AddComponent<Button>();
        newChild.GetComponent<Button>().onClick.AddListener(newChild.GetComponent<PurpleCustomButtonScript>().OnButtonClicked);

    }

    public void ReplaceUnitCards() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (units == null || units.Count() == 0 || !Array.TrueForAll(units, x => x == null)) MakeUnitsArray();

        int numberOfSoldiersPerRow = units.Count() > 10 ? units.Count() / 2 : units.Count();

        //Debug.Log(AIunits.Where(x => x.playersUnit).Count());
        float scaledCardBorderSize = cardBorderSize / (units.Count() > 10 ? 2 : 1);

        int maxWidth = Mathf.Min((int)(maxWidthOfCard + scaledCardBorderSize) * units.Count(), widthOfUnitCardBackground - (int)borderSize * 2);
        float widthOfCard = (maxWidth - units.Count() * scaledCardBorderSize) / units.Count();

        float scalingQuantity = 1 / ((((float)((units.Count() + 1) / 2) / (units.Count() / 2)) - 1) / 2 + 1);

        widthOfCard = units.Count() > 10 ? (float)widthOfCard * scalingQuantity * 2 : widthOfCard;

        int horizontalPosition = 0;
        int verticalPosition = 0;
        for (int i = 0; i < units.Count(); i++) {
            Vector2 positionOfCard = new ((float)(-(float)widthOfUnitCardBackground / 2 + borderSize + widthOfCard / 2 + (widthOfCard + scaledCardBorderSize) * horizontalPosition), units.Count() > 10 ? positionHeightOfUnitCardBackground / 2 - verticalPosition * positionHeightOfUnitCardBackground : 0);


            MakeNewCard(positionOfCard, widthOfCard - borderSize / 2, units.Count() > 10, units[i]);

            horizontalPosition++;

            if (horizontalPosition >= (float)units.Count() / 2 && units.Count() > 10) {
                verticalPosition++;
                horizontalPosition = 0;
            }
        }
    }
}
