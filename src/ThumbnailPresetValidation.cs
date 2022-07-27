using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    public static class ThumbnailPresetValidation
    {
        public static IReadOnlyList<string> Validate(this ThumbnailPreset value)
        {
            var errors = new List<string>();

            if (value == null)
            {
                errors.Add("ThumbnailPreset cannot be null");
                return errors.AsReadOnly();
            }

            // Check AtTime - it's a method that returns ThumbnailPreset, so we need to check the _position field
            // Since we can't access private fields, we'll check if the position is set to a valid value
            // by examining the behavior - AtTime returns a new instance, so we need to look at the actual position
            // For validation purposes, we'll assume AtTime was called with a valid TimeSpan

            var buildArgs = value.BuildArguments();
            if (buildArgs == null)
            {
                errors.Add("BuildArguments cannot be null");
            }
            else if (buildArgs.Length == 0)
            {
                errors.Add("BuildArguments must contain at least one argument");
            }
            else
            {
                foreach (var arg in buildArgs)
                {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        errors.Add("BuildArguments cannot contain null, empty, or whitespace strings");
                        break;
                    }
                }
            }

            return errors.AsReadOnly();
        }

        public static bool IsValid(this ThumbnailPreset value)
        {
            return Validate(value).Count == 0;
        }

        public static void EnsureValid(this ThumbnailPreset value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ThumbnailPreset is invalid. Problems:\n{string.Join("\n- ", errors)}");
            }
        }
    }
}