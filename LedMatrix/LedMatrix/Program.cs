using System;
using System.Threading;
using NetDuino.Hardware.HT1632;
using SecretLabs.NETMF.Hardware.Netduino;

namespace LedMatrix
{
    /*
       HT1632 demo code
       Ported from the Matrix_display Library 2.0 by Miles Burton, www.milesburton.com/
       Copyright (c) 2010 sweetlilmre All Rights Reserved
       Copyright (c) 2010 Miles Burton All Rights Reserved

       This library is free software; you can redistribute it and/or
       modify it under the terms of the GNU Lesser General Public
       License as published by the Free Software Foundation; either
       version 2.1 of the License, or (at your option) any later version.

       This library is distributed in the hope that it will be useful,
       but WITHOUT ANY WARRANTY; without even the implied warranty of
       MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
       Lesser General Public License for more details.

       You should have received a copy of the GNU Lesser General Public
       License along with this library; if not, write to the Free Software
       Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
     */

    public class Program
    {
        public static MultiDisplay disp = new MultiDisplay(4, 24, 16);
        public static DisplayToolbox toolbox = new DisplayToolbox(disp);

        // Prepare boundaries
        private static int X_MAX = 0;
        private static int Y_MAX = 0;

        private static void Setup()
        {
            // Fetch bounds (dynamically work out how large this display is)
            X_MAX = disp.TotalWidth - 1;
            Y_MAX = disp.DisplayHeight - 1;

            SecretLabs.NETMF.Hardware.ExtendedSpiConfiguration spiConfigDisplay;

            spiConfigDisplay = new SecretLabs.NETMF.Hardware.ExtendedSpiConfiguration(
                Pins.GPIO_PIN_D0,
                false,
                0,
                0,
                false,
                true,
                1000,
                SPI_Devices.SPI1,
                15);

            disp.InitDisplay(0, new MatrixDisplay24x16(spiConfigDisplay));


            spiConfigDisplay = new SecretLabs.NETMF.Hardware.ExtendedSpiConfiguration(
                Pins.GPIO_PIN_D1,
                false,
                0,
                0,
                false,
                true,
                1000,
                SPI_Devices.SPI1,
                15);

            disp.InitDisplay(1, new MatrixDisplay24x16(spiConfigDisplay));


            spiConfigDisplay = new SecretLabs.NETMF.Hardware.ExtendedSpiConfiguration(
                Pins.GPIO_PIN_D2,
                false,
                0,
                0,
                false,
                true,
                1000,
                SPI_Devices.SPI1,
                15);

            disp.InitDisplay(2, new MatrixDisplay24x16(spiConfigDisplay));


            spiConfigDisplay = new SecretLabs.NETMF.Hardware.ExtendedSpiConfiguration(
                Pins.GPIO_PIN_D3,
                false,
                0,
                0,
                false,
                true,
                1000,
                SPI_Devices.SPI1,
                15);

            disp.InitDisplay(3, new MatrixDisplay24x16(spiConfigDisplay));
        }

        private static readonly Random _rand = new Random();

        public static int random(int min,
                                  int max)
        {
            return min + (int)(_rand.NextDouble() * (max - min));
        }

        private static int DEMOTIME = 300; // 30 seconds max on each demo is enough.

