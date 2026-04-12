public class MusicLibraryTest {
    [Fact]
    public void TestFillTrackCount() {
        var lib = new MusicLibrary {
            Tracks = [
                new MusicTrack { AlbumArtist = "A", AlbumTitle = "A", TrackNumber = 1, DiscNumber = 1 }, // AlbumArtist, AlbumTitle の同一性
                new MusicTrack { AlbumArtist = "A", AlbumTitle = "A", TrackNumber = 2, DiscNumber = 1 }, // AlbumArtist, AlbumTitle の同一性
                new MusicTrack { AlbumArtist = "A", AlbumTitle = "B", TrackNumber = 1, DiscNumber = 1 }, // AlbumArtist, AlbumTitle の同一性
                new MusicTrack { AlbumArtist = "B", AlbumTitle = "A", TrackNumber = 1, DiscNumber = 1 }, // AlbumArtist, AlbumTitle の同一性
                new MusicTrack { AlbumArtist = "C", AlbumTitle = "C", TrackNumber = 1, DiscNumber = 1 }, // DiscNumber が別
                new MusicTrack { AlbumArtist = "C", AlbumTitle = "C", TrackNumber = 2, DiscNumber = 1 }, // DiscNumber が別
                new MusicTrack { AlbumArtist = "C", AlbumTitle = "C", TrackNumber = 1, DiscNumber = 2 }, // DiscNumber が別
                new MusicTrack { AlbumArtist = "C", AlbumTitle = "C", TrackNumber = 2, DiscNumber = 2 }, // DiscNumber が別
            ]
        };

        lib.FillTrackCount();

        Assert.Equal(2, lib.Tracks[0].TrackCount);
        Assert.Equal(2, lib.Tracks[1].TrackCount);
        Assert.Equal(1, lib.Tracks[2].TrackCount);
        Assert.Equal(1, lib.Tracks[3].TrackCount);
        Assert.Equal(2, lib.Tracks[4].TrackCount);
        Assert.Equal(2, lib.Tracks[5].TrackCount);
        Assert.Equal(2, lib.Tracks[6].TrackCount);
        Assert.Equal(2, lib.Tracks[7].TrackCount);
    }
}
