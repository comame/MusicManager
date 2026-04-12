public class ITunesMusicLibraryTest {
    [Fact]
    public void TestITLUtilCalculateAppleEpoc() {
        var dt = new DateTime(2026, 04, 13, 12, 34, 56, DateTimeKind.Utc);
        Assert.Equal(3858928496, ITLUtil.CalculateAppleEpoc(dt));
    }

    [Fact]
    public void TestITLUtilConvertPathToLocation() {
        var cases = new List<(string, string)> {
            (@"C:\Folder\file.mp3", @"file://localhost/C:/Folder/file.mp3"),
            (@"C:\Folder\file with spaces.mp3", @"file://localhost/C:/Folder/file%20with%20spaces.mp3"),
            (@"C:\Folder\名前に日本語.mp3", @"file://localhost/C:/Folder/%E5%90%8D%E5%89%8D%E3%81%AB%E6%97%A5%E6%9C%AC%E8%AA%9E.mp3"),
        };
        foreach (var (path, expected) in cases) {
            var actual = ITLUtil.ConvertPathToLocation(path);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void TestITLUtilToUTCDatetimeString() {
        var dt = new DateTime(2026, 04, 13, 12, 34, 56);
        Assert.Equal("2026-04-13T03:34:56Z", ITLUtil.ToUTCDatetimeString(dt));
    }
}
