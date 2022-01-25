using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HR.PersonalTimetable.Application.Extensions
{
    public static class HashingExtensions
    {
        /// <summary>
        /// Concatenates the data contained in the current string with the specified salt value and returns the SHA-256 hash of the result.
        /// </summary>
        /// <param name="data">A string containing the data to hash.</param>
        /// <param name="salt">A string containing the salt to append to the data.</param>
        /// <param name="encoding">The text encoding to use for converting the string to a byte array. Defaults to UTF-8 if not specified.</param>
        /// <returns>A lower-cased hexadecimal string containing the SHA-256 hash.</returns>
        public static string ToSha256(this string data, string salt, Encoding encoding = null)
        {
            return ToHexString((encoding ?? Encoding.UTF8).GetBytes(data).ToSha256((encoding ?? Encoding.UTF8).GetBytes(salt)));

            static string ToHexString(byte[] bytes)
                => string.Join(string.Empty, bytes.Select(b => b.ToString("x2")));
        }

        /// <summary>
        /// Concatenates the data contained in the current byte array with the specified salt value and returns the SHA-256 hash of the result.
        /// </summary>
        /// <param name="data">A byte array containing the data to hash.</param>
        /// <param name="salt">A byte array containing the salt to append to the data.</param>
        /// <returns>A byte array containing the SHA-256 hash.</returns>
        public static byte[] ToSha256(this byte[] data, byte[] salt)
        {
            using var cryptoProvider = HashAlgorithm.Create(HashAlgorithmName.SHA256.Name);
            return cryptoProvider.ComputeHash(data.Concat(salt).ToArray());
        }
    }
}
