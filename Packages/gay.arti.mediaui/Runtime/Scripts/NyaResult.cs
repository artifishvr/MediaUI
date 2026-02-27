using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace Arti
{
    public class NyaResult : UdonSharpBehaviour
    {
        public NyaResultsManager ResultsManager = null;

        public UdonBehaviour UiController = null;
        public VRCUrlInputField UrlInputField = null;

        public Text uiTitle = null;
        public Text uiRating = null;
        public Text uiRuntimeOrEpisode = null;

        public GameObject uiPlayButton = null;
        public GameObject uiDetailsButton = null;


        public int resultType = 0; // 0 - normal, 1 - season, 2 - episode

        private DataToken dataToken;
        [HideInInspector] public VRCUrl url = null;

        void Start()
        {

        }

        public void PlayVideo()
        {
            if (url == null)
            {
                Debug.LogError("URL is null in NyaResult (should be a tv show?)");
                return;
            }

            if (UrlInputField != null) UrlInputField.SetUrl(url);
            else Debug.LogError("UrlInputField is not set in NyaResult");

            // ProTV 2 and 3
            UiController.SendCustomEvent("_EndEditUrlInput");
            UiController.SendCustomEvent("EndEditUrlInput");
            // VizVid
            UiController.SendCustomEvent("_OnURLEndEdit");
            UiController.SendCustomEvent("_InputConfirmClick");
            // USharpVideo
            UiController.SendCustomEvent("OnURLInput");
            // YAMA
            UiController.SendCustomEvent("PlayUrlTop");
        }

        public void ShowDetails()
        {
            if (ResultsManager != null && ResultsManager.DetailsManager != null)
            {
                ResultsManager.DetailsManager.UpdateDataToken(dataToken);
                ResultsManager.DetailsManager.gameObject.SetActive(true);
            }
        }

        public void UpdateDataToken(DataToken searchResultEntryData)
        {
            dataToken = searchResultEntryData;

            SetUiTextValueFromDataToken(uiTitle, "title", searchResultEntryData, TokenType.String, defaultString: "No Title");

            // Recalculate size
            uiTitle.gameObject.SetActive(false);
            uiTitle.gameObject.SetActive(true);

            SetUiTextValueFromDataToken(uiRating, "rating", searchResultEntryData, TokenType.String, defaultString: "No Rating");


            if (dataToken.DataDictionary.TryGetValue("vrcurl", TokenType.Double, out DataToken value))
            {
                uiPlayButton.SetActive(true);
                uiDetailsButton.SetActive(false);
            }
            else
            {
                uiPlayButton.SetActive(false);
                uiDetailsButton.SetActive(true);
            }
        }

        public void UpdateDataTokenSeasonsEpisodes(DataToken searchResultEntryData)
        {
            if (resultType == 1)
            {
                SetUiTextValueFromDataToken(uiTitle, "seasonName", searchResultEntryData, TokenType.String, defaultString: "No Title");
            }
            else if (resultType == 2)
            {
                SetUiTextValueFromDataToken(uiTitle, "name", searchResultEntryData, TokenType.String, defaultString: "No Title");
            }
            else
            {
                Debug.LogError("Result type is not set in NyaResult");
                return;
            }

            // Recalculate size
            uiTitle.gameObject.SetActive(false);
            uiTitle.gameObject.SetActive(true);
        }

        public void GetSeasonsEpisodes()
        {
            if (resultType == 1)
            {
                ResultsManager.DetailsManager.GetEpisodes(url);
            }
            else
            {
                PlayVideo();

            }
        }

        private void SetUiTextValueFromDataToken(Text uiText, string valueName, DataToken dataToken, TokenType type = TokenType.String, string defaultString = "NoData")
        {
            if (!Utilities.IsValid(uiText)) return;
            if (dataToken.DataDictionary.TryGetValue(valueName, type, out DataToken value))
            {
                if (Utilities.IsValid(uiText)) uiText.text = value.ToString();
            }
            else
            {
                if (Utilities.IsValid(uiText)) uiText.text = defaultString;
            }
        }

    }
}