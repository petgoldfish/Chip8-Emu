# Chip8 Emulator

This is a Chip8 Virtual Console Emulator written in C#. It is still a work in progress, but basic programs such as the IBM logo should work. However at the moment, the console is being used as the display and there is no audio yet.

Do note that I started this project as a way to learn how emulators work, and to hone my C#, so I took a bunch of help from the internet. However, much of the code was written without help, such as the opcode decoding (which is 85% of the curent code).

# Planned work

1. Implement Keyboard input support.
2. Implement Audio support.
3. Implement actual graphics (probably using SDL or OpenGL).

# References

1. [CowGod's technical reference](http://devernay.free.fr/hacks/chip8/C8TECH10.HTM)
2. [Chip8 on Wikipedia](https://en.wikipedia.org/wiki/CHIP-8)
3. [GiawaVideos on Youtube](https://www.youtube.com/channel/UCCr-745lOvoWD4-5zbd26og) - Shoutout to GiawaVideos! Was super useful when it came to setting up the loop and for some of the harder opcodes. Thank you [Giawa](https://github.com/giawa)!
