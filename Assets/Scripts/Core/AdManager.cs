using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace CubeShift.Core
{
    /// <summary>
    /// Manages Google Mobile Ads (AdMob) initialization, loading, and showing of Interstitial ads.
    /// Uses DontDestroyOnLoad to persist across levels.
    /// </summary>
    public sealed class AdManager : MonoBehaviour
    {
        private static AdManager instance;
        public static AdManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // Look for existing instance in the scene
                    instance = FindAnyObjectByType<AdManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AdManager");
                        instance = go.AddComponent<AdManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Ad Unit IDs (Test IDs by default)")]
        [Tooltip("Test ID: ca-app-pub-3940256099942544/1033173712")]
        [SerializeField] private string androidInterstitialUnitId = "ca-app-pub-7705121801777574/7942640596";
        
        [Header("Settings")]
        [SerializeField, Tooltip("Show an ad every N level completions")]
        private int showAdEveryNLevels = 2;

        private InterstitialAd interstitialAd;
        private int completedLevelsCount = 0;
        private bool isInitialized = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAdMob();
        }

        private void InitializeAdMob()
        {
            Debug.Log("[AdManager] Initializing Google Mobile Ads SDK...");
            
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log("[AdManager] Google Mobile Ads SDK Initialized.");
                isInitialized = true;
                
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    LoadInterstitialAd();
                });
            });
        }

        /// <summary>
        /// Loads a new Interstitial Ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] Cannot load ad: SDK is not initialized yet.");
                return;
            }

            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            Debug.Log("[AdManager] Loading Interstitial Ad...");

            string adUnitId = androidInterstitialUnitId;
#if UNITY_EDITOR
            // Safely use test unit ID in editor to prevent account suspension
            adUnitId = "ca-app-pub-3940256099942544/1033173712";
#endif

            AdRequest adRequest = new AdRequest();

            InterstitialAd.Load(adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"[AdManager] Interstitial ad failed to load ({adUnitId}): {error}");
                        return;
                    }

                    Debug.Log($"[AdManager] Interstitial ad loaded successfully.");
                    interstitialAd = ad;
                    RegisterAdEvents(interstitialAd);
                });
        }

        /// <summary>
        /// Shows the interstitial ad if it is loaded and frequency requirement is met.
        /// </summary>
        public void ShowInterstitialAdWithInterval()
        {
            completedLevelsCount++;
            Debug.Log($"[AdManager] Level completed. Total completions: {completedLevelsCount}. Frequency target: {showAdEveryNLevels}");

            if (completedLevelsCount % showAdEveryNLevels == 0)
            {
                ShowInterstitialAd();
            }
            else
            {
                Debug.Log("[AdManager] Interval not reached. Not showing ad for this level.");
            }
        }

        /// <summary>
        /// Directly shows the interstitial ad if ready.
        /// </summary>
        public void ShowInterstitialAd()
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                Debug.Log("[AdManager] Showing Interstitial Ad.");
                interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("[AdManager] Interstitial ad is not ready. Attempting to load one now.");
                LoadInterstitialAd();
            }
        }

        private void RegisterAdEvents(InterstitialAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"[AdManager] Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
            };

            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("[AdManager] Interstitial ad recorded an impression.");
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("[AdManager] Interstitial ad was clicked.");
            };

            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdManager] Interstitial ad full screen content opened.");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdManager] Interstitial ad full screen content closed. Preloading next ad.");
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    LoadInterstitialAd();
                });
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdManager] Interstitial ad failed to open full screen content: {error}");
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    LoadInterstitialAd();
                });
            };
        }
    }
}
