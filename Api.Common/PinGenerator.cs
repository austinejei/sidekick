using System;
using System.Security.Cryptography;

namespace Api.Common
{
    /// <summary>
    /// Generates random codes for IVR.
    /// Thanks to http://www.edmundyeung.com/Code/c-random-password-generator
    /// </summary>
    public class PinGenerator
    {
        // Define default password length.
        private const int DefaultPasswordLength = 5;

        //No characters that are confusing: i, I, l, L, o, O, 0, 1, u, v

        public static string PasswordCharsAlpha ="abcdefghjkmnpqrstwxyzABCDEFGHJKMNPQRSTWXYZ";
        public static string PasswordCharsNumeric = "23456789";
        public static string PasswordCharsSpecial = "*$-+?_&=!%{}/";
        //public static string PasswordCharsSpecial = "";
        public static string PasswordCharsAlphanumeric =
                                PasswordCharsAlpha + PasswordCharsNumeric;
        public static string PasswordCharsAll =PasswordCharsAlphanumeric + PasswordCharsSpecial;

        //These overloads are only necesary in versions of .NET below 4.0
        #region Overloads

        /// <summary>
        /// Generates a random password with the default length.
        /// </summary>
        /// <returns>Randomly generated password.</returns>
        public static string Generate()
        {
            return Generate(DefaultPasswordLength,PasswordCharsAll);
        }

        /// <summary>
        /// Generates a random password with the default length.
        /// </summary>
        /// <returns>Randomly generated password.</returns>
        public static string GenerateAlphaNumeric(int length)
        {
            return Generate(length, PasswordCharsAlphanumeric);
        }

        /// <summary>
        /// Generates a random password with the default length.
        /// </summary>
        /// <returns>Randomly generated password.</returns>
        public static string Generate(string passwordChars)
        {
            return Generate(DefaultPasswordLength,
                            passwordChars);
        }

        /// <summary>
        /// Generates a random password with the default length.
        /// </summary>
        /// <returns>Randomly generated password.</returns>
        public static string Generate(int passwordLength)
        {
            return Generate(passwordLength,
                            PasswordCharsAll);
        }

        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <returns>Randomly generated password.</returns>
        public static string Generate(int passwordLength,string passwordChars)
        {
            return GeneratePassword(passwordLength,passwordChars);
        }

        #endregion


        /// <summary>
        /// Generates the password.
        /// </summary>
        /// <returns></returns>
        private static string GeneratePassword(int passwordLength,string passwordCharacters)
        {
            if (passwordLength < 0)
                throw new ArgumentOutOfRangeException("Password Length");

            if (string.IsNullOrEmpty(passwordCharacters))
                throw new ArgumentOutOfRangeException("Password Characters");

            var password = new char[passwordLength];

            var random = GetRandom();

            for (int i = 0; i < passwordLength; i++)
                password[i] = passwordCharacters[random.Next(passwordCharacters.Length)];

            return new string(password);
        }

        
        /// <summary>
        /// Gets a random object with a real random seed
        /// </summary>
        /// <returns></returns>
        private static Random GetRandom()
        {
            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            var randomBytes = new byte[4];

            // Generate 4 random bytes.
            new RNGCryptoServiceProvider().GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            int seed = (randomBytes[0] & 0x7f) << 24 |
                        randomBytes[1] << 16 |
                        randomBytes[2] << 8 |
                        randomBytes[3];

            // Now, this is real randomization.
            return new Random(seed);
        }


    }
}