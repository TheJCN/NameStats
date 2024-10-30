using System.Reflection;

namespace NameStats;

public class Program
{
    public static NameData[] NamesData;

    public static void Main(string[] args)
    {
        NamesData = ReadData();
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://localhost:1234");
            });

    private static NameData[] ReadData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "NameStats.names.txt"; 

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        var lines = reader.ReadToEnd()
            .Split(new[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        var names = new NameData[lines.Length];
        for (var i = 0; i < lines.Length; i++)
            names[i] = NameData.ParseFrom(lines[i]);

        return names;
    }
}