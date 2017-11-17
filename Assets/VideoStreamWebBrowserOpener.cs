﻿
public static class VideoStreamWebBrowserOpener {

    /// <summary>
    ///     Opens the system's default web browser with a url pointing to the youtube video/stream with the given id.
    /// </summary>
    /// <param name="videoId">The id of the video/stream to be opened.</param>
	public static void Open (string videoId) {
        System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=" + videoId);
    }
}
