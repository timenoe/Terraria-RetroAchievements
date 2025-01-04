using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RetroAchievementsMod.Utils
{
    public static class HashUtils
    {
        // Generate an MD5 hash from a string
        public static string GenerateMD5(string str)
        {
            return string.Join("", MD5.HashData(Encoding.ASCII.GetBytes(str)).Select(s => s.ToString("x2")));
        }
    }
}
