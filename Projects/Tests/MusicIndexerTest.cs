public class MusicIndexerTest {
    [Fact]
    public void TestConvertWindowsFullPathToRelativePath() {
        var cases = new List<(string, string)> {
            (@"C:\Folder\file.mp3", @"file.mp3"),
            (@"C:\Folder\Subfolder\file.mp3", @"Subfolder\file.mp3"),
            (@"C:\Folder\Subfolder\Another Folder\file.mp3", @"Subfolder\Another Folder\file.mp3"),
        };
        foreach (var (full, expected) in cases) {
            var actual = MusicIndexer.ConvertWindowsFullPathToRelativePath(full, @"C:\Folder");
            Assert.Equal(expected, actual);

            var actual2 = MusicIndexer.ConvertWindowsFullPathToRelativePath(full, @"C:\Folder\");
            Assert.Equal(expected, actual2);
        }
    }

    [Fact]
    public void TestConvertRelativePathToWindowsFullPath() {
        var cases = new List<(string, string)> {
            (@"file.mp3", @"C:\Folder\file.mp3"),
            (@"Subfolder\file.mp3", @"C:\Folder\Subfolder\file.mp3"),
            (@"Subfolder\Another Folder\file.mp3", @"C:\Folder\Subfolder\Another Folder\file.mp3"),
        };
        foreach (var (relative, expected) in cases) {
            var actual = MusicIndexer.ConvertRelativePathToWindowsFullPath(relative, @"C:\Folder");
            Assert.Equal(expected, actual);

            var actual2 = MusicIndexer.ConvertRelativePathToWindowsFullPath(relative, @"C:\Folder\");
            Assert.Equal(expected, actual2);
        }
    }
}
