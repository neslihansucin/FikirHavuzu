namespace FikirHavuzu.Business.Utilities
{
    public static class PasswordGenerator
    {
        private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Lower = "abcdefghijkmnpqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Specials = "!@#$%*-_";

        public static string GenerateTemporaryPassword(int length = 10)
        {
            var random = new Random();
            var chars = new List<char>
            {
                Upper[random.Next(Upper.Length)],
                Lower[random.Next(Lower.Length)],
                Digits[random.Next(Digits.Length)],
                Specials[random.Next(Specials.Length)]
            };

            string allChars = Upper + Lower + Digits + Specials;
            for (int i = chars.Count; i < length; i++)
            {
                chars.Add(allChars[random.Next(allChars.Length)]);
            }

            return new string(chars.OrderBy(_ => random.Next()).ToArray());
        }

        public static string GenerateRegistrationNumber(IEnumerable<string> existingRegNumbers)
        {
            // Format: PER + yyMMdd + total index (e.g. PER26090206)
            string datePrefix = $"PER{DateTime.Now:yyMMdd}";
            int totalCount = existingRegNumbers.Count(r => !string.IsNullOrEmpty(r));
            int nextIndex = totalCount + 1;
            return $"{datePrefix}{nextIndex:D3}";
        }
    }
}
