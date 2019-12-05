using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TestLight
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

      public  class RecordItem {
            public String text;
            public int answer;
            public RecordItem(String text,int answer) {
                this.text = text;
                this.answer = answer;
            }

            public bool isFlash() {
                return (answer & TYPE_HANDLE_FLASH) == TYPE_HANDLE_FLASH;
            }

            public bool isDangerous()
            {
                return (answer & TYPE_DANGUROUS) == TYPE_DANGUROUS;
            }

            public int getSwitchValue() {
                return (MASK_SWITCH) & answer;
            }

            public int getHandleValue() {
                return (MASK_HANDLE) & answer;
            }
        }


        public const int TYPE_SWITCH_CLOSE = 0;
        public const int TYPE_SWITCH_SMALL = 1;
        public const int TYPE_SWITCH_OPEN =2;







        public const int HANDLE_SHIFT = 3;
        public const int MASK_SWITCH = (TYPE_SWITCH_SMALL | TYPE_SWITCH_OPEN| TYPE_SWITCH_CLOSE);

        //灯光操作功能区
        public const int TYPE_HANDLE_FLASH = 2 << HANDLE_SHIFT;//闪光
        public const int TYPE_HANDLE_FAR = 1 << HANDLE_SHIFT;//远光
        public const int TYPE_HANDLE_CLOSE = 0 << HANDLE_SHIFT;//关闭



        public const int MASK_HANDLE = TYPE_HANDLE_CLOSE | TYPE_HANDLE_FLASH| TYPE_HANDLE_FAR;


        public const int TYPE_DANGUROUS = 1<<7;


        //        string str= "(1) 夜间在没有路灯，照明不良条件下行驶; (大灯)

        //(2) 夜间在窄路与非机动车会车;(近光)

        //　　(3) 夜间同方向近距离跟车行驶;(近光)

        //　　(4) 夜间与机动车会车(近光)

        //　　(5) 夜间通过拱桥、人行横道;(远近光交替)

        //　　(6) 夜间通过急弯、坡路;(远近光交替)

        //　　(7) 夜间通过没有交通信号灯控制的路口;(远近光交替)

        //　　(8) 夜间在道路上发生故障，妨碍交通又难以移动。(示宽灯、危险报警灯);
        List<RecordItem> lists = new List<RecordItem>();
        private void Form1_Load(object sender, EventArgs e)
        {

            lists.Add(new RecordItem("夜间与机动车会车", TYPE_SWITCH_OPEN));
            lists.Add(new RecordItem("夜间在照明良好的道路行驶", TYPE_SWITCH_OPEN));
            lists.Add(new RecordItem("夜间在同方向近距离跟车行驶", TYPE_SWITCH_OPEN));
            lists.Add(new RecordItem("夜间通过路口", TYPE_SWITCH_OPEN));

            lists.Add(new RecordItem("夜间通过急弯、坡路", TYPE_SWITCH_OPEN | TYPE_HANDLE_FLASH));
            lists.Add(new RecordItem("夜间超越前方车辆", TYPE_SWITCH_OPEN | TYPE_HANDLE_FLASH));



            lists.Add(new RecordItem("路边临时停车", TYPE_SWITCH_SMALL | TYPE_DANGUROUS));

            lists.Add(new RecordItem("夜间在没有路灯，照明不良条件下行驶", TYPE_SWITCH_OPEN | TYPE_HANDLE_FAR));

        
            lists.Add(new RecordItem("夜间通过没有交通信号灯控制的路口", TYPE_SWITCH_OPEN | TYPE_HANDLE_FLASH));



            randomHandle();
        }




        private void button3_Click(object sender, EventArgs e)
        {
            randomHandle();
        }

        private void randomHandle() {
            Random r = new Random();
            selVarSwitch = r.Next(3);

            selVarHandle = r.Next(2);

            selVarDangus = r.Next(2);

            updateUi();
        }

        private void updateUi() {
            RadioButton [] butons = {radioButton1,radioButton2,radioButton3 };
            butons[selVarSwitch].Checked = true;
            RadioButton[] butons2 = { radioButton4, radioButton5 };
            butons2[selVarHandle].Checked = true;

            checkBox1.Checked = selVarDangus == 1;
        }


        public int selVarSwitch;
        public int selVarHandle;
        public int selVarDangus;

        int testStatus= STATUS_END;

        const int STATUS_END = 0;
        const int STATUS_TESTING = 1;
        const int STATUS_SUCCESS = 2;

        int maxTestCount;

        int MAX_WAITTING_TIME = 5 * 1000;//5秒作答时间


        int waittingTime =0;

        int clickTimes;

        private void button1_Click(object sender, EventArgs e)
        {

            if (testStatus == STATUS_END||testStatus== STATUS_SUCCESS)
            {
                testStatus = STATUS_TESTING;
            }
            else {
                testStatus = STATUS_END;
            }

            MAX_TEST_COUNT=int.Parse(textBox1.Text);
            if (MAX_TEST_COUNT > lists.Count|| MAX_TEST_COUNT<=1) {
                MAX_TEST_COUNT = 5;
                textBox1.Text = MAX_TEST_COUNT + "";
            }

            MAX_WAITTING_TIME = int.Parse(textBox2.Text);

            if (MAX_WAITTING_TIME > 20 || MAX_WAITTING_TIME <= 1)
            {
                MAX_WAITTING_TIME = 5;
                textBox2.Text = MAX_WAITTING_TIME + "";
            }
            MAX_WAITTING_TIME *= 1000;

            updateTestStatus();
        }




        public void updateTestStatus() {
            if (testStatus == STATUS_END)
            {
                timer1.Enabled = false;
                button1.Text = "开始考试";
                label3.Text = "结束...";
                label2.Text = "...";
            }
            else if (testStatus == STATUS_TESTING)
            {
                waittingTime = -1;
                maxTestCount = -1;
                RandomSortList(lists);
                timer1.Enabled = true;
                button1.Text = "结束考试";
            }
            else if(testStatus== STATUS_SUCCESS)
            {
                timer1.Enabled = false;
                button1.Text = "开始考试";
                label3.Text = "测试完成，合格";
                label2.Text = "...";
                readText(label3.Text);
            }
        }

        RecordItem itemCur;

        RadioButton[] switchs;
        RadioButton[] handles;
        private void checkAnswer() {
            if (waittingTime == 0) {
                if (itemCur != null) {
                    if (switchs == null) {
                        switchs = new RadioButton[] { radioButton1, radioButton2, radioButton3 };
                        handles = new RadioButton[] { radioButton5, radioButton4 };
                    }
                    int ansSwitch = getRadioButtonCheckedIndex(switchs);

                    int ansHandle = getRadioButtonCheckedIndex(handles);


                    LogUtil.Print(Convert.ToString(itemCur.answer,2) + "  answer");

                    LogUtil.Print(Convert.ToString(ansSwitch,2) + "  ansSwitch");
                    LogUtil.Print(Convert.ToString((ansHandle << HANDLE_SHIFT),2) + "  ansHandle");



                    LogUtil.Print(Convert.ToString(MASK_SWITCH, 2) + "  MASK_SWITCH");

                    LogUtil.Print(Convert.ToString(MASK_HANDLE, 2) + "  MASK_HANDLE");

                    LogUtil.Print(Convert.ToString(itemCur.getSwitchValue(),2) + "  getSwitchValue");
                    LogUtil.Print(Convert.ToString(itemCur.getHandleValue(), 2) + "  getHandleValue");

                    int result = ansSwitch | (ansHandle << HANDLE_SHIFT);
                    if (checkBox1.Checked) {
                        result |= TYPE_DANGUROUS;
                    }

                    LogUtil.Print(Convert.ToString(result, 2) + "  middle");
                    if (clickTimes >= 2) {
                        LogUtil.Print(Convert.ToString(MASK_HANDLE, 2) + "  HANDLE MASK");
                       
                        result &= ~MASK_HANDLE;
                        result |= TYPE_HANDLE_FLASH;
                    }

                    LogUtil.Print(Convert.ToString(result, 2) + "  result");
                    if (itemCur.answer == result) {
                        return;
                    }
                    testStatus = STATUS_END;
                    timer1.Enabled = false;

                    int handleVar = (itemCur.getHandleValue() >> HANDLE_SHIFT);
                    int switchVar = itemCur.getSwitchValue();
                    string text = "考试不合格!\n 正确答案是:开关->" + switchs[switchVar].Text + " 操作区->" + (itemCur.isFlash() ? "闪光灯" : handles[handleVar].Text) + " 危险报警闪光灯->" + (itemCur.isDangerous() ? "开" : "关");
                    readText(text);
                    MessageBox.Show(text);
                }

            }
        }

        private void readText(string str) {
            System.Speech.Synthesis.SpeechSynthesizer sp = new System.Speech.Synthesis.SpeechSynthesizer();
            sp.SpeakAsync(str);
        }


        public static int getRadioButtonCheckedIndex(params RadioButton[] buttons)
        {
            int index = 0;
            foreach (RadioButton button in buttons)
            {
                if (button.Checked)
                {
                    return index;
                }
                index++;
            }

            return -1;

        }

        int MAX_TEST_COUNT = 5;

        private void timer1_Tick(object sender, EventArgs e)
        {



            checkAnswer();

            if (testStatus == STATUS_END) {
                updateTestStatus();
                return;
            }

            if (maxTestCount+1 < MAX_TEST_COUNT)
            {
                if (waittingTime <=0)
                {
                    waittingTime = MAX_WAITTING_TIME;
                    maxTestCount++;
                    clickTimes = 0;
                    RecordItem item = lists[maxTestCount];
                    itemCur = item;
                    label2.Text = string.Format("第{0:d}/{1:d}题 ",maxTestCount+1, MAX_TEST_COUNT) +item.text;
                    readText(item.text);
                }
            }
            else {
                if (waittingTime <= 0) {
                    testStatus = STATUS_SUCCESS;
                    updateTestStatus();
                    return;
                }
            }
            waittingTime -= timer1.Interval;
            label3.Text = waittingTime / 1000f + "s";

        }


        //打乱List数组
        public void RandomSortList<T>(List<T> ListT)
        {

            System.Random random = new System.Random();
            List<T> newList = new List<T>();
            for (int i = 0; i < ListT.Count; i++) {

                int exchangeIndex = random.Next(ListT.Count);
                T tt = ListT[i];
                ListT[i] = ListT[exchangeIndex];
                ListT[exchangeIndex] = tt;
            }
     
        }

        private void button1_Click2(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            clickTimes++;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            waittingTime = 0;
            itemCur = lists[0];
            label2.Text = itemCur.text + " " + itemCur.answer;
            checkAnswer();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //using (MailMessage mailMessage = new MailMessage())
            //{
            //    SmtpClient smtpClient = new SmtpClient("smtp.qq.com");
            //    mailMessage.To.Add("986850427@qq.com");
            //    mailMessage.To.Add("991763318@qq.com");

            //    mailMessage.Body = "测试邮件";

            //    mailMessage.From = new MailAddress("986850427@qq.com");

            //    mailMessage.Subject = "邮件标题";

            //    smtpClient.Credentials = new System.Net.NetworkCredential("986850427@qq.com", "ujsidfwvvyrybffe");//如果启用了“客户端授权码”，要用授权码代替密码 

            //    smtpClient.Send(mailMessage);
            //}
        }
    }
}
