using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Services
{
    public static class RandomStringGeneratorService
    {
        public static string GetRandomString(int length)
        {
            Random rand = new Random();
            string src = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string newString = string.Empty;

            for (int i = 0; i < length; i++)
            {
                int x = rand.Next(src.Length);
                newString = newString + src[x];
            }

            return newString;
        }
    }
}
