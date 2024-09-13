// Copyright (c) ZeroC, Inc.

module Interop::Tests::Slice
{
    exception EngineException
    {
        string errorCode;
    }

    exception CylinderException extends EngineException
    {
        int cylinder;
    }

    // Only defined in .ice file.
    exception FuelPumpException extends EngineException
    {
    }

    exception TirePressureException
    {
        string tireId;
    }

    // Only defined in .ice file.
    exception WiperException
    {
    }

    interface Engine
    {
        void start() throws EngineException;
    }
}
