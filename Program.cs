﻿using System;
using System.IO;

namespace Chip8Emulator
{
	class Program
	{
		static void Main(string[] args)
		{
			using (BinaryReader reader = new BinaryReader(new FileStream("roms\\test.ch8", FileMode.Open)))
			{
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					ushort opcode = (ushort)((reader.ReadByte() << 8) | reader.ReadByte());
					Console.WriteLine($"{opcode.ToString("X4")}");
				}

				Console.ReadKey();
			}
		}
	}

	public class CPU
	{
		public byte[] Memory = new byte[4096];    // RAM
		public byte[] V = new byte[16];           // Registers V0 to VF
		public ushort I;                          // Address Register
		public ushort PC;                         // Program Counter
		public ushort[] Stack = new ushort[24];   // Stack
		public byte SP = 0;                       // Stack Pointer 
		public byte DelayTimer;                   // Delay Timer
		public byte SoundTimer;                   // Sound Timer
		public byte KeyBoard;                     // Keyboard Input
		public byte[] Screen = new byte[64 * 32]; // Screen

		public void executeOpcode(ushort opcode)
		{
			switch (opcode & 0xF000)
			{
				case 0x0000:
					if (opcode == 0x00E0) // Clear Screen
					{
						for(int i = 0; i < Screen.Length; i++) Screen[i] = 0;
					}
					else if (opcode == 0x00EE) // Return from subroutine
					{
						PC = Stack[SP--];
					}
					break;
					
			}
		}
	}
}
