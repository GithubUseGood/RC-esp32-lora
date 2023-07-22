# Radio control solution using esp32 LoRa.
 This uses a c# console application to forward mapped data of a external controller.
 Then the esp32 picks up on it from serial communication and sends it to the 2nd esp32 with LoRa functions.
 
 This code is particulary designed for the Heltec Esp32 LoRa 433-510mhz board but the code can always be modified.
 Feel free to fix or add features to the code.
 
 how to use:
 get a controller plug it into the host pc running the c# code.
 plug in a esp32 lora board and it will send data to the second esp32 board.

 You cant directly connect the controller to the esp32 because it lacks usb functions.
 And the delay is already pretty good at the minimum spreading factor ≈ 60ms.
 
 todo: add a keybind for the receiver esp to send back some data such as gps.
       fix serial monitor in c#
       make it so spread factor increases with weaker RSSI
       add failsafe for rc airplane uses.
 
