namespace ClimaChamberDriver
{
    static class ParsingTestHelper
    {
        public static string GetOldClimaString()
        {
            return @"$01?
ASCII-2 PROTOCOL CONFIGURATION
Configuration type: DYNAMIC

Example of an ASCII E-String:
$01E CV01 CV02 SV01 SV02 SV03 DO00 DO01 DO02 DO03 DO04 DO05 DO06 DO07 DO08 DO09
DO10 DO11 DO12 DO13 DO14 DO15 DO16 DO17 DO18 DO19 DO20 DO21 DO22 DO23 DO24 DO25
<CR>

Description:
CV01  value min:  -72.0   value max:  182.0   Temperature
CV02  value min:    0.0   value max:   98.0   Humidity
SV01  value min:   25.0   value max:  100.0   Option
SV02  value min:    0.0   value max:   30.0   Option
SV03  value min:    2.0   value max:   30.0   Option
DO00  unused
DO01  Start
DO02  Custom Out1
DO03  Custom Out2
DO04  Custom Out3
DO05  Custom Out4
DO06  Cond.protect
DO07  Clarificat.
DO08  Option
DO09  Option
DO10  Option
DO11  Option
DO12  Option
DO13  Option
DO14  Option
DO15  Option
DO16  No GreenMode
DO17  Option
DO18  60068-2-30
DO19  60068-2-38
DO20  Option
DO21  Option
DO22  Custom Out5
DO23  Custom Out6
DO24  Custom Out7
DO25  Custom Out8
--------------------------------------------------------------------------------

Example of  an ASCII I-String:
$01I<CR>
CV01 CV01 CV02 CV02 SV01 SV02 SV03 MV01 MV02 MV03 MV04 DO00 DO01 DO02 DO03 DO04
DO05 DO06 DO07 DO08 DO09 DO10 DO11 DO12 DO13 DO14 DO15 DO16 DO17 DO18 DO19 DO20
DO21 DO22 DO23 DO24 DO25 DO26 DO27 DO28 DO29 DO30 DO31 <CR>

Description:
CV01  nominal value Temperature
CV01  actual value  Temperature
CV02  nominal value Humidity
CV02  actual value  Humidity
SV01  set value     Option
SV02  set value     Option
SV03  set value     Option
MV01 actual value T.sensor 1
MV02 actual value T.sensor 2
MV03 actual value T.sensor 3
MV04 actual value T.sensor 4
DO00  unused
DO01  Start
DO02  Custom Out1
DO03  Custom Out2
DO04  Custom Out3
DO05  Custom Out4
DO06  Cond.protect
DO07  Clarificat.
DO08  Option
DO09  Option
DO10  Option
DO11  Option
DO12  Option
DO13  Option
DO14  Option
DO15  Option
DO16  No GreenMode
DO17  Option
DO18  60068-2-30
DO19  60068-2-38
DO20  Option
DO21  Option
DO22  Custom Out5
DO23  Custom Out6
DO24  Custom Out7
DO25  Custom Out8
DO26  unused
DO27  unused
DO28  unused
DO29  unused
DO30  unused
DO31  unused
--------------------------------------------------------------------------------

Configured Messages:
none
";
        }

        public static string GetNewClimaString()
        {
            return @"$01?
ASCII-2 PROTOCOL CONFIGURATION
Configuration type: DYNAMIC

Check Extern mode: enabled
Show target setpoint: disabled
Example of an ASCII E-String:
$01E CV01 CV02 SV01 SV02 SV03 DO00DO01DO02DO03DO04DO05DO06DO07DO08DO09DO10DO11DO
12DO13DO14DO15DO16DO17DO18DO19DO20DO21DO22DO23DO24DO25<CR>

Description:
CV01  value min:  -72.0   value max:  182.0   Temperature
CV02  value min:    0.0   value max:   98.0   Humidity
SV01  value min:   25.0   value max:  100.0   Option
SV02  value min:    0.0   value max:   30.0   Option
SV03  value min:    2.0   value max:   30.0   Option
DO00  unused
DO01  Start
DO02  Custom Out1
DO03  Custom Out2
DO04  Custom Out3
DO05  Custom Out4
DO06  Cond.protect
DO07  Clarificat.
DO08  Option
DO09  Option
DO10  Option
DO11  Option
DO12  Option
DO13  Option
DO14  Option
DO15  Option
DO16  No GreenMode
DO17  Option
DO18  60068-2-30
DO19  60068-2-38
DO20  Option
DO21  Option
DO22  Custom Out5
DO23  Custom Out6
DO24  Custom Out7
DO25  Custom Out8
--------------------------------------------------------------------------------

Example of  an ASCII I-String:
$01I<CR>
CV01 CV01 CV02 CV02 SV01 SV02 SV03 MV01 MV02 MV03 MV04 DO00DO01DO02DO03DO04DO05D
O06DO07DO08DO09DO10DO11DO12DO13DO14DO15DO16DO17DO18DO19DO20DO21DO22DO23DO24DO25D
O26DO27DO28DO29DO30DO31<CR>

Description:
CV01  nominal value Temperature
CV01  actual value  Temperature
CV02  nominal value Humidity
CV02  actual value  Humidity
SV01  set value     Option
SV02  set value     Option
SV03  set value     Option
MV01 actual value T.sensor 1
MV02 actual value T.sensor 2
MV03 actual value T.sensor 3
MV04 actual value T.sensor 4
DO00  unused
DO01  Start
DO02  Custom Out1
DO03  Custom Out2
DO04  Custom Out3
DO05  Custom Out4
DO06  Cond.protect
DO07  Clarificat.
DO08  Option
DO09  Option
DO10  Option
DO11  Option
DO12  Option
DO13  Option
DO14  Option
DO15  Option
DO16  No GreenMode
DO17  Option
DO18  60068-2-30
DO19  60068-2-38
DO20  Option
DO21  Option
DO22  Custom Out5
DO23  Custom Out6
DO24  Custom Out7
DO25  Custom Out8
DO26  unused
DO27  unused
DO28  unused
DO29  unused
DO30  unused
DO31  unused
--------------------------------------------------------------------------------

Configured Messages:
none
";
        }
    }
}
