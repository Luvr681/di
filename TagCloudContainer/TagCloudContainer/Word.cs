﻿namespace TagCloudContainer;

public class Word
{
    public string Value { get; set; }
    public int Weight { get; set; }
    public Point Position { get; set; }
    public Size Size { get; set; }
    public int RotateAngle { get; set; }
}