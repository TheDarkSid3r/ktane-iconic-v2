using System.Collections.Generic;
using System.Linq;

public sealed class IconPartData
{
    public string Name { get; private set; }

    public int[] Indices { get; private set; }

    public IconPartData(string data, string[] dict)
    {
        string[] parts = data.Split('$');
        Name = dict[int.Parse(parts[0])];

        List<int> indices = new List<int>();
        if (parts[1].Length > 0)
        {
            string[] ranges = parts[1].Split(',');
            foreach (string range in ranges)
            {
                string[] rangeParts = range.Split(':');
                int start = int.Parse(rangeParts[0]);
                if (rangeParts.Length > 1)
                    indices.AddRange(Enumerable.Range(start, int.Parse(rangeParts[1])));

                else
                    indices.Add(start);
            }
        }

        Indices = indices.ToArray();
    }
}
