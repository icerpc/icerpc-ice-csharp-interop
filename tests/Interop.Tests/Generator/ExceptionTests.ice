// Copyright (c) ZeroC, Inc.

module Interop::Tests::Generator
{
#ifdef __ICERPC__
    ["cs:identifier:EngineExceptionTwin"]
#endif
    exception EngineException
    {
        string errorCode;
    }

#ifdef __ICERPC__
    ["cs:identifier:CylinderExceptionTwin"]
#endif
    exception CylinderException extends EngineException
    {
        int cylinder;
    }

#ifdef __ICERPC__
    exception BatteryException extends EngineException
    {
        float voltage;
    }
#else
    // Only defined for Ice.
    exception FuelPumpException extends EngineException
    {
    }
#endif

#ifdef __ICERPC__
    ["cs:identifier:TirePressureExceptionTwin"]
#endif
    exception TirePressureException
    {
        string tireId;
    }

#ifndef __ICERPC__
    // Only defined for Ice.
    exception WiperException
    {
    }
#endif

    interface Engine
    {
        void start() throws EngineException;
    }
}
