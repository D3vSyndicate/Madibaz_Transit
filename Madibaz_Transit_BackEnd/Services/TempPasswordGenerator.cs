// Services/TempPasswordGenerator.cs
// Generates a random temporary password — never something predictable
// like "Driver123". The ShuttleManager sees this ONCE, in the create
// response, and has to relay it to the driver directly (phone call,
// printed slip, whatever their real process is). We never store it
// in plain text — only its hash, same as any other password.

using System;
using System.Security.Cryptography;

namespace Madibaz_Transit_BackEnd.Services
{
    public static class TempPasswordGenerator
    {
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        // (deliberately excludes easily-confused characters like 0/O, 1/l/I)

        public static string Generate(int length = 10)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = Chars[bytes[i] % Chars.Length];
            return new string(result);
        }
    }
}