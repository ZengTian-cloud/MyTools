using System.Net;
using UnityEngine;
using CSharpZip.GZip;

namespace NetWork.socket
{

    public class CodecFactory
    {

        public static byte[] EncodeMessage(IGameMessage request)
        {
            GameMessageHeader header = request.GetHeader();
            int totalSize = header.getHeaderSize();

            byte[] bodyBytes = request.Write();

            if (bodyBytes != null)
            {
                
               //TODO 添加包体是否压缩的判断
                if (!header.IsForcedUncompression && bodyBytes.Length > GameClient.CompressSize)
                {
                    header.IsCompress = true;
                    //TODO 对消息进行压缩
                    bodyBytes = GZip.GZipCompress(bodyBytes);
                }
                //TODO 对消息进行加密
                Debug.Log("EncodeMessage - messageID:" + request.GetHeader().MessageId + " -bodyBytes.Length:" + bodyBytes.Length);
                if (!header.IsForcedUnencryption && bodyBytes.Length > 0)
                {
                    header.IsEncrypt = true;
                    bodyBytes = RsaHelp.EncryptByAes(bodyBytes);
                }
                totalSize += bodyBytes.Length;
            }

            header.MessageSize = totalSize;
            ByteBuf byteBuf = new ByteBuf(totalSize);
            // byteBuf.WriteInt(totalSize);
            header.Write(byteBuf);

            if (bodyBytes != null)
            {
                
                byteBuf.WriteBytes(bodyBytes);
               
            }
            return byteBuf.ToArray();
        }

        public static void DecodeMessage(ByteBuf byteBuf, IGameMessage gameMessage)
        {
            GameMessageHeader header = gameMessage.GetHeader();
            header.Read(byteBuf);
            if (byteBuf.IsReadable())
            {
                //错误码，要读取错误数据
                int size = byteBuf.ReadableBytes();
                byte[] bytes = new byte[size];
                byteBuf.ReadBytes(bytes);

                // TODO:(2023-7-20 11:38). 先解密, 再解压..
                if (header.IsEncrypt && bytes.Length > 0)
                {
                    //TODO 对消息进行解密
                    //bytes = Aes
                    bytes = RsaHelp.DecryptByAes(bytes);
                }
                if (header.IsCompress)
                {
                    //TODO 服务器对数据进行了压缩，这里要进行一下解压缩
                    bytes = GZip.GZipDecompress(bytes);

                }
                gameMessage.Read(bytes);
                /*
                if (header.ErrorCode != 0)
                {
                    DecodeMessage(byteBuf, header, gameMessage, header.ErrorLen);
                }
                else 
                {
                    
                    DecodeMessage(byteBuf,header,gameMessage,size);
                }*/
                
            }

        }
    }
}
