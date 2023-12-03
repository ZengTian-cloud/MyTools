using System;
using System.IO;
using Basics;
using CSharpZip.GZip;
using CSharpZip.Zip;

// 压缩工具
public static class ziptool
{
	private const int BUFFER_SIZE = 2048;
	private static int readstream = 0;
	private static byte[] bytebuffer = new byte[BUFFER_SIZE];

	public static byte[] onpressstream(byte[] binary)
	{
		MemoryStream mStream = new MemoryStream();
		GZipOutputStream tOutputStream = new GZipOutputStream(mStream);
		tOutputStream.Write(binary, 0, binary.Length);
		tOutputStream.Close();
		return mStream.ToArray();
	}

	public static byte[] unpressstream(byte[] binary)
	{
		GZipInputStream tInputStream = new GZipInputStream(new MemoryStream(binary));
		MemoryStream mStream = StreamToMemoryStream(tInputStream);
		return mStream.ToArray();
	}

	public static void onzippath(string tonePath, string toutputPath, string tpassword, Action<string> oencb, Action<int> allcb)
	{
		onziparray(new string[] { tonePath }, toutputPath, tpassword, oencb, allcb);
	}

	public static void onziparray(string[] tinPathArray, string toutputPath, string tpassword, Action<string> oencb, Action<int> allcb)
	{
		int zipStatus = NetStatus.OK;
		if (tinPathArray != null && !string.IsNullOrEmpty(toutputPath))
		{
			ZipOutputStream tOutputStream = new ZipOutputStream(File.Create(toutputPath));
			tOutputStream.SetLevel(6);
			if (!string.IsNullOrEmpty(tpassword))
			{
				tOutputStream.Password = tpassword;
			}

			for (int idx = 0; idx < tinPathArray.Length; idx++)
			{
				string tfileOrDirectory = tinPathArray[idx];
				if (Directory.Exists(tfileOrDirectory))
				{
					zipStatus = onzipfolder(tfileOrDirectory, string.Empty, tOutputStream, oencb);
				}
				else if (File.Exists(tfileOrDirectory))
				{
					zipStatus = onzipfile(tfileOrDirectory, string.Empty, tOutputStream, oencb);
				}

				if (zipStatus != NetStatus.OK)
				{
					break;
				}
			}

			if (zipStatus == NetStatus.OK)
			{
				tOutputStream.Finish();
			}
			tOutputStream.Close();
		}
		else
		{
			zipStatus = NetStatus.CNotFind;
		}
		allcb?.Invoke(zipStatus);
	}

	public static void unzippath(string tzippath, string toutputPath, string tpassword, Action<string> oencb, Action<int> allcb)
	{
		int zipStatus = NetStatus.OK;
		if (!string.IsNullOrEmpty(tzippath) && !string.IsNullOrEmpty(toutputPath))
		{
			zipStatus = unzipfile(File.OpenRead(tzippath), toutputPath, tpassword, oencb);
		}
		else
		{
			zipStatus = NetStatus.CNotFind;
		}
		allcb?.Invoke(zipStatus);
	}

	public static void unzipbytes(byte[] vzipbytes, string toutputPath, string tpassword, Action<string> oencb, Action<int> allcb)
	{
		int zipStatus = NetStatus.OK;
		if (vzipbytes != null && !string.IsNullOrEmpty(toutputPath))
		{
			zipStatus = unzipfile(new MemoryStream(vzipbytes), toutputPath, tpassword, oencb);
		}
		else
		{
			zipStatus = NetStatus.CNotFind;
		}
		allcb?.Invoke(zipStatus);
	}

	private static int onzipfolder(string tfolderpath, string tparentRelPath, ZipOutputStream tOutputStream, Action<string> oencb)
	{
		ZipEntry zentry = null;

		try
		{
			string entryName = pathtool.combine(tparentRelPath, Path.GetFileName(tfolderpath));
			zentry = new ZipEntry(entryName + "/");
			zentry.DateTime = System.DateTime.Now;
			zentry.Size = 0;

			tOutputStream.PutNextEntry(zentry);
			tOutputStream.Flush();

			string[] vfolder = Directory.GetDirectories(tfolderpath);
			for (int idx = 0; idx < vfolder.Length; idx++)
			{
				onzipfolder(vfolder[idx], entryName, tOutputStream, oencb);
			}

			string[] vfile = Directory.GetFiles(tfolderpath);
			for (int idx = 0; idx < vfile.Length; idx++)
			{
				onzipfile(vfile[idx], entryName, tOutputStream, oencb);
			}
		}
		catch (IOException ioEx)
		{
			return NetException.CheckIOException(ioEx);
		}
		catch
		{
			return NetStatus.CSystemException;
		}

		return NetStatus.OK;
	}

	private static int onzipfile(string tfilePathName, string tparentRelPath, ZipOutputStream tOutputStream, Action<string> oencb)
	{
		ZipEntry zentry = null;

		try
		{
			string entryName = pathtool.combine(tparentRelPath, Path.GetFileName(tfilePathName));
			zentry = new ZipEntry(entryName);
			zentry.DateTime = System.DateTime.Now;

			byte[] buffer = File.ReadAllBytes(tfilePathName);
			zentry.Size = buffer.Length;

			tOutputStream.PutNextEntry(zentry);
			tOutputStream.Write(buffer, 0, buffer.Length);
		}
		catch (IOException ioEx)
		{
			return NetException.CheckIOException(ioEx);
		}
		catch
		{
			return NetStatus.CSystemException;
		}

		oencb?.Invoke(tfilePathName);

		return NetStatus.OK;
	}

	private static int unzipfile(Stream tinputStream, string toutputPath, string tpassword, Action<string> oencb)
	{
		if (tinputStream != null && !string.IsNullOrEmpty(toutputPath))
		{
			if (!Directory.Exists(toutputPath))
			{
				Directory.CreateDirectory(toutputPath);
			}

			ZipEntry zentry = null;
			using (ZipInputStream zinputStream = new ZipInputStream(tinputStream))
			{
				if (!string.IsNullOrEmpty(tpassword))
				{
					zinputStream.Password = tpassword;
				}

				while ((zentry = zinputStream.GetNextEntry()) != null)
				{
					if (string.IsNullOrEmpty(zentry.Name))
					{
						continue;
					}

					string tfilepath = pathtool.combine(toutputPath, zentry.Name);

					if (zentry.IsDirectory)
					{
						Directory.CreateDirectory(tfilepath);
						continue;
					}
					pathtool.checkfiledirectory(tfilepath);
					MemoryStream mStream = StreamToMemoryStream(zinputStream);
					var temstatus = filetool.writeallbytes(tfilepath, mStream.ToArray());
					oencb?.Invoke(tfilepath);
					if (temstatus != NetStatus.OK)
					{
						return temstatus;
					}
				}
			}
		}
		else
		{
			return NetStatus.CNotFind;
		}

		return NetStatus.OK;
	}

	private static MemoryStream StreamToMemoryStream(Stream instream)
	{
		var outstream = new MemoryStream();
		while ((readstream = instream.Read(bytebuffer, 0, BUFFER_SIZE)) > 0)
		{
			outstream.Write(bytebuffer, 0, readstream);
		}
		return outstream;
	}
}