using System.Security.Cryptography;
using SEET = Skylark.Enum.EncodeType;
using SHE = Skylark.Helper.Encode;
using SHG = Skylark.Helper.Guidly;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Unique
    {
        public static Guid Generate(string Value)
        {
            return SHG.ByteToGuid(MD5.HashData(SHE.GetBytes(Value, SEET.UTF8)));
        }

        public static Guid GenerateFips(string Value)
        {
            byte[] Bytes = new byte[16];

            Array.Copy(SHA256.HashData(SHE.GetBytes(Value, SEET.UTF8)), Bytes, 16);

            return SHG.ByteToGuid(Bytes);
        }

        public static string GenerateText(string Value)
        {
            return SHG.GuidToText(Generate(Value));
        }

        public static string GenerateFipsText(string Value)
        {
            return SHG.GuidToText(GenerateFips(Value));
        }
    }
}