﻿using System.Text.Json.Serialization;

namespace CmlLib.Core.Installer.FabricMC
{
    public class FabricLoader
    {
        [JsonPropertyName("separator")]
        public string? Separator { get; set; }
        [JsonPropertyName("build")]
        public string? Build { get; set; }
        [JsonPropertyName("maven")]
        public string? Maven { get; set; }
        [JsonPropertyName("version")]
        public string? Version { get; set; }
        [JsonPropertyName("stable")]
        public bool Stable { get; set; }
    }
}
