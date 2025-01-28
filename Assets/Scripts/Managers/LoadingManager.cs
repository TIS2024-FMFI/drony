using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField]
    private GameObject loadingPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Na zaciatku skryt loading panel
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoading()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
    }

    public void HideLoading()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
}
