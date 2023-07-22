using System.IO.Ports;
using SharpDX.XInput;
using System;
using System.Threading.Tasks;
using NodaTime;
namespace zohdUAVscript
{
    internal class Program
    {



        static void Main(string[] args)
        {
            Console.WriteLine("ZOHD drift controller V3");
            Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Green;
            Controller controller = new Controller(UserIndex.One);
            bool PortExists = false;
            bool ForwardData = true;
            bool CanSendMessage = true;
            bool IsReadyForReceive = true;
            string ControllerState = "";
            string oldControllerState = "";
           



        ControllerNotFoundGoto:

            if (controller.IsConnected == true) // Checks if the controller is conected
            {
                Console.WriteLine("Controller found and connected");

            LoRaNotRealPort:
                Console.WriteLine("Chose port on which the LoRa module is connected to options are:");

                foreach (string s in SerialPort.GetPortNames()) // gets all open ports
                {
                    Console.WriteLine(s);
                }
                string Port = Console.ReadLine();
                foreach (string s in SerialPort.GetPortNames()) // checks if that port exists
                {
                    if (Port == s)
                    {
                        PortExists = true;
                    }
                }
                if (PortExists == false)
                {
                    Console.WriteLine("Non existnant port");
                    goto LoRaNotRealPort;
                   
                }
                SerialPort LoRa = new SerialPort();
                LoRa.BaudRate = 115200;
               
                LoRa.PortName = Port;
        

                try // attempts to send controller data
                {
                    LoRa.Open();
                    Console.WriteLine("Attemptng connection");
                    Console.WriteLine("Type end to end data transfer, or type serial monitor to get a serial monitor - not finished");

                    while (ForwardData == true)
                    {
                        if (CanSendMessage == true) // notify user that the script is connected once rather than every loop
                        {
                            Console.WriteLine("Connected and transfering data now");
                            CanSendMessage = false;
                        }
                        if (LoRa.IsOpen == false)
                        {
                            LoRa.Open();
                        }

                        ControllerState = GetOutput();
                        while (ControllerState.Length < 35) // normalizes data for esp32 buffer
                        {
                            ControllerState = ControllerState + ".";
                        }
                        if (ControllerState.Length > 35) // message is to long for esp32 buffer
                        {
                            Console.WriteLine("WARNING: Controller state to long. Edit max lenght in c# and arduino scripts.");
                           
                        }
                        
                            LoRa.WriteLine(ControllerState + "."); // tells esp32 when the message is finished

                        CheckForCommands(LoRa);
                    }
                    Console.WriteLine("transmit ended");
                    goto ControllerNotFoundGoto;


                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not connect to LoRa microcontroller");
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(2000);
                    goto LoRaNotRealPort;
                }

            }
            else
            {
                Console.WriteLine("Controller not found");
                Console.WriteLine("Retrying in 10 seconds");
                Thread.Sleep(10000);
                goto ControllerNotFoundGoto;
            }





            static int MapValue(int oldValue, int oldMin, int oldMax, int newMin, int newMax)
            {
                return (int)(((double)(oldValue - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin);
            }

            void CheckForCommands(SerialPort LoRa)
            {

                Task.Run(async () => // ne sinkronizirano pregledava za commande da ne usporava glavnu funkciju.
                {
                    string command = await Task.Run(() => Console.ReadLine());
                    if (command != null)
                    {
                        switch (command)
                        {
                            case "serial monitor": // todo: fix

                                Console.Clear();
                                while (true)
                                {
                                    
                                    Console.WriteLine("Welcome to the serial monitor");
                                    Console.WriteLine("SENDING: " + ControllerState);
                                    Console.WriteLine("READING: " + LoRa.ReadExisting());
                                    Console.SetCursorPosition(0, 0);

                                }




                                break;

                            case "end": // todo: fix
                                ForwardData = false;
                                break;

                            default:
                                Console.WriteLine("unrecognised command type it again");

                                break;


                        }

                    }
                });


            }

            string GetOutput() // used to get controller state as a string
            {
                State state;
                state = controller.GetState();
                string message = "";

                message = MapValue(state.Gamepad.LeftThumbX, 0, 32767, 0, 180) + "q"
                 + MapValue(state.Gamepad.LeftThumbY, 0, 32767, 0, 180) + "w"
                  + MapValue(state.Gamepad.RightThumbX, 0, 32767, 0, 180) + "e"
                   + MapValue(state.Gamepad.RightThumbY, 0, 32767, 0, 180) + "r"
                    + state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y) + "t" + state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) ; //inneficent but latency is already around 50ms


                return message;

            }

        }

    }

}
