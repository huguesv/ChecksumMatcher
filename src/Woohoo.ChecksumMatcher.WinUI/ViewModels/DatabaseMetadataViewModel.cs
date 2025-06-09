// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Text;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public class DatabaseMetadataViewModel
{
    private readonly IClipboardService clipboardService;
    private readonly RomDatabase db;

    public DatabaseMetadataViewModel(IClipboardService clipboardService, RomDatabase db)
    {
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(db);

        this.clipboardService = clipboardService;
        this.db = db;
    }

    public string Author => this.db.Author;

    public string Category => this.db.Category;

    public string Comment => this.db.Comment;

    public string Date => this.db.Date;

    public string Description => this.db.Description;

    public string Email => this.db.Email;

    public string Homepage => this.db.Homepage;

    public string Name => this.db.Name;

    public string Url => this.db.Url;

    public string Version => this.db.Version;

    public void CopyToClipboard()
    {
        var current = new StringBuilder();

        if (!string.IsNullOrEmpty(this.Name))
        {
            current.AppendLine(Localized.HeaderName);
            current.AppendLine(this.Name);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Description))
        {
            current.AppendLine(Localized.HeaderDescription);
            current.AppendLine(this.Description);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Category))
        {
            current.AppendLine(Localized.HeaderCategory);
            current.AppendLine(this.Category);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Version))
        {
            current.AppendLine(Localized.HeaderVersion);
            current.AppendLine(this.Version);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Date))
        {
            current.AppendLine(Localized.HeaderDate);
            current.AppendLine(this.Date);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Author))
        {
            current.AppendLine(Localized.HeaderAuthor);
            current.AppendLine(this.Author);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Email))
        {
            current.AppendLine(Localized.HeaderEmail);
            current.AppendLine(this.Email);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Homepage))
        {
            current.AppendLine(Localized.HeaderHomepage);
            current.AppendLine(this.Homepage);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Url))
        {
            current.AppendLine(Localized.HeaderUrl);
            current.AppendLine(this.Url);
            current.AppendLine();
        }

        if (!string.IsNullOrEmpty(this.Comment))
        {
            current.AppendLine(Localized.HeaderComment);
            current.AppendLine(this.Comment);
            current.AppendLine();
        }

        this.clipboardService.SetText(current.ToString());
    }
}
