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

            if (controller.IsConnected == true) // provjerava jeli controller spojen
            {
                Console.WriteLine("Controller found and connected");

            LoRaNotRealPort:
                Console.WriteLine("Chose port on which the LoRa module is connected to options are:");

                foreach (string s in SerialPort.GetPortNames()) // nalazi sve portove otvorene
                {
                    Console.WriteLine(s);
                }
                string Port = Console.ReadLine();
                foreach (string s in SerialPort.GetPortNames()) // provjeri jeli postoji taj port
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
        

                try // pokusa se spojiti sa ESP32
                {
                    LoRa.Open();
                    Console.WriteLine("Attemptng connection");
                    Console.WriteLine("Type end to end data transfer, or type serial to get a serial monitor");

                    while (ForwardData == true)
                    {
                        if (CanSendMessage == true) // samo jednom poslati 
                        {
                            Console.WriteLine("Connected and transfering data now");// ovu poruku
                            CanSendMessage = false;
                        }
                        if (LoRa.IsOpen == false)
                        {
                            LoRa.Open();
                        }

                        ControllerState = GetOutput();
                        while (ControllerState.Length < 35) // tu je  jer mora duljina poruke uvjek biti ista zbog arduina //
                        {
                            ControllerState = ControllerState + ".";
                        }
                        if (ControllerState.Length > 35) // poruka je preduga
                        {
                            Console.WriteLine("WARNING: Controller state to long. Edit max lenght in c# and arduino scripts.");
                           
                        }
                        
                            LoRa.WriteLine(ControllerState + ".");
                        
                           
                        
                            
                       
                       
                        
                 

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
                            case "serial monitor": // ne radi bas :(

                                Console.Clear();
                                while (true)
                                {
                                    
                                    Console.WriteLine("Welcome to the serial monitor");
                                    Console.WriteLine("SENDING: " + ControllerState);
                                    Console.WriteLine("READING: " + LoRa.ReadExisting());
                                    Console.SetCursorPosition(0, 0);

                                }




                                break;

                            case "end":
                                ForwardData = false;
                                break;

                            default:
                                Console.WriteLine("unrecognised command type it again");

                                break;


                        }

                    }
                });


            }

            string GetOutput() // uzme stanje controllera i vrati u stringu
            {
                State state;
                state = controller.GetState();
                string message = "";

                message = MapValue(state.Gamepad.LeftThumbX, 0, 32767, 0, 180) + "q" + MapValue(state.Gamepad.LeftThumbY, 0, 32767, 0, 180) + "w" + MapValue(state.Gamepad.RightThumbX, 0, 32767, 0, 180) + "e" + MapValue(state.Gamepad.RightThumbY, 0, 32767, 0, 180) + "r" + state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y) + "t" + state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) ;


                return message;

            }

         










        }


    }



}