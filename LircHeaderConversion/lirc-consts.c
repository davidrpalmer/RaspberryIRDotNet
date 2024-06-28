/*
* To construct this file goto  /usr/include/linux/lirc.h  on the Pi.
* Then run the file contens through regular expression:   #define ([a-zA-Z0-9_]+) +
* This will give you a list of all the constants.
* Then wrap each item in the 'printval' functon call and paste into this file.
*
* Then compile and run this program.
* The output can then be copied into a C# file to give access to the values.
* This file was last updated to a copy of lirc.h that was last modified 2010/07/13.
*/

#include <stdio.h>
#include <linux/lirc.h>

void printval(const char *name, __u32 value);

int main()
{
  printval("PULSE_BIT", PULSE_BIT);
  printval("PULSE_MASK", PULSE_MASK);
  printval("LIRC_MODE2_SPACE", LIRC_MODE2_SPACE);
  printval("LIRC_MODE2_PULSE", LIRC_MODE2_PULSE);
  printval("LIRC_MODE2_FREQUENCY", LIRC_MODE2_FREQUENCY);
  printval("LIRC_MODE2_TIMEOUT", LIRC_MODE2_TIMEOUT);
  printval("LIRC_MODE2_OVERFLOW", LIRC_MODE2_OVERFLOW);
  printval("LIRC_VALUE_MASK", LIRC_VALUE_MASK);
  printval("LIRC_MODE2_MASK", LIRC_MODE2_MASK);
  printval("LIRC_MODE_RAW", LIRC_MODE_RAW);
  printval("LIRC_MODE_PULSE", LIRC_MODE_PULSE);
  printval("LIRC_MODE_MODE2", LIRC_MODE_MODE2);
  printval("LIRC_MODE_SCANCODE", LIRC_MODE_SCANCODE);
  printval("LIRC_MODE_LIRCCODE", LIRC_MODE_LIRCCODE);
  printval("LIRC_CAN_SEND_RAW", LIRC_CAN_SEND_RAW);
  printval("LIRC_CAN_SEND_PULSE", LIRC_CAN_SEND_PULSE);
  printval("LIRC_CAN_SEND_MODE2", LIRC_CAN_SEND_MODE2);
  printval("LIRC_CAN_SEND_LIRCCODE", LIRC_CAN_SEND_LIRCCODE);
  printval("LIRC_CAN_SEND_MASK", LIRC_CAN_SEND_MASK);
  printval("LIRC_CAN_SET_SEND_CARRIER", LIRC_CAN_SET_SEND_CARRIER);
  printval("LIRC_CAN_SET_SEND_DUTY_CYCLE", LIRC_CAN_SET_SEND_DUTY_CYCLE);
  printval("LIRC_CAN_SET_TRANSMITTER_MASK", LIRC_CAN_SET_TRANSMITTER_MASK);
  printval("LIRC_CAN_REC_RAW", LIRC_CAN_REC_RAW);
  printval("LIRC_CAN_REC_PULSE", LIRC_CAN_REC_PULSE);
  printval("LIRC_CAN_REC_MODE2", LIRC_CAN_REC_MODE2);
  printval("LIRC_CAN_REC_SCANCODE", LIRC_CAN_REC_SCANCODE);
  printval("LIRC_CAN_REC_LIRCCODE", LIRC_CAN_REC_LIRCCODE);
  printval("LIRC_CAN_REC_MASK", LIRC_CAN_REC_MASK);
  printval("LIRC_CAN_SET_REC_CARRIER", LIRC_CAN_SET_REC_CARRIER);
  printval("LIRC_CAN_SET_REC_CARRIER_RANGE", LIRC_CAN_SET_REC_CARRIER_RANGE);
  printval("LIRC_CAN_GET_REC_RESOLUTION", LIRC_CAN_GET_REC_RESOLUTION);
  printval("LIRC_CAN_SET_REC_TIMEOUT", LIRC_CAN_SET_REC_TIMEOUT);
  printval("LIRC_CAN_MEASURE_CARRIER", LIRC_CAN_MEASURE_CARRIER);
  printval("LIRC_CAN_USE_WIDEBAND_RECEIVER", LIRC_CAN_USE_WIDEBAND_RECEIVER);
  printval("LIRC_GET_FEATURES", LIRC_GET_FEATURES);
  printval("LIRC_GET_SEND_MODE", LIRC_GET_SEND_MODE);
  printval("LIRC_GET_REC_MODE", LIRC_GET_REC_MODE);
  printval("LIRC_GET_REC_RESOLUTION", LIRC_GET_REC_RESOLUTION);
  printval("LIRC_GET_MIN_TIMEOUT", LIRC_GET_MIN_TIMEOUT);
  printval("LIRC_GET_MAX_TIMEOUT", LIRC_GET_MAX_TIMEOUT);
  printval("LIRC_GET_LENGTH", LIRC_GET_LENGTH);
  printval("LIRC_SET_SEND_MODE", LIRC_SET_SEND_MODE);
  printval("LIRC_SET_REC_MODE", LIRC_SET_REC_MODE);
  printval("LIRC_SET_SEND_CARRIER", LIRC_SET_SEND_CARRIER);
  printval("LIRC_SET_REC_CARRIER", LIRC_SET_REC_CARRIER);
  printval("LIRC_SET_SEND_DUTY_CYCLE", LIRC_SET_SEND_DUTY_CYCLE);
  printval("LIRC_SET_TRANSMITTER_MASK", LIRC_SET_TRANSMITTER_MASK);
  printval("LIRC_SET_REC_TIMEOUT", LIRC_SET_REC_TIMEOUT);
  printval("LIRC_SET_REC_TIMEOUT_REPORTS", LIRC_SET_REC_TIMEOUT_REPORTS);
  printval("LIRC_SET_MEASURE_CARRIER_MODE", LIRC_SET_MEASURE_CARRIER_MODE);
  printval("LIRC_SET_REC_CARRIER_RANGE", LIRC_SET_REC_CARRIER_RANGE);
  printval("LIRC_SET_WIDEBAND_RECEIVER", LIRC_SET_WIDEBAND_RECEIVER);
  printval("LIRC_GET_REC_TIMEOUT", LIRC_GET_REC_TIMEOUT);
  printval("LIRC_SCANCODE_FLAG_TOGGLE", LIRC_SCANCODE_FLAG_TOGGLE);
  printval("LIRC_SCANCODE_FLAG_REPEAT", LIRC_SCANCODE_FLAG_REPEAT);

  return 0;
}


void printval(const char *name, __u32 value)
{
  //printf("public const uint %s = %lu;\n", name, value); // Print C# code in decimal.
  printf("public const uint %s = 0x%08x;\n", name, value); // Print C# code in hex.
}
