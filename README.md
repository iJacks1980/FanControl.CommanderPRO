## Commander PRO Plugin

<p align=center>Commander PRO Plugin for Rem0o's Fan Control</span>

<br />
<br />

## Main features

## Commander PRO

* Reads fan speed as both a % of power and raw RPM
* Updates fan speed as a % of power
* Read temperature probes

## Commander CORE

* Reads fan speed as raw RPM
* Read temperature probes

## Commander CORE XT

* Reads fan speed as raw RPM
* Read temperature probes

## Todo

* Support Commander CORE and Commander CORE XT - in progress, currently read only
* Support multiple Commander devices (IE, 2 or more Commander PRO's) - in progress

## Installation

* Download the [plugin](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/download/v1.0.4/FanControl.CommanderPro.zip)
* Unzip the file "FanControl.CommanderPro.dll" and place into the Plugins folder of Fan Control.
	* Windows might block the file by default, simply right click and go to properties  then click the "Unblock this file" checkbox and press OK.
* Restart Fan Control.

## Known Issues

* If Corsair iCUE (or any other software that reads from Commander devices such as HWiNFO64) is installed and running you will get random data at best, nothing at worst.  Sadly the way that the hardware works means things have to be one or the other :(
	* There is a workaround for HWiNFO64, go into the main setting for HWiNFO64 under the safety tab and uncheck the "CorsairLink and Asetek Support".  This will prevent HWiNFO64 from trying to read from Corair Commander devices (fan, pump and temperature data)

## Version history

## [v1.0.0](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/tag/v1.0.0)

Initial release supporting Commander PRO fans only.

## [v1.0.1](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/tag/v1.0.1a)

Beta release with added support for reading temperature values from the probes connected to the Commander PRO.

## [v1.0.2](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/tag/v1.0.2a)

Beta release with partial support for Commander CORE device (may also work with Commander CORE XT).

## [v1.0.3](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/tag/v1.0.3a)

Beta release with support for Commander PRO, CORE and CORE XT.

## [v1.0.4](https://github.com/iJacks1980/FanControl.CommanderPRO/releases/tag/v1.0.4)

Updated release supporting FanControl v113.

Supports:

Command PRO - Read and set fan speed. Read temperature probes.
Commander CORE - Read fan speed. Read temperature probes.
Commander CORE XT - Read fan speed. Read temperature probes.