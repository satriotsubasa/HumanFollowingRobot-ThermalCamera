using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LattePanda.Firmata;

namespace ThermalCam
{
    public static class ExtensionMethods
    {
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
    class Motor
    {
        public const int PWM_Front = 6;
        public const int PWM_Left = 10;
        public const int PWM_Right = 9;
        public const int IN_F1 = 18;
        public const int IN_F2 = 19;
        public const int IN_R1 = 7;
        public const int IN_R2 = 8;
        public const int IN_L1 = 11;
        public const int IN_L2 = 12;
    }
    static class Globals
    {
        public static int XTotal = 400;
        public static int Koordinat_Data = 0;
        public static double Area = 0.00;
        public const double RAD = 57.3; //Jari Jari Roda
        public const double ALPHA = 0; //Error Sudut
        public const double RADIUS = 0.14; //Jari Jari Robot
        public const double max_speed = 200;
        public const int MOTOR_KANAN = 0;
        public const int MOTOR_KIRI = 1;
        public const int MOTOR_DEPAN = 2;
    }
    class Program
    {
        static Arduino arduino = new Arduino();        
        static void Main(string[] args)
        {
            string InputText;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            arduino.pinMode(Motor.PWM_Front, Arduino.PWM);
            arduino.pinMode(Motor.PWM_Left, Arduino.PWM);
            arduino.pinMode(Motor.PWM_Right, Arduino.PWM);
            arduino.pinMode(Motor.IN_F1, Arduino.OUTPUT);
            arduino.pinMode(Motor.IN_F2, Arduino.OUTPUT);
            arduino.pinMode(Motor.IN_R1, Arduino.OUTPUT);
            arduino.pinMode(Motor.IN_R2, Arduino.OUTPUT);
            arduino.pinMode(Motor.IN_L1, Arduino.OUTPUT);
            arduino.pinMode(Motor.IN_F1, Arduino.OUTPUT);
            while (true)
            {
                using (var filex = new FileStream(@"C:\Users\lattepanda\Documents\SatrioTsubasa\Visual Studio Program\SerialC\x64\Debug\data.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sourcex = new StreamReader(filex, Encoding.Default))
                {
                    InputText = sourcex.ReadToEnd();
                }
                string[] InputData = InputText.Split('#');

                if(InputData[0] != "" && InputData[1] != "")
                {
                    Globals.Koordinat_Data = Convert.ToInt32(InputData[0]);
                    Globals.Area = Convert.ToDouble(InputData[1]);
                }
                Process();
            }
        }

        static double pwm_motor(double kec_translasi, int nomor_motor, int arah, int kec_putar)
        {
            double[] sudut = {120,60,0};

            double kec_x = kec_translasi * Math.Cos(arah / Globals.RAD);
            double kec_y = kec_translasi * Math.Sin(arah / Globals.RAD);
            double temp = (kec_x * Math.Cos((sudut[nomor_motor] + Globals.ALPHA) / Globals.RAD)) + (kec_y * Math.Sin((sudut[nomor_motor] + Globals.ALPHA) / Globals.RAD)) + (Globals.RADIUS * kec_putar);
            return temp;
        }

        static void Process()
        {
            int Koordinat, Abs_Koordinat;
            decimal PWM;
            double Area;
            double _PWM;
            double kec_1, kec_2, kec_3;
            Koordinat = (Globals.XTotal /2) - Globals.Koordinat_Data;
            Koordinat = (9 * Koordinat /40) + 90;

            Area = Globals.Area;
            Abs_Koordinat = Math.Abs(Koordinat);
           
            if (Area < 30000 && Area > 500)
            {
                PWM = Convert.ToDecimal(Area).Map(500, 30000, 190, 150);
                _PWM = Convert.ToDouble(PWM);

                kec_1 = pwm_motor(_PWM, Globals.MOTOR_KANAN, Koordinat, 0);
                kec_2 = -1 * pwm_motor(_PWM, Globals.MOTOR_KIRI, Koordinat, 0);
                kec_3 = pwm_motor(_PWM, Globals.MOTOR_DEPAN, Koordinat, 0);

                if (kec_1 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Right, Convert.ToInt32(kec_1));
                    arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Right, Math.Abs(Convert.ToInt32(kec_1)));
                    arduino.digitalWrite(Motor.IN_R1, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
                }
                if (kec_2 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Left, Convert.ToInt32(kec_2));
                    arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_L1, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Left, Math.Abs(Convert.ToInt32(kec_2)));
                    arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
                }
                if (kec_3 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Front, Convert.ToInt32(kec_3));
                    arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_F2, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Front, Math.Abs(Convert.ToInt32(kec_3)));
                    arduino.digitalWrite(Motor.IN_F1, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
                }


            }
            else if (Area > 45000)
            {
                PWM = Convert.ToDecimal(Area).Map(45000, 155000, 150, 190);
                _PWM = Convert.ToDouble(PWM);
                kec_1 = pwm_motor(_PWM, Globals.MOTOR_KANAN, Koordinat+180, 0);
                kec_2 = -1 * pwm_motor(_PWM, Globals.MOTOR_KIRI, Koordinat+180, 0);
                kec_3 = pwm_motor(_PWM, Globals.MOTOR_DEPAN, Koordinat+180, 0);

                if (kec_1 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Right, Convert.ToInt32(kec_1));
                    arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Right, Math.Abs(Convert.ToInt32(kec_1)));
                    arduino.digitalWrite(Motor.IN_R1, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
                }
                if (kec_2 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Left, Convert.ToInt32(kec_2));
                    arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_L1, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Left, Math.Abs(Convert.ToInt32(kec_2)));
                    arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
                }
                if (kec_3 > 0)
                {
                    arduino.analogWrite(Motor.PWM_Front, Convert.ToInt32(kec_3));
                    arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
                    arduino.digitalWrite(Motor.IN_F2, Arduino.HIGH);
                }
                else
                {
                    arduino.analogWrite(Motor.PWM_Front, Math.Abs(Convert.ToInt32(kec_3)));
                    arduino.digitalWrite(Motor.IN_F1, Arduino.HIGH);
                    arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
                }
            }
            else
                MotorStop();

        }

        static void MotorStop()
        {
            arduino.analogWrite(Motor.PWM_Front,0);
            arduino.analogWrite(Motor.PWM_Left, 0);
            arduino.analogWrite(Motor.PWM_Right, 0);
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
        }

        static void MotorMaju()
        {
            arduino.analogWrite(Motor.PWM_Front, 250);
            arduino.analogWrite(Motor.PWM_Left, 250);
            arduino.analogWrite(Motor.PWM_Right, 250);

            //Maju
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);  
        }

        static void MotorPutarKiri()
        {
            arduino.analogWrite(Motor.PWM_Front, 200);
            arduino.analogWrite(Motor.PWM_Left, 200);
            arduino.analogWrite(Motor.PWM_Right, 200);

            //Rotate CCW
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
        }

        static void MotorPutarKanan()
        {
            arduino.analogWrite(Motor.PWM_Front, 200);
            arduino.analogWrite(Motor.PWM_Left, 200);
            arduino.analogWrite(Motor.PWM_Right, 200);

            //Rotate CW
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.HIGH);
        }

        static void Motorx()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            arduino.analogWrite(Motor.PWM_Front, 200);
            arduino.analogWrite(Motor.PWM_Left, 200);
            arduino.analogWrite(Motor.PWM_Right, 200);

            //Maju
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
            Thread.Sleep(4000);

            arduino.analogWrite(Motor.PWM_Front, 250);
            arduino.analogWrite(Motor.PWM_Left, 140);
            arduino.analogWrite(Motor.PWM_Right, 140);

            //Kanan
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_L1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
            Thread.Sleep(3000);

            arduino.analogWrite(Motor.PWM_Front, 250);
            arduino.analogWrite(Motor.PWM_Left, 140);
            arduino.analogWrite(Motor.PWM_Right, 140);

            //Kiri
            arduino.digitalWrite(Motor.IN_F1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
            Thread.Sleep(3000);

            arduino.analogWrite(Motor.PWM_Front, 200);
            arduino.analogWrite(Motor.PWM_Left, 200);
            arduino.analogWrite(Motor.PWM_Right, 200);

            //Rotate CCW
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R1, Arduino.HIGH);
            arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.HIGH);
            Thread.Sleep(2000);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            arduino.analogWrite(Motor.PWM_Front, 0);
            arduino.analogWrite(Motor.PWM_Left, 0);
            arduino.analogWrite(Motor.PWM_Right, 0);
            arduino.digitalWrite(Motor.IN_F1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_F2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_R2, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L1, Arduino.LOW);
            arduino.digitalWrite(Motor.IN_L2, Arduino.LOW);
            Console.WriteLine("I'm out of here");
        }
    }
}
