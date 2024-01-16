#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace BD.SteamClient8.Models;

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
public sealed class AuthenticatorEncryptedSecretDataException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorEncryptedSecretDataException"/> class.
    /// </summary>
    public AuthenticatorEncryptedSecretDataException() : base()
    {
    }
}

/// <summary>
/// Config decryption bad password
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthenticatorBadPasswordException"/> class.
/// </remarks>
/// <param name="innerException"></param>
public sealed class AuthenticatorBadPasswordException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorBadPasswordException"/> class.
    /// </summary>
    public AuthenticatorBadPasswordException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorBadPasswordException"/> class.
    /// </summary>
    /// <param name="innerException"></param>
    public AuthenticatorBadPasswordException(Exception innerException) : base(null, innerException)
    {
    }
}

/// <summary>
/// Invalid encryption detected
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthenticatorInvalidEncryptionException"/> class.
/// </remarks>
/// <param name="plain"></param>
/// <param name="password"></param>
/// <param name="encrypted"></param>
/// <param name="decrypted"></param>
public sealed class AuthenticatorInvalidEncryptionException(string plain, string? password, string encrypted, string decrypted) : AuthenticatorException()
{
    public string Plain { get; set; } = plain;

    public string? Password { get; set; } = password;

    public string Encrypted { get; set; } = encrypted;

    public string Decrypted { get; set; } = decrypted;
}

/// <summary>
/// Initializes a new instance of the <see cref="AuthenticatorBase32DecodingException"/> class.
/// </summary>
/// <param name="message"></param>
public sealed class AuthenticatorBase32DecodingException(string message) : AuthenticatorException(message)
{
}

/// <summary>
/// Exception for error or unexpected return from server for enroll
/// </summary>
public sealed class AuthenticatorInvalidEnrollResponseException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorInvalidEnrollResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public AuthenticatorInvalidEnrollResponseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorInvalidEnrollResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public AuthenticatorInvalidEnrollResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for error or unexpected return from server for sync
/// </summary>
public sealed class AuthenticatorInvalidSyncResponseException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorInvalidSyncResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public AuthenticatorInvalidSyncResponseException(string message) : base(message)
    {
    }
}

public class AuthenticatorInvalidRestoreResponseException : AuthenticatorException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorInvalidRestoreResponseException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public AuthenticatorInvalidRestoreResponseException(string message) : base(message)
    {
    }
}

public sealed class AuthenticatorInvalidRestoreCodeException : AuthenticatorInvalidRestoreResponseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatorInvalidRestoreCodeException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public AuthenticatorInvalidRestoreCodeException(string message) : base(message)
    {
    }
}