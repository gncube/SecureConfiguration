using System.Text;

namespace SE.AppConfiguration
{
    public class Common
    {
    }

    public static class Helper
    {
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}
