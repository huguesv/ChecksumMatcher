// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Text;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed class DatabaseMetadataViewModel
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

        AppendProperty(Localized.HeaderName, this.Name);
        AppendProperty(Localized.HeaderDescription, this.Description);
        AppendProperty(Localized.HeaderCategory, this.Category);
        AppendProperty(Localized.HeaderVersion, this.Version);
        AppendProperty(Localized.HeaderDate, this.Date);
        AppendProperty(Localized.HeaderAuthor, this.Author);
        AppendProperty(Localized.HeaderEmail, this.Email);
        AppendProperty(Localized.HeaderHomepage, this.Homepage);
        AppendProperty(Localized.HeaderUrl, this.Url);
        AppendProperty(Localized.HeaderComment, this.Comment);

        this.clipboardService.SetText(current.ToString());

        void AppendProperty(string header, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                current.AppendLine(header);
                current.AppendLine(value);
                current.AppendLine();
            }
        }
    }
}
