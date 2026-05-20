using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CameraStuff : MonoBehaviour
{
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private RawImage thumbnailDisplay;
    [SerializeField] private Button captureButton;
    [SerializeField] private bool useFrontCamera;

    private WebCamTexture _webCamTexture;

    private IEnumerator Start()
    {
        captureButton.onClick.AddListener(CapturePhoto);
        yield return RequestCameraPermission();
    }

    private void CapturePhoto()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;

        Texture2D photo = new Texture2D(
            _webCamTexture.width,
            _webCamTexture.height,
            TextureFormat.RGBA32,
            false);
        
        photo.SetPixels(_webCamTexture.GetPixels());
        photo.Apply();

        thumbnailDisplay.texture = photo;        
        thumbnailDisplay.gameObject.SetActive(true);

        SavetoGallery(photo);
    }

    private void SavetoGallery(Texture2D photo)
    {
        string filename = $"capture_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        System.IO.File.WriteAllBytes(path, photo.EncodeToPNG());
        
#if UNITY_ANDROID
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        using var intent = new AndroidJavaObject("android.content.Intent","android.intent.action.MEDIA_SCANNER_SCAN_FILE");
        using var uri =
            new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "file://" + path);
        intent.Call<AndroidJavaObject>("setData", uri);
        activity.Call("sendBroadcast", intent);
#endif
    }

    private IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogWarning("Camera permission denied");
            yield break;
        }

        InitializeCamera();
    }

    private void InitializeCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogWarning("No camera devices found");
            return;
        }

        string cameraName = null;
        foreach (var device in devices)
        {
            if (device.isFrontFacing == useFrontCamera)
            {
                cameraName = device.name;
                break;
            }
        }

        cameraName ??= devices[0].name;
        _webCamTexture = new WebCamTexture(cameraName, 1080, 1920, 30);
        
        cameraDisplay.texture = _webCamTexture;
        _webCamTexture.Play();

        StartCoroutine(AdjustDisplayAfterFrame());
    }

    private IEnumerator AdjustDisplayAfterFrame()
    {
        yield return null;

        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -_webCamTexture.videoRotationAngle);

        if (useFrontCamera)
        {
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(-1, -1, 0);
        }
    }
}