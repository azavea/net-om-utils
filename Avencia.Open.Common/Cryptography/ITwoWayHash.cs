using System;

namespace Avencia.Open.Common.Cryptography
{
    /// <summary>
    /// An interface for classes that perform two-way encryption/decryption on an input.
    /// </summary>
    public abstract class ITwoWayHash
    {
        /// <summary>
        /// Encrypts a plaintext string into cyphertext
        /// </summary>
        /// <param name="input">plaintext</param>
        /// <returns>cyphertext</returns>
        public abstract string Encrypt(string input);

        /// <summary>
        /// decrypts cyphertext into plaintext
        /// </summary>
        /// <param name="input">cyphertext</param>
        /// <returns>plaintext</returns>
        public abstract string Decrypt(string input);
    }
}
