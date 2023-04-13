// Copyright (c) ZeroC, Inc.

/// Slice1 enumerations are encoded as a Slice1 size so we test two enumeration types: one whose maximum value is
/// inferior to 255 and one whose maximum value is superior to 254.

module Interop
{
    module Tests
    {
        module Slice
        {
            enum OneByteEnum
            {
                value1,
                value2 = 200
            }

            enum FiveBytesEnum
            {
                value1,
                value2 = 400,
                value3 = 100000
            }
        }
    }
}
