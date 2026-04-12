using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public class MusicLibrary {
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
        });
    }

    /// <summary>
    ///     Stream から MusicLibrary のインスタンスを作成する
    /// </summary>
    public static MusicLibrary? FromJSONReader(Stream r) {
        try {
            var l = JsonSerializer.Deserialize<MusicLibrary>(r);
            if (l == null) {
                return null;
            }

            return l;
        } catch (Exception) {
            return null;
        }
    }
}
