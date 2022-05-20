using System;

namespace FanControl.CommanderCore
{
    internal class Constants
    {
        public const String ERROR_LOG_FILE_NAME = "CommanderCORE.err.log";

#if DEBUG
        public const String TRACE_LOG_FILE_NAME = "CommanderCORE.trc.log";
#else
        public const String TRACE_LOG_FILE_NAME = "";
#endif

        public const Int32 VENDOR_ID = 0x1b1c;
        public const Int32 PRODUCT_ID = 0x0c1c;

        public const Int32 COMMAND_SIZE = 97;
        public const Int32 RESPONSE_SIZE = 97;

        public static Byte[] COMMAND_WAKE = { 0x01, 0x03, 0x00, 0x02 };
        public static Byte[] COMMAND_SLEEP = { 0x01, 0x03, 0x00, 0x01 };
        public static Byte[] COMMAND_RESET = { 0x05, 0x01, 0x00 };
        public static Byte[] COMMAND_SET_MODE = { 0x0d, 0x00 };
        public static Byte[] COMMAND_READ = { 0x08, 0x00 };
        public static Byte[] COMMAND_WRITE = { 0x06, 0x00 };

        public static Byte[] READ_FIRMWARE_VERSION = { 0x02, 0x13 };

        public static Byte[] MODE_CONNECTED = { 0x1a };
        public static Byte[] MODE_GET_SPEEDS = { 0x17 };
        public static Byte[] MODE_GET_TEMPS = { 0x21 };
        public static Byte[] MODE_HW_SPEED_MODE = { 0x60, 0x6d };
        public static Byte[] MODE_HW_FIXED_PERCENT = { 0x61, 0x6d };

        public static Byte[] DATA_TYPE_FIRMWARE = { 0x02, 0x00 };
        public static Byte[] DATA_TYPE_HW_CONNECTED = { 0x08, 0x03 };
        public static Byte[] DATA_TYPE_SW_CONNECTED = { 0x09, 0x00 };
        public static Byte[] DATA_TYPE_SPEEDS = { 0x06, 0x00 };
        public static Byte[] DATA_TYPE_TEMPS = { 0x10, 0x00 };
        public static Byte[] DATA_TYPE_HW_SPEED_MODE = { 0x03, 0x00 };
        public static Byte[] DATA_TYPE_HW_FIXED_PERCENT = { 0x04, 0x00 };

        //public static Object _MODE_LED_COUNT = ValueTuple.Create(0x20);

        //public static Object _DATA_TYPE_LED_COUNT = (0x0f, 0x00);
    }
}