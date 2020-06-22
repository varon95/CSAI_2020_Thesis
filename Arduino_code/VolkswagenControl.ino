//the potentiometer controll was based on this tutorial: https://www.arduino.cc/en/Tutorial/DigitalPotControl?from=Tutorial.SPIDigitalPot

//digipot
#include <SPI.h>
byte addressSpeed = B00010000;
byte addressTurn= B00000000;
int CS= 10;

//data
String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
String commandString = "";
String newInterval = "";

void setup() {
//digipot
pinMode (CS, OUTPUT);
SPI.begin();
//data
Serial.begin(9600);
}

void loop() { 

  if(stringComplete)
  {
    stringComplete = false;
    getCommand();
  
    if(newInterval.length()>1){        
        if(commandString.equals("SPED")){digitalPotWriteSpeed(newInterval.toInt());}
        if(commandString.equals("TURN")){digitalPotWriteTurn(newInterval.toInt());}
        }
    newInterval = ""; 
    inputString = "";
  }
}

void getCommand()
{
  if(inputString.length()>2)
  {
     commandString = inputString.substring(1,5);
  }
}

void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;

  
    if(isDigit(inChar)){
        newInterval += inChar;}
    
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}

int digitalPotWriteSpeed(int valueSpeed)
{
digitalWrite(CS, LOW);
SPI.transfer(addressSpeed);
SPI.transfer(valueSpeed);
digitalWrite(CS, HIGH);
}

int digitalPotWriteTurn(int valueTurn)
{
digitalWrite(CS, LOW);
SPI.transfer(addressTurn);
SPI.transfer(valueTurn);
digitalWrite(CS, HIGH);
}