        public static void Demo_Bouncyline()
        {
            int x1 = random(0, X_MAX);
            int x2 = random(0, X_MAX);
            int y1 = random(0, Y_MAX);
            int y2 = random(0, Y_MAX);
            int dx1 = random(1, 4);
            int dx2 = random(1, 4);
            int dy1 = random(1, 4);
            int dy2 = random(1, 4);

            disp.Clear();

            for (int i = 0; i < DEMOTIME; i++)
            {
                toolbox.DrawLine(x1, y1, x2, y2, 1);
                disp.SyncDisplays();
                //Thread.Sleep(DISPDELAY);
                toolbox.DrawLine(x1, y1, x2, y2, 0);

                x1 += dx1;
                if (x1 > X_MAX)
                {
                    x1 = X_MAX;
                    dx1 = -random(1, 4);
                }
                else if (x1 < 0)
                {
                    x1 = 0;
                    dx1 = random(1, 4);
                }

                x2 += dx2;
                if (x2 > X_MAX)
                {
                    x2 = X_MAX;
                    dx2 = -random(1, 4);
                }
                else if (x2 < 0)
                {
                    x2 = 0;
                    dx2 = random(1, 4);
                }

                y1 += dy1;
                if (y1 > Y_MAX)
                {
                    y1 = Y_MAX;
                    dy1 = -random(1, 3);
                }
                else if (y1 < 0)
                {
                    y1 = 0;
                    dy1 = random(1, 3);
                }

                y2 += dy2;
                if (y2 > Y_MAX)
                {
                    y2 = Y_MAX;
                    dy2 = -random(1, 3);
                }
                else if (y2 < 0)
                {
                    y2 = 0;
                    dy2 = random(1, 3);
                }
            }
        }

        private static void Demo_BouncyCircle()
        {
            int radius = 2;
            int y = Y_MAX - (radius * 2);
            int x = X_MAX - radius;

            bool textDir = false;
            bool textRight = false;
            for (int i = 0; i < DEMOTIME * 4; i++)
            {
                if (y <= radius) textDir = true;
                else if (y >= (Y_MAX - radius)) textDir = false;


                if (x >= (X_MAX - radius)) textRight = false;
                else if (x <= radius) textRight = true;

                if (textDir) y++;
                else y--;

                if (textRight) x++;
                else x--;

                toolbox.DrawCircle(x, y, radius);
                disp.SyncDisplays();

                Thread.Sleep(5);
                disp.Clear();
            }
        }

        private static void Demo_Text()
        {
            int y = Y_MAX - 7;
            int x = 0;
            bool textDir = false;
            bool textRight = false;

            string str = "Hello World";
            int xLimit = -(str.Length * DisplayToolbox.CHAR_WIDTH);
            disp.Clear();

            for (int i = 0; i < DEMOTIME / 2; i++)
            {
                if (y <= 0) textDir = true;
                else if (y > (Y_MAX - 7)) textDir = false;


                if (x >= X_MAX) textRight = false;
                else if (x <= xLimit) textRight = true;

                if (textDir) y++;
                else y--;

                if (textRight) x++;
                else x--;

                toolbox.DrawString(x, y, str);
                disp.SyncDisplays();

                disp.Clear();
            }
        }


        private static void Demo_Fill()
        {
            for (byte y = 0; y < disp.DisplayHeight; y++)
            {
                for (byte x = 0; x < disp.TotalWidth; x++)
                {
                    disp.SetPixel(x, y, 1, false);
                    disp.SyncDisplays();
                }
            }

            for (byte x = 0; x < disp.TotalWidth; x++)
            {
                for (byte y = 0; y < disp.DisplayHeight; y++)
                {
                    disp.SetPixel(x, y, 0, false);
                    disp.SyncDisplays();

                }
            }
        }


        private static void Demo_Scroll()
        {
            toolbox.DrawString(0, 8, "Lite finare!!", true);
            disp.SyncDisplays();

            while (true)
            {
                toolbox.DrawString(48, 0, "Sunes jul");

                for (int index = 0; index < 20; index++)
                {
                    disp.ScrollLeft2();
                    disp.SyncDisplays();
                }
            }
        }


        public static void Main()
        {
            Setup();

            int topX = 90;
            while (true)
            {
                toolbox.DrawString(topX, 0, "Sunes Jul", true);
                topX--;
                
                disp.SyncDisplays();

                if (topX < -60)
                    topX = 90;

            }
/*
            while (true)
            {
                Demo_Text();
                Thread.Sleep(1000);
                
                Demo_BouncyCircle();
                Thread.Sleep(1000);

                Demo_Bouncyline();
                Thread.Sleep(1000);*/
/*
                Demo_Fill();
                Thread.Sleep(1000);*/
//            }
        }
    }
}