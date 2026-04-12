using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager;

internal class UserPreference : IUserPreference
{

    public string LibraryPath()
    {
        return StringGetter("LibraryPath", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
    }

    public void SetLibraryPath(string path)
    {
        StringSetter("LibraryPath", path);
    }

    public (int, int)? WindowPosition()
    {
        object x = Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionX"];
        object y = Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionY"];
        if (x == null || y == null)
        {
            return null;
        }
        return ((int)x, (int)y);
    }

    public void SetWindowPosition((int, int) position)
    {
        Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionX"] = position.Item1;
        Windows.Storage.ApplicationData.Current.LocalSettings.Values["WindowPositionY"] = position.Item2;
    }

    public void ClearAll()
    {
        Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();
    }

    private static string StringGetter(string key, string defaultValue)
    {
        var current = Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
        if (current == null)
        {
            return defaultValue;
        }

        return (string)current;
    }

    private static void StringSetter(string key, string value)
    {
        Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
    }
}
