namespace TumicseSite.Utilities;

public static class YouTubeVideoReferenceParser
{
    public static bool TryExtractVideoId(string? input, out string videoId)
    {
        videoId = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var candidate = input.Trim();
        if (LooksLikeVideoId(candidate))
        {
            videoId = candidate;
            return true;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        videoId = ExtractFromUri(uri);
        return LooksLikeVideoId(videoId);
    }

    private static string ExtractFromUri(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();

        if (host is "youtu.be" or "www.youtu.be")
        {
            return uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        }

        if (host.Contains("youtube.com", StringComparison.Ordinal))
        {
            var path = uri.AbsolutePath.Trim('/');
            if (path.StartsWith("watch", StringComparison.OrdinalIgnoreCase))
            {
                var query = uri.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var part in query)
                {
                    var pieces = part.Split('=', 2);
                    if (pieces.Length == 2 && string.Equals(pieces[0], "v", StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(pieces[1]);
                    }
                }

                return string.Empty;
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length >= 2 && segments[0] is "embed" or "shorts" or "live")
            {
                return segments[1];
            }
        }

        return string.Empty;
    }

    private static bool LooksLikeVideoId(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        return candidate.Length is >= 6 and <= 20
            && candidate.All(character => char.IsLetterOrDigit(character) || character is '-' or '_');
    }
}
