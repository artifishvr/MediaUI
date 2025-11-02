using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using UnityEngine.UI;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;


namespace Arti
{

    public class DetailsManager : UdonSharpBehaviour
    {
        public NyaResultsManager ResultsManager = null;

        public Text uiTitle = null;
        public Text uiRating = null;
        public Text uiOverview = null;

        public GameObject seasonsEpisodesTemplate = null;
        public Transform seasonsEpisodesParent = null;
        public Scrollbar seasonsEpisodesScrollbar = null;
        private GameObject[] seasonsEpisodesCache = { };

        public GameObject throbber = null;
        public VRCUrlsArray urlsArray = null;

        public GameObject SeasonsButton = null;

        private DataToken dataToken;
        private DataToken seasonsData;

        private bool hasSeasons = true;


        void Start()
        {

        }

        public void CloseDetails()
        {
            ClearScrollviewContent();
            gameObject.SetActive(false);
            throbber.SetActive(false);
            SeasonsButton.SetActive(false);
        }

        public void UpdateDataToken(DataToken searchResultEntryData)
        {
            dataToken = searchResultEntryData;

            searchResultEntryData.DataDictionary.TryGetValue("seasons", TokenType.DataList, out seasonsData);

            SetUiTextValueFromDataToken(uiTitle, "title", searchResultEntryData, TokenType.String, defaultString: "No Title");

            // Recalculate size
            uiTitle.gameObject.SetActive(false);
            uiTitle.gameObject.SetActive(true);

            SetUiTextValueFromDataToken(uiRating, "rating", searchResultEntryData, TokenType.Double, defaultString: "No Rating");
            SetUiTextValueFromDataToken(uiOverview, "overview", searchResultEntryData, TokenType.String, defaultString: "No Overview");

            if (dataToken.DataDictionary.TryGetValue("episodesUrl", TokenType.Double, out DataToken value))
            {
                hasSeasons = false;
                SeasonsButton.SetActive(false);
                GetEpisodes(urlsArray.vrcurls_pool[(int)value.Double]);
            }
            else
            {
                UpdateSeasonsUI();
                throbber.SetActive(false);
            }
        }

        public void Seasons()
        {
            UpdateSeasonsUI();
        }

        private void UpdateSeasonsUI()
        {
            ClearScrollviewContent();
            SeasonsButton.SetActive(false);
            hasSeasons = true;
            foreach (DataToken token in seasonsData.DataList.ToArray())
            {

                GameObject _newSearchRes = Instantiate(seasonsEpisodesTemplate, seasonsEpisodesParent);
                NyaResult _res = _newSearchRes.GetComponent<NyaResult>();



                int vrcurl_id = (int)token.DataDictionary["vrcurl"].Double;

                _res.url = urlsArray.vrcurls_pool[vrcurl_id];

                _res.resultType = 1;

                _res.UpdateDataTokenSeasonsEpisodes(token);

                // showoff
                _newSearchRes.SetActive(true);

                // Cache them so we can destroy them later uwu
                seasonsEpisodesCache = seasonsEpisodesCache.Add(_newSearchRes);
            }
        }

        public void GetEpisodes(VRCUrl url)
        {
            Debug.Log("Getting episodes for season url: " + url);

            throbber.SetActive(true);
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);

        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log("String downloaded successfully: " + result.Result);

            ClearScrollviewContent();

            // JSON to stupid vrc format
            DataToken ReceivedData;
            if (VRCJson.TryDeserializeFromJson(result.Result, out DataToken deserializedData))
            {
                ReceivedData = deserializedData;
            }
            else
            {
                Debug.Log($"JSON Deserialization error message: {deserializedData}");
                Debug.Log("Donwloaded String Deserialization Failed");
                return;
            }

            // Extracting data from deserialized
            DataToken ResultsData;
            if (ReceivedData.DataDictionary.TryGetValue("episodes", TokenType.DataList, out DataToken ResultsDataToken))
            {
                ResultsData = ResultsDataToken;
            }
            else
            {
                Debug.Log("JSON doesn't contain episodes object");
                return;
            }
            if (ResultsData.DataList.ToArray().Length == 0)
            {
                Debug.Log("JSON doesn't contain any episodes (no episodes found)");
                return;
            }


            // Creating UI elements for each result
            foreach (DataToken token in ResultsData.DataList.ToArray())
            {

                GameObject _newSearchRes = Instantiate(seasonsEpisodesTemplate, seasonsEpisodesParent);
                NyaResult _res = _newSearchRes.GetComponent<NyaResult>();


                int vrcurl_id = (int)token.DataDictionary["vrcurl"].Double;

                _res.url = urlsArray.vrcurls_pool[vrcurl_id];
                _res.resultType = 2;

                _res.UpdateDataTokenSeasonsEpisodes(token);


                // Main Videoplayer
                _res.UiController = ResultsManager.VideoPlayerUIController;
                _res.UrlInputField = ResultsManager.UrlInputField;

                _newSearchRes.SetActive(true);

                // Cache them so we can destroy them later uwu
                seasonsEpisodesCache = seasonsEpisodesCache.Add(_newSearchRes);
            }
            throbber.SetActive(false);
            if (hasSeasons)
                SeasonsButton.SetActive(true);

        }

        public void ClearScrollviewContent()
        {
            foreach (GameObject obj in seasonsEpisodesCache)
            {
                Destroy(obj);
            }
            seasonsEpisodesScrollbar.value = 1;
            seasonsEpisodesCache = seasonsEpisodesCache.Resize(0);
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