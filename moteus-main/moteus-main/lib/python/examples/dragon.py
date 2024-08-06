# MOTEUS COMMAND LINE
# TVIEW: python -m moteus_gui.tview
# STOP:  python -m moteus.moteus_tool --target 1 --stop
# ZERO:  python -m moteus.moteus_tool --target 1 --zero-offset

import asyncio
import moteus
import socket
import struct
import math
import time


host, port = "10.32.13.235", 25001
data = [0.0]
dataReceived = [-2.0, 0.2, 0.0, 0.25]

TORQUE_CONSTANT = 0.25 # The amount of feedforward torque needed to compensate for gravity when the arm is parallel to the ground
RESTING_ANGLE = 0.025 # The true resting angle of the motor arm in radians

start_time = time.time()

def lerp(a, b, f):
    return a + f * (b - a)

def calculate_feedforward_torque(x: float) -> float:
    return math.cos(x*6.2832 + RESTING_ANGLE) * TORQUE_CONSTANT


async def main():

    # SOCK_STREAM means TCP socket 
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        sock.bind((host, port))
        sock.listen(1)
        conn, addr = sock.accept()
        print(f"Connected by {addr}")
    finally:
        sock.close()

    with conn:

        # Construct a default controller at id 1 and clear outstanding faults
        c = moteus.Controller()
        await c.set_stop()

        # Wherever the moteus is currently, that becomes position 0.0
        # Make sure the motor arm is on the table as far as it can go counterclockwise / to the left
        await c.set_output_exact(position = 0.0)

        current_feedforward_torque = 0.0;

        while True:

            state = await c.query()
            position = state.values[moteus.Register.POSITION]
            data[0] = position
            
            conn.sendall(struct.pack('<1f', *data))

            # Receive stuff back from C#:
            dataReceived = struct.unpack('<3f', conn.recv(12))

            print("%5d: [%2d, %6.4f, %6.4f]" % (int(time.time() - start_time), dataReceived[0], dataReceived[1], dataReceived[2]))

            if (dataReceived[0] == 1.0): # targeted + floaty
                current_feedforward_torque = calculate_feedforward_torque(position)
                await c.set_position(position = dataReceived[1],
                                     accel_limit = 1.5,
                                     velocity_limit = dataReceived[2],
                                     feedforward_torque = current_feedforward_torque)
            elif (dataReceived[0] == 2.0): # targeted + weighty
                current_feedforward_torque = lerp(current_feedforward_torque, -0.5 * calculate_feedforward_torque(position), 0.2);
                await c.set_position(position=dataReceived[1],
                                     accel_limit=1.5,
                                     velocity_limit = dataReceived[2],
                                     feedforward_torque = current_feedforward_torque)
            elif (dataReceived[0] == 3.0): # idle flying (floaty)
                current_feedforward_torque = calculate_feedforward_torque(position)
                await c.set_position(position=position,
                                     accel_limit=1.5, 
                                     velocity_limit = dataReceived[2],
                                     feedforward_torque = current_feedforward_torque)
            elif (dataReceived[0] == 0.0): # power motor off
                result = await c.set_stop()
            elif (dataReceived[0] == -1.0): # power motor off and quit program
                result = await c.set_stop()
                conn.close()
                quit()

if __name__ == '__main__':
    asyncio.run(main())