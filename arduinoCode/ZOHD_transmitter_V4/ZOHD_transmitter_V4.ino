/* Heltec Automation Ping Pong communication test example
 *
 * Function:
 * 
 * Description:
 * 1. Only hardware layer communicate, no LoRaWAN protocol support;
 * 2. Download the same code into two  devices, then they will begin Ping Pong test each other;
 * 3. This example is for CubeCell hardware basic test.
 *
 * HelTec AutoMation, Chengdu, China
 * �ɶ��������Զ����Ƽ����޹�˾
 * www.heltec.org
 *
 * */

#include "LoRaWan_APP.h"
#include "Arduino.h"



#define RF_FREQUENCY                                433000000 // Hz

#define TX_OUTPUT_POWER                             5        // dBm

#define LORA_BANDWIDTH                              2         // [0: 125 kHz,
                                                              //  1: 250 kHz,
                                                              //  2: 500 kHz,
                                                              //  3: Reserved]
#define LORA_SPREADING_FACTOR                       7         // [SF7..SF12]
#define LORA_CODINGRATE                             4         // [1: 4/5,
                                                              //  2: 4/6,
                                                              //  3: 4/7,
                                                              //  4: 4/8]
#define LORA_PREAMBLE_LENGTH                        8         // Same for Tx and Rx
#define LORA_SYMBOL_TIMEOUT                         0         // Symbols
#define LORA_FIX_LENGTH_PAYLOAD_ON                  false
#define LORA_IQ_INVERSION_ON                        false


#define RX_TIMEOUT_VALUE                            100
#define BUFFER_SIZE                                 30 // Define the payload size here

char txpacket[BUFFER_SIZE];
char rxpacket[BUFFER_SIZE];

static RadioEvents_t RadioEvents;
void OnTxDone( void );
void OnTxTimeout( void );
void OnRxDone( uint8_t *payload, uint16_t size, int16_t rssi, int8_t snr );

typedef enum
{
    LOWPOWER,
    STATE_RX,
    STATE_TX
}States_t;

int16_t txNumber;
States_t state;
bool sleepMode = false;
int16_t Rssi,rxSize;


void setup() {
    Serial.begin(115200);
    Mcu.begin();
    txNumber=0;
    Rssi=0;

    RadioEvents.TxDone = OnTxDone;
    RadioEvents.TxTimeout = OnTxTimeout;
    RadioEvents.RxDone = OnRxDone;

    Radio.Init( &RadioEvents );
    Radio.SetChannel( RF_FREQUENCY );
    Radio.SetTxConfig( MODEM_LORA, TX_OUTPUT_POWER, 0, LORA_BANDWIDTH,
                                   LORA_SPREADING_FACTOR, LORA_CODINGRATE,
                                   LORA_PREAMBLE_LENGTH, LORA_FIX_LENGTH_PAYLOAD_ON,
                                   true, 0, 0, LORA_IQ_INVERSION_ON, 3000 );

    Radio.SetRxConfig( MODEM_LORA, LORA_BANDWIDTH, LORA_SPREADING_FACTOR,
                                   LORA_CODINGRATE, 0, LORA_PREAMBLE_LENGTH,
                                   LORA_SYMBOL_TIMEOUT, LORA_FIX_LENGTH_PAYLOAD_ON,
                                   0, true, 0, 0, LORA_IQ_INVERSION_ON, true );
    state=STATE_TX;
}



void loop()
{
  switch(state)
  {
    case STATE_TX:
     {
        txNumber++;
      Serial.flush();
      for(int i=-1; i<30; i++)
      {
        txpacket[i] = Serial.read();
      }
      Serial.printf("read from console: %s",txpacket);
      Serial.println("");
      Radio.Send( (uint8_t *)txpacket, strlen(txpacket) );
     
      memset(txpacket, 0, 30);
      state=LOWPOWER;
      Serial.flush();

     }     
      break;
    case STATE_RX:
    {
       
      Radio.Rx( 0 );
      state=LOWPOWER;
    }     
      break;
    case LOWPOWER:
    {
      Radio.IrqProcess( );
    }     
      break;
    default:
    {
      
    }
      break;
  }
}

void OnTxDone( void )
{
 
  state=STATE_RX;
}

void OnTxTimeout( void )
{
    Radio.Sleep( );
   
    state=STATE_TX;
}

void OnRxDone( uint8_t *payload, uint16_t size, int16_t rssi, int8_t snr )
{
    Rssi=rssi;
    rxSize=size;
    memcpy(rxpacket, payload, size );
    rxpacket[size]='\0';
    Radio.Sleep( );

    

    state=STATE_TX;
}
