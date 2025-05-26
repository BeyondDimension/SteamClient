/*
* Copyright (C) 2010 Colin Mackie.
* This software is distributed under the terms of the GNU General Public License.
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* https://github.com/BeyondDimension/original.winauth/blob/master/Authenticator/AuthenticatorException.cs
*/

namespace WinAuth;

/// <summary>
/// Base Authenticator exception class
/// </summary>
public abstract class AuthenticatorException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorException"/> class.
    /// </summary>
    public AuthenticatorException()
      : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public AuthenticatorException(string? message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public AuthenticatorException(string? message, Exception? innerException)
      : base(message, innerException)
    {
    }
}

/// <summary>
/// Config has been encrypted and we need a key
/// </summary>
public sealed class EncryptedSecretDataException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptedSecretDataException"/> class.
    /// </summary>
    public EncryptedSecretDataException() : base()
    {
    }
}

/// <summary>
/// Config decryption bad password
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BadPasswordException"/> class.
/// </remarks>
/// <param name="innerException"></param>
public sealed class BadPasswordException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadPasswordException"/> class.
    /// </summary>
    public BadPasswordException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadPasswordException"/> class.
    /// </summary>
    /// <param name="innerException"></param>
    public BadPasswordException(Exception innerException) : base(null, innerException)
    {
    }
}

/// <summary>
/// Invalid encryption detected
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvalidEncryptionException"/> class.
/// </remarks>
/// <param name="plain"></param>
/// <param name="password"></param>
/// <param name="encrypted"></param>
/// <param name="decrypted"></param>
public sealed class InvalidEncryptionException(string plain, string? password, string encrypted, string decrypted) : AuthenticatorException()
{
    public string Plain { get; set; } = plain;

    public string? Password { get; set; } = password;

    public string Encrypted { get; set; } = encrypted;

    public string Decrypted { get; set; } = decrypted;
}

/// <summary>
/// Initializes a new instance of the <see cref="Base32DecodingException"/> class.
/// </summary>
/// <param name="message"></param>
public sealed class Base32DecodingException(string message) : AuthenticatorException(message)
{
}

/// <summary>
/// Exception for error or unexpected return from server for enroll
/// </summary>
public sealed class InvalidEnrollResponseException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEnrollResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public InvalidEnrollResponseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEnrollResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public InvalidEnrollResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for error or unexpected return from server for sync
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvalidSyncResponseException"/> class.
/// </remarks>
/// <param name="message"></param>
public sealed class InvalidSyncResponseException(string message) : AuthenticatorException(message)
{
}

/// <summary>
/// Initializes a new instance of the <see cref="InvalidRestoreResponseException"/> class.
/// </summary>
/// <param name="message"></param>
public class InvalidRestoreResponseException(string message) : AuthenticatorException(message)
{
}

/// <summary>
/// Initializes a new instance of the <see cref="InvalidRestoreCodeException"/> class.
/// </summary>
/// <param name="message"></param>
public sealed class InvalidRestoreCodeException(string message) : InvalidRestoreResponseException(message)
{
}