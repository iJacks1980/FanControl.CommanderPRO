using System;

namespace FanControl.CommanderPro
{
    internal class Constants
    {
        public const String ERROR_LOG_FILE_NAME = "CommanderPRO.err.log";

#if DEBUG
        public const String TRACE_LOG_FILE_NAME = "CommanderPRO.trc.log";
#else
        public const String TRACE_LOG_FILE_NAME = "";
#endif

        public const Int32 VENDOR_ID = 0x1b1c;
        public const Int32 PRODUCT_ID = 0x0c10;

        public const Int32 COMMAND_SIZE = 64;
        public const Int32 RESPONSE_SIZE = 16;

        public const Int32 READ_STATUS = 0x01;
        public const Int32 READ_FIRMWARE_VERSION = 0x02;
        public const Int32 READ_DEVICE_ID = 0x03;
        public const Int32 WRITE_DEVICE_ID = 0x04;
        public const Int32 START_FIRMWARE_UPDATE = 0x05;
        public const Int32 READ_BOOTLOADER_VERSION = 0x06;
        public const Int32 WRITE_TEST_FLAG = 0x07;
        public const Int32 READ_TEMPERATURE_MASK = 0x10;
        public const Int32 READ_TEMPERATURE_VALUE = 0x11;
        public const Int32 READ_VOLTAGE_VALUE = 0x12;
        public const Int32 READ_FAN_MASK = 0x20;
        public const Int32 READ_FAN_SPEED = 0x21;
        public const Int32 READ_FAN_POWER = 0x22;
        public const Int32 WRITE_FAN_POWER = 0x23;
        public const Int32 WRITE_FAN_SPEED = 0x24;
        public const Int32 WRITE_FAN_CURVE = 0x25;
        public const Int32 WRITE_FAN_EXTERNAL_TEMP = 0x26;
        public const Int32 WRITE_FAN_FORCE_THREE_PIN_MODE = 0x27;
        public const Int32 WRITE_FAN_DETECTION_TYPE = 0x28;
        public const Int32 READ_FAN_DETECTION_TYPE = 0x29;
        public const Int32 READ_LED_STRIP_MASK = 0x30;
        public const Int32 WRITE_LED_RGB_VALUE = 0x31;
        public const Int32 WRITE_LED_COLOR_VALUES = 0x32;
        public const Int32 WRITE_LED_TRIGGER = 0x33;
        public const Int32 WRITE_LED_CLEAR = 0x34;
        public const Int32 WRITE_LED_GROUP_SET = 0x35;
        public const Int32 WRITE_LED_EXTERNAL_TEMP = 0x36;
        public const Int32 WRITE_LED_GROUPS_CLEAR = 0x37;
        public const Int32 WRITE_LED_MODE = 0x38;
        public const Int32 WRITE_LED_BRIGHTNESS = 0x39;
        public const Int32 WRITE_LED_COUNT = 0x3A;
        public const Int32 WRITE_LED_PORT_TYPE = 0x3B;
        public const Int32 PROTOCOL_RESPONSE_OK = 0x00;
        public const Int32 PROTOCOL_RESPONSE_ERROR = 0x01;

        public const Int32 Fan_Auto_Mode = 0;
        public const Int32 Fan_3pin_Mode = 1;
        public const Int32 Fan_4pin_Mode = 2;
    }
}