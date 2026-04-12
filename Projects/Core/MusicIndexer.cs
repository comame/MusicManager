using System.Runtime.Versioning;

public class MusicIndexer {
    /// <summary>
    /// ライブラリの存在するディレクトリ (library.jsonが存在するディレクトリ) から MusicLibrary を読み込む
    /// </summary>
    public static MusicLibrary? LoadFromIndexFile(string libraryPath) {
        try {
            using var f = new FileStream(IndexFilePath(libraryPath), FileMode.Open, FileAccess.Read);
            var library = MusicLibrary.FromJSONReader(f);
            return library;
        }
        catch (FileNotFoundException) {
            return null;
        }
    }

    public static int CountMusicFiles(string searchDirectory) {
        var files = FindMusicFiles(searchDirectory);
        return files.Count;
    }

    [SupportedOSPlatform("windows")]
    public static MusicLibrary? UpdateIndex(
        string libraryPath,
        Action<double> onProgress,
        in CancellationToken ctx
    ) {
        var files = FindMusicFiles(libraryPath);
        if (files.Count == 0) {
            return null;
        }

        // インデックスし直すので、ライブラリは新規作成してよい
        var library = new MusicLibrary();

        for (var i = 0; i < files.Count; i++) {
            if (ctx.IsCancellationRequested) {
                return null;
            }

            var file = files[i];
            var meta = GetMusicMetadata(file, libraryPath);
            // Path はそうそう変わらないので、persistentID は Path から生成する
            meta.PersistentID = ITLUtil.CalculatePersistentID(meta.Path);
            library.Tracks.Add(meta);

            if (i % 30 == 0) {
                onProgress((double)i / files.Count * 100);
            }
        }

        library.FillTrackCount();
        library.SortByImportedDate();

        using var f = new FileStream(IndexFilePath(libraryPath), FileMode.Create, FileAccess.Write);
        library.WriteJSON(f);
        f.Flush();

        return library;
    }

    /// <summary>
    ///
    /// </summary>
    public static void GenerateITLFile(in MusicLibrary library, string libraryPath) {
        using var f = new StreamWriter(ITLFilePath(libraryPath), append: false);

        ITLUtil.WriteLibraryXMLHeader(f);
        for (var i = 0; i < library.Tracks.Count; i++) {
            var t = ITLTrack.FromMusicMetadata(
                library.Tracks[i],
                i,
                ConvertRelativePathToWindowsFullPath(library.Tracks[i].Path, libraryPath)
            );
            t.WriteTo(f);
        }
        ITLUtil.WriteLibraryXMLFooter(f, libraryPath);

        f.Flush();
    }

    /// <summary>
    /// 指定したディレクトリ内の音楽ファイルのフルパスのリストを返す。
    /// </summary>
    private static List<string> FindMusicFiles(string directory) {
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".m4a" };
        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(file => extensions.Contains(Path.GetExtension(file)))
            .ToList();
        return files;
    }

    /// <summary>
    /// 指定した音楽ファイルのメタデータを取得する。ファイル単体から推測できない、TrackNumber は取得しない。
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static MusicTrack GetMusicMetadata(string fullPath, string libraryPath) {
        using var ps = PropertyStore.Open(fullPath);

        var m = new MusicTrack() {
            // タグ
            Name = ps.GetString(NativePropertySystem.PKEY_Title),
            AlbumArtist = ps.GetString(NativePropertySystem.PKEY_Music_AlbumArtist),
            AlbumTitle = ps.GetString(NativePropertySystem.PKEY_Music_AlbumTitle),
            Artists = ps.GetStringList(NativePropertySystem.PKEY_Music_Artist),
            Genre = ps.GetStringList(NativePropertySystem.PKEY_Music_Genre),
            Year = (int)ps.GetUInt(NativePropertySystem.PKey_Media_Year),
            TrackNumber = (int)ps.GetUInt(NativePropertySystem.PKEY_Music_TrackNumber),
            DurationMilliSeconds = ps.GetUlong(NativePropertySystem.PKey_Media_Duration) / 10_000,
            // DiscNumber は PartOfSet から取得する

            // オーディオ
            Format = Path.GetExtension(fullPath).ToLowerInvariant() switch {
                ".mp3" => "mp3",
                ".m4a" => "m4a",
                _ => throw new Exception("拡張子が未知"),
            },
            Channels = (int)ps.GetUInt(NativePropertySystem.PKEY_Audio_ChannelCount),
            IsVBR = ps.GetBool(NativePropertySystem.PKEY_Audio_IsVariableBitRate),
            SampleRate = ps.GetUInt(NativePropertySystem.PKEY_Audio_SampleRate),
            Bitrate = ps.GetUInt(NativePropertySystem.PKEY_Audio_EncodingBitrate),
            Imported = ps.GetDateTime(NativePropertySystem.PKey_DateImported), // コンテンツの作成日; おおむね追加日として使用する

            // ファイル
            Path = ConvertWindowsFullPathToRelativePath(fullPath, libraryPath),
            Modified = ps.GetDateTime(NativePropertySystem.PKEY_DateModified),
            Created = ps.GetDateTime(NativePropertySystem.PKEY_DateCreated),
            SizeBytes = ps.GetUlong(NativePropertySystem.PKEY_Size),
        };

        // DiscNumeber, DiscCount を PartOfSet から取得する
        var partOfSet = ps.GetString(NativePropertySystem.PKEY_Music_PartOfSet);
        var partOfSetSplit = partOfSet.Split('/');
        if (partOfSetSplit.Length == 2) {
            if (int.TryParse(partOfSetSplit[0], out int discNumber)) {
                m.DiscNumber = discNumber;
            }
            if (int.TryParse(partOfSetSplit[1], out int discCount)) {
                m.DiscCount = discCount;
            }
        }

        return m;
    }

    public static string IndexFilePath(string libraryPath) => libraryPath + "\\library.json";
    public static string ITLFilePath(string libraryPath) => libraryPath + "\\iTunes Music Library.xml";

    /// <summary>
    ///  Windowsのフルパスを相対パスに変換する
    /// </summary>
    public static string ConvertWindowsFullPathToRelativePath(string full, string parent) {
        var fullUri = new Uri(full);
        var parentUri = new Uri(parent.EndsWith("\\") ? parent : parent + "\\");
        var relativeUri = parentUri.MakeRelativeUri(fullUri);
        return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
    }

    /// <summary>
    /// 相対パスを Windows のフルパスに変換する
    /// </summary>
    public static string ConvertRelativePathToWindowsFullPath(string relative, string parent) {
        var parentUri = new Uri(parent.EndsWith("\\") ? parent : parent + "\\");
        var fullUri = new Uri(parentUri, relative);
        return fullUri.LocalPath;

    }
}
