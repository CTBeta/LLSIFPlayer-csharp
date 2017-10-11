using Dapplo.Windows.Input;
using Dapplo.Windows.Input.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLSIFPlayer
{
    class KeyAction: IComparable<KeyAction>
    {
        public int Num;
        public KeyType Type;
        public double Offset;

        public int CompareTo(KeyAction other)
        {
            return Offset - other.Offset > 0 ? 1 : -1;
        }

        public static SortedSet<KeyAction> ParseJson(string path)
        {
            SortedSet<KeyAction> keys = new SortedSet<KeyAction>();
            using (StreamReader r = new StreamReader("./test.json"))
            {
                dynamic obj = JsonConvert.DeserializeObject(r.ReadToEnd());
                dynamic res = obj.response_data;
                foreach (var n in res.live_info[0].notes_list)
                {
                    switch ((int)n.effect.Value)
                    {
                        case 1:
                        case 2:
                        case 4:
                            keys.Add(new KeyAction()
                            {
                                Num = n.position,
                                Type = KeyType.KeyPress,
                                Offset = n.timing_sec
                            });
                            break;
                        case 3:
                            keys.Add(new KeyAction()
                            {
                                Num = n.position,
                                Type = KeyType.KeyDown,
                                Offset = n.timing_sec
                            });
                            keys.Add(new KeyAction()
                            {
                                Num = n.position,
                                Type = KeyType.KeyUp,
                                Offset = n.timing_sec + (double)n.effect_value
                            });
                            break;
                    }
                }
            }
            return keys;
        }

        public void SendInput()
        {
            switch (Type)
            {
                case KeyType.KeyDown:
                    InputGenerator.KeyDown((VirtualKeyCodes)(Num + 48));
                    Debug.WriteLine("Pushed" + Num.ToString());
                    break;
                case KeyType.KeyUp:
                    InputGenerator.KeyUp((VirtualKeyCodes)(Num + 48));
                    Debug.WriteLine("Released " + Num.ToString());
                    break;
                case KeyType.KeyPress:
                    InputGenerator.KeyPress((VirtualKeyCodes)(Num + 48));
                    Debug.WriteLine("Pressed" + Num.ToString());
                    break;
            }

        }
    }
}
