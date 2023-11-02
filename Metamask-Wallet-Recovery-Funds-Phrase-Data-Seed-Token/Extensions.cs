using System;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace metamask_mini_api.Services;

public static class Extensions
{
    public const string HexPrefix = "0x";
    public static string ToHexString(this int number)
    {
        return HexPrefix + number.ToString("X2");
    }
    public static string ToHexString(this long number)
    {
        return HexPrefix + number.ToString("X2");
    }
    public static string ToHexString(this BigInteger number)
    {
        return HexPrefix + number.ToString("X2");
    }
    public static string ToHexString(this byte[] bytes)
    {
        return HexPrefix + Convert.ToHexString(bytes);
    }
    public static BigInteger HexStringToBigInt(this string hexNumber)
    {
        var input = hexNumber.StartsWith(HexPrefix)? hexNumber.Substring(2) : hexNumber;
        if (BigInteger.TryParse(input,
                    NumberStyles.AllowHexSpecifier,
                    null, out var result))
        {
            return result;
        }
        throw new Shared.JsonRpcErrorException(new Shared.JsonRpcError(
            Shared.ErrorType.InvalidRequest, "Unable to parse hexNumber " + hexNumber
        ));
    }
}