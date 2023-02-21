using System.Linq;

public sealed class IconData
{
    public string Name { get; private set; }

    public string IconName { get; private set; }

    public int LocationX { get; private set; }

    public int LocationY { get; private set; }

    public IconPartData[] Parts { get; private set; }

    public IconData(string data, string[] dict)
    {
        string[] parts = data.Split('|');

        // parts[0] is the mod id

        Name = dict[int.Parse(parts[1])];
        IconName = dict[int.Parse(parts[2])];

        int[] location = parts[3].Split(',').Select(int.Parse).ToArray();
        LocationX = location[0];
        LocationY = location[1];

        Parts = parts[4].Split('!').Select(d => new IconPartData(d, dict)).ToArray();
    }
}
