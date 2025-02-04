﻿namespace nio2so.Formats.TSOData
{
    public enum TSODataFieldClassification : byte
    {
        SingleField,
        Map,
        TypedList
    }

    public enum TSOFieldMaskValues : byte
    {
        None,
        Keep,
        Remove
    }

    public enum TSODataStringCategories
    {
        None,
        Field,
        FirstLevel,
        SecondLevel,
        Derived,
        Unspecified
    }
}
