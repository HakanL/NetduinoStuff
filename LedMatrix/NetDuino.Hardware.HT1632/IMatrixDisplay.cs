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

    public interface IMatrixDisplay
    {
        void SyncDisplay();
        void Clear(bool paint = false);
        void SetPixel(byte x, byte y, byte value, bool paint = false);
        byte GetPixel(byte x, byte y);
        void SetBrightness(int pwmValue);
    }
}
