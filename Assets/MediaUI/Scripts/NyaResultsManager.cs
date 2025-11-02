using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using UnityEngine.UI;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;
using TMPro;


namespace Arti
{


    public class NyaResultsManager : UdonSharpBehaviour
    {
        public VRCUrlInputField SearchInputField = null;
        public GameObject searchResultTemplate = null;
        public Transform searchResultParent;
        public UdonBehaviour VideoPlayerUIController = null;
        public VRCUrlInputField UrlInputField = null;
        public VRCUrlsArray urlsArray = null;

        public TextMeshProUGUI resultsInfoText = null;

        public Scrollbar ResultsScrollbar = null;
        public GameObject throbber = null;

        public DetailsManager DetailsManager = null;

        private int mode = 0; // 0 - movies, 1 - tv, 2 - anime
        private VRCUrl searchURL_movies = new VRCUrl("https://vrc.nya.llc/v1/mov/search?input=                        Movie Search →   ");
        private VRCUrl searchURL_tv = new VRCUrl("https://vrc.nya.llc/v1/tv/search?input=                        TV Search →   ");
        private VRCUrl searchURL_anime = new VRCUrl("https://vrc.nya.llc/v1/anime/search?input=                        Anime Search →   ");

        private VRCUrl moviesTopURL = new VRCUrl("https://vrc.nya.llc/v1/mov/top");
        private VRCUrl tvTopURL = new VRCUrl("https://vrc.nya.llc/v1/tv/top");
        private VRCUrl animeTopURL = new VRCUrl("https://vrc.nya.llc/v1/anime/top");

        public GameObject InfoPanel = null;

        private GameObject[] searchResultsCache = { };

        void Start()
        {
            UpdateSearchInputField();
        }


        public void Movies()
        {
            Debug.Log("Movies button clicked");
            if (InfoPanel != null)
            {
                InfoPanel.SetActive(false);
            }

            mode = 0;

            UpdateSearchInputField();


            resultsInfoText.text = "";
            throbber.SetActive(true);
            ClearScrollviewContent();

            VRCStringDownloader.LoadUrl(moviesTopURL, (IUdonEventReceiver)this);
        }

        public void TV()
        {
            Debug.Log("TV button clicked");
            if (InfoPanel != null)
            {
                InfoPanel.SetActive(false);
            }

            mode = 1;

            UpdateSearchInputField();

            resultsInfoText.text = "";
            throbber.SetActive(true);
            ClearScrollviewContent();

            VRCStringDownloader.LoadUrl(tvTopURL, (IUdonEventReceiver)this);

        }

        public void Anime()
        {
            Debug.Log("Anime button clicked");
            if (InfoPanel != null)
            {
                InfoPanel.SetActive(false);
            }

            mode = 2;

            UpdateSearchInputField();

            resultsInfoText.text = "";
            throbber.SetActive(true);
            ClearScrollviewContent();

            VRCStringDownloader.LoadUrl(animeTopURL, (IUdonEventReceiver)this);

        }

        public void Search()
        {
            Debug.Log("Search button clicked");

            VRCUrl searchURL = SearchInputField.GetUrl();

            throbber.SetActive(true);
            ClearScrollviewContent();

            VRCStringDownloader.LoadUrl(searchURL, (IUdonEventReceiver)this);

            UpdateSearchInputField();
        }

        public void Info()
        {
            if (InfoPanel != null)
            {
                InfoPanel.SetActive(!InfoPanel.activeSelf);
            }
        }

        void UpdateSearchInputField()
        {
            if (mode == 0)
            {
                SearchInputField.SetUrl(searchURL_movies);
            }
            else if (mode == 1)
            {
                SearchInputField.SetUrl(searchURL_tv);
            }
            else if (mode == 2)
            {
                SearchInputField.SetUrl(searchURL_anime);
            }
        }

        public void ClearScrollviewContent()
        {
            foreach (GameObject obj in searchResultsCache)
            {
                Destroy(obj);
            }
            ResultsScrollbar.value = 1;
            searchResultsCache = searchResultsCache.Resize(0);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log("String downloaded successfully: " + result.Result);

            ClearScrollviewContent();

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

            // Extract data
            DataToken ResultsData;
            if (ReceivedData.DataDictionary.TryGetValue("results", TokenType.DataList, out DataToken ResultsDataToken))
            {
                ResultsData = ResultsDataToken;
            }
            else
            {
                Debug.Log("JSON doesn't contain Results object");
                return;
            }
            if (ResultsData.DataList.ToArray().Length == 0)
            {
                Debug.Log("JSON doesn't contain any results (nothing found)");
                resultsInfoText.text = "No results found";
                return;
            }

            throbber.SetActive(false);

            // Creating UI elements for each result
            foreach (DataToken token in ResultsData.DataList.ToArray())
            {

                GameObject _newSearchRes = Instantiate(searchResultTemplate, searchResultParent);
                NyaResult _res = _newSearchRes.GetComponent<NyaResult>();


                if (token.DataDictionary.TryGetValue("vrcurl", TokenType.Double, out DataToken value))
                {
                    int vrcurl_id = (int)value.Double;

                    _res.url = urlsArray.vrcurls_pool[vrcurl_id];
                }
                _res.UpdateDataToken(token);

                _res.UiController = VideoPlayerUIController;
                _res.UrlInputField = UrlInputField;

                _res.ResultsManager = this;

                // Display it!
                _newSearchRes.SetActive(true);

                // Cache them so we can destroy them later uwu
                searchResultsCache = searchResultsCache.Add(_newSearchRes);

            }
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError("Failed to download string: " + result.Error);
        }
    }

}