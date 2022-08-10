using System;
using FFmpegFluent;

namespace FFmpegFluent
{
    public static class FilterGraphExtensions
    {
        public static FilterGraph Overlay(this FilterGraph graph, string overlayFilter)
        {
            return graph.AddFilter("overlay", ("x", "0"), ("y", "0"), ("overlay", overlayFilter));
        }

        public static FilterGraph Scale(this FilterGraph graph, string scaleFilter)
        {
            return graph.AddFilter("scale", ("size", scaleFilter));
        }

        public static FilterGraph Fade(this FilterGraph graph, string fadeFilter)
        {
            return graph.AddFilter("fade", ("t", fadeFilter), ("type", "in"));
        }

        public static FilterGraph Crop(this FilterGraph graph, string cropFilter)
        {
            return graph.AddFilter("crop", ("w", cropFilter), ("h", cropFilter));
        }
    }
}
