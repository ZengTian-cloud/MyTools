using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NetWork {
	public class BufferWrite {
		private List<byte> listbyte = new List<byte> ();

		public BufferWrite () {
			listbyte.Clear ();
		}

		public void WriteInt64 (Int64 value) {
			Int64 data64 = IPAddress.HostToNetworkOrder (value);
			byte[] temp = BitConverter.GetBytes (data64);
			listbyte.AddRange (temp);
		}

		public void WriteInt32 (Int32 value) {
			Int32 data32 = IPAddress.HostToNetworkOrder (value);
			byte[] temp = BitConverter.GetBytes (data32);
			listbyte.AddRange (temp);
		}

		public void WriteInt16 (Int16 value) {
			Int16 data16 = IPAddress.HostToNetworkOrder (value);
			byte[] temp = BitConverter.GetBytes (data16);
			listbyte.AddRange (temp);
		}

		public void WriteInt8 (Char value) {
			listbyte.AddRange (BitConverter.GetBytes (value));
		}

		public void WriteString (string value) {
			listbyte.AddRange (Encoding.UTF8.GetBytes (value));
		}

		public void WriteBytes (byte[] value) {
			listbyte.AddRange (value);
		}

		public byte[] ToBytes () {
			return listbyte.ToArray ();
		}
	}
}