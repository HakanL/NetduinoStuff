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

    public class MatrixDisplay24x16 : IMatrixDisplay
    {
        private const int BACKBUFFER_SIZE = 48;

        private readonly byte _backBufferSize;
        private readonly OutputPort _clkPin;
        private readonly OutputPort _dataPin;

        private readonly byte[] _displayBuffers; // Will store the pixel data for each display
        private readonly OutputPort[] _displayPins; // Will contain the pins for each CS
        private readonly byte[] _shadowBuffers; // Will store the pixel data for each display

        public byte DisplayCount { get; private set; }

        public int DisplayWidth
        {
            get
            {
                return (24);
            }
        }

        public int DisplayHeight
        {
            get
            {
                return (16);
            }
        }

        public MatrixDisplay24x16(byte numDisplays,
                              Cpu.Pin clkPin,
                              Cpu.Pin dataPin,
                              bool buildShadow)
        {
            _backBufferSize = BACKBUFFER_SIZE;
            DisplayCount = numDisplays;

            // set data & clock pin modes
            _clkPin = new OutputPort(clkPin, true);
            _dataPin = new OutputPort(dataPin, true);

            // allocate RAM buffer for display bits
            // 24 columns * 16 rows / 8 bits = 48 bytes
            int sz = DisplayCount * _backBufferSize;
            _displayBuffers = new byte[sz];

            if (buildShadow)
            {
                // allocate RAM buffer for display bits
                _shadowBuffers = new byte[sz];
            }

            // allocate a buffer for pin assignments
            _displayPins = new OutputPort[numDisplays];

            // Prepare
            PreCommand();
        }

        // Initalise a display
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
            WriteDataBE(8, (byte)HT1632_CMD.COMS11, true);
            WriteDataBE(8, (byte)HT1632_CMD.LEDON, true);
            WriteDataBE(8, (byte)HT1632_CMD.BLOFF, true);
            WriteDataBE(8, (byte)HT1632_CMD.PWM + 15, true);

            ReleaseDisplay(displayNum);
            Clear(displayNum, true);
        }


        // Sync display using progressive write (Can be buggy, very fast)
        public void SyncDisplay(byte displayNum)
        {
            int bufferOffset = _backBufferSize * displayNum;

            SelectDisplay(displayNum);
            WriteDataBE(3, (byte)HT1632_ID.WR); // Send "write to display" command
            WriteDataBE(7, 0); // Send initial address (aka 0)

            // Operating in progressive addressing mode
            for (int addr = 0; addr < _backBufferSize; ++addr) // Thought i'd simplify this a touch
            {
                byte value = _displayBuffers[addr + bufferOffset];
                WriteDataLE(8, value);
            }

            ReleaseDisplay(displayNum);
        }

        public void SyncDisplays()
        {
            for (byte dispNum = 0; dispNum < DisplayCount; dispNum++)
                SyncDisplay(dispNum);
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
            if (paint && !useShadow) SyncDisplay(displayNum);
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
                    WriteDataLE(8, 0x00); // Write nada
                }

                for (int i = 0; i < DisplayCount; ++i) ReleaseDisplay(i); // Disable all displays
            }
        }

        public void SetPixel(byte displayNum,
                              byte x,
                              byte y,
                              byte value,
                              bool paint,
                              bool useShadow = false)
        {
            //      if( displayNum >= DisplayCount )
            //        throw new ArgumentOutOfRangeException( "displayNum" );

            // calculate a pointer into the display buffer (6 bit offset)
            // cap X coordinate at 32 column
            // was xyToIndex, will need to change for 2416 support
            int address = xyToIndex(x, y);

            // offset to the correct buffer for the display
            address += _backBufferSize * displayNum;
            byte bit = (byte)(1 << (y > 7 ? y - 8 : y));

            // ...and apply the value
            if (value != 0)
            {
                if (useShadow)
                {
                    _shadowBuffers[address] |= bit;
                }
                else
                {
                    _displayBuffers[address] |= bit;
                }
            }
            else
            {
                if (useShadow)
                {
                    _shadowBuffers[address] &= (byte)~bit;
                }
                else
                {
                    _displayBuffers[address] &= (byte)~bit;
                }
            }

            if (useShadow || !paint) return;

            byte dispAddress = DisplayXYToIndex(x, y);
            value = _displayBuffers[address];
            if (((y >> 2) & 1) != 0)
            // Devide y by 4. Work out whether it's odd or even. 8 pixels packed into 1 byte. 16 pixels are in two bytes. We need to figure out whether to shift the buffer
            {
                value = (byte)(_displayBuffers[address] >> 4);
            }

            WriteNibble(displayNum, dispAddress, value);
        }

        public byte GetPixel(byte displayNum,
                              byte x,
                              byte y,
                              bool useShadow)
        {
            if (displayNum >= DisplayCount)
                throw new ArgumentOutOfRangeException("displayNum");

            // Encode XY to an appropriate XY address
            // cap X coordinate at 32 column
            // was xyToIndex, will need to change for 2416 support
            int address = xyToIndex(x, y);

            // offset to the correct buffer for the display
            address += _backBufferSize * displayNum;

            byte bit = (byte)(1 << (y > 7 ? y - 8 : y));

            byte value;

            if (useShadow)
            {
                value = _shadowBuffers[address];
            }
            else
            {
                value = _displayBuffers[address];
            }


            return (byte)(((value & bit) != 0) ? 1 : 0);
        }


        public void SetBrightness(int displayNum,
                                   int pwmValue)
        {
            if (displayNum >= DisplayCount)
                throw new ArgumentOutOfRangeException("displayNum");

            // Check boundaries
            if (pwmValue > 15) pwmValue = 15;
            else if (pwmValue < 0) pwmValue = 0;

            SelectDisplay(displayNum);
            PreCommand();
            WriteDataBE(8, (byte)(HT1632_CMD.PWM + pwmValue), true);
            ReleaseDisplay(displayNum);
        }

        // Converts a cartesian coordinate to a display index
        private byte DisplayXYToIndex(byte x, byte y)
        {
            int address = y == 0 ? 0 : (y / 4);
            address += x << 2;

            return (byte)address;
        }

        // Converts caretesian coordinate to the custom display buffer index
        private byte xyToIndex(byte x, byte y)
        {
            // cap X coordinate at 32 column
            x = (x > 23) ? (byte)23 : x;
            // cap Y coordinate at 16
            y &= 0xF;

            //             int address = y >> 2;
            //             address += ((x & 24) + (7 - (x & 7))) << 2;

            int address = y > 7 ? 1 : 0;
            address += ((x & 0xf8) + (7 - (x & 7))) << 1;
//            address += x << 1;

            return (byte)address;
        }

        // Enables/disables a specific display in the series
        private void SelectDisplay(int displayNum)
        {
            //Debug.Print("Selecting Display: " + displayNum);
            _displayPins[displayNum].Write(false);
        }

        private void ReleaseDisplay(int displayNum)
        {
            //Debug.Print("Releasing Display: " + displayNum);
            _displayPins[displayNum].Write(true);
        }

        // Writes data to the write MSB first
        private void WriteDataBE(int bitCount,
                                  byte data,
                                  bool useNop = false)
        {
            // assumes correct display is selected
            for (int i = bitCount - 1; i >= 0; --i)
            {
                _clkPin.Write(false);
                _dataPin.Write(((data >> i) & 1) > 0);
                _clkPin.Write(true);
            }

            if (useNop)
            {
                _clkPin.Write(false); //clk = 0 for data ready
                // _nop();
                // _nop();
                _clkPin.Write(true); //clk = 1 for data write into 1632
            }
        }

        // Writes data to the wire LSB first
        private void WriteDataLE(int bitCount,
                                  byte data)
        {
            //if(bitCount <= 0 || bitCount > 8) return;

            // assumes correct display is selected
            for (int i = 0; i < bitCount; ++i)
            {
                _clkPin.Write(false);
                _dataPin.Write(((data >> i) & 1) > 0);
                _clkPin.Write(true);
            }
        }

        // Write command to write
        private void writeCommand(byte displayNum,
                                   byte command)
        {
            SelectDisplay(displayNum);
            _dataPin.Write(true);
            WriteDataBE(3, (byte)HT1632_ID.CMD); // Write out MSB [3 bits]
            WriteDataBE(8, command); // Then MSB [7 8 bits]
            WriteDataBE(1, 0); // 1 bit extra 
            _dataPin.Write(false);
            ReleaseDisplay(displayNum);
        }

        private void PreCommand() // Sends 100 down the line
        {
            _clkPin.Write(false);
            _dataPin.Write(true);
            // _nop();

            _clkPin.Write(true);
            // _nop();
            // _nop();

            _clkPin.Write(false);
            _dataPin.Write(false);
            // _nop();

            _clkPin.Write(true);
            // _nop();
            // _nop();

            _clkPin.Write(false);
            _dataPin.Write(false);
            // _nop();

            _clkPin.Write(true);
            // _nop();
            // _nop();
        }


        // Write a single nybble to the display (the display writes 4 bits at a time min)
        private void WriteNibble(int displayNum,
                                  byte addr,
                                  byte data)
        {
            SelectDisplay(displayNum); // Select chip
            WriteDataBE(3, (byte)HT1632_ID.WR); // send ID: WRITE to RAM
            WriteDataBE(7, addr); // Send address
            WriteDataLE(4, data); // send 4 bits of data
            ReleaseDisplay(displayNum); // done
        }

        // Write a single nybble to the display (the display writes 4 bits at a time min)
        private void WriteNibbles(int displayNum,
                                   byte addr,
                                   byte[] data,
                                   byte nybbleCount)
        {
            SelectDisplay(displayNum); // Select chip
            WriteDataBE(3, (byte)HT1632_ID.WR); // send ID: WRITE to RAM
            WriteDataBE(7, addr); // Send address
            for (int i = 0; i < nybbleCount; ++i) WriteDataLE(4, data[i]); // send multiples of 4 bits of data
            ReleaseDisplay(displayNum); // done
        }

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
        }
    }
}
