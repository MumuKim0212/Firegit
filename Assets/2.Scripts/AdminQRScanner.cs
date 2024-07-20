using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class AdminQRScanner : MonoBehaviour
{
    [SerializeField] private Text resultText;
    public RawImage cameraFeed;
    public Dictionary<string, GameObject> menu;

    private bool isCameraRunning = false;
    private WebCamTexture camTexture;
    private GameObject DBManager;
    private string data;
    private bool isScanning = false; // ��ĵ �󵵸� �����ϱ� ���� ����
    public GameObject extinguisher;

    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                yield break;
            }

            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    camTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                    break;
                }
            }

            if (camTexture == null)
            {
                yield break;
            }

            // ī�޶� ȭ���� �����ϱ� ���� RenderTexture�� ���
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            cameraFeed.texture = renderTexture;
            cameraFeed.material.mainTexture = camTexture;

            camTexture.Play();
            isCameraRunning = true;
        }
    }


    void Update()
    {
        if (isCameraRunning && camTexture.isPlaying && !isScanning)
        {
            StartCoroutine(ScanQRCode());
        }
    }
    IEnumerator ScanQRCode()
    {
        isScanning = true; // ��ĵ ������ ǥ��
        yield return new WaitForSeconds(1f); // 1�� ���

        IBarcodeReader barcodeReader = new BarcodeReader();
        Result result = barcodeReader.Decode(camTexture.GetPixels32(), camTexture.width, camTexture.height);
        if (result != null)
        {
            data = result.ToString();
            menu[data].SetActive(true);
            StopAllCoroutines();
        }
        else if (data != "")
        {
            StartCoroutine(ClosePanel());
        }

        isScanning = false; // ��ĵ �Ϸ� ǥ��
    }
    private IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(2f);
        menu[data].SetActive(false);
        data = "";
    }
    public void SendData()
    {
        // string data = result.ToString();
        DBManager.GetComponent<DBManager>().LoadData(data);

    }
    public void OpenExtinguisher()
    {
        extinguisher.SetActive(true);
    }

    void OnDisable()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
            isCameraRunning = false;
        }
    }
}