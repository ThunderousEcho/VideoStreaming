using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Uses ffmpeg and VideoStreamSource to stream audio data from an AudioListener to YouTube Live
/// </summary>
[RequireComponent(typeof(AudioListener))]
public class AudioStreamSource : MonoBehaviour {

    /// <summary>The port used when sending audio information to ffmpeg.</summary>
    public const int port = 44963;

    /// <summary>The socket used to listen for a connection from ffmpeg.</summary>
    private Socket ffmpegListenSock;

    /// <summary>The socket used to send audio information to ffmpeg.</summary>
    private Socket ffmpegSock;

    /// <summary>The endpoint used to listen for a connection from ffmpeg.</summary>
    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

    /// <summary>The endpoint used to listen for a connection from ffmpeg.</summary>
    float[] samples;

    /// <summary>
    /// Starts sending audio data to ffmpeg.
    /// </summary>
    public void OnEnable() {
        ffmpegListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ffmpegListenSock.Bind(endpoint);
        ffmpegListenSock.Listen(1);
        ffmpegListenSock.BeginAccept(EndSocketAccept, ffmpegListenSock);
    }

    /// <summary>
    /// Accepts and prepares the TCP connection to ffmpeg.
    /// </summary>
    void EndSocketAccept(IAsyncResult asynchResult) {
        ffmpegSock = ffmpegListenSock.EndAccept(asynchResult);
        ffmpegListenSock.Close();
        ffmpegListenSock = null;
        ffmpegSock.SendBufferSize = int.MaxValue;
        ffmpegSock.NoDelay = true;
    }

    /// <summary>
    /// Captures audio data from the Audio Source and sends it to ffmpeg.
    /// </summary>
    void OnAudioFilterRead(float[] data, int channels) {
        if (ffmpegSock == null)
            return;
        byte[] output = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, output, 0, output.Length);
        ffmpegSock.BeginSend(output, 0, output.Length, SocketFlags.None, EndTcpWrite, ffmpegSock);
    }

    /// <summary> Used because ffmpegSock needs to call a function when it's done writing.</summary>
    void EndTcpWrite(IAsyncResult asyncResult) { }

    /// <summary>
    /// Stops sending audio data to ffmpeg, freeing all resources.
    /// </summary>
    public void OnDisable() {
        if (ffmpegSock != null)
            ffmpegSock.Close();
        ffmpegSock = null;
        if (ffmpegListenSock != null)
            ffmpegListenSock.Close();
        ffmpegListenSock = null;
    }
}
