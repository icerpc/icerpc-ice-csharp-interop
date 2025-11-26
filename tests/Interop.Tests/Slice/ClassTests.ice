// Copyright (c) ZeroC, Inc.

module Interop
{
    module Tests
    {
        module Slice
        {
            class Vehicle
            {
                string name;
            }

            sequence<Vehicle> VehicleSeq;

            class Bicycle extends Vehicle
            {
                bool hasBasket;
            }

            class Truck extends Vehicle
            {
                VehicleSeq cargo;
            }

            // Only in .ice file.
            class MountainBike extends Bicycle
            {
                string suspensionType;
            }
        }
    }
}
