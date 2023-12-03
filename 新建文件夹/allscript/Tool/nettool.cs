using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// 网络工具
public static class nettool
{
	private static byte placeHolder;
	private static byte[] encodingTable;
	private static byte[] decodingTable;
	private static DateTime t19700101 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static bool useAes = true;
	public static string aesKey = "0s3Gk5HO6hacuqTV";
	public static string aesIV = "6QfC7eodr1SuLw9H";

	public static long timestamp
	{
		get
		{
			return Convert.ToInt64((DateTime.UtcNow - t19700101).TotalMilliseconds);
		}
	}
	public static DateTime convertdatetimetoint(long time)
	{

		DateTime Time = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
		DateTime dateTime = Time.AddMilliseconds(time);
		return dateTime;
	}

	public static bool isconnect { get { return Application.internetReachability > NetworkReachability.NotReachable; } }

	public static bool isdatanetwork { get { return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork; } }

	public static bool isareanetwork { get { return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork; } }

	public static string doescapeurl(string param)
	{
		return UnityWebRequest.EscapeURL(param);
	}

	public static string unescapeurl(string param)
	{
		return UnityWebRequest.UnEscapeURL(param);
	}

	public static string encodestring(string param)
	{
		byte[] bytearr = System.Text.Encoding.UTF8.GetBytes(param);
		return System.Text.Encoding.UTF8.GetString(encodebyte(bytearr)).Replace("#", "@");
	}

	public static string decodestring(string param)
	{
		byte[] bytearr = System.Text.Encoding.UTF8.GetBytes(param);
		return System.Text.Encoding.UTF8.GetString(decodebyte(bytearr));
	}

	/// <summary>AES加密</summary>
	/// <param name="text">明文</param>
	/// <param name="key">密钥,长度为16的字符串</param>
	/// <param name="iv">偏移量,长度为16的字符串</param>
	/// <returns>密文</returns>
	public static string EncodeAES(string text, string key, string iv)
	{
		byte[] plainText = Encoding.UTF8.GetBytes(text);
		return Convert.ToBase64String(EncodeAES(plainText, key, iv));
	}

	public static byte[] EncodeAES(byte[] value, string key, string iv) {
		RijndaelManaged rijndaelCipher = new RijndaelManaged();
		rijndaelCipher.Mode = CipherMode.CBC;
		rijndaelCipher.Padding = PaddingMode.Zeros;
		rijndaelCipher.KeySize = 128;
		rijndaelCipher.BlockSize = 128;
		byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(key);
		byte[] keyBytes = new byte[16];
		int len = pwdBytes.Length;
		if (len > keyBytes.Length)
			len = keyBytes.Length;
		System.Array.Copy(pwdBytes, keyBytes, len);
		rijndaelCipher.Key = keyBytes;
		rijndaelCipher.IV = Encoding.UTF8.GetBytes(iv);
		ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

		byte[] cipherBytes = transform.TransformFinalBlock(value, 0, value.Length);
		return cipherBytes;
	}

	/// <summary>AES解密</summary>
	/// <param name="text">密文</param>
	/// <param name="key">密钥,长度为16的字符串</param>
	/// <param name="iv">偏移量,长度为16的字符串</param>
	/// <returns>明文</returns>
	public static string DecodeAES(string text, string key, string iv)
	{
		byte[] encryptedData = Convert.FromBase64String(text);
		return Encoding.UTF8.GetString(DecodeAES(encryptedData,key,iv));
	}

	public static byte[] DecodeAES(byte[] encryptedData, string key, string iv)
	{
		RijndaelManaged rijndaelCipher = new RijndaelManaged();
		rijndaelCipher.Mode = CipherMode.CBC;
		rijndaelCipher.Padding = PaddingMode.Zeros;
		rijndaelCipher.KeySize = 128;
		rijndaelCipher.BlockSize = 128;
		
		byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(key);
		byte[] keyBytes = new byte[16];
		int len = pwdBytes.Length;
		if (len > keyBytes.Length)
			len = keyBytes.Length;
		System.Array.Copy(pwdBytes, keyBytes, len);
		rijndaelCipher.Key = keyBytes;
		rijndaelCipher.IV = Encoding.UTF8.GetBytes(iv);

		ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
		byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
		return plainText;
	}

