using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WMPLib;
using TrainCrew;

namespace 車掌用知らせ灯
{
    public partial class Form1 : Form
    {
        private Timer timer; // UI更新用タイマー
        private StringBuilder sb = new StringBuilder(); // 状態表示用テキスト
        private int IsAvail = 0; // 音声ファイルが利用可能かどうか
        private int IsPlayed = 0; // 発車放送が再生されたか
        private int IsStopped = 0; // 停止状態のフラグ
        private string sound; // 音声ファイルのパス

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;

            // 初期化処理
            TrainCrewInput.Init();

            // タイマー設定
            timer = new Timer();
            timer.Tick += Timer_Tick;
            timer.Tick += CircleDraw;
            timer.Interval = 200;
            timer.Start();

            // 音声ファイルの確認
            IsAudioFileAvail();

            // チェックボックスの初期状態設定
            checkBox1.Checked = true;
        }

        // タイマーのTickイベント
        private void Timer_Tick(object sender, EventArgs e)
        {
            var state = TrainCrewInput.GetTrainState();

            sb.Clear();
            sb.AppendLine("速度: " + state.Speed.ToString("0.0km/h"));
            sb.AppendLine("戸閉: " + state.AllClose);
            sb.AppendLine(state.nextStaName + " " + state.nextStopType);
            sb.AppendLine("残り: " + state.nextStaDistance + "m");
            sb.AppendLine("制限: " + state.speedLimit + "km/h");
        }

        // フォームが閉じられるときの処理
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TrainCrewInput.Dispose();
        }

        // 発車放送再生の処理
        private void CircleDraw(object sender, EventArgs e)
        {
            var state = TrainCrewInput.GetTrainState();

            if (state.AllClose && IsPlayed == 0 && IsStopped == 0 && IsAvail == 1 && !string.IsNullOrEmpty(sound))
            {
                var player = new WindowsMediaPlayer
                {
                    URL = sound
                };
                player.controls.play();
                IsPlayed = 1;
            }
            else if (!state.AllClose)
            {
                IsPlayed = 0; // 戸閉が解除されたらリセット
            }
        }

        // 音声ファイルの存在確認
        private void IsAudioFileAvail()
        {
            try
            {
                string soundFilePath = Path.Combine(Application.StartupPath, "departure.wav");
                if (File.Exists(soundFilePath))
                {
                    sound = soundFilePath;
                    label1.Text = "発車放送が見つかりました";
                    IsAvail = 1;
                }
                else
                {
                    label1.Text = "発車放送が見つかりません";
                }
            }
            catch (Exception ex)
            {
                label1.Text = "エラー: " + ex.Message;
            }
        }

        // 停止状態の切り替え
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            IsStopped = checkBox2.Checked ? 1 : 0;
        }

        // 車種判定とUI更新
        private void Hantei()
        {
            if (checkBox1.Checked)
            {
                try
                {
                    var state = TrainCrewInput.GetTrainState();
                    string carmodel = state.CarStates[0].CarModel;

                    if (carmodel == "3300V" || carmodel == "5600" || carmodel == "4600")
                    {
                        IsStopped = 0;
                        label2.Text = "ワンマン車両";
                    }
                    else
                    {
                        IsStopped = 1;
                        label2.Text = "ワンマン車両でない";
                    }
                }
                catch
                {
                    label2.Text = "車種判定不能";
                }
            }
            else
            {
                IsStopped = 0;
                label2.Text = "チェックボックスが押下されていない";
            }
        }

        // チェックボックス変更時の処理
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Hantei();
        }

        // ボタン押下時の処理
        private void button1_Click(object sender, EventArgs e)
        {
            Hantei();
        }
    }
}
