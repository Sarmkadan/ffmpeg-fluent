using FFmpegFluent;

public static class WatermarkHelperExtensions
{
    public static WatermarkHelper TopLeft(this WatermarkHelper helper, int margin = 10)
    {
        return helper.At(WatermarkPosition.TopLeft, margin);
    }

    public static WatermarkHelper TopRight(this WatermarkHelper helper, int margin = 10)
    {
        return helper.At(WatermarkPosition.TopRight, margin);
    }

    public static WatermarkHelper BottomLeft(this WatermarkHelper helper, int margin = 10)
    {
        return helper.At(WatermarkPosition.BottomLeft, margin);
    }

    public static WatermarkHelper BottomRight(this WatermarkHelper helper, int margin = 10)
    {
        return helper.At(WatermarkPosition.BottomRight, margin);
    }
}
