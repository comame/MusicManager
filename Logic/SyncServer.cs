using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager.Logic;

class SyncServer {
    private readonly HttpListener listener;
    private readonly MusicLibrary library;

    public SyncServer(MusicLibrary library) {
        this.library = library;

        listener = new();
        // netsh http add urlacl url=http://*:9000/ user=comame
        listener.Prefixes.Add("http://*:9000/");
        listener.IgnoreWriteExceptions = true;
    }

    public async void Listen() {
        listener.Start();

        while (listener.IsListening) {
            try {
                var ctx = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(ctx));
            } catch (HttpListenerException) {
                break;
            } catch (ObjectDisposedException) {
                break;
            }
        }
    }

    public void Stop() {
        listener.Stop();
    }

    private void HandleRequest(HttpListenerContext ctx) {
        var req = ctx.Request;
        var res = ctx.Response;

        if (req.HttpMethod != "GET") {
            res.StatusCode = (int)HttpStatusCode.NotFound;
            res.OutputStream.Close();
            return;
        }

        Debug.WriteLine($"request {req.Url.AbsolutePath}");

        try {
            if (req.Url.AbsolutePath == "/library.json") {
                RespondWithFile(res, MusicIndexer.IndexFilePath);
                return;
            }
            if (req.Url.AbsolutePath.StartsWith("/track/")) {
                var persistentID = req.Url.AbsolutePath[7..];
                Debug.WriteLine($"persistentID {persistentID}");
                RespondWithMusicTrackFile(res, persistentID);
                return;
            }

            throw new FileNotFoundException();
        } catch (FileNotFoundException) {
            res.StatusCode = (int)HttpStatusCode.NotFound;
            res.OutputStream.Close();
            return;
        }
    }

    private void RespondWithMusicTrackFile(HttpListenerResponse res, string persistentID) {
        var track = library.Tracks.Find(t => t.PersistentID == persistentID);
        if (track == null) {
            throw new FileNotFoundException();
        }

        RespondWithFile(res, track.Path);
    }

    private static void RespondWithFile(HttpListenerResponse res, string filePath) {
        using var f = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var contentType = "application/octet-stream";
        switch (Path.GetExtension(filePath).ToLower()) {
            case ".json":
                contentType = "application/json";
                break;
            case ".m4a":
                contentType = "audio/mp4";
                break;
            case ".mp3":
                contentType = "audio/mpeg";
                break;
        }

        res.Headers.Add("Content-Type", contentType);
        res.ContentLength64 = f.Length;
        f.CopyTo(res.OutputStream);

        //var buf = new byte[64 * 1024];
        //while (true) {
        //    var n = f.Read(buf);
        //    if (n == 0) {
        //        break;
        //    }
        //    res.OutputStream.Write(buf, 0, n);
        //}

        res.OutputStream.Close();
    }
}