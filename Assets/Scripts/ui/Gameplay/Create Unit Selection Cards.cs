using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField]
    int widthOfUnitCardBackground;
    [SerializeField]
    int thicknessHeightOfUnitCardBackground;
    [SerializeField]
    int positionHeightOfUnitCardBackground;

    CameraScript camera;

    Unit[] units;

    void Start() {
        camera = CameraScript.instance;
        ReplaceUnitCards();
    }

    void MakeUnitsArray() {
        units = FindObjectsByType<Unit>(FindObjectsSortMode.None).Where(x => x.playersUnit).ToArray();
        Debug.Log(string.Join<Unit>(", ", FindObjectsByType<Unit>(FindObjectsSortMode.None).ToArray()));
    }

    void MakeNewCard(Vector2 pos, float width, bool halved, Unit unit) {
        //Debug.Log(pos + ", " + width + ", " + halved + ", " + unit);
        GameObject newChild = new GameObject();
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

        //Debug.Log(units.Where(x => x.playersUnit).Count());
        float scaledCardBorderSize = cardBorderSize / (units.Count() > 10 ? 2 : 1);
        Debug.Log(cardBorderSize + ", " + scaledCardBorderSize);

        int maxWidth = Mathf.Min((int)(maxWidthOfCard + scaledCardBorderSize) * units.Count(), widthOfUnitCardBackground - (int)borderSize * 2);
        float widthOfCard = (maxWidth - units.Count() * scaledCardBorderSize) / units.Count();

        float scalingQuantity = 1 / ((((float)((units.Count() + 1) / 2) / (units.Count() / 2)) - 1) / 2 + 1);

        widthOfCard = units.Count() > 10 ? (float)widthOfCard * scalingQuantity * 2 : widthOfCard;
        Debug.Log((float)((int)(units.Count() + 1) / 2));
        Debug.Log((float)((int)units.Count() / 2));
        Debug.Log((float)((units.Count() + 1) / 2) / (units.Count() / 2));
        Debug.Log(1f / (float)((units.Count() + 1) / 2) * (units.Count() / 2) * 2);

        int horizontalPosition = 0;
        int verticalPosition = 0;
        for (int i = 0; i < units.Count(); i++) {
            Vector2 positionOfCard = new Vector2((float)(-(float)widthOfUnitCardBackground / 2 + borderSize + widthOfCard / 2 + (widthOfCard + scaledCardBorderSize) * horizontalPosition), units.Count() > 10 ? positionHeightOfUnitCardBackground / 2 - verticalPosition * positionHeightOfUnitCardBackground : 0);

            Debug.Log(positionOfCard);

            MakeNewCard(positionOfCard, widthOfCard - borderSize / 2, units.Count() > 10, units[i]);

            horizontalPosition++;

            if (horizontalPosition >= (float)units.Count() / 2 && units.Count() > 10) {
                verticalPosition++;
                horizontalPosition = 0;
            }
        }
    }
}
