using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.IO;

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Linq;
using System.Net;

namespace PreviewDemo
{
	/// <summary>
	/// Form1 µÄÕªÒªËµÃ÷¡£
	/// </summary>
	public class Preview : System.Windows.Forms.Form
	{
		private CHCNetSDK.VOICEDATACALLBACKV30 _voiceData;

        //inside 'btnVioceTalk_Click' event handler
        //if (_voicedata == null) {
        //   _voicedata = new chcnetsdk.voicedatacallbackv30(voicedatacallback);
        //}
        private byte[] totalByte = new byte[0];
        private int totalLen;
		public static string fileName;
	private uint iLastErr = 0;
		private static Int32 m_lUserID = -1;
		private bool m_bInitSDK = false;
        private static bool m_bRecord = false;
        private bool m_bTalk = false;
		private static Int32 m_lRealHandle = -1;
        private int lVoiceComHandle = -1;
        private string str;

        CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;

        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnPreview;
		private System.Windows.Forms.PictureBox RealPlayWnd;
        private TextBox textBoxIP;
        private TextBox textBoxPort;
        private TextBox textBoxUserName;
        private TextBox textBoxPassword;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Button btnBMP;
        private Button btnJPEG;
        private Label label11;
        private Label label12;
        private Label label13;
        private TextBox textBoxChannel;
        private Button btnRecord;
        private Label label14;
        private Button btn_Exit;
        private Button btnVioceTalk;
        private Label label16;
        private Label label17;
        private TextBox textBoxID;
        /*private Button PtzGet;
        private Button PtzSet;*/
        private Label label19;
        /*private ComboBox comboBox1;
        private TextBox textBoxPanPos;
        private TextBox textBoxTiltPos;
        private TextBox textBoxZoomPos;*/
        private Label label20;
        private Label label21;
        private Label label22;
        private Button PreSet;
        private Label label23;

        //private GroupBox groupBox1;

        /// <summary>
        /// ±ØÐèµÄÉè¼ÆÆ÷±äÁ¿¡£
        /// </summary>
        private System.ComponentModel.Container components = null;
		private HttpListener _httpListener;

		public Preview()
		{
			//
			// Windows ´°ÌåÉè¼ÆÆ÷Ö§³ÖËù±ØÐèµÄ
			//
			InitializeComponent();
			m_bInitSDK = CHCNetSDK.NET_DVR_Init();
			if (m_bInitSDK == false)
			{
				MessageBox.Show("NET_DVR_Init error!");
				return;
			}
			else
			{
                //±£´æSDKÈÕÖ¾ To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
			}

			Console.WriteLine("Hello, World!");
			string path = null;
			string ShowData = null;
			List<long> chatIds = new List<long>();
			CHCNetSDK.MSGCallBack m_falarmData = null;
			Int32 m_lGetCardCfgHandle = -1;   // Create and start the timer

			// Keep the application running
			Console.WriteLine("Press [Enter] to exit.");
			Console.ReadLine();
			//var p = new Program();
			//Login();
			btnLogin_Click();
			btnPreview_Click();
			
			_httpListener = new HttpListener();
			_httpListener.Prefixes.Add("http://localhost:8080/");
			_httpListener.Start();

			Thread listenerThread = new Thread(() =>
			{
				while (true)
				{
					HttpListenerContext context = _httpListener.GetContext();
					string responseString = "Command received";

					if (context.Request.Url.AbsolutePath == "/trigger")
					{
						// Trigger some action in the Windows Forms app
						Invoke(new Action(() => TriggerAction()));
					}

					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
					context.Response.ContentLength64 = buffer.Length;
					using (var output = context.Response.OutputStream)
					{
						output.Write(buffer, 0, buffer.Length);
					}
				}
			});
			listenerThread.Start();
			//_timer = new System.Threading.Timer(TimerCallback, null, TimeSpan.Zero, _interval);
			//p.btnLogin_Click();
			//p.btnPreview_Click();
			//p.btnRecord_Click();
			// Îæèäàíèå íàæàòèÿ êëàâèøè äëÿ çàâåðøåíèÿ ïðîãðàììû
			Console.WriteLine("Íàæìèòå [Enter] äëÿ âûõîäà èç ïðîãðàììû.");
			Console.ReadLine();
			//
			// TODO: ÔÚ InitializeComponent µ÷ÓÃºóÌí¼ÓÈÎºÎ¹¹Ôìº¯Êý´úÂë
			//
		}