	static nettool()
	{
		placeHolder = Convert.ToByte('l');
		byte[] tmp0 = new byte[] {
			Convert.ToByte ('0'), Convert.ToByte ('9'), Convert.ToByte ('_'),
				Convert.ToByte ('-'), Convert.ToByte ('1'), Convert.ToByte ('8'), Convert.ToByte ('6'),
				Convert.ToByte ('3'), Convert.ToByte ('4'), Convert.ToByte ('2'), Convert.ToByte ('7'),
				Convert.ToByte ('5'), Convert.ToByte ('#')
		};
		byte[] tmp1 = new byte[tmp0.Length];
		for (int i = 0; i < tmp1.Length; i++)
		{
			tmp1[i] = (byte)(i + 0x41);
		}
		byte[] tmp2 = new byte[tmp1.Length];
		for (int i = 0; i < tmp2.Length; i++)
		{
			tmp2[i] = (byte)(tmp1[tmp1.Length - 1] + i + 1);
		}
		byte[] tmp3 = new byte[tmp2.Length];
		for (int i = 0; i < tmp3.Length; i++)
		{
			tmp3[i] = (byte)(i + 0x61);
		}
		byte[] tmp4 = new byte[tmp3.Length];
		for (int i = 0; i < tmp3.Length; i++)
		{
			tmp4[i] = (byte)(tmp3[tmp3.Length - 1] + i + 1);
		}
		byte[][] data2Arr = new byte[][] { tmp3, tmp0, tmp1, tmp4, tmp2 };

		tmp4 = tmp3 = tmp2 = tmp1 = tmp0 = null;
		byte[] data65 = new byte[data2Arr[0].Length * data2Arr.Length];
		int quotient = 0;
		int remainder = 0;
		int idx = 0;
		for (int i = 0; i < data65.Length; i++)
		{
			quotient = i / 5;
			remainder = i % 5;
			data65[i] = data2Arr[remainder][quotient];
			if (data65[i] == placeHolder)
			{
				idx = i;
			}
		}
		byte[] data64 = new byte[data65.Length - 1];
		Array.Copy(data65, 0, data64, 0, idx);
		Array.Copy(data65, idx + 1, data64, idx, 64 - idx);
		encodingTable = data64;
		decodingTable = new byte[(data65.Length - 1) << 1];
		data65 = null;
		for (int i = 0; i < encodingTable.Length; i++)
		{
			decodingTable[encodingTable[i]] = (byte)i;
		}
	}

	public static byte[] encodebyte(byte[] data)
	{
		byte[] bytes;
		int modulus = data.Length % 3;
		if (modulus == 0)
		{
			bytes = new byte[(4 * data.Length) / 3];
		}
		else
		{
			bytes = new byte[4 * ((data.Length / 3) + 1)];
		}
		int dataLength = (data.Length - modulus);
		int a1;
		int a2;
		int a3;
		for (int i = 0, j = 0; i < dataLength; i += 3, j += 4)
		{
			a1 = data[i] & 0xff;
			a2 = data[i + 1] & 0xff;
			a3 = data[i + 2] & 0xff;
			bytes[j] = encodingTable[(foo(a1, 2)) & 0x3f];
			bytes[j + 1] = encodingTable[((a1 << 4) | (foo(a2, 4))) & 0x3f];
			bytes[j + 2] = encodingTable[((a2 << 2) | (foo(a3, 6))) & 0x3f];
			bytes[j + 3] = encodingTable[a3 & 0x3f];
		}
		int b1;
		int b2;
		int b3;
		int d1;
		int d2;
		switch (modulus)
		{
			case 0:
				break;
			case 1:
				d1 = data[data.Length - 1] & 0xff;
				b1 = (foo(d1, 2)) & 0x3f;
				b2 = (d1 << 4) & 0x3f;
				bytes[bytes.Length - 4] = encodingTable[b1];
				bytes[bytes.Length - 3] = encodingTable[b2];
				bytes[bytes.Length - 2] = placeHolder;
				bytes[bytes.Length - 1] = placeHolder;
				break;
			case 2:
				d1 = data[data.Length - 2] & 0xff;
				d2 = data[data.Length - 1] & 0xff;
				b1 = (foo(d1, 2)) & 0x3f;
				b2 = ((d1 << 4) | (foo(d2, 4))) & 0x3f;
				b3 = (d2 << 2) & 0x3f;
				bytes[bytes.Length - 4] = encodingTable[b1];
				bytes[bytes.Length - 3] = encodingTable[b2];
				bytes[bytes.Length - 2] = encodingTable[b3];
				bytes[bytes.Length - 1] = (byte)placeHolder;
				break;
		}
		return bytes;
	}

