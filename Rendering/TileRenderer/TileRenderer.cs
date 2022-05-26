using Mapster.Common.MemoryMappedTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public static class TileRenderer
{
    public static BaseShape Tessellate(this MapFeatureData feature, ref BoundingBox boundingBox, ref PriorityQueue<BaseShape, int> shapes)
    {
        BaseShape? baseShape = null;

        var featureType = feature.Type;
        if (feature.Properties.Any(p =>
                p.Key == GroundType.Highway && Enum.IsDefined(typeof(GroundSubType), p.Value.Item2.ToString())))
        {
            var coordinates = feature.Coordinates;
            var road = new Road(coordinates);
            baseShape = road;
            shapes.Enqueue(road, road.ZIndex);
        }
        else if (feature.Properties.Any(p => p.Key == GroundType.Water) && feature.Type != GeometryType.Point)
        {
            var coordinates = feature.Coordinates;

            var waterway = new Waterway(coordinates, feature.Type == GeometryType.Polygon);
            baseShape = waterway;
            shapes.Enqueue(waterway, waterway.ZIndex);
        }
        else if (Border.ShouldBeBorder(feature))
        {
            var coordinates = feature.Coordinates;
            var border = new Border(coordinates);
            baseShape = border;
            shapes.Enqueue(border, border.ZIndex);
        }
        else if (PopulatedPlace.ShouldBePopulatedPlace(feature))
        {
            var coordinates = feature.Coordinates;
            var popPlace = new PopulatedPlace(coordinates, feature);
            baseShape = popPlace;
            shapes.Enqueue(popPlace, popPlace.ZIndex);
        }
        else if (feature.Properties.Any(p => p.Key == GroundType.Railway))
        {
            var coordinates = feature.Coordinates;
            var railway = new Railway(coordinates);
            baseShape = railway;
            shapes.Enqueue(railway, railway.ZIndex);
        }
        else if (feature.Properties.Any(p => p.Key == GroundType.Natural && featureType == GeometryType.Polygon))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, feature);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Properties.Any(p => p.Key == GroundType.Boundary && p.Value.Item2 == GroundSubType.Forest))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Properties.Any(p => p.Key == GroundType.Landuse && (p.Value.Item2 == GroundSubType.Forest || p.Value.Item2 == GroundSubType.Orchard)))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon && feature.Properties.Any(p
                     => p.Key == GroundType.Landuse &&
                        (p.Value.Item2 == GroundSubType.Residential || p.Value.Item2 == GroundSubType.Cemetery ||
                         p.Value.Item2 == GroundSubType.Industrial || p.Value.Item2 == GroundSubType.Commercial ||
                         p.Value.Item2 == GroundSubType.Square || p.Value.Item2 == GroundSubType.Construction ||
                         p.Value.Item2 == GroundSubType.Military || p.Value.Item2 == GroundSubType.Quarry ||
                         p.Value.Item2 == GroundSubType.Brownfield)))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon && feature.Properties.Any(p
                     => p.Key == GroundType.Landuse &&
                        (p.Value.Item2 ==GroundSubType.Farm || p.Value.Item2 == GroundSubType.Meadow || p.Value.Item2 == GroundSubType.Grass ||
                         p.Value.Item2 == GroundSubType.Greenfield ||
                         p.Value.Item2 == GroundSubType.RecreationGround || p.Value.Item2 == GroundSubType.WinterSports ||
                         p.Value.Item2 == GroundSubType.Allotments)))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Plain);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon &&
                 feature.Properties.Any(p => p.Key == GroundType.Landuse && (p.Value.Item2 == GroundSubType.Reservoir || p.Value.Item2 == GroundSubType.Basin)))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Water);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon && feature.Properties.Any(p => p.Key == GroundType.Building))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon && feature.Properties.Any(p => p.Key == GroundType.Leisure))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }
        else if (feature.Type == GeometryType.Polygon && feature.Properties.Any(p => p.Key == GroundType.Amenity))
        {
            var coordinates = feature.Coordinates;
            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
            baseShape = geoFeature;
            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
        }

        if (baseShape != null)
        {
            for (var j = 0; j < baseShape.ScreenCoordinates.Length; ++j)
            {
                boundingBox.MinX = Math.Min(boundingBox.MinX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MaxX = Math.Max(boundingBox.MaxX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MinY = Math.Min(boundingBox.MinY, baseShape.ScreenCoordinates[j].Y);
                boundingBox.MaxY = Math.Max(boundingBox.MaxY, baseShape.ScreenCoordinates[j].Y);
            }
        }

        return baseShape;
    }

    public static Image<Rgba32> Render(this PriorityQueue<BaseShape, int> shapes, BoundingBox boundingBox, int width, int height)
    {
        var canvas = new Image<Rgba32>(width, height);

        // Calculate the scale for each pixel, essentially applying a normalization
        var scaleX = canvas.Width / (boundingBox.MaxX - boundingBox.MinX);
        var scaleY = canvas.Height / (boundingBox.MaxY - boundingBox.MinY);
        var scale = Math.Min(scaleX, scaleY);

        // Background Fill
        canvas.Mutate(x => x.Fill(Color.White));
        while (shapes.Count > 0)
        {
            var entry = shapes.Dequeue();
            entry.TranslateAndScale(boundingBox.MinX, boundingBox.MinY, scale, canvas.Height);
            canvas.Mutate(x => entry.Render(x));
        }

        return canvas;
    }

    public struct BoundingBox
    {
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }
}
