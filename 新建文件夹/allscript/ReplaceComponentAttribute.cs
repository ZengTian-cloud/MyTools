using System;

public class ReplaceComponentAttribute : Attribute
{
    public Type ReplaceType { get; private set; }

    public ReplaceComponentAttribute(Type replaceType)
    {
        ReplaceType = replaceType;
    }
}

