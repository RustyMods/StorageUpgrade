using System;

namespace StorageUpgrade;

public enum JoyButton
{
    [InternalName("JoyButtonA")] A,
    [InternalName("JoyButtonB")] B,
    [InternalName("JoyButtonX")] X,
    [InternalName("JoyButtonY")] Y,
    [InternalName("JoyBack")] Select,
    [InternalName("JoyStart")] Start,
    [InternalName("JoyLStick")] LStick,
    [InternalName("JoyRStick")] RStick,
    [InternalName("JoyLBumper")] LBumper,
    [InternalName("JoyRBumper")] RBumper,
    [InternalName("JoyLTrigger")]  LTrigger,
    [InternalName("JoyRTrigger")] RTrigger,
}

public class InternalName(string name) : Attribute
{
    public readonly string name = name;
}