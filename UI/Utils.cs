namespace UI;

public static class Utils
{
    public static int ComputePosition(
        Alignment? alignment,
        string startValue,
        string endValue,
        int thisDimensionSize,
        int parentDimensionSize)
    {
        int value = 0;
        if (alignment != null)
        {
            value = ToPixels(alignment.Value, thisDimensionSize: thisDimensionSize, parentDimensionSize: parentDimensionSize);
        }
        if (startValue != null)
        {
            value += ToPixels(startValue, parentDimensionSize);
        }
        else if (endValue != null)
        {
            if (alignment != null)
            {
                value -= ToPixels(endValue, parentDimensionSize);
            }
            else
            {
                value += parentDimensionSize - thisDimensionSize - ToPixels(endValue, parentDimensionSize);
            }
        }
        return value;
    }

    public static int ToPixels(string positionValue, int parentDimensionSize)
    {
        if (positionValue.EndsWith('%'))
        {
            var fraction = float.Parse(positionValue.Substring(0, positionValue.Length - 1)) / 100f;
            return (int)(parentDimensionSize * fraction);
        }
        return int.Parse(positionValue);
    }

    public static int ToPixels(Alignment alignment, int thisDimensionSize, int parentDimensionSize)
    {
        switch (alignment)
        {
            case Alignment.Top:
            case Alignment.Left:
                return 0;
            case Alignment.Bottom:
            case Alignment.Right:
                return parentDimensionSize - thisDimensionSize;
            case Alignment.Center:
                return (parentDimensionSize - thisDimensionSize) / 2;
            default:
                // Just draw something
                return 0;
        }
    }
}
