using System;
using Microsoft.SPOT.Hardware;

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


    public class MatrixDisplay24x16 : IMatrixDisplay
    {
        private Microsoft.SPOT.Hardware.SPI.Configuration _spiConfig;

        private readonly ushort[] _mainBuffer;        // Used to output data with SPI
        private readonly ushort[] _mainBuffer2;        // Used to output data with SPI, single byte array

        
        public MatrixDisplay24x16(Microsoft.SPOT.Hardware.SPI.Configuration spiConfig)
        {
            this._spiConfig = spiConfig;

            _mainBuffer = new ushort[26];
            _mainBuffer2 = new ushort[1];

            InitWriteBuffer();

            // Init HT1632 display
            using (Microsoft.SPOT.Hardware.SPI spi = new SPI(_spiConfig))
            {
                WriteCommand(spi, HT1632_CMD.SYSDIS);
                WriteCommand(spi, HT1632_CMD.SYSON);
                WriteCommand(spi, HT1632_CMD.COMS11);
                WriteCommand(spi, HT1632_CMD.LEDON);
                WriteCommand(spi, HT1632_CMD.BLOFF);
                WriteCommand(spi, HT1632_CMD.PWM + 15);
            }

            Clear(true);
        }


        private void InitWriteBuffer()
        {
            Array.Clear(_mainBuffer, 1, 25);
            _mainBuffer[0] = (ushort)HT1632_ID.WR << 12;
            _mainBuffer2[0] = (ushort)((ushort)HT1632_ID.WR << 12) + 0xbe0;
        }


        // Write command to write
        private void WriteCommand(Microsoft.SPOT.Hardware.SPI spi, HT1632_CMD command)
        {
            ushort value = (ushort)HT1632_ID.CMD << 12;
            value += (ushort)((byte)command << 4);

            spi.Write(new ushort[] { value });
        }

        
        private int GetBitIndex(byte x, byte y)
        {
            x = (x > 23) ? (byte)23 : x;
            // cap Y coordinate at 16
            y &= 0xF;

            int address = y;
            address += ((x & 0xf8) + (7 - (x & 7))) << 4;

            return address;
        }


        private void SetBitInBuffer(int bitAddress, byte value)
        {
            ushort mask;
            if (bitAddress >= 380)
            {
                mask = (ushort)(0x10 >> (bitAddress - 380));

                if (value != 0)
                    _mainBuffer2[0] |= mask;
                else
                    _mainBuffer2[0] &= (ushort)~mask;

                return;
            }

            // Offset for cmd
            bitAddress += 10;

            int bufferAddr = bitAddress / 15;
            mask = (ushort)(0x4000 >> (bitAddress % 15));

            if (value != 0)
                _mainBuffer[bufferAddr] |= mask;
            else
                _mainBuffer[bufferAddr] &= (ushort)~mask;
        }


        private byte GetBitInBuffer(int bitAddress)
        {
            ushort mask;
            if (bitAddress >= 380)
            {
                mask = (ushort)(0x10 >> (bitAddress - 380));

                return (byte)((_mainBuffer2[0] & mask) != 0 ? 1 : 0);
            }

            // Offset for cmd
            bitAddress += 10;

            int bufferAddr = bitAddress / 15;
            mask = (ushort)(0x4000 >> (bitAddress % 15));

            return (byte)((_mainBuffer[bufferAddr] & mask) != 0 ? 1 : 0);
        }


        public void SyncDisplay()
        {
            using (Microsoft.SPOT.Hardware.SPI spi = new SPI(_spiConfig))
            {
                spi.Write(_mainBuffer);
                spi.Write(_mainBuffer2);
            }
        }


        public void Clear(bool paint = false)
        {
            InitWriteBuffer();
            if (paint)
                SyncDisplay();
            return;
        }


        public void SetPixel(byte x, byte y, byte value, bool paint = false)
        {
            int bitIndex = y + (((x & 0xf8) + (7 - (x & 7))) << 4);

            SetBitInBuffer(bitIndex, value);

            if (paint)
                SyncDisplay();
        }


        public void Set8Pixels(byte x, byte y, byte value)
        {
            int bitIndex = y + (((x & 0xf8) + (7 - (x & 7))) << 4);

            SetDataInBuffer(bitIndex, value);
        }


        public byte GetPixel(byte x, byte y)
        {
            int bitIndex = y + (((x & 0xf8) + (7 - (x & 7))) << 4);

            // Encode XY to an appropriate XY address
            // cap X coordinate at 32 column
            // was xyToIndex, will need to change for 2416 support
            int address = GetBitIndex(x, y);

            return GetBitInBuffer(bitIndex);
        }


        public void SetBrightness(int pwmValue)
        {
            // Check boundaries
            if (pwmValue > 15)
                pwmValue = 15;
            else
                if (pwmValue < 0)
                    pwmValue = 0;

            using (Microsoft.SPOT.Hardware.SPI spi = new SPI(_spiConfig))
            {
                WriteCommand(spi, HT1632_CMD.PWM + pwmValue);
            }
        }


        private byte GetDataFromBuffer(int bitIndex)
        {
            if (bitIndex >= 380)
                //TODO
                return 0;

            bitIndex += 10;

            int bufferAddr = bitIndex / 15;
            int bitPos = bitIndex % 15;
            int mask = (int)(0x7f800000 >> bitPos);

            int existing = _mainBuffer[bufferAddr] << 16;
            if (bitPos > 8 && bufferAddr < 25)
                existing += _mainBuffer[bufferAddr + 1] << 1;

            return (byte)((existing & mask) >> (23 - bitPos));
        }

        private void SetDataInBuffer(int bitIndex, byte data)
        {
            if(bitIndex >= 380)
                //TODO
                return;

            bitIndex += 10;

            int bufferAddr = bitIndex / 15;
            int bitPos = bitIndex % 15;
            int newData = (data << (23 - bitPos));
            int mask = (int)(0x7f800000 >> bitPos);

            int existing = _mainBuffer[bufferAddr] << 16;
            if (bitPos > 8 && bufferAddr < 25)
                existing += _mainBuffer[bufferAddr + 1] << 1;

            existing &= ~mask;
            existing |= newData;

            _mainBuffer[bufferAddr] = (ushort)(existing >> 16);
            if (bitPos > 8 && bufferAddr < 25)
                _mainBuffer[bufferAddr + 1] = (ushort)((existing & 0xffff) >> 1);
        }


        public byte ScrollLeft(byte newData)
        {
            byte outData = 0;
            byte y = 0;

            int bitIndex = GetBitIndex(0, y);

            outData = GetDataFromBuffer(bitIndex);

            for (byte x = 1; x < 24; x++)
            {
                int newIndex = GetBitIndex(x, y);

                byte data = GetDataFromBuffer(newIndex);
                
                SetDataInBuffer(bitIndex, data);

                bitIndex = newIndex;
            }

            SetDataInBuffer(bitIndex, newData);

            return outData;
        }


        public ushort ScrollLeft2(ushort newData)
        {
            ushort outData = 0;
            byte y = 0;

            int bitIndex1 = GetBitIndex(0, y);
            int bitIndex2 = GetBitIndex(1, y);

            outData = (ushort)(GetDataFromBuffer(bitIndex1) << 8);
            outData += GetDataFromBuffer(bitIndex2);

            for (byte x = 2; x < 23; x += 2)
            {
                int newIndex1 = GetBitIndex(x, y);
                int newIndex2 = GetBitIndex((byte)(x + 1), y);

                byte data1 = GetDataFromBuffer(newIndex1);
                byte data2 = GetDataFromBuffer(newIndex2);

                SetDataInBuffer(bitIndex1, data1);
                SetDataInBuffer(bitIndex2, data2);

                bitIndex1 = newIndex1;
                bitIndex2 = newIndex2;
            }

            SetDataInBuffer(bitIndex1, (byte)(newData >> 8));
            SetDataInBuffer(bitIndex2, (byte)newData);

            return outData;
        }

    
    }
}
