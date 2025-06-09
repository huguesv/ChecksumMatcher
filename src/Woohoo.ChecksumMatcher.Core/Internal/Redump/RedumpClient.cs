// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Redump;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

internal sealed class RedumpClient
{
    public static readonly Regex LoginTokenRegex = new(@"<input type=""hidden"" name=""csrf_token"" value=""(.*?)"" />", RegexOptions.Compiled);

    private readonly HttpClient httpClient;

    private readonly int retryCount = 3;

    public RedumpClient(TimeSpan timeout)
    {
        this.httpClient = new HttpClient(new HttpClientHandler { UseCookies = true }) { Timeout = timeout };
    }

    public bool LoggedIn { get; private set; } = false;

    public bool IsStaff { get; private set; } = false;

    public static async Task<bool?> ValidateCredentialsAsync(string username, string password, CancellationToken ct)
    {
        // If options are invalid or we're missing something key, just return
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        // Try logging in with the supplied credentials otherwise
        var redumpClient = new RedumpClient(TimeSpan.FromSeconds(30));
        return await redumpClient.LoginAsync(username, password, ct);
    }

    public async Task<bool?> LoginAsync(string username, string password, CancellationToken ct)
    {
        // Credentials verification
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        // HTTP encode the password
        password = WebUtility.UrlEncode(password);

        // Attempt to login up to 3 times
        for (var i = 0; i < 3; i++)
        {
            try
            {
                // Get the current token from the login page
                var loginPage = await this.DownloadStringAsync(RedumpUrls.Login, ct);
                var token = LoginTokenRegex.Match(loginPage ?? string.Empty).Groups[1].Value;

                // Construct the login request
                var postContent = new StringContent($"form_sent=1&redirect_url=&csrf_token={token}&req_username={username}&req_password={password}&save_pass=0", Encoding.UTF8);
                postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                // Send the login request and get the result
                var response = await this.httpClient.PostAsync(RedumpUrls.Login, postContent, ct);
                string? responseContent = null;
                if (response?.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync(ct);
                }

                // An empty response indicates an error
                if (string.IsNullOrEmpty(responseContent))
                {
                    Debug.WriteLine($"An error occurred while trying to log in on attempt {i}: No response");
                    continue;
                }

                // Explcit confirmation the login was wrong
                if (responseContent.Contains("Incorrect username and/or password."))
                {
                    Debug.WriteLine("Invalid credentials entered, continuing without logging in...");
                    return false;
                }

                // The user was able to be logged in
                Debug.WriteLine("Credentials accepted! Logged into Redump...");
                this.LoggedIn = true;

                // If the user is a moderator or staff, set accordingly
                if (responseContent.Contains("http://forum.redump.org/forum/9/staff/"))
                {
                    this.IsStaff = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An exception occurred while trying to log in on attempt {i}: {ex}");
            }
        }

        Debug.WriteLine("Could not login to Redump in 3 attempts, continuing without logging in...");
        return false;
    }

    public async Task DownloadToFileAsync(string uri, string outDir, string? subfolder, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(uri);
        ArgumentException.ThrowIfNullOrEmpty(outDir);

        var tempfile = Path.Combine(outDir, "tmp" + Guid.NewGuid().ToString());
        try
        {
            var remoteFileName = await this.DownloadFileAsync(uri, tempfile, ct);
            MoveOrDelete(tempfile, remoteFileName, outDir, subfolder);
        }
        catch (Exception)
        {
            // Clean up in case of an error or cancellation
            if (File.Exists(tempfile))
            {
                try
                {
                    File.Delete(tempfile);
                }
                catch
                {
                }
            }

            throw;
        }
    }

    private static void MoveOrDelete(string tempfile, string? newfile, string outDir, string? subfolder)
    {
        // If we don't have a file to move to, just delete the temp file
        if (string.IsNullOrEmpty(newfile))
        {
            File.Delete(tempfile);
            return;
        }

        // If we have a subfolder, create it and update the newfile name
        if (!string.IsNullOrEmpty(subfolder))
        {
            if (!Directory.Exists(Path.Combine(outDir, subfolder)))
            {
                Directory.CreateDirectory(Path.Combine(outDir, subfolder));
            }

            newfile = Path.Combine(subfolder, newfile);
        }

        // If the file already exists, don't overwrite it
        if (File.Exists(Path.Combine(outDir, newfile)))
        {
            File.Delete(tempfile);
        }
        else
        {
            File.Move(tempfile, Path.Combine(outDir, newfile));
        }
    }

    private async Task<string?> DownloadFileAsync(string uri, string fileName, CancellationToken ct)
    {
        // Make the call to get the file
        var response = await this.httpClient.GetAsync(uri, ct);
        if (response?.Content?.Headers == null || !response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Could not download {uri}");
            return null;
        }

        // Copy the data to a local temp file
        using (var responseStream = await response.Content.ReadAsStreamAsync(ct))
        using (var tempFileStream = File.OpenWrite(fileName))
        {
            await responseStream.CopyToAsync(tempFileStream, ct);
        }

        return response.Content.Headers.ContentDisposition?.FileName?.Replace("\"", string.Empty);
    }

    private async Task<string?> DownloadStringAsync(string uri, CancellationToken ct)
    {
        // Only retry a positive number of times
        if (this.retryCount <= 0)
        {
            return null;
        }

        for (var i = 0; i < this.retryCount; i++)
        {
            try
            {
                return await this.httpClient.GetStringAsync(uri, ct);
            }
            catch
            {
            }

            await Task.Delay(100, ct);
        }

        return null;
    }
}
