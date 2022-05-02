using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;         //匯入網路通訊協定相關函數
using System.Net.Sockets; //匯入網路插座功能函數
using System.Threading;

namespace TCP_Client
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        Socket T;    //通訊物件
        Thread TH; //監聽執行序
        string User; //使用者
        //登入伺服器 
        private void button1_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            string IP = textBox1.Text;                                 //伺服器IP
            int Port = int.Parse(textBox2.Text);                       //伺服器Port
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), Port); //伺服器的連線端點資訊
            //建立可以雙向通訊的TCP連線
            T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            User = textBox3.Text;                                     //使用者名稱 
            try
            {
                T.Connect(EP);                        //連上伺服器的端點EP(類似撥號給電話總機)
                TH = new Thread(Listen);
                TH.IsBackground = true;
                TH.Start();
                textBox4.Text = "已連線伺服器！" + "\r\n";
                
                Send("0" + User);                     //連線後隨即傳送自己的名稱給伺服器
            }
            catch (Exception)
            {
                textBox4.Text = "無法連線伺服器！" + "\r\n";
                //MessageBox.Show("無法連上伺服器！");  //連線失敗時顯示訊息
                return;
            }
            button1.Enabled = false;                  //讓連線按鍵失效，避免重複連線 
            button_send.Enabled = true;
        }
        //傳送訊息給 Server (Send Message to the Server)
        private void Send(string Str)
        {
            byte[] B = Encoding.Default.GetBytes(Str);//翻譯字串Str為Byte陣列B
            T.Send(B, 0, B.Length, SocketFlags.None); //使用連線物件傳送資料
        }
        //關閉視窗代表離線登出 
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button1.Enabled == false)
            {
                Send("9" + User); //傳送自己的離線訊息給伺服器
                T.Close();        //關閉網路通訊器T
            }
        }
        private void Listen()
        {
            EndPoint ServerEP = (EndPoint)T.RemoteEndPoint;
            byte[] B = new byte[1023];
            int inLen = 0;
            string Msg; //接收到的完整訊息
            string St; //指令碼
            string Str; //訊息內容

            while (true)
            {
                try
                {
                    inLen = T.ReceiveFrom(B, ref ServerEP);
                }
                catch(Exception e)
                {
                    T.Close();
                    listBox1.Items.Clear();
                    MessageBox.Show("伺服器斷線了！！");
                    button1.Enabled = true;
                    TH.Abort();
                }
                Msg = Encoding.Default.GetString(B, 0, inLen);
                St = Msg.Substring(0, 1);
                Str = Msg.Substring(1);
                switch (St)
                {
                    case "L":
                        listBox1.Items.Clear();
                        string[] M = Str.Split(',');
                        foreach (string user in M)
                        {
                            listBox1.Items.Add(user);
                        }
                        break;
                    case "1": //接收到廣播訊息
                        textBox4.Text += "(公開)" + Str + "\r\n";
                        textBox4.SelectionStart = textBox4.Text.Length;
                        textBox4.ScrollToCaret();
                        break;
                    case "2": //接收到私密訊息
                        textBox4.Text += "(私密)" + Str + "\r\n";
                        textBox4.SelectionStart = textBox4.Text.Length;
                        textBox4.ScrollToCaret();
                        break;
                }
            }
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            if (textBox_Send.Text == "") return;
            if (listBox1.SelectedIndex < 0) //廣播
            {
                Send("1" + User + "公告" + textBox_Send.Text);
            }
            else
            {
                Send("2" + "來自" + User + ":" + textBox_Send.Text + "|");
                textBox_Send.Text += "告訴" + listBox1.SelectedItem + ":" + textBox_Send + "\r\n";
            }
            textBox_Send.Text = "";
        }

        private void button_snndAll_Click(object sender, EventArgs e)
        {
            listBox1.ClearSelected();
        }
    }
}
