using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using ZXing;
using System.Collections;
using System.Collections.Generic;

public class AndroidQRScanner : MonoBehaviour
{
    [SerializeField] private Text resultText;
    public RawImage cameraFeed;
    private bool isCameraRunning = false;
    private WebCamTexture camTexture;
    private GameObject DBManager;
    public Dictionary<string, GameObject> menu;
    private string data;
    private bool isScanning = false; // ��ĵ �󵵸� �����ϱ� ���� ����
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
            //RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            cameraFeed.texture = camTexture;
            //cameraFeed.material.mainTexture = camTexture;

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

    void OnDisable()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
            isCameraRunning = false;
        }
    }
}