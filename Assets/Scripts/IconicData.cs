using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public sealed class IconicData : MonoBehaviour
{
    [SerializeField]
    private string _remotePath;

    [SerializeField]
    private TextAsset _dataAsset;

    [SerializeField]
    private Texture2D _dataSprite;

    public static IconicData Instance { get; private set; }

    private double _localTimestamp;

    private static readonly DateTime _origin = new DateTime(1970, 1, 1);

    public bool RemoteCheckComplete { get; private set; }

    public IconData[] Icons { get; private set; }

    public Texture2D IconSprite { get; private set; }

    public IconData GetIcon(string name)
    {
        return Icons.FirstOrDefault(icon => icon.Name == name);
    }

    private void Awake()
    {
        Instance = this;
        RemoteCheckComplete = false;
    }

    private void Log(string format, params object[] args)
    {
        Debug.LogFormat("[IconicData] {0}", string.Format(format, args));
    }

    private void Start()
    {
        LoadData(_dataAsset.text, _dataSprite, false);
        StartCoroutine(LoadRemoteData());
    }

    private IEnumerator LoadRemoteData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(Path.Combine(_remotePath, "iconic.txt")))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Log("Failed to load remote data (code {0}): {1}", request.responseCode, request.error);
                RemoteCheckComplete = true; // still complete whether or not the request was successful
                yield break;
            }

            string data = request.downloadHandler.text;

            using (UnityWebRequest spriteRequest = UnityWebRequestTexture.GetTexture(Path.Combine(_remotePath, "iconic.png")))
            {
                yield return spriteRequest.SendWebRequest();
                if (spriteRequest.isHttpError || spriteRequest.isNetworkError)
                {
                    Log("Failed to load remote sprite (code {0}): {1}", spriteRequest.responseCode, spriteRequest.error);
                    RemoteCheckComplete = true; // still complete whether or not the request was successful
                    yield break;
                }

                Texture2D sprite = DownloadHandlerTexture.GetContent(spriteRequest);

                LoadData(data, sprite, true);
                RemoteCheckComplete = true; // still complete whether or not the data is used
            }
        }
    }

    private string FormatTimestamp(double timestamp)
    {
        DateTime dateTime = _origin.AddMilliseconds(timestamp).ToLocalTime();
        return string.Format("built on {0} at {1} local time", dateTime.ToString("MMM dd, yyyy"), dateTime.ToString("T"));
    }

    private void LoadData(string data, Texture2D sprite, bool remote)
    {
        string[] parts = data.Split('#');
        double timestamp = double.Parse(parts[0]);
        if (remote)
        {
            if (timestamp <= _localTimestamp)
            {
                Log("Loaded remote data ({0}) but bundled data is up to date or newer", FormatTimestamp(timestamp));
                return;
            }
        }
        else
        {
            _localTimestamp = timestamp;
        }

        string[] dict = parts[2].Replace("{{HASH}}", "#").Split('^').Select(t => t.Replace("{{CARET}}", "^")).ToArray();
        IconData[] icons = parts[1].Split('^').Select(d => new IconData(d, dict)).ToArray();

        Icons = icons;
        IconSprite = sprite;
        IconSprite.filterMode = FilterMode.Point;

        Log((remote ? "Replaced bundled data with remote data" : "Loaded bundled data") + " ({0} icons, {1})", Icons.Length, FormatTimestamp(timestamp));
    }
}
