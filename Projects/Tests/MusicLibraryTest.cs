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

    [Fact]
    public void TestGetTrackRelativePath() {
        var cases = new List<(string, string)> {
            (@"C:\Folder\file.mp3", @"file.mp3"),
            (@"C:\Folder\Subfolder\file.mp3", @"Subfolder\file.mp3"),
            (@"C:\Folder\Subfolder\Another Folder\file.mp3", @"Subfolder\Another Folder\file.mp3"),
        };
        foreach (var (full, expected) in cases) {
            var library1 = new MusicLibrary { LibraryPath = @"C:\Folder" };
            var actual = library1.GetTrackRelativePath(full);
            Assert.Equal(expected, actual);

            var library2 = new MusicLibrary { LibraryPath = @"C:\Folder\" };
            var actual2 = library2.GetTrackRelativePath(full);
            Assert.Equal(expected, actual2);
        }
    }

    [Fact]
    public void TestGetTrackFileFullPath() {
        var cases = new List<(string, string)> {
            (@"file.mp3", @"C:\Folder\file.mp3"),
            (@"Subfolder\file.mp3", @"C:\Folder\Subfolder\file.mp3"),
            (@"Subfolder\Another Folder\file.mp3", @"C:\Folder\Subfolder\Another Folder\file.mp3"),
        };
        foreach (var (relative, expected) in cases) {
            var library1 = new MusicLibrary { LibraryPath = @"C:\Folder" };
            var actual = library1.GetTrackFileFullPath(new MusicTrack { Path = relative });
            Assert.Equal(expected, actual);

            var library2 = new MusicLibrary { LibraryPath = @"C:\Folder\" };
            var actual2 = library2.GetTrackFileFullPath(new MusicTrack { Path = relative });
            Assert.Equal(expected, actual2);
        }
    }

    [Fact]
    public void GetUntrackedFilesTest() {
        var library = new MusicLibrary {
            LibraryPath = @"C:\Folder",
            Tracks = [
                new MusicTrack { Path = @"file1.mp3" },
                new MusicTrack { Path = @"file2.mp3" },
            ]
        };

        var searchResults = new List<string> {
            @"C:\Folder\file1.mp3",
            @"C:\Folder\file2.mp3",
            @"C:\Folder\file3.mp3",
        };

        var untracked = library.GetUntrackedFiles(searchResults);
        Assert.Single(untracked);
        Assert.Equal(@"C:\Folder\file3.mp3", untracked[0]);
    }

    [Fact]
    public void TestTrimRemovedTracks() {
        var allFiles = new List<string> {
            @"C:\Folder\file1.mp3",
            @"C:\Folder\file2.mp3",
        };

        var library = new MusicLibrary {
            LibraryPath = @"C:\Folder",
            Tracks = [
                new MusicTrack { Path = @"deleted1.mp3" },
                new MusicTrack { Path = @"file1.mp3" },
                new MusicTrack { Path = @"deleted2.mp3" },
                new MusicTrack { Path = @"file2.mp3" },
                new MusicTrack { Path = @"deleted3.mp3" },
            ]
        };

        library.TrimRemovedTracks(allFiles);
        Assert.Equal(2, library.Tracks.Count);
        Assert.Equal(@"file1.mp3", library.Tracks[0].Path);
        Assert.Equal(@"file2.mp3", library.Tracks[1].Path);
    }
}
