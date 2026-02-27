using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using TMPro;
using VRC.SDK3.Data;


namespace Arti
{
    public class LockPanel : UdonSharpBehaviour
    {

        public VRCUrlInputField SearchInputField = null;
        public GameObject Panel = null;
        public UdonBehaviour UiController = null;
        public TextMeshProUGUI mainText = null;
        public TextMeshProUGUI APIPromo = null;

        public TextMeshProUGUI UntrustedURLsText = null;



        private VRCUrl initURL = new VRCUrl("https://");
        private VRCUrl testURL = new VRCUrl("https://nya.llc/vrc");

        private string neededURL = "https://nya.llc";

        void Start()
        {
            if (SearchInputField == null)
            {
                Debug.LogError("SearchInputField is not assigned!");
                return;
            }
            SearchInputField.SetUrl(initURL);
        }

        public void TryURL()
        {
            if (SearchInputField == null || Panel == null)
            {
                Debug.LogError("SearchInputField or Panel is not assigned!");
                if (SearchInputField != null) SearchInputField.SetUrl(initURL);
                return;
            }

            VRCUrl inputVrc = SearchInputField.GetUrl();
            string inputUrl = "";
            if (inputVrc != null)
            {
                inputUrl = inputVrc.Get() ?? "";
            }

            inputUrl = inputUrl.Trim().TrimEnd('/');
            neededURL = neededURL.Trim().TrimEnd('/');

            if (inputUrl == neededURL)
            {
                VRCStringDownloader.LoadUrl(testURL, (IUdonEventReceiver)this);
                Debug.Log("Supported URL!");

            }
            else if (inputUrl.Contains("vr-m.net"))
            {
                if (mainText != null)
                {
                    mainText.text = "cmon.......";
                }
                SearchInputField.SetUrl(initURL);
            }
            else
            {
                Debug.Log("Invalid URL, resetting input field");
                SearchInputField.SetUrl(initURL);
            }
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log("String downloaded successfully: " + result.Result);

            DataToken ReceivedData;
            if (VRCJson.TryDeserializeFromJson(result.Result, out DataToken deserializedData))
            {
                ReceivedData = deserializedData;
            }
            else
            {
                Debug.Log($"JSON Deserialization error message: {deserializedData}");
                Debug.Log("Downloaded String Deserialization Failed");
                return;
            }

            if (ReceivedData.DataDictionary.TryGetValue("message", TokenType.String, out DataToken ResultsDataToken))
            {
                if (APIPromo != null)
                {
                    APIPromo.text = ResultsDataToken.String;
                }
            }
            else
            {
                Debug.Log("JSON doesn't contain message");
                return;
            }

            if (UiController != null)
            {
                UiController.SendCustomEvent("Movies");
            }
            Panel.SetActive(false);

        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError("Failed to download string: " + result.Error);

            if (UntrustedURLsText != null)
            {
                UntrustedURLsText.text = "Error contacting API. Please make sure you have Untrusted URLs enabled in your VRChat settings.";
            }

            if (APIPromo != null)
            {
                APIPromo.text = "API unreachable.";
            }

        }
    }
}