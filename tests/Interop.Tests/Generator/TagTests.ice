// Copyright (c) ZeroC, Inc.

module Interop::Tests::Generator
{
#ifdef __ICERPC__
    ["cs:identifier:OneByteTwin"]
#endif
    struct OneByte
    {
        byte value;
    }

    sequence<OneByte> OneByteSeq;

#ifdef __ICERPC__
    ["cs:identifier:PointTwin"]
#endif
    struct Point // fixed size
    {
        int x;
        int y;
    }
    sequence<Point> PointSeq;

#ifdef __ICERPC__
    ["cs:identifier:FruitTwin"]
#endif
    enum Fruit { Apple, Pineapple, Orange, Raspberry }

#ifdef __ICERPC__
    ["cs:identifier:PersonTwin"]
#endif
    struct Person // variable size
    {
        string name;
        Fruit favoriteFruit;
    }

    sequence<string> StringSeq;

    interface TagTest
    {
        void opFixed(
            optional(1) bool f1,
            optional(2) short f2,
            optional(4) int f4,
            optional(8) double f8);

        void opEnum(optional(1) Fruit fruit); // tag type = Size

        void opProxy(optional(1) Object* proxy); // tag type = FSize

        void opStruct(
            optional(1) Point point,      // tag type = VSize
            optional(5) Person person);   // tag type = FSize

        void opSequence(
            optional(0) string str,              // tag type = VSize (+ optimization)
            optional(1) OneByteSeq oneByteSeq,   // tag type = VSize (+ optimization)
            optional(5) PointSeq pointSeq,       // tag type = VSize
            optional(10) StringSeq stringSeq);   // tag type = FSize
    }
}
