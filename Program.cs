using System;
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
}
