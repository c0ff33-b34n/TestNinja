using System.IO;

namespace TestNinja.Mocking
{
    public interface IFileReader
    {
        string Read(string path);
    }

    class FileReader : IFileReader
    {
        public string Read(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
