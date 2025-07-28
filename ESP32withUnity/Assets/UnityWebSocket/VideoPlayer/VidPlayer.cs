using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class VidPlayer : MonoBehaviour
{
    [SerializeField] private string _serverIp = "127.0.0.1";
    [SerializeField] private int _serverPort = 48654;
    [Header("Video Size")]
    [SerializeField] private int _width = 1024;
    [SerializeField] private int _height = 768;

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private Texture2D _texture;
    private RenderTexture _renderTexture;
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _tcpClient = new TcpClient(_serverIp, _serverPort);
        _networkStream = _tcpClient.GetStream();
        _texture = new Texture2D(1, 1);
        _renderTexture = new RenderTexture(_width, _height, 0);
        _renderer.material.mainTexture = _renderTexture;

        Debug.Log("Connected to server");
    }

    private async void Update()
    {
        if (_networkStream != null && _networkStream.DataAvailable)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1 MB buffer
            int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                {
                    _texture.LoadImage(ms.ToArray());
                    Graphics.Blit(_texture, _renderTexture);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        _networkStream?.Close();
        _tcpClient?.Close();
    }
}
