namespace NetDuino.Hardware.HT1632
{
    /*
      HT1632 DisplayToolbox class
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

    public class DisplayToolbox
    {
        private readonly IMatrixDisplay _display;
        public const int CHAR_WIDTH = 6;

        public DisplayToolbox(IMatrixDisplay display)
        {
            _display = display;
        }

        private byte CalcDisplayNum(ref int x)
        {
            int displayNum = 0;
            if (x >= _display.DisplayWidth)
            {
                displayNum = x / _display.DisplayWidth;
                x -= (24 * displayNum);
            }
            return (byte)displayNum;
        }

        public void DrawCircle(int xp,
                                int yp,
                                int radius,
                                byte col = (byte) 1)
        {
            int xoff = 0;
            int yoff = radius;
            int balance = -radius;
            while (xoff <= yoff)
            {
                SetPixel(xp + xoff, yp + yoff, col);
                SetPixel(xp - xoff, yp + yoff, col);
                SetPixel(xp - xoff, yp - yoff, col);
                SetPixel(xp + xoff, yp - yoff, col);
                SetPixel(xp + yoff, yp + xoff, col);
                SetPixel(xp - yoff, yp + xoff, col);
                SetPixel(xp - yoff, yp - xoff, col);
                SetPixel(xp + yoff, yp - xoff, col);
                if ((balance += xoff++ + xoff) >= 0)
                {
                    balance -= --yoff + yoff;
                }
            }
        }

        private static int Abs(int x)
        {
            return x >= 0 ? x : -x;
        }

        public void DrawLine(int x1,
                              int y1,
                              int x2,
                              int y2,
                              byte val)
        {
            int deltax = Abs(x2 - x1); // The difference between the x's
            int deltay = Abs(y2 - y1); // The difference between the y's
            int x = x1; // Start x off at the first pixel
            int y = y1; // Start y off at the first pixel
            int xinc1, xinc2, yinc1, yinc2, den, num, numadd, numpixels, curpixel;

            if (x2 >= x1)
            {
                // The x-values are increasing
                xinc1 = 1;
                xinc2 = 1;
            }
            else
            {
                // The x-values are decreasing
                xinc1 = -1;
                xinc2 = -1;
            }

            if (y2 >= y1) // The y-values are increasing
            {
                yinc1 = 1;
                yinc2 = 1;
            }
            else // The y-values are decreasing
            {
                yinc1 = -1;
                yinc2 = -1;
            }

            if (deltax >= deltay) // There is at least one x-value for every y-value
            {
                xinc1 = 0; // Don't change the x when numerator >= denominator
                yinc2 = 0; // Don't change the y for every iteration
                den = deltax;
                num = deltax / 2;
                numadd = deltay;
                numpixels = deltax; // There are more x-values than y-values
            }
            else // There is at least one y-value for every x-value
            {
                xinc2 = 0; // Don't change the x for every iteration
                yinc1 = 0; // Don't change the y when numerator >= denominator
                den = deltay;
                num = deltay / 2;
                numadd = deltax;
                numpixels = deltay; // There are more y-values than x-values
            }

            for (curpixel = 0; curpixel <= numpixels; curpixel++)
            {
                SetPixel(x, y, val); // Draw the current pixel
                num += numadd; // Increase the numerator by the top of the fraction
                if (num >= den) // Check if numerator >= denominator
                {
                    num -= den; // Calculate the new numerator value
                    x += xinc1; // Change the x as appropriate
                    y += yinc1; // Change the y as appropriate
                }
                x += xinc2; // Change the x as appropriate
                y += yinc2; // Change the y as appropriate
            }
        }

        public void SetPixel(int x,
                              int y,
                              int val,
                              bool paint = false)
        {
            // setPixel
            // _display Number
            // X Cordinate
            // Y Cordinate
            // Value (either on or off, 1, 0)
            // Do you want to write this change straight to the _display? (yes: slower)
            if (x < 0 || y < 0)
                return;
            byte displayNum = CalcDisplayNum(ref x);
            if (displayNum < _display.DisplayCount)
            {
                _display.SetPixel(displayNum, (byte)x, (byte)y, (byte)val, paint);
            }
        }

        public byte GetPixel(int x,
                              int y,
                              bool fromShadow)
        {
            return _display.GetPixel(CalcDisplayNum(ref x), (byte)x, (byte)y, fromShadow);
        }

        public void SetBrightness(byte pwmValue)
        {
            for (int displayNum = 0; displayNum < _display.DisplayCount; ++displayNum)
                _display.SetBrightness(displayNum, pwmValue);
        }

        public void DrawRectangle(int x,
                                   int y,
                                   int width,
                                   int height,
                                   byte colour,
                                   bool filled = false)
        {
            if (filled)
            {
                for (int y1 = y; y1 < y + height; y1++)
                {
                    DrawLine(x, y1, (byte)x + width, y1, colour);
                }
            }
            else
            {
                DrawLine(x, y, x, (byte)(y + height), colour); // Left side of box
                DrawLine(x + width, y, x + width, y + height, colour); // Right side of box

                DrawLine(x, y, x + width, y, colour); // top of box
                DrawLine(x, y + height, x + width, y + height, colour); // bottom of box
            }
        }

        //void drawFilledRectangle(int, int, int, int, int);

        private readonly byte[][] _myfont = new byte[47][]
                                        {
                                          new byte[]
                                          {
                                            0, 0, 0, 0, 0
                                          }, // space!
                                          new byte[]
                                          {
                                            0x3f, 0x48, 0x48, 0x48, 0x3f
                                          }, // A
                                          new byte[]
                                          {
                                            0x7f, 0x49, 0x49, 0x49, 0x36
                                          },
                                          new byte[]
                                          {
                                            0x3e, 0x41, 0x41, 0x41, 0x22
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x41, 0x41, 0x22, 0x1c
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x49, 0x49, 0x49, 0x41
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x48, 0x48, 0x48, 0x40
                                          },
                                          new byte[]
                                          {
                                            0x3e, 0x41, 0x49, 0x49, 0x2e
                                          }, // G
                                          new byte[]
                                          {
                                            0x7f, 0x08, 0x08, 0x08, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x00, 0x41, 0x7f, 0x41, 0x00
                                          },
                                          new byte[]
                                          {
                                            0x06, 0x01, 0x01, 0x01, 0x7e
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x08, 0x14, 0x22, 0x41
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x01, 0x01, 0x01, 0x01
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x20, 0x10, 0x20, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x10, 0x08, 0x04, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x3e, 0x41, 0x41, 0x41, 0x3e
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x48, 0x48, 0x48, 0x30
                                          }, // P
                                          new byte[]
                                          {
                                            0x3e, 0x41, 0x45, 0x42, 0x3d
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x48, 0x4c, 0x4a, 0x31
                                          },
                                          new byte[]
                                          {
                                            0x31, 0x49, 0x49, 0x49, 0x46
                                          },
                                          new byte[]
                                          {
                                            0x40, 0x40, 0x7f, 0x40, 0x40
                                          },
                                          new byte[]
                                          {
                                            0x7e, 0x01, 0x01, 0x01, 0x7e
                                          },
                                          new byte[]
                                          {
                                            0x7c, 0x02, 0x01, 0x02, 0x7c
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x02, 0x04, 0x02, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x63, 0x14, 0x08, 0x14, 0x63
                                          },
                                          new byte[]
                                          {
                                            0x60, 0x10, 0x0f, 0x10, 0x60
                                          },
                                          new byte[]
                                          {
                                            0x43, 0x45, 0x49, 0x51, 0x61
                                          }, // Z
  
                                          new byte[]
                                          {
                                            0x3e, 0x45, 0x49, 0x51, 0x3e
                                          }, // 0 (pretty)
                                          new byte[]
                                          {
                                            0x00, 0x10, 0x20, 0x7f, 0x00
                                          },
                                          new byte[]
                                          {
                                            0x47, 0x49, 0x49, 0x49, 0x31
                                          },
                                          new byte[]
                                          {
                                            0x42, 0x49, 0x59, 0x69, 0x46
                                          },
                                          new byte[]
                                          {
                                            0x08, 0x18, 0x28, 0x7f, 0x08
                                          },
                                          new byte[]
                                          {
                                            0x71, 0x49, 0x49, 0x49, 0x46
                                          },
                                          new byte[]
                                          {
                                            0x3e, 0x49, 0x49, 0x49, 0x06
                                          },
                                          new byte[]
                                          {
                                            0x40, 0x47, 0x48, 0x50, 0x60
                                          },
                                          new byte[]
                                          {
                                            0x36, 0x49, 0x49, 0x49, 0x36
                                          },
                                          new byte[]
                                          {
                                            0x30, 0x49, 0x49, 0x49, 0x3e
                                          }, // 9
  
                                          new byte[]
                                          {
                                            0x7f, 0x41, 0x41, 0x41, 0x7f
                                          }, // 0 (block)
                                          new byte[]
                                          {
                                            0x00, 0x00, 0x00, 0x00, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x4f, 0x49, 0x49, 0x49, 0x79
                                          },
                                          new byte[]
                                          {
                                            0x49, 0x49, 0x49, 0x49, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x78, 0x08, 0x08, 0x08, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x79, 0x49, 0x49, 0x49, 0x4f
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x49, 0x49, 0x49, 0x4f
                                          },
                                          new byte[]
                                          {
                                            0x40, 0x40, 0x40, 0x40, 0x7f
                                          },
                                          new byte[]
                                          {
                                            0x7f, 0x49, 0x49, 0x49, 0x7f
                                          }, // 8
                                          new byte[]
                                          {
                                            0x79, 0x49, 0x49, 0x49, 0x7f
                                          }
                                        };

        /*
         * Copy a character glyph from the myfont data structure to
         * display memory, with its upper left at the given coordinate
         * This is unoptimized and simply uses setPixel() to draw each dot.
         */

        private void DrawChar(int x,
                               int y,
                               char c)
        {
            int index = 0;

            if (c >= 'A' && c <= 'Z')
            {
                index = c - 'A' + 1; // A-Z maps to 1-26
            }
            else if (c >= 'a' && c <= 'z')
            {
                index = c - 'a' + 1; // A-Z maps to 1-26
            }
            else if (c >= '0' && c <= '9')
            {
                index = ((c - '0') + 27);
            }
            else if (c == ' ')
            {
                index = 0; // space
            }

            for (byte col = 0; col < 5; col++)
            {
                byte dots = _myfont[index][col];
                for (byte row = 0; row < 7; row++)
                {
                    SetPixel(x + col, y + row, (dots & (64 >> row)) != 0 ? 1 : 0);
                }
            }
        }


        // Write out an entire string (Null terminated)
        public void DrawString(int x,
                                int y,
                                string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                DrawChar(x, y, s[i]);
                x += CHAR_WIDTH; // Width of each character
            }
        }
    }
}