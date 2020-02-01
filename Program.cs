using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Chip8Emulator
{
	class Program
	{
		static void Main(string[] args)
		{
			CPU cpu = new CPU();

			using (BinaryReader reader = new BinaryReader(new FileStream("roms\\test.ch8", FileMode.Open)))
			{
				List<byte> program = new List<byte>();

				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					program.Add(reader.ReadByte());
				}

				cpu.LoadProgram(program.ToArray());

			}
			Console.Clear(); // Clear "Screen"
			Console.CursorVisible = false; // Hide Cursor
			while (true)
			{
				cpu.ExecuteOpcode();
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

		public bool WaitingForKeyPress = false;

		private Stopwatch stopWatch = new Stopwatch();

		private Random rng = new Random(Environment.TickCount); // Random number generator for CXKK

		public void LoadProgram(byte[] program)
		{
			InitializeFontSet();
			for (int i = 0; i < program.Length; i++)
			{
				Memory[512 + i] = program[i];
			}
			PC = 512;
		}

		public void Draw()
		{
			Console.SetCursorPosition(0, 0);
			for (int y = 0; y < 32; y++)
			{
				StringBuilder line = new StringBuilder("");
				for (int x = 0; x < 64; x++)
				{
					if (Screen[x + y * 64] == 0)
						line.Append(" ");
					else
						line.Append("█");
				}
				Console.WriteLine(line.ToString());
			}
		}

		private void InitializeFontSet()
		{
			byte[] characters = new byte[] {
				0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
				0x20, 0x60, 0x20, 0x20, 0x70, // 1
				0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
				0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
				0x90, 0x90, 0xF0, 0x10, 0x10, // 4
				0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
				0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
				0xF0, 0x10, 0x20, 0x40, 0x40, // 7
				0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
				0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
				0xF0, 0x90, 0xF0, 0x90, 0x90, // A
				0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
				0xF0, 0x80, 0x80, 0x80, 0xF0, // C
				0xE0, 0x90, 0x90, 0x90, 0xE0, // D
				0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
				0xF0, 0x80, 0xF0, 0x80, 0x80  // F
			};
			Array.Copy(characters, Memory, characters.Length);
		}

		public void ExecuteOpcode()
		{
			if(!stopWatch.IsRunning) stopWatch.Start(); // Start stopwatch if not running

			if(stopWatch.ElapsedMilliseconds > 16.666) // Update at 60 Hz
			{
				if(SoundTimer > 0) SoundTimer--;
				if(DelayTimer > 0) DelayTimer--;

				stopWatch.Restart();
			}
			
			ushort opcode = (ushort)((Memory[PC] << 8) | Memory[PC + 1]);

			if (WaitingForKeyPress)
			{
				V[(opcode & 0x0f00) >> 8] = KeyBoard;
				return;
			}

			PC += 2;

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
							V[0xf] = (byte)((V[X] & 0x80) == 0x80 ? 1 : 0); // 8XYE - Left shift VX, Set VF to MSB
							V[X] = (byte)(V[X] << 1);
							break;
						default:
							throw new Exception($"Unsupported opcode: {opcode.ToString("X4")}");
					}
					break;
				case 0x9000:
					if (V[(opcode & 0x0f00) >> 8] != V[(opcode & 0x00f0) >> 4]) PC += 2; PC += 2; // 9XY0 - Skip next instruction if VX != VY
					break;
				case 0xA000:
					I = (ushort)(opcode & 0x0fff); // ANNN - Set I to NNN
					break;
				case 0xB000:
					PC = (ushort)(V[0] + (opcode & 0x0fff)); // BNNN - Set PC = V0 + NNN
					break;
				case 0xC000:
					V[(opcode & 0x0f00) >> 8] = (byte)(rng.Next(0, 255) & (opcode & 0x00ff)); // CXKK - Set VX = Rand(0, 255) & KK
					break;
				case 0xD000:
					// DXYN - Draw n byte sprite at location X, Y starting at I
					int dx = V[(opcode & 0x0f00) >> 8];
					int dy = V[(opcode & 0x00f0) >> 4];
					int dn = opcode & 0x000f;

					V[0xf] = 0;

					for (int i = 0; i < dn; i++)
					{
						byte location = Memory[I + i];
						for (int j = 0; j < 8; j++)
						{
							byte pixel = (byte)((location >> (7 - j)) & 0x1);
							int index = dx + j + (dy + i) * 64;
							if (pixel == 1 && Screen[index] == 1) V[0xf] = 1;
							Screen[index] = (byte)(Screen[index] ^ pixel);
						}
					}

					Draw();
					break;
				case 0xE000:
					if ((opcode & 0x00ff) == 0x9e)
					{
						// EX9E - Skip next instruction if key pressed corresponds to VX
						if (((KeyBoard >> V[(opcode & 0x0f00) >> 8]) & 0x01) == 0x01) PC += 2;
						break;
					}
					else if ((opcode & 0x00ff) == 0xa1)
					{
						// EXA1 - Skip next instruction if key pressed does not correspond to VX
						if (((KeyBoard >> V[(opcode & 0x0f00) >> 8]) & 0x01) == 0x01) PC += 2;
						break;
					}
					else throw new Exception($"Unsupported opcode: {opcode.ToString("X4")}");
				case 0xF000:
					int x = (opcode & 0x0f00) >> 8; // Get X
					switch (opcode & 0x00ff)
					{
						case 0x07:
							V[x] = DelayTimer; // FX07 - Set VX = Delay Timer
							break;
						case 0x0a:
							WaitingForKeyPress = true; // FX0A - Wait for key press and set VX = Key pressed
							PC -= 2;
							break;
						case 0x15:
							DelayTimer = V[x]; // FX15 - Set Delay Timer = VX
							break;
						case 0x18:
							SoundTimer = V[x]; // FX18 - Set Sound Timer = VX
							break;
						case 0x1E:
							I = (ushort)(I + V[x]); // FX1E - Set I = I + VX
							break;
						case 0x29:
							I = (ushort)(V[x] * 5); // FX29 - Set I = location of sprite for digit VX
							break;
						case 0x33:
							// FX33 - Store BCD representation of Vx in memory locations I, I+1, and I+2 
							Memory[I] = (byte)(V[x] / 100);
							Memory[I + 1] = (byte)((V[x] % 100) / 10);
							Memory[I + 2] = (byte)(V[x] % 10);
							break;
						case 0x55:
							// FX55 - Store state of registers V0 through VX in Memory at I
							for (int i = 0; i <= x; i++)
								Memory[I + i] = V[i];
							break;
						case 0x65:
							// FX55 - Restore state of registers V0 through VX from Memory at I
							for (int i = 0; i <= x; i++)
								V[i] = Memory[I + i];
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
