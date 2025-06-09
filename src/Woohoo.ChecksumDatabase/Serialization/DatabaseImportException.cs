// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;

[Serializable]
public sealed class DatabaseImportException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseImportException"/> class.
    /// </summary>
    public DatabaseImportException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseImportException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DatabaseImportException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseImportException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public DatabaseImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
