using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


/// <summary>
/// Incomplete. Do not use.
/// </summary>
public class VideoStreamDestination : MonoBehaviour {

    public string pathToFfmpegDotExe = "ffmpeg.exe";

    const int port = 44964;

    private Socket ffmpegListenSock;

    private Socket ffmpegSock;

    private Process ffmpegProcess;

    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

    private int inputWidth = -1;

    private int inputHeight = -1;

    byte[] buf;

    Queue<Texture2D> frames = new Queue<Texture2D>();

    public Material mat;

    public void SetInputResolution(int width, int height) {
        if (ffmpegProcess != null) {
            inputWidth = width;
            inputHeight = height;
        } else {
            UnityEngine.Debug.LogError("The input resolution cannot be changed while running.");
        }
    }

    public void SetInputResolution(int p) {
        SetInputResolution((p * 16) / 9, p);
    }

    public void AutoInputResolution() {
        SetInputResolution(Screen.width, Screen.height);
    }

    void OnEnable() {

        if (inputWidth <= 0)
            inputWidth = 854;
        if (inputHeight <= 0)
            inputHeight = 480;

        ffmpegListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ffmpegListenSock.Bind(endpoint);
        ffmpegListenSock.Listen(1);
        ffmpegListenSock.BeginAccept(EndSocketAccept, ffmpegListenSock);

        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = pathToFfmpegDotExe;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = false;

        ffmpegProcess.StartInfo.Arguments = string.Format(
            "-i in.webm" +
            "-f rawvideo -pix_fmt rgb24 -s {0}x{1} tcp://127.0.0.1:{2}",
            inputWidth, inputHeight, port
        );

        buf = new byte[inputWidth * inputHeight * 3];

        try {
            ffmpegProcess.Start();
        } catch (Exception e) {
            UnityEngine.Debug.LogError("The ffmpeg Process could not be started: " + e.Message);
        }
    }

    void EndSocketAccept(IAsyncResult asynchResult) {
        ffmpegSock = ffmpegListenSock.EndAccept(asynchResult);
        ffmpegListenSock.Close();
        ffmpegListenSock = null;
        ffmpegSock.SendBufferSize = int.MaxValue;
        ffmpegSock.NoDelay = true;

        ffmpegSock.BeginReceive(buf, 0, buf.Length, SocketFlags.None, EndTcpRead, ffmpegSock);
    }

    void EndTcpRead(IAsyncResult asynchResult) {
        var frame = new Texture2D(inputWidth, inputHeight, TextureFormat.RGB24, false);
        frame.LoadRawTextureData(buf);
        ffmpegSock.BeginReceive(buf, 0, buf.Length, SocketFlags.None, EndTcpRead, ffmpegSock);
        //frames.Enqueue(frame);
        mat.mainTexture = frame;
    }
}
