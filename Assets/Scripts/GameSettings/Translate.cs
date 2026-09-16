using UnityEngine;
using UnityEngine.Localization.Settings;

public class Translate : MonoBehaviour
{
    [SerializeField] private LocalizationSettings localizeSettings;
    public string currentlyTranslating = "en";
    private void Awake()
    {
        if(PlayerPrefs.HasKey("language"))
        {
            SetLanguage(PlayerPrefs.GetString("language"));
        }
    }
    public void SetLanguage(string language)
    {
        Debug.Log("Setting language to: " + language);
        currentlyTranslating = language;
        LocalizationSettings.SelectedLocale = localizeSettings.GetAvailableLocales().GetLocale(language);
        PlayerPrefs.SetString("language", language);
        PlayerPrefs.Save();
    }
}
