using System.Text;

namespace A2450ReportLogger;

internal static class Hex
{
    public static string ToString(byte[] data, int length)
    {
        var sb = new StringBuilder(length * 3);
        for (int i = 0; i < length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(data[i].ToString("X2"));
        }
        return sb.ToString();
    }
}
