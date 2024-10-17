using System.Globalization;

namespace NameStats;

public class NameData
{
    public DateTime BirthDate { get; set; }
    public string Name { get; set; }

    public NameData(DateTime birthDate, string name)
    {
        BirthDate = birthDate;
        Name = name;
    }

    public static NameData ParseFrom(string textLine)
    {
        var parts = textLine.Split('\t');
        const string format = "dd.MM.yyyy";
        var date = DateTime.ParseExact(parts[0], format, CultureInfo.InvariantCulture);
        return new NameData(date, parts[1]);
    }
}