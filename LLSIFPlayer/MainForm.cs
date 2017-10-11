using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Dapplo.Windows.Input;
using Dapplo.Windows.Input.Enums;

namespace LLSIFPlayer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void WriteLine(string str)
        {
            BeginInvoke(new Action(() => button1.Text = str));
        }

        private void hookKeyboard()
        {
            KeyboardHook.KeyboardEvents.Subscribe(e =>
            {
                switch (e.Key)
                {
                    case VirtualKeyCodes.KEY_D:
                        grandOffset += 0.005;
                        Debug.WriteLine($"offset up to {grandOffset}");
                        break;
                    case VirtualKeyCodes.KEY_A:
                        grandOffset -= 0.005;
                        Debug.WriteLine($"offset down to {grandOffset}");
                        break;
                    case VirtualKeyCodes.KEY_W:
                        amp += 0.005;
                        Debug.WriteLine($"amp up to {amp}");
                        break;
                    case VirtualKeyCodes.KEY_S:
                        amp -= 0.005;
                        Debug.WriteLine($"amp down to {amp}");
                        break;
                }
            });
            Debug.WriteLine("Keyboard hooked");
        }

        private void prepare()
        {
            WriteLine("3");
            Thread.Sleep(1000);
            WriteLine("2");
            Thread.Sleep(1000);
            WriteLine("1");
            Thread.Sleep(1000);
            InputGenerator.KeyPress(VirtualKeyCodes.KEY_K);
            Thread.Sleep(233);

        }

        private double grandOffset = 0.0;
        private double amp = 1.0;

        /// <summary>
        /// Wait strategy
        /// </summary>
        /// <param name="_keys"></param>
        private void waitRunner(object _keys)
        {
            SortedSet<KeyAction> keys = (SortedSet<KeyAction>)_keys;
            prepare();
            DateTime initTime = DateTime.Now;
            double lastStop = 0.0;

            foreach (var item in keys)
            {
                double offset = item.Offset;
                if (lastStop == 0.0)
                {
                    lastStop = offset;
                }

                int waitMs = (int)((offset - lastStop) * 1000 * amp);
                Debug.WriteLine("Waiting for ms:" + waitMs.ToString());
                Thread.Sleep(waitMs);
                lastStop = offset;

                item.SendInput();
        
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Loop strategy
        /// </summary>
        /// <param name="_keys"></param>
        private void loopRunner(object _keys)
        {
            SortedSet<KeyAction> keys = (SortedSet<KeyAction>)_keys;
            prepare();
            DateTime initTime = DateTime.Now;
            DateTime nowTime;
            double initOffset = keys.First().Offset;

            foreach (var item in keys)
            {
                nowTime = DateTime.Now;
                while ((nowTime - initTime).TotalMilliseconds < (item.Offset - initOffset - grandOffset) * 1000 * amp)
                {
                    Thread.Sleep(1);
                    nowTime = DateTime.Now;
                }

                item.SendInput();

            }

            Environment.Exit(0);
        }

        Thread t;

        private void start(Action<object> strategy)
        {
            var keys = KeyAction.ParseJson("./test.json");
            t = new Thread(new ParameterizedThreadStart(strategy));
            t.Start(keys);
            hookKeyboard();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            start(waitRunner);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (t != null)
            {
                t.Abort();
            }
            Environment.Exit(0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = grandOffset.ToString();
            textBox2.Text = amp.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            start(loopRunner);
        }
    }
}
