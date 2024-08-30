using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleApp1
{
	internal class Program
	{

		[STAThread]
		public static void Main(string[] args)
		{
			uint iLastErr = 0;
			Int32 m_lUserID = -1;
			bool m_bInitSDK = false;
			bool m_bRecord = false;
			bool m_bTalk = false;
			Int32 m_lRealHandle = -1;
			int lVoiceComHandle = -1;
			string str;

			CHCNetSDK.REALDATACALLBACK RealData = null;
			CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;
			Timer _timer;
			//var p = new Program();
			m_bInitSDK = CHCNetSDK.NET_DVR_Init();
			//p.btnLogin_Click();
			//p.btnPreview_Click();
			//p.btnRecord_Click();
			_timer = new Timer(5000);

			// Подписка на событие Elapsed
			//_timer.Elapsed += btnVioceTalk_Click;

			// Запуск таймера
			_timer.AutoReset = true; // Устанавливаем AutoReset в true, чтобы таймер повторялся
			_timer.Enabled = true;
			// Ожидание нажатия клавиши для завершения программы
			Console.WriteLine("Нажмите [Enter] для выхода из программы.");
			Console.ReadLine();


			void OnTimedEvent(Object source, ElapsedEventArgs e)
			{
				Console.WriteLine($"Событие вызвано в {e.SignalTime}");
			}
			void btnVioceTalk_Click(Object sender, ElapsedEventArgs e)
			{
				Console.WriteLine($"Событие вызвано в {e.SignalTime}");
				string mp4FilePath = "c:\\j\\Record_test.mp4";
				string wavFilePath = "c:\\j\\recorddd.wav";

				// Path to ffmpeg executable
				string ffmpegPath = "C:\\Users\\н\\Downloads\\ffmpeg-2024-08-18-git-7e5410eadb-essentials_build\\ffmpeg-2024-08-18-git-7e5410eadb-essentials_build\\bin\\ffmpeg.exe"; // e.g., "C:\\ffmpeg\\bin\\ffmpeg.exe"

				ConvertMp4ToWav(mp4FilePath, wavFilePath, ffmpegPath);
				RecognizeSpeechFromPcmFile();
				string filePath = "c:\\j\\recorddd.wav";

				try
				{
					// Проверяем, существует ли файл
					if (File.Exists(filePath))
					{
						// Удаляем файл
						File.Delete(filePath);
						Console.WriteLine("Файл успешно удален.");
					}
					else
					{
						Console.WriteLine("Файл не найден.");
					}
				}
				catch (Exception ex)
				{
					// Обработка ошибок
					Console.WriteLine($"Ошибка при удалении файла: {ex.Message}");
				}
			}
			string subscriptionKey = "5ecfb4e8657441d389f7640466560c11"; // Замените на ваш ключ API
			string serviceRegion = "eastus"; // Замените на ваш регион, например, "westus"



			async Task RecognizeSpeechFromPcmFile()
			{
				// Path to the audio file (make sure it's in a supported format like WAV)
				string audioFilePath = "C:\\j\\recorddd.wav";

				var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);
				speechConfig.SpeechRecognitionLanguage = "ru-RU";
				var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);

				var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

				Console.WriteLine("Recognizing...");

				var result = await recognizer.RecognizeOnceAsync();

				if (result.Reason == ResultReason.RecognizedSpeech)
				{
					Console.WriteLine($"Recognized: {result.Text}");
					string url = "https://gettuktuk.com/houses/code/hikvision";
					string jsonData = "{ \"action\": \"activation_code\", \"value\": \"" + Convert.ToInt32(result.Text) + "\" }";

					using (HttpClient client = new HttpClient())
					{
						HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

						try
						{
							HttpResponseMessage response = await client.PostAsync(url, content);

							if (response.IsSuccessStatusCode)
							{
								string responseBody = await response.Content.ReadAsStringAsync();
								Console.WriteLine("Response:");
								Console.WriteLine(responseBody);
							}
							else
							{
								Console.WriteLine($"Error: {response.StatusCode}");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Exception: {ex.Message}");
						}
					}
				}
				else if (result.Reason == ResultReason.NoMatch)
				{
					Console.WriteLine("No speech could be recognized.");
				}
				else if (result.Reason == ResultReason.Canceled)
				{
					var cancellation = CancellationDetails.FromResult(result);
					Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

					if (cancellation.Reason == CancellationReason.Error)
					{
						Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
						Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
					}
				}
			}

			void ConvertMp4ToWav(string inputFilePath, string outputFilePath, string ffmpegPath)
			{
				// Ensure that the input file existsF
				if (!File.Exists(inputFilePath))
				{
					Console.WriteLine("Input file does not exist.");
					return;
				}

				// Ensure that the ffmpeg executable exists
				if (!File.Exists(ffmpegPath))
				{
					Console.WriteLine("FFmpeg executable not found.");
					return;
				}

				// Build the ffmpeg command
				string arguments = $"-i \"{inputFilePath}\" \"{outputFilePath}\"";

				// Start the ffmpeg process
				using (var process = new Process())
				{
					process.StartInfo.FileName = ffmpegPath;
					process.StartInfo.Arguments = arguments;
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.CreateNoWindow = true;

					// Start the process
					process.Start();

					// Read the output and error streams
					string output = process.StandardOutput.ReadToEnd();
					string error = process.StandardError.ReadToEnd();

					// Wait for the process to complete
					process.WaitForExit();

					// Output results
					Console.WriteLine("FFmpeg output:");
					Console.WriteLine(output);

					if (process.ExitCode != 0)
					{
						Console.WriteLine("FFmpeg error:");
						Console.WriteLine(error);
					}
					else
					{
						Console.WriteLine("Conversion completed successfully.");
					}
				}
			}
			void btnPreview_Click()
			{
				if (m_lUserID < 0)
				{
					Console.WriteLine("Please login the device firstly");
					return;
				}

				if (m_lRealHandle < 0)
				{
					CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
					lpPreviewInfo.dwStreamType = 0;//ВлБчАаРНЈє0-ЦчВлБчЈ¬1-ЧУВлБчЈ¬2-ВлБч3Ј¬3-ВлБч4Ј¬ТФґЛАаНЖ
					lpPreviewInfo.dwLinkMode = 0;//Б¬ЅУ·ЅКЅЈє0- TCP·ЅКЅЈ¬1- UDP·ЅКЅЈ¬2- ¶аІҐ·ЅКЅЈ¬3- RTP·ЅКЅЈ¬4-RTP/RTSPЈ¬5-RSTP/HTTP 
					lpPreviewInfo.bBlocked = true; //0- ·ЗЧиИыИЎБчЈ¬1- ЧиИыИЎБч
					lpPreviewInfo.dwDisplayBufNum = 1; //ІҐ·ЕївІҐ·Е»єіеЗшЧоґу»єіеЦЎКэ
					lpPreviewInfo.byProtoType = 0;
					lpPreviewInfo.byPreviewMode = 0;




					if (RealData == null)
					{
						RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//Ф¤ААКµК±Бч»ШµчєЇКэ
					}

					IntPtr pUser = new IntPtr();//УГ»§КэѕЭ

					//ґтїЄФ¤АА Start live view 
					m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
					if (m_lRealHandle < 0)
					{
						iLastErr = CHCNetSDK.NET_DVR_GetLastError();
						str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //Ф¤ААК§°ЬЈ¬КдіцґнОуєЕ
						return;
					}
					else
					{
						//Ф¤ААіЙ№¦
					}
				}
				else
				{
					//НЈЦ№Ф¤АА Stop live view 
					if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
					{
						iLastErr = CHCNetSDK.NET_DVR_GetLastError();
						str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
						return;
					}
					m_lRealHandle = -1;

				}
				return;
			}

			void btnLogin_Click()
			{
				string DVRIPAddress = "192.168.1.74"; 
				//string DVRIPAddress = "147.30.77.222"; //Йи±ёIPµШЦ·»тХЯУтГы
				Int16 DVRPortNumber = Int16.Parse("8000");//Йи±ё·юОс¶ЛїЪєЕ
				string DVRUserName = "admin";//Йи±ёµЗВјУГ»§Гы
				string DVRPassword = "001002aa";//Йи±ёµЗВјГЬВл

				CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

				//µЗВјЙи±ё Login the device
				m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
				if (m_lUserID < 0)
				{
					iLastErr = CHCNetSDK.NET_DVR_GetLastError();
					str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //µЗВјК§°ЬЈ¬КдіцґнОуєЕ
					return;
				}
				else
				{
					//µЗВјіЙ№¦
				}
			}
			void btnRecord_Click()
			{
				//ВјПс±ЈґжВ·ѕ¶єНОДјюГы the path and file name to save
				string sVideoFileName;
				sVideoFileName = "c:\\j\\Record_test.mp4";

				if (m_bRecord == false)
				{
					//ЗїЦЖIЦЎ Make a I frame
					int lChannel = Int16.Parse("1"); //НЁµАєЕ Channel number
					CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, lChannel);

					//їЄКјВјПс Start recording
					if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName))
					{
						iLastErr = CHCNetSDK.NET_DVR_GetLastError();
						str = "NET_DVR_SaveRealData failed, error code= " + iLastErr;
						Console.WriteLine(str);
						return;
					}
					else
					{
						m_bRecord = true;
					}
				}
				else
				{
					//НЈЦ№ВјПс Stop recording
					if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
					{
						iLastErr = CHCNetSDK.NET_DVR_GetLastError();
						str = "NET_DVR_StopSaveRealData failed, error code= " + iLastErr;
						Console.WriteLine(str);
						return;
					}
					else
					{
						str = "Successful to stop recording and the saved file is " + sVideoFileName;
						Console.WriteLine(str);
						m_bRecord = false;
					}
				}

				return;
			}


			void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
			{
				if (dwBufSize > 0)
				{
					byte[] sData = new byte[dwBufSize];
					Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

					//string str = "asas.ps";
					//FileStream fs = new FileStream(str, FileMode.Create);
					//int iLen = (int)dwBufSize;
					//fs.Write(sData, 0, iLen);
					//fs.Close();
				}
			}
		}
	}
}
