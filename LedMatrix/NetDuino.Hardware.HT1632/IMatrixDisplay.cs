using System;
using Microsoft.SPOT.Hardware;

namespace NetDuino.Hardware.HT1632
{
    /*
      HT1632 MatrixDisplay class
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

    public interface IMatrixDisplay
    {
        byte DisplayCount { get; }

        int DisplayWidth { get; }
        int DisplayHeight { get; }
        /*
        public void InitDisplay(byte displayNum,
                                 Cpu.Pin pin,
                                 bool isMaster)
        {
            if (displayNum >= DisplayCount)
                throw new ArgumentOutOfRangeException("displayNum");

            // Associate the pin with this display
            // Disable chip (pull high)
            _displayPins[displayNum] = new OutputPort(pin, true);


            SelectDisplay(displayNum);
            // Send Precommand
            PreCommand();
            // Take advantage of successive mode and write the options
            WriteDataBE(8, (byte)HT1632_CMD.SYSDIS, true);
            //sendOptionCommand(HT1632_CMD.MSTMD);
            WriteDataBE(8, (byte)HT1632_CMD.SYSON, true);
            WriteDataBE(8, (byte)HT1632_CMD.COMS10, true);
            WriteDataBE(8, (byte)HT1632_CMD.LEDON, true);
            WriteDataBE(8, (byte)HT1632_CMD.BLOFF, true);
            WriteDataBE(8, (byte)HT1632_CMD.PWM + 15, true);
            ReleaseDisplay(displayNum);
            Clear(displayNum, true);
        }


        // Sync display using progressive write (Can be buggy, very fast)
        public void SyncDisplays()
        {
            for (int dispNum = 0; dispNum < DisplayCount; ++dispNum)
            {
                int bufferOffset = _backBufferSize * dispNum;

                SelectDisplay(dispNum);
                WriteDataBE(3, (byte)HT1632_ID.WR); // Send "write to display" command
                WriteDataBE(7, 0); // Send initial address (aka 0)

                // Operating in progressive addressing mode
                for (int addr = 0; addr < _backBufferSize; ++addr) // Thought i'd simplify this a touch
                {
                    byte value = _displayBuffers[addr + bufferOffset];
                    WriteDataLE(8, value);
                }

                ReleaseDisplay(dispNum);
            }
        }

        // Clear a single display. 
        // paint ? Send data to display : Only clear data
        public void Clear(byte displayNum,
                           bool paint = false,
                           bool useShadow = false)
        {
            if (displayNum >= DisplayCount)
                throw new ArgumentOutOfRangeException("displayNum");

            // clear the display's backbuffer
            Array.Clear(useShadow ? _shadowBuffers : _displayBuffers, displayNum * _backBufferSize, _backBufferSize);


            // Write out change
            if (paint && !useShadow) SyncDisplays();
        }

        // Clear all displays
        public void Clear(bool paint = false,
                           bool useShadow = false)
        {
            Array.Clear(useShadow ? _shadowBuffers : _displayBuffers, 0, DisplayCount * _backBufferSize);

            // Select all displays and clear
            if (paint && !useShadow)
            {
                for (int i = 0; i < DisplayCount; ++i) SelectDisplay(i); // Enable all displays

                // Use progressive write mode, faster
                WriteDataBE(3, (byte)HT1632_ID.WR); // Send "write to display" command
                WriteDataBE(7, 0); // Send initial address (aka 0)

                for (byte i = 0; i < _backBufferSize; ++i)
                {
                    WriteDataLE(0, 0x00); // Write nada
                }

                for (int i = 0; i < DisplayCount; ++i) ReleaseDisplay(i); // Disable all displays
            }
        }
*/
        void SetPixel(byte displayNum,
                              byte x,
                              byte y,
                              byte value,
                              bool paint,
                              bool useShadow = false);

        byte GetPixel(byte displayNum,
                              byte x,
                              byte y,
                              bool useShadow);

        void SetBrightness(int displayNum,
                                   int pwmValue);

/*
        // Shadow 
        public void CopyBuffer()
        {
            Array.Copy(_displayBuffers, _shadowBuffers, (_backBufferSize * DisplayCount));
        }

        // Shift the buffer Left|Right
        private void ShiftLeft()
        {
            Array.Copy(_displayBuffers, 0, _displayBuffers, 2, (_backBufferSize * DisplayCount) - 2);
        }

        private void ShiftRight()
        {
            Array.Copy(_displayBuffers, 2, _displayBuffers, 0, (_backBufferSize * DisplayCount) - 2);
        }*/
    }
}