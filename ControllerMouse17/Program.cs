using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using XInputDotNetPure;
using System.Numerics;
using System.Reflection;

namespace ControllerMouse17
{
    class Program
    {
        public class ButtonCollection
        {
            public List<Button> bl = new List<Button>();

            public void UpdateLast()
            {
                foreach (Button b in bl)
                {
                    b.UpdateLast();
                }
            }
        }

        public class Buttons : ButtonCollection
        {
            public Buttons()
            {
                A = new Button("a"); B = new Button("b"); X = new Button("x"); Y = new Button("y");
                Back = new Button("back"); Start = new Button("start"); Guide = new Button("guide");
                LeftShoulder = new Button("leftShoulder"); RightShoulder = new Button("rightShoulder");
                LeftStick = new Button("leftButton"); RightStick = new Button("rightButton");
                bl.AddRange(new Button[] { A, B, X, Y, Back, Start, Guide, LeftShoulder, RightShoulder, LeftStick, RightStick });
                //default actions
                A.action[0] = delegate { Win32.MouseEvent(Win32.MouseEventFlags.LeftDown);};
                A.action[1] = delegate { Win32.MouseEvent(Win32.MouseEventFlags.LeftUp);};
                B.action[0] = delegate { Win32.MouseEvent(Win32.MouseEventFlags.RightDown); };
                B.action[1] = delegate { Win32.MouseEvent(Win32.MouseEventFlags.RightUp); };
                X.action[0] = delegate { Win32.PressKeyDown(Win32.Keys.VK_SPACE);};
                X.action[1] = delegate { Win32.PressKeyUp(Win32.Keys.VK_SPACE);};
                //LeftStick.action[0] = delegate { Console.Out.WriteLine("Left Stick"); };
                //RightStick.action[0] = delegate { Console.Out.WriteLine("Right Stick"); };
                LeftShoulder.action[0] = delegate { Win32.VolDown(); };
                RightShoulder.action[0] = delegate { Win32.VolUp(); };
            }
            public Button A, B, X, Y, Back, Start, Guide,LeftShoulder,RightShoulder,LeftStick,RightStick;
        }

        public class VDPad : ButtonCollection
        {
            public VDPad()
            {
                Up = new Button("up"); Down = new Button("down"); Left = new Button("left"); Right = new Button("Right");
                Up.action[3] = delegate { Win32.MouseScroll(tmpScrollSpeed); };
                Down.action[3] = delegate { Win32.MouseScroll(-tmpScrollSpeed); };
                Left.action[3] = delegate { Win32.MouseScroll(-tmpScrollSpeed, true); };
                Right.action[3] = delegate { Win32.MouseScroll(tmpScrollSpeed, true); };

                bl.AddRange(new Button[] { Up, Down, Left, Right });
            }

            public Button Up, Down, Left, Right;
        }

        public class Button
        {
            public Button(string name)
            {
                this.name = name;
            }

            protected string name;
            public virtual bool Value { get;set;
            }

            public bool LastValue { get; private set; }

            public void UpdateLast() { LastValue = Value; }
            public Action[] action = new Action[] { delegate { },delegate { },delegate { },delegate { } };//[0] on true press, [1] on false press(release), [2] on change, [3] on every update true
        }

        public class MultiButton : Button
        {
            public MultiButton(string name,params Button [] buttons) : base(name)
            {
                //this.name = name;
                //foreach (Button button in buttons)
                //{
                //    allButtons.Add(button);
                //}
                allButtons.AddRange(buttons);
            }
            //readonly String name;
            protected List<Button> allButtons = new List<Button>();
            public override bool Value
            {
                get { return AllTrue(); }
                set { throw new Exception("should not write multibutton value"); }
            }
            //public void updateButton(string buttonName, bool value)
            //{
            //    if (allButtons.ContainsKey(buttonName))
            //        allButtons[buttonName] = value;
            //}

            public virtual bool AllTrue()
            {
                foreach (Button b in allButtons)
                {
                    if (b.Value == false) return false;
                }
                return true;
            }
        }
        
        public class Timestamp
        {
            public Timestamp(string id)
            {
                this.id = id;
                //Console.Out.WriteLine("Datetime now");
                dateTime = DateTime.Now;
            }

            public Timestamp(string id, DateTime dateTime) : this (id)
            {
                //Console.Out.WriteLine("Datetime manual");
                this.dateTime = dateTime;
            }

            public Timestamp(string id, double addedTimeMilli) : this(id)
            {
                dateTime = DateTime.Now.AddMilliseconds(addedTimeMilli);
            }

            public readonly DateTime dateTime;
            public readonly string id;

        }