		private void TriggerAction()
		{
			// Example action: show a message box
			Console.WriteLine("Action Triggered!"); 
			

				btnRecord_Click();

				btnVioceTalk_Click().GetAwaiter();
		}
		private static System.Threading.Timer _timer1;
		private static TimeSpan _interval1 = TimeSpan.FromSeconds(10);
		private static async void TimerCallback(object state)
		{
			await WaitThenStartAsync(10);
			// Wrap the async call in a Task to ensure it runs properly
			await Task.Run(async () =>
			{
			});
		}
		static async Task WaitThenStartAsync(int seconds)
		{
			
			// Wait asynchronously for the specified number of seconds
			await Task.Delay(TimeSpan.FromSeconds(seconds));

			// Call the method after the delay
			btnRecord_Click();
		}
		/// <summary>
		/// ÇåÀíËùÓÐÕýÔÚÊ¹ÓÃµÄ×ÊÔ´¡£
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (m_lRealHandle >= 0)
			{
				CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
			}
			if (m_lUserID >= 0)
			{
				CHCNetSDK.NET_DVR_Logout(m_lUserID);
			}
			if (m_bInitSDK == true)
			{
				CHCNetSDK.NET_DVR_Cleanup();
			}
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows ´°ÌåÉè¼ÆÆ÷Éú³ÉµÄ´úÂë
		/// <summary>
		/// Éè¼ÆÆ÷Ö§³ÖËùÐèµÄ·½·¨ - ²»ÒªÊ¹ÓÃ´úÂë±à¼­Æ÷ÐÞ¸Ä
		/// ´Ë·½·¨µÄÄÚÈÝ¡£
		/// </summary>
		private void InitializeComponent()
        {
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnLogin = new System.Windows.Forms.Button();
			this.btnPreview = new System.Windows.Forms.Button();
			this.RealPlayWnd = new System.Windows.Forms.PictureBox();
			this.textBoxIP = new System.Windows.Forms.TextBox();
			this.textBoxPort = new System.Windows.Forms.TextBox();
			this.textBoxUserName = new System.Windows.Forms.TextBox();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.btnBMP = new System.Windows.Forms.Button();
			this.btnJPEG = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.textBoxChannel = new System.Windows.Forms.TextBox();
			this.btnRecord = new System.Windows.Forms.Button();
			this.label14 = new System.Windows.Forms.Label();
			this.btn_Exit = new System.Windows.Forms.Button();
			this.btnVioceTalk = new System.Windows.Forms.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.textBoxID = new System.Windows.Forms.TextBox();
			this.PreSet = new System.Windows.Forms.Button();
			this.label23 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.RealPlayWnd)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 21);
			this.label1.TabIndex = 34;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 21);
			this.label2.TabIndex = 35;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(83, 21);
			this.label3.TabIndex = 36;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(0, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(83, 21);
			this.label4.TabIndex = 37;
			// 
			// btnLogin
			// 
			this.btnLogin.Location = new System.Drawing.Point(362, 35);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(66, 47);
			this.btnLogin.TabIndex = 1;
			this.btnLogin.Text = "Login";
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// btnPreview
			// 
			this.btnPreview.Location = new System.Drawing.Point(14, 530);
			this.btnPreview.Name = "btnPreview";
			this.btnPreview.Size = new System.Drawing.Size(64, 32);
			this.btnPreview.TabIndex = 7;
			this.btnPreview.Text = "Live View";
			this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
			// 
			// RealPlayWnd
			// 
			this.RealPlayWnd.BackColor = System.Drawing.SystemColors.WindowText;
			this.RealPlayWnd.Location = new System.Drawing.Point(15, 97);
			this.RealPlayWnd.Name = "RealPlayWnd";
			this.RealPlayWnd.Size = new System.Drawing.Size(413, 376);
			this.RealPlayWnd.TabIndex = 4;
			this.RealPlayWnd.TabStop = false;
			this.RealPlayWnd.Click += new System.EventHandler(this.RealPlayWnd_Click);
			// 
			// textBoxIP
			// 
			this.textBoxIP.Location = new System.Drawing.Point(65, 22);
			this.textBoxIP.Name = "textBoxIP";
			this.textBoxIP.Size = new System.Drawing.Size(95, 20);
			this.textBoxIP.TabIndex = 2;
			this.textBoxIP.Text = "10.18.37.120";
			this.textBoxIP.TextChanged += new System.EventHandler(this.textBoxIP_TextChanged);
			// 
			// textBoxPort
			// 
			this.textBoxPort.Location = new System.Drawing.Point(257, 22);
			this.textBoxPort.Name = "textBoxPort";
			this.textBoxPort.Size = new System.Drawing.Size(93, 20);
			this.textBoxPort.TabIndex = 3;
			this.textBoxPort.Text = "8003";
			this.textBoxPort.TextChanged += new System.EventHandler(this.textBoxPort_TextChanged);
			// 
			// textBoxUserName
			// 
			this.textBoxUserName.Location = new System.Drawing.Point(65, 65);
			this.textBoxUserName.Name = "textBoxUserName";
			this.textBoxUserName.Size = new System.Drawing.Size(95, 20);
			this.textBoxUserName.TabIndex = 4;
			this.textBoxUserName.Text = "admin";
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxPassword.Location = new System.Drawing.Point(257, 65);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(93, 20);
			this.textBoxPassword.TabIndex = 5;
			this.textBoxPassword.Text = "abcd_";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 31);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(54, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Device IP";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(195, 31);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(63, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Device Port";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 73);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(60, 13);
			this.label7.TabIndex = 11;
			this.label7.Text = "User Name";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(197, 73);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(53, 13);
			this.label8.TabIndex = 12;
			this.label8.Text = "Password";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(15, 511);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(44, 13);
			this.label9.TabIndex = 13;
			this.label9.Text = "preview";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(0, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(83, 21);
			this.label10.TabIndex = 33;
			// 
			// btnBMP
			// 
			this.btnBMP.Location = new System.Drawing.Point(92, 531);
			this.btnBMP.Name = "btnBMP";
			this.btnBMP.Size = new System.Drawing.Size(66, 32);
			this.btnBMP.TabIndex = 8;
			this.btnBMP.Text = "Capture BMP ";
			this.btnBMP.UseVisualStyleBackColor = true;
			this.btnBMP.Click += new System.EventHandler(this.btnBMP_Click);
			// 
			// btnJPEG
			// 
			this.btnJPEG.Location = new System.Drawing.Point(173, 530);
			this.btnJPEG.Name = "btnJPEG";
			this.btnJPEG.Size = new System.Drawing.Size(81, 32);
			this.btnJPEG.TabIndex = 9;
			this.btnJPEG.Text = "Capture JPEG";
			this.btnJPEG.UseVisualStyleBackColor = true;
			this.btnJPEG.Click += new System.EventHandler(this.btnJPEG_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(94, 511);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(69, 13);
			this.label11.TabIndex = 17;
			this.label11.Text = "BMP capture";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(178, 511);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(73, 13);
			this.label12.TabIndex = 18;
			this.label12.Text = "JPEG capture";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(14, 484);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(126, 13);
			this.label13.TabIndex = 19;
			this.label13.Text = "preview/capture channel";
			// 
			// textBoxChannel
			// 
			this.textBoxChannel.Location = new System.Drawing.Point(142, 480);
			this.textBoxChannel.Name = "textBoxChannel";
			this.textBoxChannel.Size = new System.Drawing.Size(46, 20);
			this.textBoxChannel.TabIndex = 6;
			this.textBoxChannel.Text = "1";
			// 
			// btnRecord
			// 
			this.btnRecord.Location = new System.Drawing.Point(266, 530);
			this.btnRecord.Name = "btnRecord";
			this.btnRecord.Size = new System.Drawing.Size(83, 32);
			this.btnRecord.TabIndex = 10;
			this.btnRecord.Text = "Start Record";
			this.btnRecord.UseVisualStyleBackColor = true;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(267, 511);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(79, 13);
			this.label14.TabIndex = 22;
			this.label14.Text = "client recording";
			// 
			// btn_Exit
			// 
			this.btn_Exit.Location = new System.Drawing.Point(365, 571);
			this.btn_Exit.Name = "btn_Exit";
			this.btn_Exit.Size = new System.Drawing.Size(63, 30);
			this.btn_Exit.TabIndex = 11;
			this.btn_Exit.Tag = "";
			this.btn_Exit.Text = "Exit";
			this.btn_Exit.UseVisualStyleBackColor = true;
			this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
			// 
			// btnVioceTalk
			// 
			this.btnVioceTalk.Location = new System.Drawing.Point(15, 595);
			this.btnVioceTalk.Name = "btnVioceTalk";
			this.btnVioceTalk.Size = new System.Drawing.Size(63, 32);
			this.btnVioceTalk.TabIndex = 25;
			this.btnVioceTalk.Text = "Start Talk";
			this.btnVioceTalk.UseVisualStyleBackColor = true;
			this.btnVioceTalk.Click += new System.EventHandler(this.btnVioceTalk_Click);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(15, 577);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(77, 13);
			this.label16.TabIndex = 26;
			this.label16.Text = "TwoWayAudio";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(198, 483);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(52, 13);
			this.label17.TabIndex = 27;
			this.label17.Text = "stream ID";
			// 
			// textBoxID
			// 
			this.textBoxID.Location = new System.Drawing.Point(250, 479);
			this.textBoxID.Name = "textBoxID";
			this.textBoxID.Size = new System.Drawing.Size(188, 20);
			this.textBoxID.TabIndex = 28;
			this.textBoxID.TextChanged += new System.EventHandler(this.textBoxID_TextChanged);
			// 
			// PreSet
			// 
			this.PreSet.Location = new System.Drawing.Point(96, 595);
			this.PreSet.Name = "PreSet";
			this.PreSet.Size = new System.Drawing.Size(81, 31);
			this.PreSet.TabIndex = 31;
			this.PreSet.Text = "PTZ Control";
			this.PreSet.UseVisualStyleBackColor = true;
			this.PreSet.Click += new System.EventHandler(this.PreSet_Click);
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(99, 577);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(63, 13);
			this.label23.TabIndex = 32;
			this.label23.Text = "PTZ control";
			// 
			// Preview
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(531, 687);
			this.Controls.Add(this.label23);
			this.Controls.Add(this.PreSet);
			this.Controls.Add(this.textBoxID);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.btnVioceTalk);
			this.Controls.Add(this.btn_Exit);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.btnRecord);
			this.Controls.Add(this.textBoxChannel);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.btnJPEG);
			this.Controls.Add(this.btnBMP);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBoxPassword);
			this.Controls.Add(this.textBoxUserName);
			this.Controls.Add(this.textBoxPort);
			this.Controls.Add(this.textBoxIP);
			this.Controls.Add(this.RealPlayWnd);
			this.Controls.Add(this.btnPreview);
			this.Controls.Add(this.btnLogin);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Name = "Preview";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Preview";
			this.Load += new System.EventHandler(this.Preview_Load);
			((System.ComponentModel.ISupportInitialize)(this.RealPlayWnd)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private static System.Threading.Timer _timer;
		private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Semaphore with a single slot
		private static TimeSpan _interval = TimeSpan.FromSeconds(5); // Timer interval

		/// <summary>
		/// Ó¦ÓÃ³ÌÐòµÄÖ÷Èë¿Úµã¡£
		/// </summary>
		[STAThread]
		 static void Main()
		{
			var p = new Preview();
			Application.Run(p);

			//Console.WriteLine("Hello, World!");
			//string path = null;
			//string ShowData = null;
			//List<long> chatIds = new List<long>();
			//CHCNetSDK.MSGCallBack m_falarmData = null;
			//Int32 m_lGetCardCfgHandle = -1;   // Create and start the timer

			//// Keep the application running
			//Console.WriteLine("Press [Enter] to exit.");
			//Console.ReadLine();
			////var p = new Program();
			////Login();
			//p.btnLogin_Click();
			//p.btnPreview_Click();
			//Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
			//Thread.Sleep(TimeSpan.FromSeconds(10));
			//btnRecord_Click();
			//_timer = new System.Threading.Timer(btnVioceTalk_Click, null, TimeSpan.Zero, _interval);
			////p.btnLogin_Click();
			////p.btnPreview_Click();
			////p.btnRecord_Click();
			//// Îæèäàíèå íàæàòèÿ êëàâèøè äëÿ çàâåðøåíèÿ ïðîãðàììû
			//Console.WriteLine("Íàæìèòå [Enter] äëÿ âûõîäà èç ïðîãðàììû.");
			//Console.ReadLine();
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		 void btnLogin_Click()
		{
				if (textBoxIP.Text == "" || textBoxPort.Text == "" ||
					textBoxUserName.Text == "" || textBoxPassword.Text == "")
				{
					Console.WriteLine("Please input IP, Port, User name and Password!");
					return;
				}
				if (m_lUserID < 0)
				{
					string DVRIPAddress = "147.30.77.222";
					//string DVRIPAddress = "147.30.77.222"; 
					//string DVRIPAddress = "192.168.1.74"; 
					//string DVRIPAddress = "192.168.8.101"; 
					//string DVRIPAddress = "5.76.34.219"; 
					//string DVRIPAddress = "145.249.185.61"; //Éè±¸IPµØÖ·»òÕßÓòÃû
                Int16 DVRPortNumber = Int16.Parse("8000");//Éè±¸·þÎñ¶Ë¿ÚºÅ
                string DVRUserName = "admin";//Éè±¸µÇÂ¼ÓÃ»§Ãû
                string DVRPassword = "001002aa";//Éè±¸µÇÂ¼ÃÜÂë

                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //µÇÂ¼Éè±¸ Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //µÇÂ¼Ê§°Ü£¬Êä³ö´íÎóºÅ
                    Console.WriteLine(str);
                    return;
                }
                else
                {
                    //µÇÂ¼³É¹¦
                    MessageBox.Show("Login Success!");
                    btnLogin.Text = "Logout";
                }
            }
            else
            {
                //×¢ÏúµÇÂ¼ Logout the device
                if (m_lRealHandle >= 0)
                {
                    MessageBox.Show("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;           
                }
                m_lUserID = -1;
                btnLogin.Text = "Login";
            }
            return;
		}

		 void btnPreview_Click()
		{
            if(m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//Ô¤ÀÀ´°¿Ú
                lpPreviewInfo.lChannel = Int16.Parse("1");//Ô¤teÀÀµÄÉè±¸Í¨µÀ
                lpPreviewInfo.dwStreamType = 0;//ÂëÁ÷ÀàÐÍ£º0-Ö÷ÂëÁ÷£¬1-×ÓÂëÁ÷£¬2-ÂëÁ÷3£¬3-ÂëÁ÷4£¬ÒÔ´ËÀàÍÆ
                lpPreviewInfo.dwLinkMode = 4;//Á¬½Ó·½Ê½£º0- TCP·½Ê½£¬1- UDP·½Ê½£¬2- ¶à²¥·½Ê½£¬3- RTP·½Ê½£¬4-RTP/RTSP£¬5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- ·Ç×èÈûÈ¡Á÷£¬1- ×èÈûÈ¡Á÷
                lpPreviewInfo.dwDisplayBufNum = 1; //²¥·Å¿â²¥·Å»º³åÇø×î´ó»º³åÖ¡Êý
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;


                if (textBoxID.Text != "")
                {
                    lpPreviewInfo.lChannel = -1;
                    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                    lpPreviewInfo.byStreamID = new byte[32];
                    byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
                }


                if (RealData == null)
                {
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//Ô¤ÀÀÊµÊ±Á÷»Øµ÷º¯Êý
                }
                
                IntPtr pUser = new IntPtr();//ÓÃ»§Êý¾Ý

                //´ò¿ªÔ¤ÀÀ Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //Ô¤ÀÀÊ§°Ü£¬Êä³ö´íÎóºÅ
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //Ô¤ÀÀ³É¹¦
                    btnPreview.Text = "Stop Live View";
                }
            }
            else
            {
                //Í£Ö¹Ô¤ÀÀ Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                btnPreview.Text = "Live View";

            }
            return;
		}

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
		{
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                string str = "asas.ps";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();            
            }
		}

        private void btnBMP_Click(object sender, EventArgs e)
        {
            string sBmpPicFileName;
            //Í¼Æ¬±£´æÂ·¾¶ºÍÎÄ¼þÃû the path and file name to save
            sBmpPicFileName = "BMP_test.bmp"; 

            //BMP×¥Í¼ Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandle, sBmpPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CapturePicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the BMP file and the saved file is " + sBmpPicFileName;
                MessageBox.Show(str); 
            }
            return;
        }

        private void btnJPEG_Click(object sender, EventArgs e)
        {
            string sJpegPicFileName;
            //Í¼Æ¬±£´æÂ·¾¶ºÍÎÄ¼þÃû the path and file name to save
            sJpegPicFileName = "JPEG_test.jpg";

            int lChannel = Int16.Parse(textBoxChannel.Text); //Í¨µÀºÅ Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //Í¼ÏñÖÊÁ¿ Image quality
            lpJpegPara.wPicSize = 0xff; //×¥Í¼·Ö±æÂÊ Picture size: 2- 4CIF£¬0xff- Auto(Ê¹ÓÃµ±Ç°ÂëÁ÷·Ö±æÂÊ)£¬×¥Í¼·Ö±æÂÊÐèÒªÉè±¸Ö§³Ö£¬¸ü¶àÈ¡ÖµÇë²Î¿¼SDKÎÄµµ

            //JPEG×¥Í¼ Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                MessageBox.Show(str);
            }
            return;
        }

        static void btnRecord_Click()
        {
			//Â¼Ïñ±£´æÂ·¾¶ºÍÎÄ¼þÃû the path and file name to save
			string sVideoFileName;
			sVideoFileName = "c:\\j\\full.mp4";

				//Ç¿ÖÆIÖ¡ Make a I frame
				int lChannel = Int16.Parse("1"); //Í¨µÀºÅ Channel number
				CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, lChannel);

				//¿ªÊ¼Â¼Ïñ Start recording
				if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName))
				{
					return;
				}
				else
				{
					m_bRecord = true;
				}
				Thread.Sleep(15000);
				//Í£Ö¹Â¼Ïñ Stop recording
				if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
				{

					return;
				}
				else
				{
					m_bRecord = false;
				}

			return;
		}

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            //Í£Ö¹Ô¤ÀÀ Stop live view 
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //×¢ÏúµÇÂ¼ Logout the device
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        private void btnPTZ_Click(object sender, EventArgs e)
        {

        }

		private static string subscriptionKey = "5ecfb4e8657441d389f7640466560c11"; // Çàìåíèòå íà âàø êëþ÷ API
		private static string serviceRegion = "eastus"; // Çàìåíèòå íà âàø ðåãèîí, íàïðèìåð, "westus"


		async Task RecognizeSpeechFromPcmFile()
		{
			// Path to the audio file (make sure it's in a supported format like WAV)
			string audioFilePath = $"C:\\j\\{fileName}.wav";

			var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);
			speechConfig.SpeechRecognitionLanguage = "ru-RU";
			//speechConfig.SpeechRecognitionLanguage = "kk-KZ";
			//speechConfig.SpeechRecognitionLanguage = "ar-EG";
			var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);

			var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

			Console.WriteLine("Recognizing...");
			try { 
			var result = await recognizer.RecognizeOnceAsync();

			if (result.Reason == ResultReason.RecognizedSpeech)
			{
				Console.WriteLine($"Recognized: {result.Text}");
				string url = "https://gettuktuk.com/houses/code/hikvision";
				string r = new string(result.Text.Where(char.IsDigit).ToArray());
				try {
					var codeOrKv = Convert.ToInt32(r);
					string jsonData = "{ \"action\": \"activation_code\", \"house_id\": \"303\", \"value\": \"" + Convert.ToInt32(r) + "\" }";
					if (codeOrKv < 9999)
					{
						jsonData = "{ \"action\": \"room_id\", \"house_id\": \"303\", \"value\": \"" + Convert.ToInt32(r) + "\" }";
					}
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
				catch (Exception ex)
				{

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
		}catch(Exception ex)
			{

			}
		}

		public byte[] AddByteToArray(byte[] bArray, byte newByte)
		{
			byte[] newArray = new byte[bArray.Length + 1];
			bArray.CopyTo(newArray, 1);
			newArray[0] = newByte;
			return newArray;
		}

		public static byte[] Combine(params byte[][] byteArrays)
		{
			int totalLength = 0;
			foreach (byte[] array in byteArrays)
			{
				totalLength += array.Length;
			}

			byte[] result = new byte[totalLength];
			int currentIndex = 0;

			foreach (byte[] array in byteArrays)
			{
				Buffer.BlockCopy(array, 0, result, currentIndex, array.Length);
				currentIndex += array.Length;
			}

			return result;
		}

		public void VoiceDataCallBack(int lVoiceComHandle, IntPtr pRecvDataBuffer, uint dwBufSize, byte byAudioFlag, System.IntPtr pUser)
        {
            byte[] sString = new byte[dwBufSize];
            
            totalByte = Combine(totalByte, sString);
            totalLen += (int)dwBufSize;
			Marshal.Copy(pRecvDataBuffer, sString, 0, (Int32)dwBufSize);
   //         if (byAudioFlag == 0)
   //         {
   //             string str = $"C:\\j\\sound1{Guid.NewGuid()}.pcm";
   //             FileStream fs = new FileStream(str, FileMode.Create);
   //             int iLen = (int)dwBufSize;
   //             fs.Write(sString, 0, iLen);
   //             fs.Close();
			//	//ResampleTo16kHz("C:\\j\\sound1.wav", "C:\\j\\sound11.wav");
			//	//RecognizeSpeechFromPcmFile("C:\\j\\sound11.pcm").GetAwaiter().GetResult();

			//}
   //         if (byAudioFlag == 1)
   //         {
   //             string str = $"C:\\j\\sound2{Guid.NewGuid()}.pcm";
   //             FileStream fs = new FileStream(str, FileMode.Create);
   //             int iLen = (int)dwBufSize;
   //             fs.Write(sString, 0, iLen);
   //             fs.Close();
   // //            ResampleTo16kHz("C:\\j\\sound2.wav", "C:\\j\\sound22.wav");
			//	//RecognizeSpeechFromPcmFile("C:\\j\\sound22.wav").GetAwaiter().GetResult();
   //         }

        }


		public static void ResampleTo16kHz(string inputFilePath, string outputFilePath)
		{
			using (var reader = new AudioFileReader(inputFilePath))
			{
				// Create a new WaveFormat for 16kHz, Mono
				var newFormat = new WaveFormat(16000, 16, 1);

				// Resample and convert to mono
				using (var resampler = new WaveFormatConversionStream(newFormat, reader))
				{
					// Save the result
					WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
				}
			}
		}

		private static short[] ByteArrayToShortArray(byte[] byteArray)
		{
			short[] shortArray = new short[byteArray.Length / 2];
			for (int i = 0; i < shortArray.Length; i++)
			{
				shortArray[i] = BitConverter.ToInt16(byteArray, i * 2);
			}
			return shortArray;
		}

		private static byte[] ShortArrayToByteArray(short[] shortArray)
		{
			byte[] byteArray = new byte[shortArray.Length * 2];
			for (int i = 0; i < shortArray.Length; i++)
			{
				BitConverter.GetBytes(shortArray[i]).CopyTo(byteArray, i * 2);
			}
			return byteArray;
		}

		static void ConvertMp4ToWav(string inputFilePath, string outputFilePath, string ffmpegPath)
		{
			try
			{
				// Ensure that the input file exists
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

				// Build the ffmpeg command arguments
				string arguments = $"-i \"{inputFilePath}\" -acodec pcm_s16le -ar 44100 -ac 2 \"{outputFilePath}\"";

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
					try
					{
						process.Start();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error starting FFmpeg process: " + ex.Message);
						return;
					}

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
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
			}
		}
	
	 async Task btnVioceTalk_Click()
		{
			Console.WriteLine($"òàéìåð ïðîêðóòèëñÿ");
			string mp4FilePath = "c:\\j\\full.mp4"; 
			Random random = new Random();
			fileName = random.Next(100, 1000).ToString();
			string wavFilePath = $"c:\\j\\{fileName}.wav";

			// Path to ffmpeg executable
			string ffmpegPath = "C:\\ffmpeg-2024-08-18-git-7e5410eadb-essentials_build\\ffmpeg-2024-08-18-git-7e5410eadb-essentials_build\\bin\\ffmpeg.exe"; // e.g., "C:\\ffmpeg\\bin\\ffmpeg.exe"
			
			ConvertMp4ToWav(mp4FilePath, wavFilePath, ffmpegPath); 
			await RecognizeSpeechFromPcmFile();
			string filePath = $"c:\\j\\{fileName}.wav";
			ForceDeleteFileCmd(filePath);
			ForceDeleteFileCmd(mp4FilePath);
			//try
			//{
			//	// Ïðîâåðÿåì, ñóùåñòâóåò ëè ôàéë
			//	if (System.IO.File.Exists(filePath))
			//	{
			//		// Óäàëÿåì ôàéë
			//		System.IO.File.Delete(filePath);
			//		Console.WriteLine("Ôàéë óñïåøíî óäàëåí.");
			//	}
			//	else
			//	{
			//		Console.WriteLine("Ôàéë íå íàéäåí.");
			//	}
			//}
			//catch (Exception ex)
			//{
			//	// Îáðàáîòêà îøèáîê
			//	Console.WriteLine($"Îøèáêà ïðè óäàëåíèè ôàéëà: {ex.Message}");
			//}
		}

		 static void ForceDeleteFileCmd(string filePath)
		{
			var processInfo = new ProcessStartInfo("cmd.exe", $"/C del /F \"{filePath}\"")
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (var process = Process.Start(processInfo))
			{
				process.WaitForExit();

				if (process.ExitCode == 0)
				{
					Console.WriteLine("File deleted successfully using cmd.");
				}
				else
				{
					string error = process.StandardError.ReadToEnd();
					Console.WriteLine($"Failed to delete file using cmd: {error}");
				}
			}
		}

		private void label18_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void Preview_Load(object sender, EventArgs e)
        {

        }

        private void Ptz_Set_Click(object sender, EventArgs e)
        {

        }

        private void PreSet_Click(object sender, EventArgs e)
        {
            PreSet dlg = new PreSet();
            dlg.m_lUserID = m_lUserID;
            dlg.m_lChannel = 1;
            dlg.m_lRealHandle = m_lRealHandle;
            dlg.ShowDialog();
            
        }

        private void RealPlayWnd_Click(object sender, EventArgs e)
        {

        }

		private void textBoxIP_TextChanged(object sender, EventArgs e)
		{

		}

		private void textBoxPort_TextChanged(object sender, EventArgs e)
		{

		}

		private void textBoxID_TextChanged(object sender, EventArgs e)
		{

		}

		private void btnLogin_Click(object sender, EventArgs e)
		{

		}

		private void btnVioceTalk_Click(object sender, EventArgs e)
		{

		}

		private void btnPreview_Click(object sender, EventArgs e)
		{

		}
	}
}
