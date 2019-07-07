/***************************************************************************
  This is a library for the AMG88xx GridEYE 8x8 IR camera

  This sketch tries to read the pixels from the sensor

  Designed specifically to work with the Adafruit AMG88 breakout
  ----> http://www.adafruit.com/products/3538

  These sensors use I2C to communicate. The device's I2C address is 0x69

  Adafruit invests time and resources providing this open source code,
  please support Adafruit andopen-source hardware by purchasing products
  from Adafruit!

  Written by Dean Miller for Adafruit Industries.
  BSD license, all text above must be included in any redistribution
 ***************************************************************************/

#include <Wire.h>
#include <Adafruit_AMG88xx.h>

Adafruit_AMG88xx amg;

float pixels[AMG88xx_PIXEL_ARRAY_SIZE];
String _tampung;
bool safe_ = false;
//char charBuf[512];

void setup() {
    Serial.begin(38400);
    //Serial.println(F("AMG88xx pixels"));

    bool status;
    
    // default settings
    status = amg.begin();
    if (!status) {
        //Serial.println("Could not find a valid AMG88xx sensor, check wiring!");
        while (1);
    }
    
    //Serial.println("-- Pixels Test --");

    //Serial.println();
   
    delay(100); // let sensor boot up
      
    //_tampung = "";
}


void loop() { 
    //read all the pixels
    
    _tampung = "";
    //charBuf[0] = (char)0;
    
    //Serial.flush();
    amg.readPixels(pixels);
        
    //Serial.print("[");
    _tampung = '[';
        
    for(int i=1; i<=AMG88xx_PIXEL_ARRAY_SIZE; i++)
    {
          //Serial.print(pixels[i-1]);
          //Serial.print(",");
          _tampung += pixels[i-1];
          _tampung += ',';
          
          if( i%8 == 0 )
            {
              //Serial.println();
               //_tampung += '\n';
            }
    }
        //Serial.print("]");
        //Serial.println("");
       _tampung += "]";
       //if (safe_ == false)
       //{
        //_tampung.toCharArray(charBuf, 512);
        Serial.print(_tampung);
        //Serial.flush();
        //safe_ = true;
       //}
        //
        //_tampung = "";
        //delay a second
        //delay(1000); 
    //Serial.end();
    //Serial.begin(19200);
}