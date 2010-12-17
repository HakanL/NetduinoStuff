using System;
using Microsoft.SPOT;


namespace NetDuino.Hardware.HT1632
{
    /*
      HT1632 MatrixDisplay interface
      Borrowed ideas from Miles Burton's implementation of the Matrix_display Library 2.0 for Arduino
      Copyright (C) 2010 Hakan Lindestaf All Rights Reserved

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

    
    public class MultiDisplay
    {
        private readonly IMatrixDisplay[] displays;
        private readonly byte displayWidth;
        private readonly byte displayHeight;


        public MultiDisplay(byte numberOfDisplays, byte displayWidth, byte displayHeight)
        {
            if (numberOfDisplays < 1 || numberOfDisplays > 4)
                throw new ArgumentOutOfRangeException("Only supports 1-4 displays");

            displays = new IMatrixDisplay[numberOfDisplays];

            this.displayWidth = displayWidth;
            this.displayHeight = displayHeight;
        }


        public void InitDisplay(byte displayNumber, IMatrixDisplay displayInstance)
        {
            displays[displayNumber] = displayInstance;
        }


        public byte DisplayWidth
        {
            get { return displayWidth; }
        }

        public byte DisplayHeight
        {
            get { return displayHeight; }
        }

        public byte TotalWidth
        {
            get { return (byte)(displays.Length * displayWidth); }
        }

        public void SetPixel(byte x, byte y, byte value, bool paint)
        {
            byte displayNum = CalcDisplayNum(ref x);

            displays[displayNum].SetPixel(x, y, value, paint);
        }

        public void SyncDisplays()
        {
            for (byte dispNum = 0; dispNum < displays.Length; dispNum++)
                SyncDisplay(dispNum);
        }

        public void SyncDisplay(byte displayNum)
        {
            displays[displayNum].SyncDisplay();
        }

        public void SetBrightness(byte pwmValue)
        {
            for (byte dispNum = 0; dispNum < displays.Length; dispNum++)
                displays[dispNum].SetBrightness(pwmValue);
        }

        public void Clear()
        {
            for (byte dispNum = 0; dispNum < displays.Length; dispNum++)
                displays[dispNum].Clear();
        }

        private byte CalcDisplayNum(ref byte x)
        {
            int displayNum = 0;
            if (x >= DisplayWidth)
            {
                displayNum = x / DisplayWidth;
                x -= (byte)(DisplayWidth * displayNum);
            }
            return (byte)displayNum;
        }


    }
}
