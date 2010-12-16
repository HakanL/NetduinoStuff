namespace NetDuino.Hardware.HT1632
{
  /*
    HT1632 utility classes
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

  public enum HT1632_ID
  {
    CMD = 4, /* ID = 100 - Commands */
    RD = 6, /* ID = 110 - Read RAM */
    WR = 5, /* ID = 101 - Write RAM */
  }

  public enum HT1632_CMD
  {
    SYSDIS = 0x00, /* CMD= 0000-0000-x Turn off oscil */
    SYSON = 0x01, /* CMD= 0000-0001-x Enable system oscil */
    LEDOFF = 0x02, /* CMD= 0000-0010-x LED duty cycle gen off */
    LEDON = 0x03, /* CMD= 0000-0011-x LEDs ON */
    BLOFF = 0x08, /* CMD= 0000-1000-x Blink OFF */
    BLON = 0x09, /* CMD= 0000-1001-x Blink On */
    SLVMD = 0x10, /* CMD= 0001-0= 0xx-x Slave Mode */
    MSTMD = 0x14, /* CMD= 0001-01xx-x Master Mode */
    RCCLK = 0x18, /* CMD= 0001-1= 0xx-x Use on-chip clock */
    EXTCLK = 0x1C, /* CMD= 0001-11xx-x Use external clock */
    COMS00 = 0x20, /* CMD= 0010-ABxx-x commons options */
    COMS01 = 0x24, /* CMD= 0010-ABxx-x commons options */
    COMS10 = 0x28, /* CMD= 0010-ABxx-x commons options */
    COMS11 = 0x2C, //P-MOS OUTPUT AND 16COMMON OPTION
    /* CMD= 0010-ABxx-x commons options */
    PWM = 0xA0, /* CMD= 101x-PPPP-x PWM duty cycle */
  }
}