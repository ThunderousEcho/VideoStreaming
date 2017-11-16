using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VideoStreamSource : MonoBehaviour {

    private Camera cam;
    private AudioStreamSource audioStreamSource;

    public string pathToFfmpegDotExe = "ffmpeg.exe";

    const int port = 44962;
    private Socket ffmpegListenSock;
    private Socket ffmpegSock;
    private Process ffmpegProcess;
    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

    private int outputWidth = -1;
    private int outputHeight = -1;
    private int outputFramerate = -1;

    private RenderTexture superIntermediateTexture;
    private Texture2D intermediateTexture;

    [HideInInspector]
    public string key;

    float lastFrameTime;

    public void SetOutputFramerate(int framerate) {
        if (ffmpegProcess != null) { //if OnEnable has not yet been called
            outputFramerate = framerate;
        } else {
            UnityEngine.Debug.LogError("The output framerate cannot be changed while running.");
        }
    }

    public void SetOutputResolution(int width, int height) {
        if (ffmpegProcess != null) { //if OnEnable has not yet been called
            outputWidth = width;
            outputHeight = height;
        } else {
            UnityEngine.Debug.LogError("The output resolution cannot be changed while running.");
        }
    }

    public void SetOutputResolution(int p) { //e.g. 480p, 720p, 1080p
        SetOutputResolution((p * 16) / 9, p);
    }

    public void AutoOutputResolution() {
        SetOutputResolution(Screen.width, Screen.height);
    }

    void OnEnable() {

        audioStreamSource = GetComponent<AudioStreamSource>();
        audioStreamSource.enabled = true;

        if (outputWidth <= 0)
            outputWidth = Screen.width;
        if (outputHeight <= 0)
            outputHeight = Screen.height;
        if (outputFramerate <= 0)
            outputFramerate = 30;

        cam = GetComponent<Camera>();

        ffmpegListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ffmpegListenSock.SendBufferSize = int.MaxValue;
        ffmpegListenSock.NoDelay = true;
        ffmpegListenSock.Bind(endpoint);
        ffmpegListenSock.Listen(1);
        ffmpegListenSock.BeginAccept(EndSocketAccept, ffmpegListenSock);

        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = pathToFfmpegDotExe;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = false;

        ffmpegProcess.StartInfo.Arguments = string.Format(
            "-framerate {2} -f rawvideo -pix_fmt rgb24 -s {0}x{1} -i tcp://127.0.0.1:{4} " + 
            "-ar {3} -ac 2 -f f32le -i tcp://127.0.0.1:{5} " +
            "-vf vflip -c:v libx264 -preset ultrafast -maxrate 3000k -bufsize 6000k -pix_fmt yuv420p -g 50 -c:a aac -b:a 160k -ac 2 -ar 44100 -f flv rtmp://a.rtmp.youtube.com/live2/9qr1-vfkp-631c-76tu",
            outputWidth, outputHeight, outputFramerate, AudioSettings.outputSampleRate, port, AudioStreamSource.port
        );

        ffmpegProcess.Start();

        if (intermediateTexture == null) {
            intermediateTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
            superIntermediateTexture = new RenderTexture(outputWidth, outputHeight, 0);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        Graphics.Blit(source, destination);

        if (ffmpegSock == null) {
            lastFrameTime = Time.realtimeSinceStartup;
            return;
        }

        if (Time.realtimeSinceStartup - lastFrameTime < 1f / outputFramerate)
            return;

        lastFrameTime += 1f / outputFramerate;

        Graphics.Blit(source, superIntermediateTexture);

        RenderTexture.active = superIntermediateTexture;
        intermediateTexture.ReadPixels(new Rect(0, 0, superIntermediateTexture.width, superIntermediateTexture.height), 0, 0);
        //Apply() is not called because we don't need the information to be mirrored in graphics memory
        RenderTexture.active = null;

        byte[] frame = intermediateTexture.GetRawTextureData();

        try {
            ffmpegSock.BeginSend(frame, 0, frame.Length, SocketFlags.None, EndTcpWrite, ffmpegSock);
        } catch (Exception e) {
            UnityEngine.Debug.LogError("The TCP socket to the encoder threw an error: " + e.Message);
        }
    }

    void EndTcpWrite(IAsyncResult asynchResult) { }

    void EndSocketAccept(IAsyncResult asynchResult) {
        ffmpegSock = ffmpegListenSock.EndAccept(asynchResult);
        ffmpegListenSock.Close();
        ffmpegListenSock = null;
    }

    void OnDisable() {

        audioStreamSource.enabled = false;

        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            ffmpegProcess.Kill();
        if (ffmpegSock != null)
            ffmpegSock.Close();
        ffmpegSock = null;
        if (ffmpegListenSock != null)
            ffmpegListenSock.Close();
        ffmpegListenSock = null;
    }
}