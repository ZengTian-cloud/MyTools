using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NetWork {
	public class BufferRead {
		private List<byte> listbyte = new List<byte> ();
		private int position = 0;

		public BufferRead (byte[] data, int tpos) {
			listbyte.Clear ();
			listbyte.AddRange (data);
			position = tpos;
		}

		public Int64 ReadInt64 () {
			byte[] temp = ReadBytes (8);
			Int64 data64 = BitConverter.ToInt64 (temp, 0);
			return IPAddress.NetworkToHostOrder (data64);
		}

		public Int32 ReadInt32 () {
			byte[] temp = ReadBytes (4);
			Int32 data32 = BitConverter.ToInt32 (temp, 0);
			return IPAddress.NetworkToHostOrder (data32);
		}

		public Int16 ReadInt16 () {
			byte[] temp = ReadBytes (2);
			Int16 data16 = BitConverter.ToInt16 (temp, 0);
			return IPAddress.NetworkToHostOrder (data16);
		}

		public Char ReadInt8 () {
			return BitConverter.ToChar (ReadBytes (1), 0);
		}

		public string ReadString (int buffLength) {
			return Encoding.UTF8.GetString (ReadBytes (buffLength));
		}

		public byte[] ReadBytes (int buffLength) {
			int tnum = 0;
			byte[] temp = new byte[buffLength];
			while (tnum < buffLength && position < listbyte.Count) {
				temp[tnum] = listbyte[position];
				tnum++;
				position++;
			}
			return temp;
		}
	}
}