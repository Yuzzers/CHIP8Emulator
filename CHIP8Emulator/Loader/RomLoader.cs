public class Romloader
{

public static byte[] LoadRom(string path)
    {
        if (!File.Exists(path))
        throw new FileNotFoundException($"Rom not found");

        return File.ReadAllBytes(path);

    }
}

    