using System;
using Microsoft.AspNetCore.Hosting;

namespace TicketManagerService.Extensions;

public static class UploadsPath
{
    public static string GetUploadsFolderPath(IWebHostEnvironment env)
    {
        if (env == null) throw new ArgumentNullException(nameof(env));
        return Path.Combine(env.ContentRootPath, "uploads");
    }

    public static string GetUploadsFolderPath(string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath)) throw new ArgumentException("Content rootpath cannot be null or empty", nameof(contentRootPath));
        return Path.Combine(contentRootPath, "uploads");
    }
}
