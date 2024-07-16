# MOTEUS COMMAND LINE
# TVIEW: python -m moteus_gui.tview
# STOP:  python -m moteus.moteus_tool --target 1 --stop
# ZERO:  python -m moteus.moteus_tool --target 1 --zero-offset

import asyncio
import moteus
import socket
import struct
import math


host, port = "127.0.0.1", 25001
data = [0.0]
dataReceived = [-2.0, 0.2, 0.0, 0.25]

# SOCK_STREAM means TCP socket 
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

sock.connect((host, port))

async def main():
    # Construct a default controller at id 1.
    c = moteus.Controller()

    # Clear any outstanding faults.
    await c.set_stop()

    # Wherever the moteus is currently, that becomes position 0.0
    # Make sure the lollipop is on the table as far as it can go counterclockwise / to the left
    # await c.set_output_exact(0.0)

    while True:

        state = await c.query()
        position = state.values[moteus.Register.POSITION]
        data[0] = position
        sock.sendall(struct.pack('<1f', *data))
        # Receive stuff back from C#:
        dataReceived = struct.unpack('<4f', sock.recv(1024))

        if (dataReceived[0] == 1.0):
            result = await c.set_position(position=dataReceived[1], accel_limit=1.5,
                                            velocity_limit = dataReceived[3], feedforward_torque = dataReceived[2])
        elif (dataReceived[0] == 2.0):
            result = await c.set_position(position=float('nan'), accel_limit=1.5, 
                                            velocity_limit = dataReceived[3], feedforward_torque = dataReceived[2])
        elif (dataReceived[0] == 0.0):
            result = await c.set_stop()
        elif (dataReceived[0] == -1.0):
            result = await c.set_stop()
            quit()

if __name__ == '__main__':
    asyncio.run(main())
