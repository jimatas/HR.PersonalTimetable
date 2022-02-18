using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HR.PersonalTimetable.Application.Extensions
{
    public static class HashingExtensions
    {
        /// <summary>
        /// Computes the SHA-256 hash of the current string.
        /// </summary>
        /// <param name="data">The string to hash.</param>
        /// <param name="encoding">The text encoding to use for converting the string to a byte array. Defaults to UTF-8 if not specified.</param>
        /// <returns>A lowercased hexadecimal string containing the hash value.</returns>
        public static string ToSha256(this string data, Encoding encoding = null)
        {
            return ToHexString((encoding ?? Encoding.UTF8).GetBytes(data).ToSha256());

            static string ToHexString(byte[] bytes)
                => string.Join(string.Empty, bytes.Select(b => b.ToString("x2")));
        }

        /// <summary>
        /// Computes the SHA-256 hash of the current byte array.
        /// </summary>
        /// <param name="data">The byte array to hash.</param>
        /// <returns>A byte array containing the hash value.</returns>
        public static byte[] ToSha256(this byte[] data)
        {
            using var hashAlgorithm = HashAlgorithm.Create(HashAlgorithmName.SHA256.Name);
            return hashAlgorithm.ComputeHash(data.ToArray());
        }
    }
}