        public class TimedButton : MultiButton
        {
            public TimedButton(string name,int duration, params Button[] buttons) : base(name,buttons)
            {
                this.duration = duration;
            }
            public readonly int duration;
            Timestamp timeOfExecute;
            bool ongoing = false;
            public Action timedAction = delegate { };
            bool allButtonsTrue { get { return AllTrue(); } }
            public override bool Value { get; set; }
            public override bool AllTrue()
            {
                foreach (Button b in allButtons)
                {
                    if (b.Value == false) {
                        ongoing = false;
                        return false;
                    }
                }
                //Console.Out.WriteLine("All true");
                return true;
            }

            public bool CheckTimer()
            {
                Value = false;
                if (allButtonsTrue)
                {
                    if (ongoing == false)
                    {
                        ongoing = true;
                        timeOfExecute = new Timestamp(this.name + "Ts",duration);
                        Console.Out.WriteLine("Started timed button: "+name);
                        UpdateLast();
                        return false;
                    }
                    if (timeOfExecute.dateTime <= DateTime.Now)
                    {
                        Value = true;
                        if (Value != LastValue)
                        {
                            if (Value)
                            {
                                timedAction();
                                UpdateLast();
                            }
                        }
                        UpdateLast();
                        return true;
                    }
                    UpdateLast();
                    return false;
                }
                else
                {
                    ongoing = false;
                    UpdateLast();
                    return false;
                }
            }

            public void Cancel()
            {
                ongoing = false;
            }

        }


        int UpdatesPerSecond = 100;
        DateTime lastRunTime = DateTime.MinValue;
        static bool running = true;

        static Program()
        {
            CosturaUtility.Initialize();
        }

        static void Main(string[] args)
        {
            Program pr = new Program();
            pr.Setup();
            pr.OutPutLog("Remember to switch the controller on");
            while (running)
            {
                pr.Wait();
                pr.UpdateInput();
                pr.DoStuff();
            }
        }

        
        MultiButton OnOffMultibutton;
        List<Button> virtualButtons = new List<Button>();

        bool con = false;
        bool evaluationAllowed = true;
        float LeftStickX = 0, LeftStickY = 0, RightStickX = 0, RightStickY = 0, LeftTrigger = 0, RightTrigger = 0;
        float speedUpScale = 0;
        
        VDPad DPad;
        int scrollSpeed = 120;
        static int tmpScrollSpeed;
        float moveScale = 0.35f;
        float inputScale = 15f;
        float speedUpscaleFactor = 3f;

        GamePadState CState1;
        bool inputValuesChanged = false;
        uint lastPackNum;
        GamePadDeadZone deadzone = GamePadDeadZone.IndependentAxes;
        
        Win32.POINT oldPos = Win32.GetCursorPosition();

        Vector2 oldP, newP;
        
        Buttons buttons;

        void Wait()
        {
            Thread.Sleep(1000 / UpdatesPerSecond);
        }

        void Setup()
        {
            buttons = new Buttons();
            DPad = new VDPad();
            virtualButtons.AddRange(buttons.bl);
            virtualButtons.AddRange(DPad.bl);

            OnOffMultibutton = new MultiButton("backStart", buttons.Back, buttons.Start);
            OnOffMultibutton.action[0] = delegate () {
                evaluationAllowed = !evaluationAllowed;
                OutPutLog("Multibutton Back+Start set App to:"+evaluationAllowed);
                Vibrate(0.5f, 0.5f, 300);
                CancelTimed();
            };

            //Custom non default Buttons
            TimedButton altF4 = new TimedButton("altF4", 5000, buttons.LeftShoulder, buttons.RightShoulder, buttons.LeftStick, buttons.RightStick);
            altF4.timedAction = delegate {
                OutPutLog("action altF4");
                Win32.PressKeyDown(Win32.Keys.VK_MENU);
                Win32.PressKeyDown(Win32.Keys.VK_F4);
                Win32.PressKeyUp(Win32.Keys.VK_MENU);
                Win32.PressKeyUp(Win32.Keys.VK_F4);
            };
            virtualButtons.Add(altF4);

            MultiButton muting = new MultiButton("muting",buttons.LeftShoulder, buttons.RightShoulder);
            muting.action[0] = delegate {
                OutPutLog("action muting");
                Win32.Mute();
            };
            virtualButtons.Add(muting);

            OutPutLog("Total buttoncount: " + virtualButtons.Count);
        }

