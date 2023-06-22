using System;
using System.Globalization;
using System.Text;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Helpers;

internal static class Utils
{
    /// <summary>
    /// Converts a buffer to a hexadecimal string.
    /// </summary>
    public static string ToHexString(byte[] buffer, bool invertEndian = false)
    {
        if (buffer == null || buffer.Length == 0)
        {
            return String.Empty;
        }

        StringBuilder builder = new StringBuilder(buffer.Length * 2);

        if (invertEndian)
        {
            for (int ii = buffer.Length - 1; ii >= 0; ii--)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", buffer[ii]);
            }
        }
        else
        {
            for (int ii = 0; ii < buffer.Length; ii++)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", buffer[ii]);
            }
        }

        return builder.ToString();
    }
}
