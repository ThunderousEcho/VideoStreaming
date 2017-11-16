using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class AudioStreamSource : MonoBehaviour {

    AudioSource source;

    public const int port = 44963;
    private Socket ffmpegListenSock;
    private Socket ffmpegSock;
    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

    float[] samples;

    public void OnEnable() {
        if (source == null)
            source = GetComponent<AudioSource>();

        ffmpegListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ffmpegListenSock.SendBufferSize = int.MaxValue;
        ffmpegListenSock.NoDelay = true;
        ffmpegListenSock.Bind(endpoint);
        ffmpegListenSock.Listen(1);
        ffmpegListenSock.BeginAccept(EndSocketAccept, ffmpegListenSock);
    }

    void EndSocketAccept(IAsyncResult asynchResult) {
        ffmpegSock = ffmpegListenSock.EndAccept(asynchResult);
        ffmpegListenSock.Close();
        ffmpegListenSock = null;
    }

    void OnAudioFilterRead(float[] data, int channels) {
        if (ffmpegSock == null)
            return;
        byte[] output = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, output, 0, output.Length);
        ffmpegSock.BeginSend(output, 0, output.Length, SocketFlags.None, EndSend, ffmpegSock);
    }

    void EndSend(IAsyncResult asyncResult) { }

    public void OnDisable() {
        if (ffmpegSock != null)
            ffmpegSock.Close();
        ffmpegSock = null;
        if (ffmpegListenSock != null)
            ffmpegListenSock.Close();
        ffmpegListenSock = null;
    }
}
