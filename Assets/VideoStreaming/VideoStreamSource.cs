using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Uses ffmpeg to stream video data from a Camera to YouTube Live
/// </summary>
[RequireComponent(typeof(Camera))]
public class VideoStreamSource : MonoBehaviour {

    /// <summary>The component responsible for sending audio information to ffmpeg.</summary>
    private AudioStreamSource audioStreamSource;

    /// <summary>The path to ffmpeg.exe.</summary>
    public string pathToFfmpegDotExe = "ffmpeg.exe";

    /// <summary>The port used when sending video information to ffmpeg.</summary>
    const int port = 44962;

    /// <summary>The socket used to listen for a connection from ffmpeg.</summary>
    private Socket ffmpegListenSock;

    /// <summary>The socket used to send video information to ffmpeg.</summary>
    private Socket ffmpegSock;

    /// <summary>The Process within which ffmpeg is run.</summary>
    private Process ffmpegProcess;

    /// <summary>The endpoint used to listen for a connection from ffmpeg.</summary>
    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

    /// <summary>The width dimension of the video information to be sent to ffmpeg.</summary>
    private int outputWidth = -1;

    /// <summary>The height dimension of the video information to be sent to ffmpeg.</summary>
    private int outputHeight = -1;

    /// <summary>The framerate at which to capture video information and send it to ffmpeg.</summary>
    private int outputFramerate = -1;

    /// <summary>Used to copy from the image rendered by the camera (step 1) to step3Texture.</summary>
    private RenderTexture step2Texture;

    /// <summary>Used to copy from step2Texture to the raw serial video information that gets sent to ffmpeg (step 4).</summary>
    private Texture2D step3Texture;

    /// <summary>The YouTube streaming key.</summary>
    [HideInInspector]
    public string key;

    /// <summary>The last time a frame was sent to ffmpeg. Used to keep a consistent framerate.</summary>
    float lastFrameTime;

    /// <summary>
    /// Sets the rate at which to capture video information and send it to ffmpeg. Cannot be called while the stream is running.
    /// </summary>
    /// <param name="framerate">The desired framerate, in frames per second.</param>
    public void SetOutputFramerate(int framerate) {
        if (ffmpegProcess != null) {
            outputFramerate = framerate;
        } else {
            UnityEngine.Debug.LogError("The output framerate cannot be changed while running.");
        }
    }

    /// <summary>
    /// Sets the resolution of the video data to be sent to ffmpeg. Cannot be called while the stream is running.
    /// </summary>
    /// <param name="width">Video frame width, in pixels.</param>
    /// <param name="height">Video frame height, in pixels.</param>
    public void SetOutputResolution(int width, int height) {
        if (ffmpegProcess != null) {
            outputWidth = width;
            outputHeight = height;
        } else {
            UnityEngine.Debug.LogError("The output resolution cannot be changed while running.");
        }
    }

    /// <summary>
    /// Sets the resolution of the video data to be sent to ffmpeg. Cannot be called while the stream is running.
    /// </summary>
    /// <param name="p">Video frame height, in pixels. E.g. 480p, 720p, or 1080p.</param>
    public void SetOutputResolution(int p) {
        SetOutputResolution((p * 16) / 9, p);
    }

    /// <summary>
    /// Sets the output resolution to the size of the screen. Cannot be called while the stream is running.
    /// </summary>
    public void AutoOutputResolution() {
        SetOutputResolution(Screen.width, Screen.height);
    }

    /// <summary>
    /// Starts the stream.
    /// </summary>
    void OnEnable() {

        //start the component responsible for streaming audio
        audioStreamSource = GetComponent<AudioStreamSource>();
        audioStreamSource.enabled = true;

        //ensure that the output parameters are valid
        if (outputWidth <= 0)
            outputWidth = 854;
        if (outputHeight <= 0)
            outputHeight = 480;
        if (outputFramerate <= 0)
            outputFramerate = 30;

        //create a socket to listen for a connection from ffmpeg
        ffmpegListenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ffmpegListenSock.Bind(endpoint);
        ffmpegListenSock.Listen(1);
        ffmpegListenSock.BeginAccept(EndSocketAccept, ffmpegListenSock);

        //start ffmpeg
        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = pathToFfmpegDotExe;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = false;
        ffmpegProcess.StartInfo.Arguments = string.Format(
            "-framerate {2} -f rawvideo -pix_fmt rgb24 -s {0}x{1} -i tcp://127.0.0.1:{4} " + //video input
            "-ar {3} -ac 2 -f f32le -i tcp://127.0.0.1:{5} " + //audio input
            "-vf vflip -c:v libx264 -preset ultrafast -maxrate 3000k -bufsize 6000k -pix_fmt yuv420p -g 50 -c:a aac -b:a 160k -ac 2 -ar 44100 -f flv rtmp://a.rtmp.youtube.com/live2/{6}?timeout=10000000", //video and audio output
            outputWidth, outputHeight, outputFramerate, AudioSettings.outputSampleRate, port, AudioStreamSource.port, key
        );
        try {
            ffmpegProcess.Start();
        } catch (Exception e) {
            UnityEngine.Debug.LogError("The ffmpeg Process could not be started: " + e.Message);
        }

        //prepare the intermediate textures used to help transfer information from the Camera to ffmpeg.
        step3Texture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
        step2Texture = new RenderTexture(outputWidth, outputHeight, 0);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        //always allow the texture to continue to the screen
        Graphics.Blit(source, destination);

        //don't execute the rest of this function if the stream isn't running
        if (ffmpegSock == null) {
            lastFrameTime = Time.realtimeSinceStartup;
            return;
        }

        //don't execute the rest of this function if it's not time for the next frame yet
        if (Time.realtimeSinceStartup - lastFrameTime < 1f / outputFramerate)
            return;

        //maintain a constant framerate
        lastFrameTime += 1f / outputFramerate;

        //copy the step 1 (source) texture to step 2
        Graphics.Blit(source, step2Texture);

        //copy the step 2 texture to step 3
        RenderTexture.active = step2Texture;
        step3Texture.ReadPixels(new Rect(0, 0, step2Texture.width, step2Texture.height), 0, 0); //Apply() is not called because we don't need the information to be mirrored in graphics memory
        RenderTexture.active = null;

        //copy the step 3 texture to step 4 (raw serialized frame)
        byte[] rawSerializedFrame = step3Texture.GetRawTextureData();

        //send the frame to ffmpeg
        try {
            ffmpegSock.BeginSend(rawSerializedFrame, 0, rawSerializedFrame.Length, SocketFlags.None, EndTcpWrite, ffmpegSock);
        } catch (Exception e) {
            UnityEngine.Debug.LogError("The TCP socket to the encoder threw an error: " + e.Message);
        }
    }

    /// <summary> Used because ffmpegSock needs to call a function when it's done writing.</summary>
    void EndTcpWrite(IAsyncResult asynchResult) { }

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
    /// Stops the stream, freeing all resources.
    /// </summary>
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
        if (step3Texture != null) {
            step3Texture = null;
            step2Texture.DiscardContents();
            step2Texture = null;
        }
    }
}