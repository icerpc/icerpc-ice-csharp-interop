// Copyright (c) ZeroC, Inc.

module Interop::Tests::Slice
{
#ifdef __ICERPC__
    ["cs:identifier:VehicleTwin"]
#endif
    class Vehicle
    {
        string name;
    }

    sequence<Vehicle> VehicleSeq;

#ifdef __ICERPC__
    ["cs:identifier:BicycleTwin"]
#endif
    class Bicycle extends Vehicle
    {
        bool hasBasket;
    }

#ifdef __ICERPC__
    ["cs:identifier:TruckTwin"]
#endif
    class Truck extends Vehicle
    {
        VehicleSeq cargo;
    }

#ifdef __ICERPC__
    class RacingBicycle extends Bicycle
    {
        double maxSpeed;
    }
#else
    // Only for Ice
    class MountainBike extends Bicycle
    {
        string suspensionType;
    }
#endif
}
