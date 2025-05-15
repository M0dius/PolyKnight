using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Coin Settings")]
    public int totalCoins = 0;
    public bool persistBetweenScenes = true;

    [Header("UI References")]
    public TMP_Text coinCountText;
    public GameObject coinIcon;

    [Header("Animation Settings")]
    public AnimationCurve scaleAnimation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.2f);
    public float animationDuration = 0.5f;

    [Header("Sound Effects")]
    public AudioClip coinCollectSound;
    public AudioClip coinSpendSound;

    private AudioSource audioSource;
    private int lastCoinCount;

    // Events
    public delegate void CoinChangedHandler(int newAmount);
    public event CoinChangedHandler OnCoinChanged;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (Instance == null)
        {
            Instance = this;
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (persistBetweenScenes)
        {
            // totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }

        lastCoinCount = totalCoins;
        UpdateCoinDisplay();
    }

    void Update()
    {
        if (totalCoins != lastCoinCount)
        {
            UpdateCoinDisplay();
            lastCoinCount = totalCoins;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCoins();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void AddCoins(int amount)
    {
        totalCoins += amount;

        if (persistBetweenScenes)
        {
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
        }

        if (audioSource && coinCollectSound)
        {
            audioSource.PlayOneShot(coinCollectSound);
        }

        UpdateCoinDisplay();
        OnCoinChanged?.Invoke(totalCoins);

        Debug.Log($"Added {amount} coins. Total: {totalCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;

            if (persistBetweenScenes)
            {
                PlayerPrefs.SetInt("TotalCoins", totalCoins);
                PlayerPrefs.Save();
            }

            if (audioSource && coinSpendSound)
            {
                audioSource.PlayOneShot(coinSpendSound);
            }

            UpdateCoinDisplay();
            OnCoinChanged?.Invoke(totalCoins);

            Debug.Log($"Spent {amount} coins. Total: {totalCoins}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough coins! Have: {totalCoins}, Need: {amount}");
            return false;
        }
    }

    void UpdateCoinDisplay()
    {
        if (coinCountText != null)
        {
            coinCountText.text = totalCoins.ToString();
            StartCoroutine(AnimateCoinCount());
        }
    }

    IEnumerator AnimateCoinCount()
    {
        if (coinCountText == null) yield break;

        Transform textTransform = coinCountText.transform;
        Vector3 originalScale = textTransform.localScale;

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animationDuration;
            float scaleMultiplier = Mathf.Min(scaleAnimation.Evaluate(normalizedTime), 1.05f);
            textTransform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    public void ResetCoins()
    {
        totalCoins = 0;
        PlayerPrefs.SetInt("TotalCoins", 0);
        PlayerPrefs.Save();
        UpdateCoinDisplay();
        OnCoinChanged?.Invoke(totalCoins);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this == null) return; // Avoid calling on destroyed object

        // Re-link UI references if needed
        if (coinCountText == null)
            coinCountText = GameObject.FindWithTag("CoinText")?.GetComponent<TMP_Text>();

        if (coinIcon == null)
            coinIcon = GameObject.FindWithTag("CoinIcon");

        StartCoroutine(DelayedUIReconnect());
    }

    private IEnumerator DelayedUIReconnect()
    {
        yield return new WaitForSeconds(0.1f); // Wait until UI is likely loaded

        coinCountText = GameObject.FindWithTag("CoinText")?.GetComponent<TMP_Text>();
        coinIcon = GameObject.FindWithTag("CoinIcon");

        // Wait a frame if still null (can happen with slow transitions/fade-ins)
        if (coinCountText == null)
            yield return null;

        UpdateCoinDisplay();
    }
}