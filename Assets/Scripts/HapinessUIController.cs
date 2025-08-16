using UnityEngine;
using UnityEngine.UI;

public class HappinessUIController : MonoBehaviour
{
    public Slider happinessSlider;
    public Image fillImage;

    void Update()
    {
        float happiness = SuperGlobal.computeHappiness();

        // Mettre à jour la valeur du slider
        happinessSlider.value = happiness;

        // Calculer la couleur : rouge → jaune → vert
        Color color;
        if (happiness < 0.5f)
        {
            // Rouge à jaune
            color = Color.Lerp(Color.red, Color.yellow, happiness / 0.5f);
        }
        else
        {
            // Jaune à vert
            color = Color.Lerp(Color.yellow, Color.green, (happiness - 0.5f) / 0.5f);
        }

        fillImage.color = color;
    }
}
