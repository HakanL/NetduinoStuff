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
        public static MatrixDisplay24x16 disp = new MatrixDisplay24x16(4, Pins.GPIO_PIN_D13, Pins.GPIO_PIN_D12, true);
        public static DisplayToolbox toolbox = new DisplayToolbox(disp);

        // Prepare boundaries
        private static int X_MAX = 0;
        private static int Y_MAX = 0;

        private static void Setup()
        {
            // Fetch bounds (dynamically work out how large this display is)
            X_MAX = disp.DisplayCount * disp.DisplayWidth - 1;
            Y_MAX = disp.DisplayHeight - 1;

            // Prepare displays
            disp.InitDisplay(0, Pins.GPIO_PIN_D0, true);
            disp.InitDisplay(1, Pins.GPIO_PIN_D1, false);
            disp.InitDisplay(2, Pins.GPIO_PIN_D2, false);
            disp.InitDisplay(3, Pins.GPIO_PIN_D3, false);
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
            for (int i = 0; i < DEMOTIME / 4; i++)
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

                Thread.Sleep(10);
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

        static void Demo_Life()
        {
            disp.Clear();

            toolbox.SetPixel(10, 3, 1);  // Plant an "acorn"; a simple pattern that
            toolbox.SetPixel(12, 4, 1); //  grows for quite a while..
            toolbox.SetPixel(9, 5, 1);
            toolbox.SetPixel(10, 5, 1);
            toolbox.SetPixel(13, 5, 1);
            toolbox.SetPixel(14, 5, 1);
            toolbox.SetPixel(15, 5, 1);

            //delay( LONGDELAY );   // Play life
            disp.CopyBuffer(); // Copy the back buffer into the shadow buffer (basically create a backup of the CURRENT display)

            for (int i = 0; i < (DEMOTIME) / 4; i++)
            {
                for (int x = 1; x < X_MAX; x++)
                {
                    for (int y = 1; y < Y_MAX; y++)
                    {
                        int neighbors = toolbox.GetPixel(x, y + 1, true) +
                                        toolbox.GetPixel(x, y - 1, true) +
                                        toolbox.GetPixel(x + 1, y, true) +
                                        toolbox.GetPixel(x + 1, y + 1, true) +
                                        toolbox.GetPixel(x + 1, y - 1, true) +
                                        toolbox.GetPixel(x - 1, y, true) +
                                        toolbox.GetPixel(x - 1, y + 1, true) +
                                        toolbox.GetPixel(x - 1, y - 1, true);

                        int newval;
                        switch (neighbors)
                        {
                            case 0:
                            case 1:
                                newval = 0;   // death by loneliness
                                break;
                            case 2:
                                newval = toolbox.GetPixel(x, y, true); // Fetch pixel from the SHADOW buffer
                                break;  // remains the same
                            case 3:
                                newval = 1;
                                break;
                            default:
                                newval = 0;  // death by overcrowding
                                break;
                        }

                        toolbox.SetPixel(x, y, newval);
                    }
                }
                // Write out display
                disp.SyncDisplays();

                // Copy buffer
                disp.CopyBuffer(); // Copy the back buffer into the shadow buffer (basically create a backup of the CURRENT display)

                //delay( DISPDELAY );
            }
        }


        public static void Main()
        {
            Setup();

            while (true)
            {
                Demo_Text();
                Thread.Sleep(1000);
                Demo_BouncyCircle();
                Thread.Sleep(1000);
                Demo_Bouncyline();
                Thread.Sleep(1000);
                Demo_Life();
                Thread.Sleep(1000);
            }
        }
    }
}