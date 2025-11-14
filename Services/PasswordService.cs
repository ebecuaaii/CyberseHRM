using System.Security.Cryptography;
using System.Text;

namespace HRMCyberse.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "HRMCyberse_Salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            // Thử hash mới trước
            var newHashedPassword = HashPassword(password);
            if (newHashedPassword == hash)
                return true;

            // Thử các format hash cũ có thể có trong database
            
            // 1. Plain text (không an toàn nhưng có thể có)
            if (password == hash)
                return true;

            // 2. MD5 hash
            try
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                var md5Bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                var md5Hash = Convert.ToBase64String(md5Bytes);
                if (md5Hash == hash)
                    return true;
            }
            catch { }

            // 3. SHA256 không có salt
            try
            {
                using var sha256 = SHA256.Create();
                var sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sha256Hash = Convert.ToBase64String(sha256Bytes);
                if (sha256Hash == hash)
                    return true;
            }
            catch { }

            // 4. Thử với các salt khác có thể có
            var commonSalts = new[] { "", "salt", "admin", "cyberse", "hrm" };
            foreach (var salt in commonSalts)
            {
                try
                {
                    using var sha256 = SHA256.Create();
                    var saltedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                    var saltedHash = Convert.ToBase64String(saltedBytes);
                    if (saltedHash == hash)
                        return true;
                }
                catch { }
            }

            return false;
        }
    }
}