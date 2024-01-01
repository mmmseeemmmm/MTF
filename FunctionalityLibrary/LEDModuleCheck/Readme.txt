1.0.9.0:
- Methods for validation current, voltage and temperature edited according to requirements.

1.0.8.0:
- LMC libraries updated.
- Methods for validation current, voltage and temperature added.

1.0.7.0:
- FillValidationTable method created (filling minimum and maximum from configuration file to the validation table).

1.0.6.0:
- PerformLowCurrentTest and PerformCameraTrigger methods added.

1.0.5.0:
- GetMotorSensorValue and GetTestStatus methods added.

1.0.4.0:
- FindEdgeOfMotorSensor method added.

1.0.3.0:
- LMC libraries updated.
- MlcGeneralControl added.

1.0.2.0
- MTF event call reworked to runtim context call -> clean up in MTF, old events style not possible any more

1.0.1.29:
 - Removed redundant code.

1.0.1.28:
 - GetTestResults method is just a demo.

1.0.1.27:
 - LMC libraries updated.

1.0.1.26:
 - LMC libraries updated.

1.0.1.25:
 - LMC libraries updated.

1.0.1.24:
 - 'GetListOfConfigFiles' method added.

1.0.1.23:
- 'TTT_TTTestVariantsCount', 'TTT_TTTestVariantsForTestedModule' and 'TTT_GetTimeSinceLedsSwitchedOnInSeconds' methods added.

1.0.1.22:
- LMC libraries updated.

1.0.1.21:
- Communication module for DML renamed.

1.0.1.20:
- LMC libraries updated.
- Added IsLedModuleDanger, IsLedModuleWithSafetyClass, GetLedComModuleName and GetLedSafetyClass methods.

1.0.1.19:
- Added two LEDs (from MCB Extended Box) to ComboBox for LEDs choice.

1.0.1.18:
- LMC libraries updated.

1.0.1.17:
- TechnoTeam methods added.
- LMC libraries updated.

1.0.1.16:
- If error occurs, MTF shows error message with relevant text (bug in StartTesting()).

1.0.1.15:
- Added setting safety mode to StartTesting methods.

1.0.1.14:
- Unnecessary 'checkCommModuleParameters' parameter removed from StartTesting() method.

1.0.1.13:
- Command ExecuteLEDCommModule() - added support for communication module 'AudiAU516CBEVDml'.
- LMC libraries updated.
- Library ImageViewerControlLibrary added.

1.0.1.12:
- Fixed command GetLEDCommModuleCommands().
- LMC libraries updated.
- To the command ExecuteLEDCommModule() added combobox with available commands.

1.0.1.11:
- Added methods GetLEDStatus(), GetMotorStatus(), GetFanStatus() and GetHeatSinkStatus().

1.0.1.10:
- Added methods for getting GlobalTestStatus, ErrorCode and ErrorText.

1.0.1.9:
- LED Switch On - small bugfix.
- Execute command - now return result of operation (STATUS).

1.0.1.8:
- Removed methods 'StartCore()' and 'ClosedCore()'.

1.0.1.7:
- Added method 'DoNotTestModuleItems()'.

1.0.1.6:
- Possibility to set the relative path to LMC.

v1.0.1.5:
- Added access to method: CheckLaserSafetyAccepted, LEDSwitchONAndAcceptSafety, AllLEDsSwitchONAndAcceptSafety

v1.0.1.4:
- Part of update to the LMC Core version 1.2.6.49

v1.0.1.3:
- Finished access to methods: ExecuteLedCommunicationModuleCommand, GetCommunicationStatus, GetStatusOfCommunicationModuleOperationResult

v1.0.1.2:
- Added access to methods: ExecuteLedCommunicationModuleCommand, GetCommunicationStatus, GetStatusOfCommunicationModuleOperationResult

v1.0.1.1:
- Added access to methods: GetLedsCount, IsExtOutON, SwitchExtOut

v1.0.1.0:
- MTF controls MCB