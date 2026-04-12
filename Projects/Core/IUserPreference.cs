public interface IUserPreference
{

    public string LibraryPath();
    public void SetLibraryPath(string path);
    public (int, int)? WindowPosition();
    public void SetWindowPosition((int, int) position);
    public void ClearAll();
}
