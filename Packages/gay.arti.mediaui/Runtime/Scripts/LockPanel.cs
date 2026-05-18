using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using TMPro;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;

namespace Arti
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LockPanel : UdonSharpBehaviour
    {
        [Header("References")]
        public VRCUrlInputField SearchInputField = null;
        public GameObject Panel = null;
        public UdonBehaviour UiController = null;
        public TextMeshProUGUI mainText = null;
        public TextMeshProUGUI APIPromo = null;
        public TextMeshProUGUI UntrustedURLsText = null;

        [Header("Persistence")]
        [SerializeField] private bool enablePersistence = false;

        [Tooltip("Keep this unique to avoid colliding with other systems using PlayerData.")]
        [SerializeField] private string persistenceKey = "Arti.LockPanel.SuccessfulUrl";

        private VRCUrl initURL = new VRCUrl("https://");
        private VRCUrl testURL = new VRCUrl("https://nya.llc/vrc");

        private string neededURL = "https://nya.llc";

        private bool _persistenceReady = false;
        private bool _unlockRequestInProgress = false;
        private bool _hasUnlockedThisSession = false;
        private bool _saveWhenPersistenceReady = false;

        private string _pendingSuccessfulUrl = "";

        void Start()
        {
            if (SearchInputField == null)
            {
                Debug.LogError("SearchInputField is not assigned!");
                return;
            }

            SearchInputField.SetUrl(initURL);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || !player.isLocal)
            {
                return;
            }

            _persistenceReady = true;

            if (!enablePersistence)
            {
                return;
            }

            // If the player unlocked before PlayerData was ready, save now.
            if (_saveWhenPersistenceReady && _pendingSuccessfulUrl != "")
            {
                SaveSuccessfulUnlock();
                return;
            }

            // Do not auto-run if they already unlocked manually this session.
            if (_hasUnlockedThisSession || _unlockRequestInProgress)
            {
                return;
            }

            string restoredUrl;
            if (!PlayerData.TryGetString(player, GetPersistenceKey(), out restoredUrl))
            {
                return;
            }

            restoredUrl = NormalizeUrl(restoredUrl);

            if (restoredUrl == "")
            {
                return;
            }

            Debug.Log("Restored LockPanel URL from persistence: " + restoredUrl);

            // Do not call SearchInputField.SetUrl(new VRCUrl(restoredUrl));
            // Udon does not expose runtime VRCUrl construction.
            TryURLInternal(restoredUrl, true);
        }

        public void TryURL()
        {
            if (SearchInputField == null)
            {
                Debug.LogError("SearchInputField is not assigned!");
                return;
            }

            VRCUrl inputVrc = SearchInputField.GetUrl();
            string inputUrl = "";

            if (inputVrc != null)
            {
                inputUrl = inputVrc.Get() ?? "";
            }

            TryURLInternal(inputUrl, false);
        }

        private void TryURLInternal(string rawInputUrl, bool fromPersistence)
        {
            if (_unlockRequestInProgress)
            {
                Debug.Log("Unlock request already in progress.");
                return;
            }

            if (SearchInputField == null || Panel == null)
            {
                Debug.LogError("SearchInputField or Panel is not assigned!");

                if (SearchInputField != null)
                {
                    SearchInputField.SetUrl(initURL);
                }

                return;
            }

            string inputUrl = NormalizeUrl(rawInputUrl);
            string requiredUrl = NormalizeUrl(neededURL);

            if (inputUrl == requiredUrl)
            {
                _pendingSuccessfulUrl = inputUrl;
                _unlockRequestInProgress = true;

                VRCStringDownloader.LoadUrl(testURL, (IUdonEventReceiver)this);

                if (fromPersistence)
                {
                    Debug.Log("Supported URL restored from persistence.");
                }
                else
                {
                    Debug.Log("Supported URL!");
                }
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
            _unlockRequestInProgress = false;

            Debug.Log("String downloaded successfully: " + result.Result);

            DataToken receivedData;
            DataToken deserializedData;

            if (VRCJson.TryDeserializeFromJson(result.Result, out deserializedData))
            {
                receivedData = deserializedData;
            }
            else
            {
                Debug.Log("JSON Deserialization error message: " + deserializedData);
                Debug.Log("Downloaded String Deserialization Failed");
                return;
            }

            DataToken resultsDataToken;

            if (receivedData.DataDictionary.TryGetValue("message", TokenType.String, out resultsDataToken))
            {
                if (APIPromo != null)
                {
                    APIPromo.text = resultsDataToken.String;
                }
            }
            else
            {
                Debug.Log("JSON doesn't contain message");
                return;
            }

            _hasUnlockedThisSession = true;
            SaveSuccessfulUnlockIfPossible();

            if (UiController != null)
            {
                UiController.SendCustomEvent("Movies");
            }

            if (Panel != null)
            {
                Panel.SetActive(false);
            }
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            _unlockRequestInProgress = false;
            _pendingSuccessfulUrl = "";

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

        private void SaveSuccessfulUnlockIfPossible()
        {
            if (!enablePersistence)
            {
                return;
            }

            if (_pendingSuccessfulUrl == "")
            {
                return;
            }

            // VRChat warns against using PlayerData before OnPlayerRestored.
            // If the API succeeds before persistence is ready, defer the save.
            if (!_persistenceReady)
            {
                _saveWhenPersistenceReady = true;
                return;
            }

            SaveSuccessfulUnlock();
        }

        private void SaveSuccessfulUnlock()
        {
            if (!enablePersistence)
            {
                return;
            }

            if (_pendingSuccessfulUrl == "")
            {
                return;
            }

            PlayerData.SetString(GetPersistenceKey(), _pendingSuccessfulUrl);
            _saveWhenPersistenceReady = false;

            Debug.Log("Saved LockPanel successful URL to persistence.");
        }

        private string NormalizeUrl(string url)
        {
            if (url == null)
            {
                return "";
            }

            return url.Trim().TrimEnd('/');
        }

        private string GetPersistenceKey()
        {
            string key = persistenceKey;

            if (key == null)
            {
                key = "";
            }

            key = key.Trim();

            if (key == "")
            {
                key = "Arti.LockPanel.SuccessfulUrl";
            }

            return key;
        }
    }
}
