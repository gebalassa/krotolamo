using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscordInvitation : MonoBehaviour
{
    [SerializeField] public static string discordLink = "https://discord.gg/taPk2A8k8K";

    [Header("Background")]
    [SerializeField] GameObject background;
    [SerializeField] Color backgroundInitialColor = new Color(0, 0, 0, 0);
    [SerializeField] Color backgroundFinalColor = new Color(0, 0, 0, .7f);

    [Header("Logo")]
    [SerializeField] GameObject logo;
    [SerializeField] Color logoInitialColor = new Color(255, 255, 255, 0);
    [SerializeField] Color logoFinalColor = new Color(255, 255, 255, 1);

    [Header("Texts")]
    [SerializeField] GameObject description, textButtonCancel, textButtonJoin;

    [Header("Buttons")]
    [SerializeField] GameObject buttonJoin;
    [SerializeField] GameObject buttonCancel;
    [SerializeField] Color buttonJoinInitialColor = new Color(.22f, .89f, .5f, 0);
    [SerializeField] Color buttonJoinFinalColor = new Color(.22f, .89f, .5f, 1);
    [SerializeField] Color buttonCancelInitialColor = new Color(1f, .33f, .13f, 0);
    [SerializeField] Color buttonCancelFinalColor = new Color(1f, .33f, .13f, 1);

    // Start is called before the first frame update
    void Start()
    {
        FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Realizar un fade sobre el elemento y sus hijos
    private void Fade(bool show)
    {
        List<GameObject> targets = new List<GameObject>()
        {
            background, logo, description, textButtonJoin, textButtonCancel, buttonJoin, buttonCancel
        };

        if (show) gameObject.SetActive(true);
        int complete = 0;
        string type = show ? "fadeIn" : "fadeOut";

        foreach (GameObject target in targets)
        {
            Color initialColor = new Color(1, 1, 1, show ? 0 : 1);
            Color finalColor = new Color(1, 1, 1, show ? 1 : 0);

            if (target.Equals(background)) {
                initialColor = show ? backgroundInitialColor : backgroundFinalColor;
                finalColor = !show ? backgroundInitialColor : backgroundFinalColor;
            }
            else if (target.Equals(logo) || target.Equals(description) || target.Equals(textButtonJoin) || target.Equals(textButtonCancel) )
            {
                initialColor = show ? logoInitialColor : logoFinalColor;
                finalColor = !show ? logoInitialColor : logoFinalColor;
            }
            else if (target.Equals(buttonJoin))
            {
                initialColor = show ? buttonJoinInitialColor : buttonJoinFinalColor;
                finalColor = !show ? buttonJoinInitialColor : buttonJoinFinalColor;
            }
            else if (target.Equals(buttonCancel))
            {
                initialColor = show ? buttonCancelInitialColor : buttonCancelFinalColor;
                finalColor = !show ? buttonCancelInitialColor : buttonCancelFinalColor;
            }

            Image image = target.GetComponent<Image>();
            Text text = target.GetComponent<Text>();

            System.Action<ITween<Color>> updateColor = (t) =>
            {
                if(image) image.color = t.CurrentValue;
                if(text) text.color = t.CurrentValue;
            };

            System.Action<ITween<Color>> updateColorComplete = (t) =>
            {
                ++complete;
                if (complete == targets.Count && !show) CompleteFadeOut();
            };

            // Hacer fade in y mostrar elemento
            target.gameObject.Tween(string.Format("Fade{0}{1}", type, target.GetInstanceID()), initialColor, finalColor, .5f, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete);
        }

       
    }

    // Mostrar elementos
    private void FadeIn()
    {
        Fade(true);
    }

    // Ocultar elementos
    private void FadeOut()
    {
        Fade(false);
    }

    // Unirse al discord
    public void Join()
    {
        FadeOut();
        Application.OpenURL(discordLink);
    }

    // Ocultar elementos
    public void Cancel()
    {
        FadeOut();
    }

    private void CompleteFadeOut()
    {
        Destroy(gameObject);
    }

}
