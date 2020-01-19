using System;
using System.IO;

namespace Chip8Emulator
{
	class Program
	{
		static void Main(string[] args)
		{
			CPU cpu = new CPU();

			using (BinaryReader reader = new BinaryReader(new FileStream("roms\\test.ch8", FileMode.Open)))
			{
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					ushort opcode = (ushort)((reader.ReadByte() << 8) | reader.ReadByte());

					try
					{
						cpu.executeOpcode(opcode);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
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
						for (int i = 0; i < Screen.Length; i++) Screen[i] = 0;
					}
					else if (opcode == 0x00EE) // Return from subroutine
					{
						PC = Stack[SP--];
					}
					break;
				case 0x1000:
					PC = (ushort)(opcode & 0x0fff); // 1NNN - Jump - Set PC to NNN
					break;
				case 0x2000:
					Stack[SP++] = PC;
					PC = (ushort)(opcode & 0x0fff); // 2NNN - Call subroutine - Stack current PC and jump
					break;
				case 0x3000:
					if (V[(opcode & 0x0f00) >> 8] == (opcode & 0x00ff)) PC += 2; // 3XNN - Skip next instruction if VX == NN
					break;
				case 0x4000:
					if (V[(opcode & 0x0f00) >> 8] != (opcode & 0x00ff)) PC += 2; // 4XNN - Skip next instruction if VX != NN
					break;
				case 0x5000:
					if (V[(opcode & 0x0f00) >> 8] == V[(opcode & 0x00f0) >> 4]) PC += 2; PC += 2; // 5XY0 - Skip next instruction if VX == VY
					break;
				case 0x6000:
					V[(opcode & 0x0f00) >> 8] = (byte)(opcode & 0x00ff); // 6XNN - Set VX to NN
					break;
				case 0x7000:
					V[(opcode & 0x0f00) >> 8] += (byte)(opcode & 0x00ff); // 6XNN - Add NN to VX
					break;
				case 0x8000: // 8XYN
					int X = (opcode & 0x0f00) >> 8; // Get X
					int Y = (opcode & 0x00f0) >> 4; // Get Y
					switch (opcode & 0x000f)
					{
						case 0:
							V[X] = V[Y]; // 8XY0 - Set VX = VY
							break;
						case 1:
							V[X] = (byte)(V[X] | V[Y]); // 8XY1 - Set VX = VX | VY
							break;
						case 2:
							V[X] = (byte)(V[X] & V[Y]); // 8XY2 - Set VX = VX & VY
							break;
						case 3:
							V[X] = (byte)(V[X] ^ V[Y]); // 8XY3 - Set VX = VX ^ VY
							break;
						case 4:
							V[0xf] = (byte)(V[X] + V[Y] > 255 ? 1 : 0); // 8XY4 - Set VX = VX + VY, Set VF to carry
							V[X] = (byte)((V[X] + V[Y]) & 0x00ff);
							break;
						case 5:
							V[0xf] = (byte)(V[X] > V[Y] ? 1 : 0); // 8XY5 - Set VX = VX - VY, Set VF to borrow
							V[X] = (byte)((V[X] - V[Y]) & 0x00ff);
							break;
						case 6:
							V[0xf] = (byte)(V[X] & 0x0001); // 8XY6 - Right shift VX, Set VF to LSB
							V[X] = (byte)(V[X] >> 1);
							break;
						case 7:
							V[0xf] = (byte)(V[Y] > V[X] ? 1 : 0); // 8XY7 - Set VX = VY - VX, Set VF to borrow
							V[X] = (byte)((V[Y] - V[X]) & 0x00ff);
							break;
						case 0xe:
							V[0xf] = (byte)((V[X] & 0x80) == 0x80 ? 1 : 0); // 8XY6 - Left shift VX, Set VF to MSB
							V[X] = (byte)(V[X] << 1);
							break;
						default:
							throw new Exception($"Unsupported opcode: {opcode.ToString("X4")}");
					}
					break;
				default:
					throw new Exception($"Unsupported opcode: {opcode.ToString("X4")}");
			}
		}
	}
}
