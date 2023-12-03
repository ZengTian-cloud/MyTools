using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static RSA;
using UnityEngine;

public class RsaHelp
{

    private static int PROV_RSA_FULL = 1;
    static int AT_KEYEXCHANGE = 1;

    public static string PrivateKey = "";
    public static string PublicKey = "";
    public static string ServerPublicKey = "";
    private static byte[] AesKey;

    private static RSA  rsa = null;

    //私钥加密priEncrypt，公钥解密pubDecrypt

    public static RSAKEY getKeyPair() {
        RSA rsa = new RSA();
        PrivateKey = rsa.GetKey().PrivateKey;
        PublicKey = rsa.GetKey().PublicKey;
        return rsa.GetKey();
    }

    private static void RandomEncryptKey()
    {
        Guid g = Guid.NewGuid();
        string value = g.ToString();
        value = value.Replace("-", "");
        Debug.Log("密钥："+value);
        AesKey = Encoding.UTF8.GetBytes(value);
        Debug.Log("aes key size:" + AesKey.Length);
    }

    public static string getEncryptKey() {
        RandomEncryptKey();
        return Encoding.UTF8.GetString(EncryptServerPublicKey(AesKey));
    }

    /// <summary>
    /// 使用客户端私钥加密
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] EncryptPrivateKey(byte[] data)
    {
        RSA rsa = new RSA();
        return rsa.EncryptByPrivateKey(data, PrivateKey);
    }

    /// <summary>
    /// 使用服务端公钥进行加密，用于2号命令将本地公钥加密后传给服务器
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] EncryptServerPublicKey(byte[] data)
    {
        RSA rsa = new RSA();
        return Encoding.UTF8.GetBytes(EncryptServerPublicKey(Encoding.UTF8.GetString(data)));
    }

    public static byte[] EncryptServerPublicKey(RSA rsa, byte[] data)
    {
        return Encoding.UTF8.GetBytes(EncryptServerPublicKey(Encoding.UTF8.GetString(data)));
    }

    /// <summary>
    /// 使用服务端公钥进行加密，用于2号命令将本地公钥加密后传给服务器
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string EncryptServerPublicKey(string data)
    {
        RSA rsa = new RSA();
        return rsa.EncryptByPublicKey(data, ServerPublicKey);
    }

    /// <summary>
    /// 使用服务端公钥解密
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] DecryptPublicKey(byte[] data)
    {
        RSA rsa = new RSA();
        return rsa.DecryptByPublicKey(data, ServerPublicKey);
    }

    /// <summary>
    /// 使用AES进行加密 用于业务
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] EncryptByAes( byte[] data)
    {
        try
        {
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = AesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);

            return resultArray;
        }
        catch (Exception e)
        {

            Debug.LogError("对称加密失败," + e.ToString());
        }
        return data;
    }

    /// <summary>
    /// 使用AES进行解密  用于业务解密
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] DecryptByAes( byte[] data)
    {
        try
        {
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = AesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
            return resultArray;
        }
        catch (Exception e)
        {
            Debug.LogError("对称解密失败，" + e.ToString());
        }
        return data;
    }

    public static byte[] Base64Decode(string value)
    {
        return Convert.FromBase64String(value);
    }
    public static string Base64Encode(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

}

