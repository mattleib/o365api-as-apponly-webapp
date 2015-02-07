//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AccessMailboxAsApp.App_Classes
{
    public static class WebConvert
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static Int64 EpocTime(DateTime time)
        {
            TimeSpan timeSpan = time - UnixEpoch;

            return ((Int64)timeSpan.TotalSeconds);
        }

        public static string Base64UrlDecode(string value)
        {
            byte[] valueBytes = Base64UrlDecodeToBytes(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static byte[] Base64UrlDecodeToBytes(string value)
        {
            value = value.Replace('-', '+').Replace('_', '/');

            switch (value.Length % 4)
            {
                case 0:
                    break;
                case 1:
                    throw new ArgumentException("Value has an invalid length");
                case 2:
                    value += "==";
                    break;
                case 3:
                    value += "=";
                    break;
            }

            return Convert.FromBase64String(value);
        }

        public static string Base64UrlEncoded(byte[] data)
        {
            string base64String = Convert.ToBase64String(data);
            return base64String.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static string Base64UrlEncoded(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return Base64UrlEncoded(valueBytes);
        }

        public static byte[] ConvertToBigEndian(Int32 i)
        {
            byte[] temp = BitConverter.GetBytes(i);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            return temp;
        }

        public static byte[] HexStringToBytes(string value)
        {
            char[] chars = value.ToCharArray();
            byte[] bytes = new byte[value.Length / 2];

            for (int i = 0; i < chars.Length / 2; i++)
            {
                string hexChar = value.Substring(i * 2, 2);

                bytes[i] = (byte)Convert.ToInt32(hexChar, 16);
            }

            return bytes;
        }
    }
}

// MIT License: 

// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// ""Software""), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 

// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 

// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.