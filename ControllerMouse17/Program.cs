using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using XInputDotNetPure;
using System.Numerics;

namespace ControllerMouse17
{
    class Program
    {
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
            pr.OutPutLog("Remember to switch the controller on");
            while (running)
            {
                pr.Wait();
                pr.UpdateInput();
                pr.DoStuff();
            }
        }



        void Wait()
        {
            Thread.Sleep(1000 / UpdatesPerSecond);
        }

        bool con = false;

        float LeftStickX = 0, LeftStickY = 0, RightStickX = 0, RightStickY = 0, LeftTrigger = 0, RightTrigger=0;
        float speedUpScale = 0;
        bool ButtonA, ButtonB, ButtonX, ButtonY;

        public struct VDPad
        {
            public bool Up, Down, Left, Right;
        }


        VDPad DPad = new VDPad();
        int scrollSpeed = 120;

        float moveScale = 0.35f;
        float inputScale = 15f;
        float speedUpscaleFactor = 3f;

        GamePadState CState1;
        bool inputValuesChanged=false;
        uint lastPackNum;
        GamePadDeadZone deadzone=GamePadDeadZone.IndependentAxes;


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

            //check for Input Value Chnages

            //do stuff
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

                    ButtonA = CState1.Buttons.A == ButtonState.Pressed ? true : false;
                    ButtonB = CState1.Buttons.B == ButtonState.Pressed ? true : false;
                    ButtonX = CState1.Buttons.X == ButtonState.Pressed ? true : false;
                    ButtonY = CState1.Buttons.Y == ButtonState.Pressed ? true : false;

                    DPad.Up = CState1.DPad.Up == ButtonState.Pressed ? true : false;
                    DPad.Down = CState1.DPad.Down == ButtonState.Pressed ? true : false;
                    DPad.Left = CState1.DPad.Left == ButtonState.Pressed ? true : false;
                    DPad.Right = CState1.DPad.Right == ButtonState.Pressed ? true : false;

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
            //ivcLate = inputValuesChanged;

        }

        Win32.POINT oldPos = Win32.GetCursorPosition();
        //Win32.POINT newPos = new Win32.POINT();

        Vector2 oldP,newP;
        bool ivcLate = false;

        void MoveCursor()
        {
            //speedUpScale = Math.Max(LeftTrigger,RightTrigger);
            //if (ivcLate)
            //{
                
            //}
            //else
            //{
                //oldP.X = oldPos.x;
                //oldP.Y = oldPos.y;
            //}

            Vector2 dir = new Vector2((LeftStickX+RightStickX) * inputScale, (-LeftStickY+-RightStickY) * inputScale);
            float l = dir.Length();
            //if (l < 0.4f)
            //    dir *= (dir*dir);
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


        bool ButtonALast,ButtonBLast,ButtonXLast,ButtonYLast;
        VDPad DPadLast = new VDPad();


        void DoInputEvents()
        {
            if (ButtonA!=ButtonALast)
            {
                if(ButtonA)
                    Win32.MouseEvent(Win32.MouseEventFlags.LeftDown);
                else
                    Win32.MouseEvent(Win32.MouseEventFlags.LeftUp);
            }
            if (ButtonB!=ButtonBLast)
            {
                if (ButtonB)
                    Win32.MouseEvent(Win32.MouseEventFlags.RightDown);
                else
                    Win32.MouseEvent(Win32.MouseEventFlags.RightUp);
            }
            if (ButtonX != ButtonXLast)
            {
                if (ButtonX)
                    Win32.PressKeyDown(Win32.Keys.VK_SPACE);
                else
                    Win32.PressKeyUp(Win32.Keys.VK_SPACE);
            }

            int scrollSpeedScaleFactor = 15;
            int tmpScrollSpeed = scrollSpeed + (int)(scrollSpeed * speedUpScale*scrollSpeedScaleFactor);
            
            if(DPad.Up != DPadLast.Up)
            {
                if (DPad.Up)
                    Win32.MouseScroll(tmpScrollSpeed);
                else
                    ;
            }
            if (DPad.Down != DPadLast.Down)
            {
                if (DPad.Down)
                    Win32.MouseScroll(-tmpScrollSpeed);
                else
                    ;
            }
            if (DPad.Right != DPadLast.Right)
            {
                if (DPad.Right)
                    Win32.MouseScroll(tmpScrollSpeed, true);
                else
                    ;
            }
            if (DPad.Left != DPadLast.Left)
            {
                if (DPad.Left)
                    Win32.MouseScroll(-tmpScrollSpeed, true);
                else
                    ;
            }

            ButtonALast = ButtonA;
            ButtonBLast = ButtonB;
            ButtonXLast = ButtonX;

            DPadLast.Up = DPad.Up;
            DPadLast.Down = DPad.Down;
            DPadLast.Right = DPad.Right;
            DPadLast.Left = DPad.Left;
        }
        
    }

}
