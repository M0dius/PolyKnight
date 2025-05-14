using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    
    [Header("Coin Settings")]
    public int totalCoins = 0;
    public bool persistBetweenScenes = true;
    
    [Header("UI References")]
    public Text coinCountText;
    public Text coinAddedText; // For showing "+X" when coins are added
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
        // Singleton pattern
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
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load saved coins
        if (persistBetweenScenes)
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }
        
        lastCoinCount = totalCoins;
        UpdateCoinDisplay();
    }
    
    void Update()
    {
        // Update display if coins changed externally
        if (totalCoins != lastCoinCount)
        {
            UpdateCoinDisplay();
            lastCoinCount = totalCoins;
        }
    }
    
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        
        // Save to PlayerPrefs if persistent
        if (persistBetweenScenes)
        {
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
        }
        
        // Play sound effect
        if (audioSource != null && coinCollectSound != null)
        {
            audioSource.PlayOneShot(coinCollectSound);
        }
        
        // Update display with animation
        UpdateCoinDisplay();
        ShowCoinAdded(amount);
        
        // Trigger event
        OnCoinChanged?.Invoke(totalCoins);
        
        Debug.Log($"Added {amount} coins. Total: {totalCoins}");
    }
    
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            
            // Save to PlayerPrefs if persistent
            if (persistBetweenScenes)
            {
                PlayerPrefs.SetInt("TotalCoins", totalCoins);
                PlayerPrefs.Save();
            }
            
            // Play sound effect
            if (audioSource != null && coinSpendSound != null)
            {
                audioSource.PlayOneShot(coinSpendSound);
            }
            
            // Update display
            UpdateCoinDisplay();
            
            // Trigger event
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
            
            // Animate coin count
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
            float scaleMultiplier = scaleAnimation.Evaluate(normalizedTime);
            textTransform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }
        
        textTransform.localScale = originalScale;
    }
    
    void ShowCoinAdded(int amount)
    {
        if (coinAddedText != null)
        {
            StartCoroutine(ShowCoinAddedCoroutine(amount));
        }
    }
    
    IEnumerator ShowCoinAddedCoroutine(int amount)
    {
        coinAddedText.text = $"+{amount}";
        coinAddedText.gameObject.SetActive(true);
        
        // Animate the added text
        Transform textTransform = coinAddedText.transform;
        Vector3 startPos = textTransform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 50f;
        
        Color startColor = coinAddedText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            textTransform.localPosition = Vector3.Lerp(startPos, endPos, timer);
            coinAddedText.color = Color.Lerp(startColor, endColor, timer);
            yield return null;
        }
        
        coinAddedText.gameObject.SetActive(false);
        textTransform.localPosition = startPos;
        coinAddedText.color = startColor;
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
}