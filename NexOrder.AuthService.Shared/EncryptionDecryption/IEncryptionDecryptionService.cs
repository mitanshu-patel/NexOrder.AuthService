// <copyright file="IEncryptionDecryptionService.cs" company="Civica Group Limited">
// Copyright (c) Civica Group Limited.  All rights reserved.
// </copyright>

namespace NexOrder.AuthService.Shared.EncryptionDecryption;

public interface IEncryptionDecryptionService
{
    string EncryptSecret(string value);

    string DecryptSecret(string value);

    bool TryDecryptValue(string value, out string? decryptedValue);
}
