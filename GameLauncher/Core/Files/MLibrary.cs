using System.Collections.Generic;

namespace CmlLib.Core.Files
{
    public class MLibrary
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
        public string? Hash { get; set; }
        public long Size { get; set; }
        public bool IsRequire { get; set; }
        public bool IsNative { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is MLibrary library &&
                   Name == library.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string?>.Default.GetHashCode(Name);
        }
    }
}
