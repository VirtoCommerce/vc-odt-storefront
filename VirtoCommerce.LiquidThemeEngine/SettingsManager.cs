using System;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Storefront.Model.Common.Exceptions;

namespace VirtoCommerce.LiquidThemeEngine
{
    public static class SettingsManager
    {
        public static JObject Merge(JObject baseJson, JObject currentJson)
        {
            if (baseJson == null)
            {
                throw new ArgumentNullException(nameof(baseJson));
            }
            if (currentJson == null)
            {
                throw new ArgumentNullException(nameof(currentJson));
            }

            var currentTheme = ReadCurrentSettings(currentJson, baseJson);
            var baseTheme = ReadBaseSettings(currentJson, baseJson);
            baseTheme.Merge(currentTheme); // write values from current theme over base theme values
            return baseTheme;
        }

        public static JObject ReadSettings(JObject json)
        {
            return ReadCurrentSettings(json);
        }

        private static JObject ReadBaseSettings(JObject currentTheme, JObject baseTheme)
        {
            var baseThemeCurrent = baseTheme.GetValue("current");
            if (baseThemeCurrent == null)
            {
                throw new StorefrontException("Base theme settings file is incorrect or not found");
            }
            if (baseThemeCurrent is JObject result)
            {
                return result;
            }
            var currentThemeCurrent = currentTheme.GetValue("current");
            JObject resultPreset = null;
            if (currentThemeCurrent is JValue currentPresetJsonValue)
            {
                // if "current" in current theme is name, try to get preset with the same name in base settings
                var presetName = currentPresetJsonValue.Value.ToString();
                resultPreset = ReadPreset(presetName, baseTheme);
            }
            if (resultPreset == null)
            {
                // if "current" in current theme is object or no such preset in base theme, try to get preset by base current name
                var presetName = ((JValue)baseThemeCurrent).Value.ToString();
                resultPreset = ReadPreset(presetName, baseTheme);
                if (resultPreset == null)
                {
                    throw new StorefrontException($"Preset with name '{presetName}' was not found in base theme settings");
                }
            }
            return resultPreset;
        }

        private static JObject ReadCurrentSettings(JObject currentTheme, JObject baseTheme = null)
        {
            var current = currentTheme.GetValue("current");
            if (current is JValue currentPresetJsonValue)
            {
                // get preset from current settings or base when they weren't found
                var presetName = currentPresetJsonValue.Value.ToString();
                var currentPresetJson = ReadPreset(presetName, currentTheme) ?? ReadPreset(presetName, baseTheme);
                if (currentPresetJson == null)
                {
                    throw new StorefrontException($"Preset with name '{presetName}' was not found in current theme settings");
                }
                return currentPresetJson;
            }
            return current as JObject;
        }

        private static JObject ReadPreset(string name, JObject theme)
        {
            var presets = theme?.GetValue("presets") as JObject;
            return presets?.GetValue(name) as JObject;
        }
    }
}
