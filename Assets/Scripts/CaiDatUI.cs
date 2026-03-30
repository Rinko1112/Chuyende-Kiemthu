using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CaiDatUI : MonoBehaviour
{
    [Header("PANEL")]
    [SerializeField] private GameObject panelCaiDat;
    [SerializeField] private GameObject panelAmThanh;

    [Header("BUTTON MỞ")]
    [SerializeField] private GameObject nutMoCaiDat;

    [Header("SLIDER ÂM THANH")]
    [SerializeField] private Slider sliderAmThanh;

    [Header("ANIMATION")]
    [SerializeField] private float tocDoAnim = 5f;

    private RectTransform rectCaiDat;
    private RectTransform rectAmThanh;

    private Vector2 viTriAn;     // trên màn hình
    private Vector2 viTriHien;   // vị trí thật

    private bool dangMoCaiDat = false;
    private bool dangMoAmThanh = false;

    private const string KEY_VOLUME = "MASTER_VOLUME";

    void Start()
    {
        rectCaiDat = panelCaiDat.GetComponent<RectTransform>();
        rectAmThanh = panelAmThanh.GetComponent<RectTransform>();

        // ===== setup vị trí =====
        viTriHien = rectCaiDat.anchoredPosition;
        viTriAn = viTriHien + new Vector2(0, 600); // trượt từ trên xuống

        rectCaiDat.anchoredPosition = viTriAn;

        panelCaiDat.SetActive(false);
        panelAmThanh.SetActive(false);

        float volume = 1f;

        sliderAmThanh.value = volume;
        sliderAmThanh.onValueChanged.AddListener(OnVolumeChanged);

        AudioListener.volume = volume;
    }

    void Update()
    {
        // ===== ANIMATION CÀI ĐẶT =====
        if (panelCaiDat.activeSelf)
        {
            rectCaiDat.anchoredPosition = Vector2.Lerp(
                rectCaiDat.anchoredPosition,
                viTriHien,
                Time.deltaTime * tocDoAnim
            );
        }

        // ===== ANIMATION ÂM THANH (SCALE) =====
        if (panelAmThanh.activeSelf)
        {
            rectAmThanh.localScale = Vector3.Lerp(
                rectAmThanh.localScale,
                Vector3.one,
                Time.deltaTime * tocDoAnim
            );
        }

        // ===== ESC =====
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DongTheoThuTu();
        }

        // ===== CLICK NGOÀI =====
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                DongTheoThuTu();
            }
        }
    }

    // ===== MỞ CÀI ĐẶT =====
    public void MoCaiDat()
    {
        panelCaiDat.SetActive(true);
        rectCaiDat.anchoredPosition = viTriAn;

        dangMoCaiDat = true;

        // 🔥 Ẩn nút mở
        if (nutMoCaiDat != null)
            nutMoCaiDat.SetActive(false);
    }

    // ===== MỞ ÂM THANH =====
    public void MoAmThanh()
    {
        panelAmThanh.SetActive(true);
        rectAmThanh.localScale = Vector3.zero;

        dangMoAmThanh = true;
    }

    // ===== ĐÓNG THEO THỨ TỰ =====
    void DongTheoThuTu()
    {
        if (panelAmThanh.activeSelf)
        {
            panelAmThanh.SetActive(false);
            return;
        }

        if (panelCaiDat.activeSelf)
        {
            panelCaiDat.SetActive(false);

            // 🔥 hiện lại nút
            if (nutMoCaiDat != null)
                nutMoCaiDat.SetActive(true);

            return;
        }
    }

    // ===== ÂM THANH =====
    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(KEY_VOLUME, value);
    }
}