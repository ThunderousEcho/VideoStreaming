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

    public string pathToFfmpegDotExe = "ffmpeg.exe";

    public int port = 44962;
    private TcpListener listenerForFfmpeg;
    private TcpClient ffmpegTcpClient;
    private NetworkStream ffmpegStream;
    private Process ffmpegProcess;

    private int outputWidth = 160;
    private int outputHeight = 90;
    const int numBufferedFrames = 256;

    private RenderTexture superIntermediateTexture;
    private Texture2D intermediateTexture;

    [HideInInspector]
    public string key;

    bool tcpWriting = false;

    List<byte> internalBuffer = new List<byte>();

    public void SetOutputResolution(int width, int height) {
        if (listenerForFfmpeg != null) { //if start has not jet been called
            outputWidth = width;
            outputHeight = height;
        } else {
            UnityEngine.Debug.LogError("The resolution to be sent to Youtube Gaming cannot be changed after Start() is called on this object.");
        }
    }

    public void SetOutputResolution(int p) { //e.g. 480p or 720p
        SetOutputResolution((p * 16) / 9, p);
    }

    void Start() {
        cam = GetComponent<Camera>();

        listenerForFfmpeg = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        listenerForFfmpeg.Start();

        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = pathToFfmpegDotExe;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = false;
        ffmpegProcess.StartInfo.Arguments = string.Format("-re -f rawvideo -pix_fmt rgb24 -s {0}x{1} -i tcp://127.0.0.1:{2}?timeout=1000000 -i in.ogg -vf vflip -c:v libx264 -preset ultrafast -maxrate 3000k -bufsize 6000k -pix_fmt yuv420p -g 50 -c:a aac -b:a 160k -ac 2 -ar 44100 -f flv rtmp://a.rtmp.youtube.com/live2/9qr1-vfkp-631c-76tu", outputWidth, outputHeight, port);
        ffmpegProcess.Start();

        intermediateTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
        superIntermediateTexture = new RenderTexture(outputWidth, outputHeight, 0);
    }

    void Update() {
        if (listenerForFfmpeg != null && listenerForFfmpeg.Pending() && ffmpegTcpClient == null) {
            ffmpegTcpClient = listenerForFfmpeg.AcceptTcpClient();
            ffmpegTcpClient.SendBufferSize = outputWidth * outputHeight * 3 * numBufferedFrames;
            ffmpegStream = ffmpegTcpClient.GetStream();
            listenerForFfmpeg.Stop();
            listenerForFfmpeg = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        Graphics.Blit(source, destination);

        if (ffmpegTcpClient == null)
            return;

        Graphics.Blit(source, superIntermediateTexture);

        RenderTexture.active = superIntermediateTexture;
        intermediateTexture.ReadPixels(new Rect(0, 0, superIntermediateTexture.width, superIntermediateTexture.height), 0, 0);
        //Apply() is not called because we don't need the information to be mirrored in graphics memory
        RenderTexture.active = null;

        byte[] frame = intermediateTexture.GetRawTextureData();
        internalBuffer.AddRange(frame);

        if (internalBuffer.Count > outputHeight * outputWidth * 3 * 60) {
            if (!tcpWriting) {
                UnityEngine.Debug.Log("Dumping buffer at time " + Time.time);
                tcpWriting = true;
                try {
                    ffmpegStream.BeginWrite(internalBuffer.ToArray(), 0, internalBuffer.Count, EndTcpWrite, ffmpegStream);
                } catch (Exception e) {
                    UnityEngine.Debug.LogError("The TCP stream to the encoder threw an error: " + e.Message);
                }
                internalBuffer.Clear();
            } else {
                //UnityEngine.Debug.Log("Delays " + Time.time);
            }
        }
    }

    void EndTcpWrite(IAsyncResult asynchResult) {
        UnityEngine.Debug.Log("End");
        //((NetworkStream)asynchResult).Flush();
        tcpWriting = false;
        UnityEngine.Debug.Log("Done");
    }

    private void OnDestroy() {
        if (ffmpegTcpClient != null)
            ffmpegTcpClient.Close();
        if (listenerForFfmpeg != null)
            listenerForFfmpeg.Stop();
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            ffmpegProcess.Kill();
    }
}
