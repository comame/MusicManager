using System.Text.Encodings.Web;
using System.Text.Json;

public class MusicLibrary {

    private string _libraryPath = "";

    /// <summary>
    /// library.jsonの存在するディレクトリのフルパス
    /// </summary>
    public string LibraryPath {
        get {
            if (_libraryPath == "") {
                throw new InvalidOperationException("LibraryPath is not set");
            }
            return _libraryPath;
        }
        set => _libraryPath = value;
    }

    public string IndexFilePath {
        get => LibraryPath + "\\library.json";
    }

    public string ITLFilePath {
        get => LibraryPath + "\\iTunes Music Library.xml";
    }

    /// <summary>
    /// この MusicLibrary が生成された日時
    /// </summary>
    public DateTime Generated { get; set; } = DateTime.Now;
    public List<MusicTrack> Tracks { get; set; } = [];

    /// <summary>
    /// このライブラリの TrackCount を埋める。
    /// </summary>
    public void FillTrackCount() {
        // アルバムキー -> tracks のインデックス
        Dictionary<string, List<int>> albums = [];

        // 楽曲をアルバムごとにまとめる
        // アルバムアーティストとアルバム名が一致したら、同一アルバムとみなす
        for (var i = 0; i < Tracks.Count; i++) {
            var m = Tracks[i];
            var albumKey = $"{m.AlbumArtist} - {m.AlbumTitle}";
            if (!albums.ContainsKey(albumKey)) {
                albums[albumKey] = [];
            }
            albums[albumKey].Add(i);
        }

        // TrackCount を DiscNumber ごとに集計する
        foreach (var key in albums.Keys) {
            var trackCountOfDisc = new Dictionary<int, int>(); // DiscNumber -> TrackCount
            foreach (var index in albums[key]) {
                var m = Tracks[index];
                var tn = m.DiscNumber;
                if (!trackCountOfDisc.ContainsKey(tn)) {
                    trackCountOfDisc[tn] = 1;
                    continue;
                }
                trackCountOfDisc[tn]++;
            }

            foreach (var index in albums[key]) {
                Tracks[index].TrackCount = trackCountOfDisc[Tracks[index].DiscNumber];
            }
        }
    }

    public void SortByImportedDate() {
        Tracks.Sort((a, b) => a.Imported.CompareTo(b.Imported));
    }

    /// <summary>
    /// Stream にこの MusicLibrary を書き出す
    /// </summary>
    public void WriteJSON(Stream w) {
        JsonSerializer.Serialize(w, this, new JsonSerializerOptions {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    /// <summary>
    ///     Stream から MusicLibrary のインスタンスを作成する
    /// </summary>
    public static MusicLibrary? FromJSONReader(Stream r, string libraryPath) {
        try {
            var l = JsonSerializer.Deserialize<MusicLibrary>(r);
            if (l == null) {
                return null;
            }
            l.LibraryPath = libraryPath;

            return l;
        }
        catch (Exception) {
            return null;
        }
    }

    /// <summary>
    /// MusicTrackのフルパスを取得する
    /// </summary>
    public string GetTrackFileFullPath(in MusicTrack track) {
            var parentUri = new Uri(LibraryPath.EndsWith("\\") ? LibraryPath : LibraryPath + "\\");
        var fullUri = new Uri(parentUri, track.Path);
        return fullUri.LocalPath;
    }

    /// <summary>
    /// MusicTrackの相対パスを求める
    /// </summary>
    public string GetTrackRelativePath(string trackFullPath) {
        var fullUri = new Uri(trackFullPath);
        var parentUri = new Uri(LibraryPath.EndsWith("\\") ? LibraryPath : LibraryPath + "\\");
        var relativeUri = parentUri.MakeRelativeUri(fullUri);
        return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
    }

    /// <summary>
    /// ライブラリに存在しないファイルのリストを返す
    /// </summary>
    public List<string> GetUntrackedFiles(in List<string> allFiles) {
        var ret = new List<string>();
        var trackPaths = new HashSet<string>(Tracks.Select(t => t.Path));
        foreach (var f in allFiles) {
            var relativePath = GetTrackRelativePath(f);
            if (!trackPaths.Contains(relativePath)) {
                ret.Add(f);
            }
        }
        return ret;
    }

    /// <summary>
    /// ライブラリには記録されているがファイルが存在しないトラックを削除する
    /// </summary>
    public void TrimRemovedTracks(in List<string> allFiles) {
        var allFilesPaths = new HashSet<string>();

        foreach (var file in allFiles) {
            var relativePath = GetTrackRelativePath(file);
            allFilesPaths.Add(relativePath);
        }

        for (var i = Tracks.Count - 1; i >= 0; i--) {
            if (allFilesPaths.Contains(Tracks[i].Path)) {
                continue;
            }

            Tracks.RemoveAt(i);
        }
    }
}
