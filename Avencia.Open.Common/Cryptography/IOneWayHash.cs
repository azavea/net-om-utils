using System;
namespace Avencia.Open.Common.Cryptography
{
    /// <summary>
    /// An interface for classes that perform a one-way encryption (no decryption) on an input.
    /// </summary>
    public abstract class IOneWayHash
    {
        /// <summary>
        /// Cryptographically hashes a plaintext string into a short-message digest
        /// (hard to decrypt)
        /// </summary>
        /// <param name="input">plaintext</param>
        /// <returns>one-way hash</returns>
        public abstract string Encrypt(string input);
    }
}
