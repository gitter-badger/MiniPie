﻿using System;
using System.Collections.Generic;
using MiniPie.Core.Enums;
using MiniPie.Core.SpotifyWeb;
using Newtonsoft.Json;

namespace MiniPie.Core {
    [JsonObject]
    public class AppSettings {

        public AppSettings() {
            Positions = new List<WindowPosition>();
            ApplicationSize = ApplicationSize.Medium;
            Language = LanguageHelper.English;
            LockScreenBehavior = LockScreenBehavior.Disabled;
            UpdatePreference = UpdatePreference.Stable;
        }

        [JsonProperty]
        public bool AlwaysOnTop { get; set; }

        [JsonProperty]
        public bool StartWithWindows { get; set; }

        [JsonProperty]
        public bool HideIfSpotifyClosed { get; set; }

        [JsonProperty]
        public bool DisableAnimations { get; set; }

        [JsonProperty]
        public Language Language { get; set; }

        public List<WindowPosition> Positions { get; set; }

        [JsonProperty]
        public ApplicationSize ApplicationSize { get; set; }
        [JsonProperty]
        public bool HotKeysEnabled { get; set; }
        [JsonProperty]
        public HotKeys HotKeys { get; set; }
        [JsonProperty]
        public bool StartMinimized { get; set; }
        [JsonProperty]
        public string CacheFolder { get; set; }
        [JsonProperty]
        public Token SpotifyToken { get; set; }
        [JsonProperty]
        public LockScreenBehavior LockScreenBehavior { get; set; }
        [JsonProperty]
        public UpdatePreference UpdatePreference { get; set; }
        [JsonProperty]
        public bool SingleClickHide { get; set; }
    }
}