	public static byte[] decodebyte(byte[] data)
	{
		byte[] bytes;
		byte b1;
		byte b2;
		byte b3;
		byte b4;
		data = discardnonbase64bytes(data);
		if (data[data.Length - 2] == placeHolder)
		{
			bytes = new byte[(((data.Length / 4) - 1) * 3) + 1];
		}
		else if (data[data.Length - 1] == placeHolder)
		{
			bytes = new byte[(((data.Length / 4) - 1) * 3) + 2];
		}
		else
		{
			bytes = new byte[((data.Length / 4) * 3)];
		}

		for (int i = 0, j = 0; i < (data.Length - 4); i += 4, j += 3)
		{
			b1 = decodingTable[data[i]];
			b2 = decodingTable[data[i + 1]];
			b3 = decodingTable[data[i + 2]];
			b4 = decodingTable[data[i + 3]];
			bytes[j] = (byte)((b1 << 2) | (b2 >> 4));
			bytes[j + 1] = (byte)((b2 << 4) | (b3 >> 2));
			bytes[j + 2] = (byte)((b3 << 6) | b4);
		}
		if (data[data.Length - 2] == placeHolder)
		{
			b1 = decodingTable[data[data.Length - 4]];
			b2 = decodingTable[data[data.Length - 3]];
			bytes[bytes.Length - 1] = (byte)((b1 << 2) | (b2 >> 4));
		}
		else if (data[data.Length - 1] == placeHolder)
		{
			b1 = decodingTable[data[data.Length - 4]];
			b2 = decodingTable[data[data.Length - 3]];
			b3 = decodingTable[data[data.Length - 2]];
			bytes[bytes.Length - 2] = (byte)((b1 << 2) | (b2 >> 4));
			bytes[bytes.Length - 1] = (byte)((b2 << 4) | (b3 >> 2));
		}
		else
		{
			b1 = decodingTable[data[data.Length - 4]];
			b2 = decodingTable[data[data.Length - 3]];
			b3 = decodingTable[data[data.Length - 2]];
			b4 = decodingTable[data[data.Length - 1]];
			bytes[bytes.Length - 3] = (byte)((b1 << 2) | (b2 >> 4));
			bytes[bytes.Length - 2] = (byte)((b2 << 4) | (b3 >> 2));
			bytes[bytes.Length - 1] = (byte)((b3 << 6) | b4);
		}
		return bytes;
	}

	private static byte[] discardnonbase64bytes(byte[] data)
	{
		byte[] temp = new byte[data.Length];
		int bytesCopied = 0;
		for (int i = 0; i < data.Length; i++)
		{
			if (isvalidbase64byte(data[i]))
			{
				temp[bytesCopied++] = data[i];
			}
		}
		byte[] newData = new byte[bytesCopied];
		Array.Copy(temp, 0, newData, 0, bytesCopied);
		return newData;
	}

	private static bool isvalidbase64byte(byte b)
	{
		if (b == placeHolder)
		{
			return true;
		}
		else if ((b < 0) || (b >= 128))
		{
			return false;
		}
		return true;
	}

	private static int foo(int x, int y)
	{
		int mask = 0x7fffffff;
		for (int i = 0; i < y; i++)
		{
			x >>= 1;
			x &= mask;
		}
		return x;
	}
}