        void UpdateInput()
        {
            CState1 = GamePad.GetState(PlayerIndex.One,deadzone);
            bool newcon = CState1.IsConnected;

            //update Cursor Position
            oldPos = Win32.GetCursorPosition();

            //output connectionchange
            if (con != newcon)
            {
                if (newcon)
                    OutPutLog("connected");
                else
                    OutPutLog("disconnected");
                 con = newcon;
            }
            
            //update input values
            if (con)
            {

                if (CState1.PacketNumber != lastPackNum)
                {
                    inputValuesChanged = true;
                    lastPackNum = CState1.PacketNumber;

                    LeftStickX = CState1.ThumbSticks.Left.X;
                    LeftStickY = CState1.ThumbSticks.Left.Y;
                    RightStickX = CState1.ThumbSticks.Right.X;
                    RightStickY = CState1.ThumbSticks.Right.Y;
                    LeftTrigger = CState1.Triggers.Left;
                    RightTrigger = CState1.Triggers.Right;

                    //OutPutLog("Rx" + RightStickX + "  Ry" + RightStickY);

                    speedUpScale = Math.Max(LeftTrigger, RightTrigger);
                    
                    DPad.Up.Value = CState1.DPad.Up == ButtonState.Pressed;
                    DPad.Down.Value = CState1.DPad.Down == ButtonState.Pressed;
                    DPad.Left.Value = CState1.DPad.Left == ButtonState.Pressed;
                    DPad.Right.Value = CState1.DPad.Right == ButtonState.Pressed;
                    
                    buttons.A.Value = CState1.Buttons.A == ButtonState.Pressed;
                    buttons.B.Value = CState1.Buttons.B == ButtonState.Pressed;
                    buttons.X.Value = CState1.Buttons.X == ButtonState.Pressed;
                    buttons.Y.Value = CState1.Buttons.Y == ButtonState.Pressed;
                    buttons.Start.Value = CState1.Buttons.Start == ButtonState.Pressed;
                    buttons.Back.Value = CState1.Buttons.Back == ButtonState.Pressed;
                    buttons.Guide.Value = CState1.Buttons.Guide == ButtonState.Pressed;
                    buttons.LeftShoulder.Value = CState1.Buttons.LeftShoulder == ButtonState.Pressed;
                    buttons.RightShoulder.Value = CState1.Buttons.RightShoulder == ButtonState.Pressed;
                    buttons.LeftStick.Value = CState1.Buttons.LeftStick == ButtonState.Pressed;
                    buttons.RightStick.Value = CState1.Buttons.RightStick == ButtonState.Pressed;
                }
                else
                {
                    inputValuesChanged = false;
                }
                
                //OutPutLog("Updated Input Values");
            }
            else
            {
                //OutPutLog("not connected");
            }
        }

        void OutPutLog(string s)
        {
            Console.Out.WriteLine(s);
        }

        void DoStuff()
        {
            if (inputValuesChanged)
            {
                DoInputEvents();
            }
            MoveCursor();
            DoEveryUpdate();
        }


        void MoveCursor()
        {
            if (!evaluationAllowed)
                return;
            Vector2 dir = new Vector2((LeftStickX+RightStickX) * inputScale, (-LeftStickY+-RightStickY) * inputScale);
            float l = dir.Length();
            if (l > 0.05f)
            {
                oldP.X = oldPos.x + (newP.X  - (float)(int)newP.X);
                oldP.Y = oldPos.y + (newP.Y  - (float)(int)newP.Y);

                dir = Vector2.Normalize(dir);
                dir *= l;
                dir *= moveScale + (moveScale * speedUpScale * speedUpscaleFactor);
                newP = oldP + dir;

                Win32.SetCursorPos((int)(newP.X ), (int)(newP.Y ));
            }
        }

        
        void DoInputEvents()
        {
            int scrollSpeedScaleFactor = 15;
            tmpScrollSpeed = (scrollSpeed + (int)(scrollSpeed * speedUpScale*scrollSpeedScaleFactor))/10;
            
            if(OnOffMultibutton.Value != OnOffMultibutton.LastValue)
            {
                if(OnOffMultibutton.Value)
                    OnOffMultibutton.action[0]();
            }
            OnOffMultibutton.UpdateLast();
            if (!evaluationAllowed)
                return;
            foreach (Button vb in virtualButtons)
            {
                if (vb.Value != vb.LastValue)
                {
                    //trigger mb action on change
                    vb.action[2]();
                    if (vb.Value)
                    {
                        //trigger mb action on change true
                        vb.action[0]();
                    }
                    else
                    {
                        //trigger mb action on change false
                        vb.action[1]();
                    }
                }

                //update LastValue
                vb.UpdateLast();
            }
            
            //buttons.UpdateLast();
            
        }
        
        void DoEveryUpdate()
        {
            if (!evaluationAllowed)
                return;
            foreach (Button vb in virtualButtons)
            {
                if(vb as TimedButton != null)
                {
                   ((TimedButton)vb).CheckTimer();
                }
                
                if (vb.Value)
                {
                    //trigger mb action every update true
                    vb.action[3]();
                }
            }
        }
        
        public void CancelTimed()
        {
            foreach (Button vb in virtualButtons)
            {
                if (vb as TimedButton != null)
                    ((TimedButton)vb).Cancel();
            }
        }

        async void Vibrate(float left, float right, double timeMilli)
        {
            GamePad.SetVibration(PlayerIndex.One,left,right);
            await Task.Delay(TimeSpan.FromMilliseconds(timeMilli));
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

        }
        
        
    }

}
