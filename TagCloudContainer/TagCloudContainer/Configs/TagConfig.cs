﻿using System.Numerics;

namespace TagCloudContainer;

public class TagConfig : ITagConfig
{
    private readonly Point _center;
    private readonly Size _standartSize;
    private IMainFormConfig _mainFormConfig;
    private SortedList<float, Point> _nearestToTheCenterPoints;
    private List<Rectangle> _putRectangles;

    public TagConfig(Point center, Size standartSize, IMainFormConfig mainFormConfig)
    {
        IsValidArguments(center, standartSize);

        _mainFormConfig = mainFormConfig;
        _center = center;
        _standartSize = standartSize;
        _nearestToTheCenterPoints = new SortedList<float, Point>();
        _putRectangles = new List<Rectangle>();
        
        AddFreePoint(center);
    }
    
    public Word ConfigureWordTag(Word word, IMainFormConfig mainFormConfig)
    {
        word.Size = TextRenderer
            .MeasureText(word.Value, new Font(_mainFormConfig.FontFamily, word.Weight * _standartSize.Width));
        
        if (word.Size.IsEmpty || word.Size == null)
            throw new ArgumentException("Word size can't be empty or null");
        if (word.Size.Width < 0 || word.Size.Height < 0) 
            throw new ArgumentException("Word size can't have negative side value");

        _mainFormConfig = mainFormConfig;
        var nearestFreePoint = GetNearestInsertionPoint(word.Size);
        var rectangle = new Rectangle(nearestFreePoint, word.Size);
        
        _putRectangles.Add(rectangle);
        AddVerticesToFreePoints(rectangle);

        word.Position = new Point(rectangle.X, rectangle.Y);
        word.RotateAngle = rectangle.Size.Width < rectangle.Size.Height ? 90 : 0;
        
        return word;
    }
    
    private Point GetNearestInsertionPoint(Size rectangleSize)
    {
        foreach (var nearestPoint in _nearestToTheCenterPoints.Values)
        {
            foreach (var rotateDirection in Enum.GetValues(typeof(RotateDirections)))
            {
                var insertionPoint = GetInsertionPoint(rectangleSize, nearestPoint, rotateDirection);
                var rectangle = new Rectangle(insertionPoint, rectangleSize);

                if (!DoesItIntersect(rectangle)) return insertionPoint;
            }
        }
        
        throw new Exception("Can't put this rectangle");
    }

    private bool DoesItIntersect(Rectangle rectangle)
    {
        foreach (var putRectangle in _putRectangles)
            if (putRectangle.IntersectsWith(rectangle)) return true;
        return false;
    }

    private Point GetInsertionPoint(Size rectangleSize, Point point, object rotateDirection)
    { 
        var insertionPoint = new Point(point.X, point.Y);
        if (rotateDirection.Equals(RotateDirections.bottom))
            insertionPoint.X -= rectangleSize.Width;
        if (rotateDirection.Equals(RotateDirections.left))
            insertionPoint.Y -= rectangleSize.Height;

        return insertionPoint;
    }

    private void AddVerticesToFreePoints(Rectangle rectangle)
    {
        for (var i = 0; i <= rectangle.Width; i += rectangle.Width)
        for (var j = 0; j <= rectangle.Height; j += rectangle.Height) 
            AddFreePoint(new Point(rectangle.X + i, rectangle.Y + j));
    }
    
    private void AddFreePoint(Point point)
    {
        var distanceFromCenter = CountDistanceFromCenter(point);
        if (!_nearestToTheCenterPoints.ContainsKey(distanceFromCenter))
            _nearestToTheCenterPoints.Add(distanceFromCenter, point);
    }
    
    private float CountDistanceFromCenter(Point point)
    {
        var distanceFromCenter = new Vector2(point.X - _center.X, point.Y - _center.Y);
        return distanceFromCenter.Length();
    }

    private void IsValidArguments(Point center, Size standartSize)
    {
        if (center.IsEmpty || center == null)
            throw new ArgumentException("Center can't be empty or null");
        if (center.X < 0 || center.Y < 0)
            throw new ArgumentException("Center can't be located outside of drawing field");
        if (standartSize.IsEmpty || standartSize == null)
            throw new ArgumentException("Standart size can't be empty or null");
        if (standartSize.Width < 0 || standartSize.Height < 0)
            throw new ArgumentException("Standart size can't be smaller than 0");
    } 
}